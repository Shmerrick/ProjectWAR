using System.Collections.Generic;
using System.Linq;

namespace WorldServer.World.Objects
{
    public class Party
    {
        // NOTE: all thread synchronization for this class should be handled at the Group.cs level

        private int _partyId;
        private List<Player> _members = new List<Player>();
        private uint[] _reserved = new uint[6];

        public Party()
        {
            _partyId = Group.GetNextGroupId();
        }

        public void Clear()
        {
            _members.Clear();
            _partyId = 0;
        }

        public List<int> ReservedSlots
        {
            get
            {
                List<int> reserved = new List<int>();

                for (int i = 0; i < _reserved.Length; i++)
                {
                    if (_reserved[i] != 0)
                        reserved.Add(i + 1 + _members.Count);
                }

                return reserved;
            }
        }

        public bool ReserveSlot(Player player)
        {
            if (IsFull)
                return false;

            // Do nothing if the player is already  has slot reserved
            if (_reserved.Any(e => e == player.CharacterId))
                return true;

            for (int i = 0; i < _reserved.Length; i++)
            {
                if (_reserved[i] == 0)
                {
                    _reserved[i] = player.CharacterId;
                    return true;
                }
            }

            UpdateReserved();

            return false;
        }

        public void CancelReservation(Player player)
        {
            for (int i = 0; i < _reserved.Length; i++)
            {
                if (_reserved[i] == player.CharacterId)
                {
                    _reserved[i] = 0;
                }
            }
            UpdateReserved();
        }

        public bool AddPlayer(Player player)
        {
            if (IsFull)
                return false;

            // Do nothing if the player is already in the party
            foreach (Player p in _members)
                if (p == player)
                    return true;

            //if there are reserved slots in the party, take it, regardless of who had it reseved.
            for (int i = 0; i < _reserved.Length; i++)
            {
                if (_reserved[i] != 0)
                {
                    _reserved[i] = 0;
                    break;
                }
            }

            UpdateReserved();
            // Add the player to the party
            lock (_members)
                _members.Add(player);

            return true;
        }

        //Fills gaps, moves reserved slots together
        private void UpdateReserved()
        {
            var reserved = _reserved.Where(e => e != 0).ToList();
            for (int i = 0; i < _reserved.Length; i++)
                _reserved[i] = 0;

            for (int i = 0; i < reserved.Count; i++)
                _reserved[i] = reserved[i];
        }

        public bool RemovePlayer(Player player)
        {
            if(_members.Contains(player))
                return _members.Remove(player);

            _reserved.ToList().ForEach(e => { if (e == player.CharacterId) e = player.CharacterId; });

            return false;
        }

        public static bool MovePlayer(Player player, Party fromParty, Party toParty)
        {
            if (toParty.IsFull)
                return false;
            if (toParty.Contains(player))
                return true;
            if (!fromParty.Contains(player))
                return false;

            fromParty._members.Remove(player);
            toParty._members.Add(player);
            return true;
        }

        public static bool SwapPlayers(Party party1, Player player1, Party party2, Player player2)
        {
            int index1 = party1.Members.IndexOf(player1);
            int index2 = party2.Members.IndexOf(player2);
            if (index1 == -1 || index2 == -1)
                return false;
            party1.Members[index1] = player2;
            party2.Members[index2] = player1;
            return true;
        }

        public bool Contains(Player player)
        {
            return _members.Contains(player);
        }

        public bool Contains(Unit unit)
        {
            if (unit != null)
            {
                var members = _members.ToList(); 

                foreach (Player player in members)
                {
                    if (player == unit)
                        return true;
                    if (player.GetPet() == unit)
                        return true;
                }
            }

            return false;
        }

        public List<Player> Members
        {
            get
            {
                var players = new List<Player>();
                lock(_members)
                    players = _members.ToList();

                return players;
            }
        }

        public int PartyID
        {
            get { return _partyId; }
        }

        public bool IsFull
        {
            get { return _members.Count == 6; }
        }

        public bool IsEmpty
        {
            get { return _members.Count == 0; }
        }
    };
}
