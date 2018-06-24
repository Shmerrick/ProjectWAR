using System;

namespace WorldServer.World.BattleFronts.Objectives
{
    /// <summary>
    /// Object bound to an objective responsible of tracking players around with an history.
    /// </summary>
    internal class QuadrantHistoryTracker
    {
        const int OBJECTIVE_CLOSE_RADIUS = 600;

        /// <summary>Tracked objective</summary>
        private ProximityFlag _objective;

        /// <summary>
        /// A history of factor of population within this BO's quadrant.
        /// </summary>
        private readonly float[][] _controlRatioHistory = { new float[5], new float[5] };
        /// <summary>
        /// A history of factor of population within this BO's close control radius.
        /// </summary>
        private readonly float[][] _closeControlRatioHistory = { new float[5], new float[5] };
        /// <summary>
        /// A factor derived from the population distribution within this quadrant to determine the lock timer's duration.
        /// </summary>
        private readonly float[] _adjustedAverageControlRatios = new float[2];
        /// <summary>
        /// The highest control factor for a realm on this objective within the last 5 minutes. Used to detect movement of zergs.
        /// </summary>
        private readonly float[] _controlHighs = new float[2];
        private readonly int[] _quadrantPop = new int[2];
        /// <summary>Number of players in </summary>
        private readonly int[] _quadrantClosePop = new int[2];

        private int _curIndex;
        const int MAX_HISTORY_INDEX = 5;

        internal QuadrantHistoryTracker(ProximityFlag objective)
        {
            _objective = objective;
        }

        /// <summary>
        /// Advances the internal history tables of this tracker.
        /// </summary>
        /// <param name="orderCount">Total number of orders in the BattleFront lake</param>
        /// <param name="destroCount">Total number of orders in the BattleFront lake</param>
        internal void AdvancePopHistory(int orderCount, int destroCount)
        {
            _controlRatioHistory[0][_curIndex] = orderCount == 0 ? 0 : (float)_quadrantPop[0] / orderCount;
            _controlRatioHistory[1][_curIndex] = destroCount == 0 ? 0 : (float)_quadrantPop[1] / destroCount;

            _closeControlRatioHistory[0][_curIndex] = orderCount == 0 ? 0 : (float)_quadrantClosePop[0] / orderCount;
            _closeControlRatioHistory[1][_curIndex] = destroCount == 0 ? 0 : (float)_quadrantClosePop[1] / destroCount;

            for (int i = 0; i < 2; ++i)
            {
                _adjustedAverageControlRatios[i] = 0f;
                _controlHighs[i] = 0f;

                for (int j = 0; j < MAX_HISTORY_INDEX; ++j)
                {
                    float curCtrl = _controlRatioHistory[i][j];

                    if (curCtrl <= 0.15f)
                        _adjustedAverageControlRatios[i] += curCtrl * 1.6f;
                    else if (curCtrl <= 0.30f)
                        _adjustedAverageControlRatios[i] += 0.25f;
                    else if (curCtrl <= 0.55f)
                        _adjustedAverageControlRatios[i] += 0.25f - Math.Max(0, (curCtrl - 0.30f));

                    if (_closeControlRatioHistory[i][j] > _controlHighs[i])
                        _controlHighs[i] = curCtrl;
                }
            }

            ++_curIndex;

            if (_curIndex == MAX_HISTORY_INDEX)
                _curIndex = 0;

            _quadrantPop[0] = 0;
            _quadrantPop[1] = 0;

            _quadrantClosePop[0] = 0;
            _quadrantClosePop[1] = 0;
        }

        /// <summary>
        /// Registers a player as around the objective.
        /// </summary>
        /// <param name="player">Player around, not null</param>
        internal void AddPlayerInQuadrant(Player player)
        {
            ++_quadrantPop[(int)player.Realm - 1];

            if (player.IsWithinRadiusFeet(_objective, OBJECTIVE_CLOSE_RADIUS))
                ++_quadrantClosePop[(int)player.Realm - 1];
        }

        /// <summary>
        /// Sends objective diagnostic information to player (gm only).
        /// </summary>
        /// <param name="player">GM to send data to</param>
        /// <param name="bLocalZone">True to display player's local zone, false for tier zones</param>
        internal void SendDiagnostic(Player plr)
        {
            plr.SendClientMessage($"Control highs: Order {_controlHighs[0].ToString("P")}, Destruction {_controlHighs[1].ToString("P")}");
            plr.SendClientMessage($"Adjusted average control: Order {_adjustedAverageControlRatios[0].ToString("P")}, Destruction {_adjustedAverageControlRatios[1].ToString("P")}");
            plr.SendClientMessage($"Order control history: {_controlRatioHistory[0][0].ToString("P")}, {_controlRatioHistory[0][1].ToString("P")}, {_controlRatioHistory[0][2].ToString("P")}, {_controlRatioHistory[0][3].ToString("P")}, {_controlRatioHistory[0][4].ToString("P")}");
            plr.SendClientMessage($"Destruction control history: {_controlRatioHistory[1][0].ToString("P")}, {_controlRatioHistory[1][1].ToString("P")}, {_controlRatioHistory[1][2].ToString("P")}, {_controlRatioHistory[1][3].ToString("P")}, {_controlRatioHistory[1][4].ToString("P")}");
        }

    }
}
