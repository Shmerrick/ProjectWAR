using GameData;
using Item = WorldServer.World.Objects.Item;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public class PlayerRewardOptions
    {
        public uint CharacterId { get; set; }
        public Item[] ItemList { get; set; }
        public uint RenownLevel { get; set; }
        public uint RenownBand { get; set; }
        public string CharacterName { get; set; }
        public Realms CharacterRealm { get; set; }
    }

   
}