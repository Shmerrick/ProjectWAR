using Common;
using FrameWork;
using System.Collections.Generic;

namespace WorldServer.Services.World
{
    [Service]
    public class TokService : ServiceBase
    {
        public static Dictionary<ushort, Tok_Info> _Toks;
        public static List<Tok_Info> DiscoveringToks;
        public static Dictionary<ushort, Tok_Bestiary> _ToksBestary;

        [LoadingFunction(true)]
        public static void LoadTok_Infos()
        {
            Log.Debug("WorldMgr", "Loading LoadTok_Infos...");

            _Toks = new Dictionary<ushort, Tok_Info>();

            IList<Tok_Info> IToks = Database.SelectAllObjects<Tok_Info>();
            DiscoveringToks = new List<Tok_Info>();

            if (IToks != null)
            {
                foreach (Tok_Info Info in IToks)
                {
                    _Toks.Add(Info.Entry, Info);
                    if (Info.EventName.Contains("discovered") || Info.EventName.Contains("unlocked"))
                    {
                        DiscoveringToks.Add(Info);
                    }
                }
            }

            Log.Success("LoadTok_Infos", "Loaded " + _Toks.Count + " Tok_Infos");
        }

        [LoadingFunction(true)]
        public static void LoadTok_Bestiary()
        {
            Log.Debug("WorldMgr", "Loading LoadTok_Bestiary...");

            _ToksBestary = new Dictionary<ushort, Tok_Bestiary>();

            IList<Tok_Bestiary> IToks = Database.SelectAllObjects<Tok_Bestiary>();

            if (IToks != null)
            {
                foreach (Tok_Bestiary Info in IToks)
                {
                    _ToksBestary.Add(Info.Creature_Sub_Type, Info);
                }
            }

            Log.Success("LoadTok_Bestiary", "Loaded " + _ToksBestary.Count + " Tok_Bestiary");
        }

        public static Tok_Info GetTok(ushort Entry)
        {
            Tok_Info tok;
            _Toks.TryGetValue(Entry, out tok);
            return tok;
        }

        public static Tok_Bestiary GetTokBestary(ushort subTypeId)
        {
            Tok_Bestiary bestiary;
            _ToksBestary.TryGetValue(subTypeId, out bestiary);
            return bestiary;
        }
    }
}