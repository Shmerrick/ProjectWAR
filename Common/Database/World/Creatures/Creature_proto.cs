using System;
using System.Collections.Generic;

using FrameWork;
using GameData;

namespace Common
{
    public enum CreatureTypes
    {
        NONE = 0,
        ANIMALS = 1,
        DEAMONS = 2,
        HUMANOIDS = 3,
        MONSTERS = 4,
        PLANTS = 5,
        UNDEAD = 6
    };

    public enum CreatureSubTypes
    {
        NONE = 0,
        BEASTS = 1,
        BIRDS = 2,
        INSECTS_ARACHNIDS = 3,
        REPTILES = 5,
        UNMARKED_DEAMONS = 7,
        DEAMONDS_OF_KHORNE = 8,
        DEAMONS_OF_TZEENTCH = 9,
        DEAMONS_OF_NURGLE = 10,
        DEAMONS_OF_SLAANESH = 11,
        BEASMEN = 12,
        DWARFS =14,
        GREENSKINS = 15,
        HUMANS = 16,
        OGRES = 17,
        SKAVEN = 18,
        CHAOS_BREEDS = 19
    };

    // Fixed value of a character
    [DataTable(PreCache = false, TableName = "creature_protos", DatabaseName = "World")]
    [Serializable]
    public class Creature_proto : DataObject
    {
        public ushort[] _Unks = new ushort[7];

        public void SetCreatureTypesAndSubTypes()
        {
            // Sets the class, family and speciees of creatures
            switch (Model1)
            {
                case 1: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SNOTLING; break;
                case 2: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 3: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 4: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 5: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 6: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 7: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 8: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 9: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 10: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 11: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DISCIPLE_OF_KHAINE; break;
                case 12: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_BLACK_ORC; break;
                case 13: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_CHOPPA; break;
                case 14: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SHAMAN; break;
                case 15: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SQUIG_HERDER; break;
                case 16: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_IRONBREAKER; break;
                case 17: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_IRONBREAKER; break;
                case 18: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_HAMMERER; break;
                case 19: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_HAMMERER; break;
                case 20: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_ENGINEER; break;
                case 21: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_ENGINEER; break;
                case 22: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_RUNEPRIEST; break;
                case 23: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_RUNEPRIEST; break;
                case 24: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHOSEN; break;
                case 25: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_MARAUDER; break;
                case 26: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_ZEALOT; break;
                case 27: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_ZEALOT; break;
                case 28: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_MAGUS; break;
                case 29: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_MAGUS; break;
                case 30: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_KNIGHT_OF_THE_BLAZING_SUN; break;
                case 31: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_KNIGHT_OF_THE_BLAZING_SUN; break;
                case 32: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_BRIGHT_WIZARD; break;
                case 33: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_BRIGHT_WIZARD; break;
                case 34: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_WITCH_HUNTER; break;
                case 35: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_WITCH_HUNTER; break;
                case 36: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_WARRIOR_PRIEST; break;
                case 37: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_WARRIOR_PRIEST; break;
                case 38: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DISCIPLE_OF_KHAINE; break;
                case 39: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_BLACK_GUARD; break;
                case 40: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_BLACK_GUARD; break;
                case 41: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_SORCERESS; break;
                case 42: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_SORCERESS; break;
                case 43: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_WITCH_ELVES; break;
                case 44: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_ARCHMAGE; break;
                case 45: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_ARCHMAGE; break;
                case 46: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_WHITE_LION; break;
                case 47: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_WHITE_LION; break;
                case 48: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_SWORDMASTER; break;
                case 49: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_SWORDMASTER; break;
                case 50: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_SHADOW_WARRIOR; break;
                case 51: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_SHADOW_WARRIOR; break;
                case 52: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_IRONBREAKER; break;
                case 53: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_IRONBREAKER; break;
                case 54: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_WARRIOR_PRIEST; break;
                case 55: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_WARRIOR_PRIEST; break;
                case 56: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_ARCHMAGE; break;
                case 57: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_ARCHMAGE; break;
                case 58: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_BLACK_ORC; break;
                case 59: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SQUIG_HERDER; break;
                case 60: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHOSEN; break;
                case 61: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_MAGUS; break;
                case 62: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_MAGUS; break;
                case 63: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_BLACK_GUARD; break;
                case 64: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 65: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 66: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 67: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 68: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 69: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 70: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 71: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 72: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 73: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 74: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 75: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 76: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 77: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 78: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 79: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 80: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 81: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 82: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 83: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 84: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 85: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 86: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 87: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 88: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 89: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 90: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 91: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 92: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 93: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 94: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 95: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 96: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 97: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 98: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 99: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 100: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 101: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 102: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 103: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 104: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 105: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 106: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 107: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 113: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_DWARF; break;
                case 114: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_DWARF; break;
                case 115: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_DWARF; break;
                case 116: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_ORC; break;
                case 117: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_ORC; break;
                case 118: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_ORC; break;
                case 119: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_ORC; break;
                case 120: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 121: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 122: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 123: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 124: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 125: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 126: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 127: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 128: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 129: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 130: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 131: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 132: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_GREAT_CAT; break;
                case 133: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_GREAT_CAT; break;
                case 134: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_GREAT_CAT; break;
                case 135: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_GREAT_CAT; break;
                case 136: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SQUIG; break;
                case 137: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SQUIG; break;
                case 138: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SQUIG; break;
                case 139: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SQUIG; break;
                case 140: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SQUIG; break;
                case 141: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_TZEENTCH; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_TZEENTCH_HORROR; break;
                case 142: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_TZEENTCH; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_TZEENTCH_HORROR; break;
                case 143: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_TZEENTCH; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_TZEENTCH_FLAMER; break;
                case 144: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_TZEENTCH; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_TZEENTCH_FIREWYRM; break;
                case 145: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 146: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 147: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 148: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 149: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 150: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 151: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 152: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 153: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 154: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_NURGLE; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_NURGLE_NURGLING; break;
                case 155: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BEAR; break;
                case 156: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SKELETONS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SKELETONS_SKELETON; break;
                case 157: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_SKAVEN; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_SKAVEN_SKAVEN; break;
                case 158: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 159: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 160: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 161: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 162: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 163: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 164: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 165: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 166: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 167: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 168: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 169: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 170: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 171: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 172: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 173: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 174: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 175: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 176: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 177: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 178: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 179: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 180: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 181: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_ORC; break;
                case 182: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_ORC; break;
                case 183: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_ORC; break;
                case 184: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 185: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 186: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 187: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 188: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SQUIG; break;
                case 189: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SQUIG; break;
                case 190: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SQUIG; break;
                case 191: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SQUIG; break;
                case 192: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SQUIG; break;
                case 193: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SQUIG; break;
                case 194: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SQUIG; break;
                case 195: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 196: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 197: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 198: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 199: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 200: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 201: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_GOBLIN; break;
                case 202: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_ORC; break;
                case 203: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 204: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 205: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 206: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 207: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_EMPIRE; break;
                case 208: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_EMPIRE; break;
                case 209: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 210: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 211: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 212: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 213: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 214: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 215: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 216: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 217: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 218: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 219: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 220: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 221: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 222: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 223: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 224: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 225: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 226: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 227: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 228: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 229: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 230: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 231: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 232: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 233: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 234: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 235: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 236: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 237: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 238: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 239: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 240: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 241: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 242: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 243: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 244: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 245: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 246: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 247: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 248: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 249: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 250: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 251: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 252: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 253: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 254: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 255: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 256: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 257: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 258: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 259: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 260: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 261: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 262: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 263: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 264: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 265: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 266: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 267: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 268: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 269: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 270: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 271: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 272: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 273: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 274: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 275: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 276: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 277: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 278: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 279: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 280: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 281: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 282: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 283: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 284: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 285: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 286: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 287: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 288: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 289: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 290: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 291: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 292: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 293: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 294: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 295: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 296: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 297: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 298: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 299: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 300: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 301: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 302: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 303: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 304: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 305: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 306: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 307: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 308: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 309: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 310: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 311: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 312: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 313: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 314: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 315: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 316: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 317: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 318: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 319: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 320: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 321: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 322: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 323: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 324: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 325: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 326: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 327: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 328: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 329: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 330: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 331: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 332: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 333: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 334: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 335: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 336: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 337: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 338: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 339: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 340: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 341: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 342: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 343: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 344: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 345: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 346: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 347: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 348: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 349: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 400: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 401: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 402: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 403: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 404: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 666: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 777: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 999: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1000: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1001: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_DWARF; break;
                case 1002: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_DWARF; break;
                case 1003: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_ORC; break;
                case 1004: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_GOBLIN; break;
                case 1005: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_EMPIRE; break;
                case 1006: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_EMPIRE; break;
                case 1007: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1008: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1009: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1010: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1011: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1012: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1013: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1014: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_OGRES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_OGRES_OGRE; break;
                case 1015: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_SKAVEN; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_SKAVEN_SKAVEN; break;
                case 1016: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_BEASTMEN; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_BEASTMEN_UNGOR; break;
                case 1017: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_BEASTMEN; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_BEASTMEN_GOR; break;
                case 1018: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_BEASTMEN; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_BEASTMEN_BESTIGOR; break;
                case 1019: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_BEASTMEN; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_BEASTMEN_DOOMBULL; break;
                case 1020: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_GNOBLAR; break;
                case 1021: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SKELETONS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SKELETONS_SKELETON; break;
                case 1022: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SKELETONS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SKELETONS_SKELETON; break;
                case 1023: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SKELETONS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SKELETONS_SKELETON; break;
                case 1024: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SKELETONS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SKELETONS_SKELETON; break;
                case 1025: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SKELETONS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SKELETONS_SKELETON; break;
                case 1026: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_ZOMBIES; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_ZOMBIES_ZOMBIE; break;
                case 1027: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_ZOMBIES; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_ZOMBIES_ZOMBIE; break;
                case 1028: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_ZOMBIES; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_ZOMBIES_ZOMBIE; break;
                case 1029: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_ZOMBIES; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_ZOMBIES_ZOMBIE; break;
                case 1030: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_ZOMBIES; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_ZOMBIES_ZOMBIE; break;
                case 1031: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_ZOMBIES; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_ZOMBIES_ZOMBIE; break;
                case 1032: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_ZOMBIES; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_ZOMBIES_ZOMBIE; break;
                case 1033: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_ZOMBIES; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_ZOMBIES_ZOMBIE; break;
                case 1034: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_ZOMBIES; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_ZOMBIES_ZOMBIE; break;
                case 1035: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_ZOMBIES; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_ZOMBIES_ZOMBIE; break;
                case 1036: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_ZOMBIES; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_ZOMBIES_ZOMBIE; break;
                case 1037: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_ZOMBIES; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_ZOMBIES_ZOMBIE; break;
                case 1038: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_ZOMBIES; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_ZOMBIES_ZOMBIE; break;
                case 1039: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_ZOMBIES; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_ZOMBIES_ZOMBIE; break;
                case 1040: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_ZOMBIES; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_ZOMBIES_ZOMBIE; break;
                case 1041: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_ZOMBIES; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_ZOMBIES_ZOMBIE; break;
                case 1042: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_DWARF; break;
                case 1043: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_DWARF; break;
                case 1044: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_ORC; break;
                case 1045: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_GOBLIN; break;
                case 1046: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_EMPIRE; break;
                case 1047: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_EMPIRE; break;
                case 1048: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1049: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1050: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1051: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1052: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1053: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1054: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1055: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_OGRES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_OGRES_GORGER; break;
                case 1056: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_SKAVEN; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_SKAVEN_SKAVEN; break;
                case 1057: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_BEASTMEN; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_BEASTMEN_GOR; break;
                case 1058: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 1059: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 1060: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 1061: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 1062: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 1063: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 1064: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 1065: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 1066: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 1067: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 1068: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 1069: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 1070: CreatureType = (byte)GameData.CreatureTypes.MONSTERS_TROLLS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_TROLLS_TROLL; break;
                case 1071: CreatureType = (byte)GameData.CreatureTypes.MONSTERS_TROLLS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_TROLLS_RIVER_TROLL; break;
                case 1072: CreatureType = (byte)GameData.CreatureTypes.MONSTERS_TROLLS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_TROLLS_STONE_TROLL; break;
                case 1073: CreatureType = (byte)GameData.CreatureTypes.MONSTERS_TROLLS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_TROLLS_CHAOS_TROLL; break;
                case 1074: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SQUIG; break;
                case 1075: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SQUIG; break;
                case 1076: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SQUIG; break;
                case 1077: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SQUIG; break;
                case 1078: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SQUIG; break;
                case 1079: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SNOTLING; break;
                case 1080: CreatureType = (byte)GameData.CreatureTypes.MONSTERS_GIANTS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_GIANTS_GIANT; break;
                case 1081: CreatureType = (byte)GameData.CreatureTypes.MONSTERS_GIANTS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_GIANTS_CHAOS_GIANT; break;
                case 1082: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_BIRD; break;
                case 1083: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_BIRD; break;
                case 1084: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_BIRD; break;
                case 1085: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_BIRD; break;
                case 1086: CreatureType = (byte)GameData.CreatureTypes.MONSTERS_CHAOS_BREEDS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_CHAOS_BREEDS_HARPY; break;
                case 1087: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1088: CreatureType = (byte)GameData.CreatureTypes.PLANTS_FOREST_SPIRITS; CreatureSubType = (byte)GameData.CreatureSubTypes.PLANTS_FOREST_SPIRITS_DRYAD; break;
                case 1089: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_UNMARKED; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_UNMARKED_DAEMONS_CHAOS_FURY; break;
                case 1090: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_NURGLE; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_NURGLE_PLAGUEBEARER; break;
                case 1091: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_NURGLE; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_NURGLE_NURGLING; break;
                case 1092: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_SLAANESH; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_SLAANESH_DAEMONETTE; break;
                case 1093: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_KHORNE; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_KHORNE_FLESH_HOUND; break;
                case 1094: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_KHORNE; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_KHORNE_BLOODLETTER; break;
                case 1095: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_KHORNE; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_KHORNE_JUGGERNAUT_OF_KHORNE; break;
                case 1096: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_TZEENTCH; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_TZEENTCH_HORROR; break;
                case 1097: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_TZEENTCH; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_TZEENTCH_FLAMER; break;
                case 1098: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_TZEENTCH; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_TZEENTCH_SCREAMER; break;
                case 1099: CreatureType = (byte)GameData.CreatureTypes.PLANTS_FOREST_SPIRITS; CreatureSubType = (byte)GameData.CreatureSubTypes.PLANTS_FOREST_SPIRITS_SPITE; break;
                case 1100: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_UNMARKED; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_UNMARKED_DAEMONS_CHAOS_HOUND; break;
                case 1101: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_INSECTS_ARACHNIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_INSECTS_ARACHNIDS_GIANT_SPIDER; break;
                case 1102: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_GIANT_BAT; break;
                case 1103: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SPIRITS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SPIRITS_SPIRIT_HOST; break;
                case 1104: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SPIRITS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SPIRITS_WRAITH; break;
                case 1105: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SPIRITS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SPIRITS_BANSHEE; break;
                case 1106: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_GREATER_UNDEAD; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_GREATER_UNDEAD_LICHE; break;
                case 1107: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_GHOUL; break;
                case 1108: CreatureType = (byte)GameData.CreatureTypes.MONSTERS_CHAOS_BREEDS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_CHAOS_BREEDS_FLAYERKIN; break;
                case 1109: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_SLAYER; break;
                case 1110: CreatureType = (byte)GameData.CreatureTypes.MONSTERS_CHAOS_BREEDS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_CHAOS_BREEDS_TUSKGOR; break;
                case 1111: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1112: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1113: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1114: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1115: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1116: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1117: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1118: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1119: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1120: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1121: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1122: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1123: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1124: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1125: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1126: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1127: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1128: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1129: CreatureType = (byte)GameData.CreatureTypes.MAMONSTERS_GICAL_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_MAGICAL_BEASTS_UNICORN; break;
                case 1130: CreatureType = (byte)GameData.CreatureTypes.MAMONSTERS_GICAL_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_MAGICAL_BEASTS_PEGASUS; break;
                case 1131: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_BAT; break;
                case 1132: CreatureType = (byte)GameData.CreatureTypes.MAMONSTERS_GICAL_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_MAGICAL_BEASTS_COCKATRICE; break;
                case 1133: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BEAR; break;
                case 1134: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BEAR; break;
                case 1135: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BEAR; break;
                case 1136: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_GREAT_CAT; break;
                case 1137: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_GREAT_CAT; break;
                case 1138: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_GREAT_CAT; break;
                case 1139: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_GREAT_CAT; break;
                case 1140: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BASILISK; break;
                case 1141: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_HOUND; break;
                case 1142: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_INSECTS_ARACHNIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_INSECTS_ARACHNIDS_GIANT_SCORPION; break;
                case 1143: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_DEER; break;
                case 1144: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_HOUND; break;
                case 1145: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_OGRES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_OGRES_YHETEE; break;
                case 1146: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_OGRES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_OGRES_GORGER; break;
                case 1147: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_CONSTRUCTS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_CONSTRUCTS_CONSTRUCT; break;
                case 1148: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_SKAVEN; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_SKAVEN_RAT_OGRE; break;
                case 1149: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 1150: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 1151: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 1152: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 1153: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 1154: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 1155: CreatureType = (byte)GameData.CreatureTypes.MAMONSTERS_GICAL_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_MAGICAL_BEASTS_GRIFFON; break;
                case 1156: CreatureType = (byte)GameData.CreatureTypes.MAMONSTERS_GICAL_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_MAGICAL_BEASTS_MANTICORE; break;
                case 1157: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_UNMARKED; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_UNMARKED_DAEMONS_CHAOS_SPAWN; break;
                case 1158: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_KHORNE; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_KHORNE_BLOODBEAST; break;
                case 1159: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_NURGLE; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_NURGLE_PLAGUEBEAST; break;
                case 1160: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_TZEENTCH; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_TZEENTCH_WATCHER; break;
                case 1161: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_SLAANESH; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_SLAANESH_FIEND; break;
                case 1162: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_RHINOX; break;
                case 1163: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1164: CreatureType = (byte)GameData.CreatureTypes.MONSTERS_CHAOS_BREEDS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_CHAOS_BREEDS_CENTIGOR; break;
                case 1165: CreatureType = (byte)GameData.CreatureTypes.PLANTS_FOREST_SPIRITS; CreatureSubType = (byte)GameData.CreatureSubTypes.PLANTS_FOREST_SPIRITS_TREEKIN; break;
                case 1166: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_CONSTRUCTS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_CONSTRUCTS_WINGED_NIGHTMARE; break;
                case 1167: CreatureType = (byte)GameData.CreatureTypes.MONSTERS_DRAGONOIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_DRAGONOIDS_WYVERN; break;
                case 1168: CreatureType = (byte)GameData.CreatureTypes.MONSTERS_CHAOS_BREEDS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_CHAOS_BREEDS_DRAGON_OGRE; break;
                case 1169: CreatureType = (byte)GameData.CreatureTypes.MONSTERS_DRAGONOIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_DRAGONOID_DRAGON; break;
                case 1170: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1171: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SQUIG; break;
                case 1172: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHOSEN; break;
                case 1173: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_SLAANESH; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_SLAANESH_KEEPER_OF_SECRETS; break;
                case 1174: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1175: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1176: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1177: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1178: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1179: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1180: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1181: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1182: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1183: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1184: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_ORC; break;
                case 1185: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SHAMAN; break;
                case 1186: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_DWARF; break;
                case 1187: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_EMPIRE; break;
                case 1188: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1189: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1190: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1191: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_GIANT_LIZARD; break;
                case 1192: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_GIANT_LIZARD; break;
                case 1193: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_GIANT_LIZARD; break;
                case 1194: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_GIANT_LIZARD; break;
                case 1195: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1196: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1197: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_SHEEP; break;
                case 1198: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_COW; break;
                case 1199: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_COW; break;
                case 1200: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_LIZARD; break;
                case 1201: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_DEER; break;
                case 1202: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_DEER; break;
                case 1203: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_DEER; break;
                case 1204: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_BIRD; break;
                case 1205: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_BIRD; break;
                case 1206: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_BIRD; break;
                case 1207: CreatureType = (byte)GameData.CreatureTypes.MONSTERS_TROLLS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_TROLLS_TROLL; break;
                case 1208: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_HARE; break;
                case 1209: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_PIG; break;
                case 1210: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1211: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_RAT; break;
                case 1212: CreatureType = (byte)GameData.CreatureTypes.MONSTERS_CHAOS_BREEDS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_CHAOS_BREEDS_CHAOS_MUTANT; break;
                case 1213: CreatureType = (byte)GameData.CreatureTypes.MONSTERS_CHAOS_BREEDS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_CHAOS_BREEDS_CHAOS_MUTANT; break;
                case 1214: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_DWARF; break;
                case 1215: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_DWARF; break;
                case 1216: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_ORC; break;
                case 1217: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_GOBLIN; break;
                case 1218: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_EMPIRE; break;
                case 1219: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_EMPIRE; break;
                case 1220: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1221: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1222: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1223: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1224: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1225: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1226: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1227: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_BRIGHT_WIZARD; break;
                case 1228: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_BRIGHT_WIZARD; break;
                case 1229: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_MAGUS; break;
                case 1230: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_MAGUS; break;
                case 1231: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1232: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_BRIGHT_WIZARD; break;
                case 1233: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_BRIGHT_WIZARD; break;
                case 1234: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_MAGUS; break;
                case 1235: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_MAGUS; break;
                case 1236: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_OGRES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_OGRES_OGRE_TYRANT; break;
                case 1237: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1238: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1239: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_SLAYER; break;
                case 1240: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_DWARF; break;
                case 1241: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1242: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SQUIG; break;
                case 1243: CreatureType = (byte)GameData.CreatureTypes.MONSTERS_DRAGONOIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_DRAGONOIDS_WYVERN; break;
                case 1244: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_KHORNE; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_KHORNE_BLOODTHIRSTER; break;
                case 1245: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_UNMARKED; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_UNMARKED_DAEMONS_DAEMONVINE; break;
                case 1246: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_ZOMBIES; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_ZOMBIES_ZOMBIE; break;
                case 1247: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_CHICKEN; break;
                case 1248: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_CHICKEN; break;
                case 1249: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_OGRES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_OGRES_OGRE_BULL; break;
                case 1250: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1251: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_UNMARKED; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_UNMARKED_DAEMONS_DAEMON_PRINCE; break;
                case 1252: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_KHORNE; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_KHORNE_BLOODLETTER; break;
                case 1253: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_GREATER_UNDEAD; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_GREATER_UNDEAD_VAMPIRE; break;
                case 1254: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SAVAGE_ORC; break;
                case 1255: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHOSEN; break;
                case 1256: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_NURGLE; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_NURGLE_GREAT_UNCLEAN_ONE; break;
                case 1257: CreatureType = (byte)GameData.CreatureTypes.MONSTERS_DRAGONOIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_DRAGONOID_DRAGON; break;
                case 1258: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SAVAGE_ORC; break;
                case 1259: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1260: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1261: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1262: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1263: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1264: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1265: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1266: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1267: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1268: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1269: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1270: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_DOG; break;
                case 1271: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_CAT; break;
                case 1272: CreatureType = (byte)GameData.CreatureTypes.MONSTERS_DRAGONOIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_DRAGONOIDS_HYDRA; break;
                case 1273: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1274: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1275: CreatureType = (byte)GameData.CreatureTypes.MONSTERS_DRAGONOIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_DRAGONOID_DRAGON; break;
                case 1276: CreatureType = (byte)GameData.CreatureTypes.MONSTERS_DRAGONOIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_DRAGONOID_DRAGON; break;
                case 1277: CreatureType = (byte)GameData.CreatureTypes.MONSTERS_DRAGONOIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_DRAGONOID_DRAGON; break;
                case 1278: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SKELETONS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SKELETONS_SKELETON; break;
                case 1279: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_HUMAN; break;
                case 1280: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_HUMAN; break;
                case 1281: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_HUMAN; break;
                case 1282: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_BIRD; break;
                case 1283: CreatureType = (byte)GameData.CreatureTypes.MAMONSTERS_GICAL_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_MAGICAL_BEASTS_IMP; break;
                case 1284: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1285: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_DWARF; break;
                case 1286: CreatureType = (byte)GameData.CreatureTypes.MONSTERS_DRAGONOIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_DRAGONOID_DRAGON; break;
                case 1287: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_CRAB; break;
                case 1288: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_DWARF; break;
                case 1289: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_DWARF; break;
                case 1290: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_ORC; break;
                case 1291: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_GOBLIN; break;
                case 1292: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_EMPIRE; break;
                case 1293: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_EMPIRE; break;
                case 1294: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1295: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1296: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1297: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1298: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1299: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1300: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1301: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_DWARF; break;
                case 1302: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_DWARF; break;
                case 1303: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_ORC; break;
                case 1304: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_GOBLIN; break;
                case 1305: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_EMPIRE; break;
                case 1306: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_EMPIRE; break;
                case 1307: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1308: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1309: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1310: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1311: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1312: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1313: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1314: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1315: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1316: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1317: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_SORCERESS; break;
                case 1318: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_WITCH_ELVES; break;
                case 1319: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_SORCERESS; break;
                case 1320: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1321: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1322: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_WITCH_ELVES; break;
                case 1323: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1324: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1325: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1326: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1327: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1328: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1329: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1330: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1331: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1332: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1333: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1334: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1335: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_DWARF; break;
                case 1336: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1337: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1338: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1339: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_GREATER_UNDEAD; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_GREATER_UNDEAD_VAMPIRE; break;
                case 1340: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1341: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1342: CreatureType = (byte)GameData.CreatureTypes.MONSTERS_CHAOS_BREEDS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_CHAOS_BREEDS_HARPY; break;
                case 1343: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_EMPIRE; break;
                case 1344: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_EMPIRE; break;
                case 1345: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_GIANT_LIZARD; break;
                case 1346: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_GIANT_LIZARD; break;
                case 1347: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_GIANT_LIZARD; break;
                case 1348: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_GIANT_LIZARD; break;
                case 1349: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_GIANT_LIZARD; break;
                case 1350: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_GIANT_LIZARD; break;
                case 1351: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_GIANT_LIZARD; break;
                case 1352: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_CONSTRUCTS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_CONSTRUCTS_WINGED_NIGHTMARE; break;
                case 1353: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_WHITE_LION; break;
                case 1354: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_WHITE_LION; break;
                case 1355: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_WITCH_ELVES; break;
                case 1356: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_WITCH_ELVES; break;
                case 1357: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_BLACK_GUARD; break;
                case 1358: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_BLACK_GUARD; break;
                case 1359: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_BLACK_GUARD; break;
                case 1360: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_BLACK_GUARD; break;
                case 1361: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SPIRITS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SPIRITS_BANSHEE; break;
                case 1362: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SPIRITS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SPIRITS_BANSHEE; break;
                case 1363: CreatureType = (byte)GameData.CreatureTypes.PLANTS_FOREST_SPIRITS; CreatureSubType = (byte)GameData.CreatureSubTypes.PLANTS_FOREST_SPIRITS_DRYAD; break;
                case 1364: CreatureType = (byte)GameData.CreatureTypes.PLANTS_FOREST_SPIRITS; CreatureSubType = (byte)GameData.CreatureSubTypes.PLANTS_FOREST_SPIRITS_DRYAD; break;
                case 1365: CreatureType = (byte)GameData.CreatureTypes.PLANTS_FOREST_SPIRITS; CreatureSubType = (byte)GameData.CreatureSubTypes.PLANTS_FOREST_SPIRITS_DRYAD; break;
                case 1366: CreatureType = (byte)GameData.CreatureTypes.PLANTS_FOREST_SPIRITS; CreatureSubType = (byte)GameData.CreatureSubTypes.PLANTS_FOREST_SPIRITS_DRYAD; break;
                case 1367: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_UNMARKED; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_UNMARKED_DAEMONS_CHAOS_FURY; break;
                case 1368: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_UNMARKED; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_UNMARKED_DAEMONS_CHAOS_FURY; break;
                case 1369: CreatureType = (byte)GameData.CreatureTypes.MONSTERS_CHAOS_BREEDS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_CHAOS_BREEDS_HARPY; break;
                case 1370: CreatureType = (byte)GameData.CreatureTypes.MONSTERS_CHAOS_BREEDS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_CHAOS_BREEDS_HARPY; break;
                case 1371: CreatureType = (byte)GameData.CreatureTypes.MONSTERS_CHAOS_BREEDS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_CHAOS_BREEDS_HARPY; break;
                case 1372: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_GIANT_LIZARD; break;
                case 1373: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_GIANT_LIZARD; break;
                case 1374: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_GIANT_LIZARD; break;
                case 1375: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_INSECTS_ARACHNIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_INSECTS_ARACHNIDS_GIANT_SPIDER; break;
                case 1376: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_INSECTS_ARACHNIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_INSECTS_ARACHNIDS_GIANT_SPIDER; break;
                case 1377: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_INSECTS_ARACHNIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_INSECTS_ARACHNIDS_GIANT_SPIDER; break;
                case 1378: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_INSECTS_ARACHNIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_INSECTS_ARACHNIDS_GIANT_SPIDER; break;
                case 1379: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_INSECTS_ARACHNIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_INSECTS_ARACHNIDS_GIANT_SPIDER; break;
                case 1380: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_INSECTS_ARACHNIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_INSECTS_ARACHNIDS_GIANT_SPIDER; break;
                case 1381: CreatureType = (byte)GameData.CreatureTypes.PLANTS_FOREST_SPIRITS; CreatureSubType = (byte)GameData.CreatureSubTypes.PLANTS_FOREST_SPIRITS_SPITE; break;
                case 1382: CreatureType = (byte)GameData.CreatureTypes.PLANTS_FOREST_SPIRITS; CreatureSubType = (byte)GameData.CreatureSubTypes.PLANTS_FOREST_SPIRITS_SPITE; break;
                case 1383: CreatureType = (byte)GameData.CreatureTypes.PLANTS_FOREST_SPIRITS; CreatureSubType = (byte)GameData.CreatureSubTypes.PLANTS_FOREST_SPIRITS_SPITE; break;
                case 1384: CreatureType = (byte)GameData.CreatureTypes.PLANTS_FOREST_SPIRITS; CreatureSubType = (byte)GameData.CreatureSubTypes.PLANTS_FOREST_SPIRITS_SPITE; break;
                case 1385: CreatureType = (byte)GameData.CreatureTypes.MAMONSTERS_GICAL_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_MAGICAL_BEASTS_IMP; break;
                case 1386: CreatureType = (byte)GameData.CreatureTypes.MAMONSTERS_GICAL_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_MAGICAL_BEASTS_IMP; break;
                case 1387: CreatureType = (byte)GameData.CreatureTypes.MAMONSTERS_GICAL_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_MAGICAL_BEASTS_IMP; break;
                case 1388: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1389: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_SWORDMASTER; break;
                case 1390: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_SHADOW_WARRIOR; break;
                case 1391: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_SHADOW_WARRIOR; break;
                case 1392: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_SHADOW_WARRIOR; break;
                case 1393: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_SHADOW_WARRIOR; break;
                case 1394: CreatureType = (byte)GameData.CreatureTypes.MAMONSTERS_GICAL_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_MAGICAL_BEASTS_MANTICORE; break;
                case 1395: CreatureType = (byte)GameData.CreatureTypes.MAMONSTERS_GICAL_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_MAGICAL_BEASTS_MANTICORE; break;
                case 1396: CreatureType = (byte)GameData.CreatureTypes.MAMONSTERS_GICAL_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_MAGICAL_BEASTS_MANTICORE; break;
                case 1397: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_CONSTRUCTS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_CONSTRUCTS_WINGED_NIGHTMARE; break;
                case 1398: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_CONSTRUCTS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_CONSTRUCTS_WINGED_NIGHTMARE; break;
                case 1399: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_CONSTRUCTS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_CONSTRUCTS_WINGED_NIGHTMARE; break;
                case 1400: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1401: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1402: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1403: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1404: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_GOBLIN; break;
                case 1405: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_GOBLIN; break;
                case 1406: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1407: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1408: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_EMPIRE; break;
                case 1409: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_GOBLIN; break;
                case 1410: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_GOBLIN; break;
                case 1411: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1412: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1413: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1414: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1415: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1416: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1417: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1418: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1419: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1420: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1421: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1422: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1423: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1424: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1425: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1426: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1427: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1428: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1429: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1430: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1431: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1432: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1433: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1434: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1435: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1436: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1437: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1438: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1439: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1440: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1441: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1442: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1443: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1444: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1445: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1446: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1447: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1448: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1449: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1450: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1451: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1452: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1453: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1454: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1455: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1456: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1457: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1458: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1459: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1460: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1461: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1462: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1463: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1464: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1465: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1466: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1467: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1468: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1469: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1470: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_SORCERESS; break;
                case 1471: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_SORCERESS; break;
                case 1472: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_SORCERESS; break;
                case 1473: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_SORCERESS; break;
                case 1474: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_SORCERESS; break;
                case 1475: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_SORCERESS; break;
                case 1476: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_SORCERESS; break;
                case 1477: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_SORCERESS; break;
                case 1478: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_SORCERESS; break;
                case 1479: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_SORCERESS; break;
                case 1480: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1481: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1482: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_WITCH_ELVES; break;
                case 1483: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_WITCH_ELVES; break;
                case 1484: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_WITCH_ELVES; break;
                case 1485: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_WITCH_ELVES; break;
                case 1486: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_WITCH_ELVES; break;
                case 1487: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_DWARF; break;
                case 1488: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_DWARF; break;
                case 1489: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_DWARF; break;
                case 1490: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_DWARF; break;
                case 1491: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_EMPIRE; break;
                case 1492: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_EMPIRE; break;
                case 1493: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_EMPIRE; break;
                case 1494: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_EMPIRE; break;
                case 1495: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_SORCERESS; break;
                case 1496: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_SORCERESS; break;
                case 1497: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1498: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1499: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1500: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1501: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1502: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1503: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1504: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1505: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_ARCHMAGE; break;
                case 1506: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_ARCHMAGE; break;
                case 1507: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1508: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1509: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1510: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_SHADOW_WARRIOR; break;
                case 1511: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_SHADOW_WARRIOR; break;
                case 1512: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1513: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1514: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1515: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1516: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1517: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1518: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1519: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1520: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_ARCHMAGE; break;
                case 1521: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_ARCHMAGE; break;
                case 1522: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1523: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1524: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1525: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1526: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_SHADOW_WARRIOR; break;
                case 1527: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_SHADOW_WARRIOR; break;
                case 1528: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1529: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1530: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1531: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1532: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1533: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1534: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1535: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1536: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1537: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1538: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1539: CreatureType = (byte)GameData.CreatureTypes.MAMONSTERS_GICAL_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_MAGICAL_BEASTS_GRIFFON; break;
                case 1540: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_EMPIRE; break;
                case 1541: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_EMPIRE; break;
                case 1542: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_UNMARKED; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_UNMARKED_DAEMONS_WALKER; break;
                case 1543: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_WARRIOR_PRIEST; break;
                case 1544: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_WARRIOR_PRIEST; break;
                case 1545: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_BRIGHT_WIZARD; break;
                case 1546: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_BRIGHT_WIZARD; break;
                case 1547: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_NIGHT_GOBLIN; break;
                case 1548: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1549: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1550: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1551: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1552: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1553: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1554: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1555: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1556: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1557: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1558: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1559: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1560: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1561: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1562: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1563: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1564: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1565: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1566: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1567: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1568: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1569: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1570: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1571: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1572: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1573: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1574: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1575: CreatureType = (byte)GameData.CreatureTypes.MONSTERS_CHAOS_BREEDS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_CHAOS_BREEDS_MAGGOT; break;
                case 1576: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_ORC; break;
                case 1577: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_ORC; break;
                case 1578: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1579: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1580: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_GHOUL; break;
                case 1581: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_GHOUL; break;
                case 1582: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1583: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1584: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_UNMARKED; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_UNMARKED_DAEMONS_WALKER; break;
                case 1585: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_UNMARKED; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_UNMARKED_DAEMONS_WALKER; break;
                case 1586: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1587: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1588: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1589: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_DWARF; break;
                case 1590: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_DWARF; break;
                case 1591: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_ORC; break;
                case 1592: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_GOBLIN; break;
                case 1593: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_EMPIRE; break;
                case 1594: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_EMPIRE; break;
                case 1595: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1596: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1597: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1598: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1599: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1600: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1601: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1602: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_DWARF; break;
                case 1603: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1604: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1605: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_INSECTS_ARACHNIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_INSECTS_ARACHNIDS_GIANT_SPIDER; break;
                case 1606: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_UNMARKED; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_UNMARKED_DAEMONS_DAEMONVINE; break;
                case 1607: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1608: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_BRIGHT_WIZARD; break;
                case 1609: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1610: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SPIRITS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SPIRITS_SPIRIT_HOST; break;
                case 1611: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SPIRITS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SPIRITS_SPIRIT_HOST; break;
                case 1612: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SPIRITS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SPIRITS_SPIRIT_HOST; break;
                case 1613: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SPIRITS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SPIRITS_SPIRIT_HOST; break;
                case 1614: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_GREAT_CAT; break;
                case 1615: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_GREAT_CAT; break;
                case 1616: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_GREAT_CAT; break;
                case 1617: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_GREAT_CAT; break;
                case 1618: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_OGRES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_OGRES_YHETEE; break;
                case 1619: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1620: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_BEASTMEN; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_BEASTMEN_UNGOR; break;
                case 1621: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_BEASTMEN; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_BEASTMEN_GOR; break;
                case 1622: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_BEASTMEN; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_BEASTMEN_BESTIGOR; break;
                case 1623: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_ZEALOT; break;
                case 1624: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_MARAUDER; break;
                case 1625: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_NURGLE; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_NURGLE_SLIME_HOUND; break;
                case 1626: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_TZEENTCH; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_TZEENTCH_LORD_OF_CHANGE; break;
                case 1627: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_UNMARKED; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_UNMARKED_DAEMONS_WALKER; break;
                case 1628: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1629: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1630: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1631: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1632: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1633: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1634: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1635: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1636: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1637: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1638: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_EMPIRE; break;
                case 1639: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1640: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_MAGUS; break;
                case 1641: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1642: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_CONSTRUCTS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_CONSTRUCTS_BONE_GIANT; break;
                case 1643: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1644: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1645: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1646: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1647: CreatureType = (byte)GameData.CreatureTypes.MAMONSTERS_GICAL_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_MAGICAL_BEASTS_IMP; break;
                case 1648: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_HUMAN; break;
                case 1649: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1650: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1651: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1652: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1653: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_UNMARKED; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_UNMARKED_DAEMONS_CHAOS_HOUND; break;
                case 1654: CreatureType = (byte)GameData.CreatureTypes.MONSTERS_CHAOS_BREEDS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_CHAOS_BREEDS_CENTIGOR; break;
                case 1655: CreatureType = (byte)GameData.CreatureTypes.MONSTERS_CHAOS_BREEDS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_CHAOS_BREEDS_TUSKGOR; break;
                case 1656: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_OGRES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_OGRES_GORGER; break;
                case 1657: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_BEASTMEN; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_BEASTMEN_DOOMBULL; break;
                case 1658: CreatureType = (byte)GameData.CreatureTypes.MONSTERS_CHAOS_BREEDS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_CHAOS_BREEDS_FLAYERKIN; break;
                case 1659: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_INSECTS_ARACHNIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_INSECTS_ARACHNIDS_GIANT_SCORPION; break;
                case 1660: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_INSECTS_ARACHNIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_INSECTS_ARACHNIDS_GIANT_SCORPION; break;
                case 1661: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_INSECTS_ARACHNIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_INSECTS_ARACHNIDS_GIANT_SCORPION; break;
                case 1662: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_INSECTS_ARACHNIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_INSECTS_ARACHNIDS_GIANT_SPIDER; break;
                case 1663: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_INSECTS_ARACHNIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_INSECTS_ARACHNIDS_GIANT_SPIDER; break;
                case 1664: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_INSECTS_ARACHNIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_INSECTS_ARACHNIDS_GIANT_SPIDER; break;
                case 1665: CreatureType = (byte)GameData.CreatureTypes.PLANTS_FOREST_SPIRITS; CreatureSubType = (byte)GameData.CreatureSubTypes.PLANTS_FOREST_SPIRITS_TREEMAN; break;
                case 1666: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1667: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1668: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1669: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1670: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_EMPIRE; break;
                case 1671: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_EMPIRE; break;
                case 1672: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1673: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1674: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1675: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1676: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1677: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1678: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1679: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1680: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1681: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_SPIDER; break;
                case 1682: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_OGRES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_OGRES_OGRE_BULL; break;
                case 1683: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1684: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 1685: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 1686: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1687: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1688: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1689: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 1690: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_MAGUS; break;
                case 1691: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_MAGUS; break;
                case 1692: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_WITCH_ELVES; break;
                case 1693: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 1694: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 1695: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1696: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1697: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1698: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 1699: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_DWARF; break;
                case 1700: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_DWARF; break;
                case 1701: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_ORC; break;
                case 1702: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_GOBLIN; break;
                case 1703: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_EMPIRE; break;
                case 1704: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_EMPIRE; break;
                case 1705: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1706: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1707: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS; break;
                case 1708: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1709: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF; break;
                case 1710: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1711: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF; break;
                case 1712: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_DWARFS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_DWARF; break;
                case 1713: CreatureType = (byte)GameData.CreatureTypes.MONSTERS_TROLLS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_TROLLS_STONE_TROLL; break;
                case 1714: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_CONSTRUCTS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_CONSTRUCTS_CONSTRUCT; break;
                case 1715: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_GREATER_UNDEAD; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_GREATER_UNDEAD_PRESERVED_DEAD; break;
                case 1716: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_GREATER_UNDEAD; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_GREATER_UNDEAD_PRESERVED_DEAD; break;
                case 1717: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_GREATER_UNDEAD; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_GREATER_UNDEAD_LICHE; break;
                case 1718: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SKELETONS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SKELETONS_SKELETON; break;
                case 1719: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_CONSTRUCTS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_CONSTRUCTS_BONE_GIANT; break;
                case 1720: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SKELETONS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SKELETONS_CARRION; break;
                case 1721: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1722: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_HOUND; break;
                case 1723: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_HOUND; break;
                case 1724: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_CONSTRUCTS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_CONSTRUCTS_SCARAB_BONE_CONSTRUCT; break;
                case 1725: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SKELETONS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SKELETONS_SKELETON; break;
                case 1726: CreatureType = (byte)GameData.CreatureTypes.MAMONSTERS_GICAL_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_MAGICAL_BEASTS_MANTICORE; break;
                case 1727: CreatureType = (byte)GameData.CreatureTypes.MAMONSTERS_GICAL_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_MAGICAL_BEASTS_GRIFFON; break;
                case 1728: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_CONSTRUCTS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_CONSTRUCTS_USHABTI; break;
                case 1729: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_INSECTS_ARACHNIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_INSECTS_ARACHNIDS_GIANT_SCORPION; break;
                case 1730: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_INSECTS_ARACHNIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_INSECTS_ARACHNIDS_GIANT_SCORPION; break;
                case 1731: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1732: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1733: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_CONSTRUCTS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_CONSTRUCTS_ASP_BONE_CONSTRUCT; break;
                case 1734: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1735: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1736: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1737: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_CONSTRUCTS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_CONSTRUCTS_LIVING_ARMOR; break;
                case 1738: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_CONSTRUCTS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_CONSTRUCTS_SCARAB_BONE_CONSTRUCT; break;
                case 1739: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1740: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1741: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1742: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1743: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1744: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1745: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1746: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1747: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1748: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1749: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1750: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1751: CreatureType = (byte)GameData.CreatureTypes.MAMONSTERS_GICAL_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_MAGICAL_BEASTS_MANTICORE; break;
                case 1752: CreatureType = (byte)GameData.CreatureTypes.MAMONSTERS_GICAL_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_MAGICAL_BEASTS_MANTICORE; break;
                case 1753: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1754: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1755: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1756: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1757: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1758: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1759: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_INSECTS_ARACHNIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_INSECTS_ARACHNIDS_GIANT_SCORPION; break;
                case 1760: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_INSECTS_ARACHNIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_INSECTS_ARACHNIDS_TOMB_SWARM; break;
                case 1761: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_INSECTS_ARACHNIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_INSECTS_ARACHNIDS_GIANT_SCORPION; break;
                case 1762: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_INSECTS_ARACHNIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_INSECTS_ARACHNIDS_GIANT_SCORPION; break;
                case 1763: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_CONSTRUCTS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_CONSTRUCTS_ASP_BONE_CONSTRUCT; break;
                case 1764: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_INSECTS_ARACHNIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_INSECTS_ARACHNIDS_GIANT_SCORPION; break;
                case 1765: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1766: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1767: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_MAGUS; break;
                case 1768: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_MAGUS; break;
                case 1769: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_CONSTRUCTS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_CONSTRUCTS_USHABTI; break;
                case 1770: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_CONSTRUCTS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_CONSTRUCTS_USHABTI; break;
                case 1771: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_CONSTRUCTS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_CONSTRUCTS_SCARAB_BONE_CONSTRUCT; break;
                case 1772: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_CONSTRUCTS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_CONSTRUCTS_BONE_GIANT; break;
                case 1773: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1774: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SKELETONS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SKELETONS_CARRION; break;
                case 1775: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_INSECTS_ARACHNIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_INSECTS_ARACHNIDS_GIANT_SCARAB; break;
                case 1776: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_DEER; break;
                case 1777: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_DEER; break;
                case 1778: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_HOUND; break;
                case 1779: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_HOUND; break;
                case 1780: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 1781: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 1782: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_DEER; break;
                case 1783: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1784: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_WHITE_LION; break;
                case 1785: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_WHITE_LION; break;
                case 1786: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_WHITE_LION; break;
                case 1787: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_ELVES; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_ELVES_WHITE_LION; break;
                case 1788: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1789: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1790: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1791: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1792: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SKELETONS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SKELETONS_SKELETON; break;
                case 1793: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1794: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SKELETONS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SKELETONS_CARRION; break;
                case 1795: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_INSECTS_ARACHNIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_INSECTS_ARACHNIDS_GIANT_SCORPION; break;
                case 1796: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_INSECTS_ARACHNIDS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_INSECTS_ARACHNIDS_GIANT_SCARAB; break;
                case 1797: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1798: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1799: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1800: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_GREATER_UNDEAD; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_GREATER_UNDEAD_LICHE; break;
                case 1801: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SKELETONS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SKELETONS_SKELETON; break;
                case 1802: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_GREATER_UNDEAD; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_GREATER_UNDEAD_VAMPIRE; break;
                case 1803: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_GREATER_UNDEAD; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_GREATER_UNDEAD_VAMPIRE; break;
                case 1804: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_GREATER_UNDEAD; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_GREATER_UNDEAD_VAMPIRE; break;
                case 1805: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_GREATER_UNDEAD; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_GREATER_UNDEAD_VAMPIRE; break;
                case 1806: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_ZOMBIES; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_ZOMBIES_ZOMBIE; break;
                case 1807: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_ZOMBIES; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_ZOMBIES_ZOMBIE; break;
                case 1808: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_GHOUL; break;
                case 1809: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SKELETONS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SKELETONS_SKELETON; break;
                case 1810: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SKELETONS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SKELETONS_SKELETON; break;
                case 1811: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 1812: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 1813: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_WOLF; break;
                case 1814: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SPIRITS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SPIRITS_WRAITH; break;
                case 1815: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SPIRITS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SPIRITS_WRAITH; break;
                case 1816: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SPIRITS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SPIRITS_BANSHEE; break;
                case 1817: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SPIRITS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SPIRITS_SPIRIT_HOST; break;
                case 1818: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_GIANT_BAT; break;
                case 1819: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_GIANT_BAT; break;
                case 1820: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_ZOMBIES; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_ZOMBIES_ZOMBIE; break;
                case 1821: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_ZOMBIES; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_ZOMBIES_ZOMBIE; break;
                case 1822: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_ZOMBIES; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_ZOMBIES_ZOMBIE; break;
                case 1823: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_ZOMBIES; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_ZOMBIES_ZOMBIE; break;
                case 1824: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_GREATER_UNDEAD; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_GREATER_UNDEAD_VAMPIRE; break;
                case 1825: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_ZOMBIES; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_ZOMBIES_ZOMBIE; break;
                case 1826: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_ZOMBIES; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_ZOMBIES_ZOMBIE; break;
                case 1827: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_CONSTRUCTS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_CONSTRUCTS_WINGED_NIGHTMARE; break;
                case 1828: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1829: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1830: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SKELETONS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SKELETONS_SKELETON; break;
                case 1831: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SKELETONS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SKELETONS_SKELETON; break;
                case 1832: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_HUMAN; break;
                case 1833: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_HUMAN; break;
                case 1834: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_HUMAN; break;
                case 1835: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1836: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1837: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1838: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1839: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BEAR; break;
                case 1840: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BEAR; break;
                case 1841: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BEAR; break;
                case 1842: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BEAR; break;
                case 1843: CreatureType = (byte)GameData.CreatureTypes.MAMONSTERS_GICAL_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_MAGICAL_BEASTS_IMP; break;
                case 1844: CreatureType = (byte)GameData.CreatureTypes.MAMONSTERS_GICAL_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_MAGICAL_BEASTS_IMP; break;
                case 1845: CreatureType = (byte)GameData.CreatureTypes.MAMONSTERS_GICAL_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_MAGICAL_BEASTS_IMP; break;
                case 1846: CreatureType = (byte)GameData.CreatureTypes.MAMONSTERS_GICAL_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_MAGICAL_BEASTS_IMP; break;
                case 1847: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_HOUND; break;
                case 1848: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_HOUND; break;
                case 1849: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_HOUND; break;
                case 1850: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_HOUND; break;
                case 1851: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1852: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1853: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1854: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1855: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1856: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1857: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 1858: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 1859: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 1860: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_BOAR; break;
                case 1861: CreatureType = (byte)GameData.CreatureTypes.MAMONSTERS_GICAL_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_MAGICAL_BEASTS_GRIFFON; break;
                case 1862: CreatureType = (byte)GameData.CreatureTypes.MAMONSTERS_GICAL_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_MAGICAL_BEASTS_GRIFFON; break;
                case 1863: CreatureType = (byte)GameData.CreatureTypes.MAMONSTERS_GICAL_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_MAGICAL_BEASTS_GRIFFON; break;
                case 1864: CreatureType = (byte)GameData.CreatureTypes.MAMONSTERS_GICAL_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.MONSTERS_MAGICAL_BEASTS_GRIFFON; break;
                case 1865: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SNOTLING; break;
                case 1866: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SNOTLING; break;
                case 1867: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SNOTLING; break;
                case 1868: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SNOTLING; break;
                case 1869: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_BIRD; break;
                case 1870: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_BIRD; break;
                case 1871: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_BIRD; break;
                case 1872: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_BIRD; break;
                case 1873: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_BIRD; break;
                case 1874: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_BIRD; break;
                case 1875: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_BIRD; break;
                case 1876: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_BIRD; break;
                case 1877: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SQUIG; break;
                case 1878: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SQUIG; break;
                case 1879: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SQUIG; break;
                case 1880: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SQUIG; break;
                case 1881: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SKELETONS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SKELETONS_SKELETON; break;
                case 1882: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SKELETONS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SKELETONS_SKELETON; break;
                case 1883: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SKELETONS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SKELETONS_SKELETON; break;
                case 1884: CreatureType = (byte)GameData.CreatureTypes.UNDEAD_SKELETONS; CreatureSubType = (byte)GameData.CreatureSubTypes.UNDEAD_SKELETONS_SKELETON; break;
                case 1885: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_HUMAN; break;
                case 1886: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_HUMAN; break;
                case 1887: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_HUMAN; break;
                case 1888: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_HUMAN; break;
                case 1889: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_HUMAN; break;
                case 1890: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_HUMAN; break;
                case 1891: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_HUMAN; break;
                case 1892: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_HUMANS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_HUMAN; break;
                case 1893: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1894: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1895: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1896: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_LIVESTOCK; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_LIVESTOCK_HORSE; break;
                case 1897: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_BIRD; break;
                case 1898: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_BIRD; break;
                case 1899: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_BIRD; break;
                case 1900: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_CRITTER; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_CRITTER_BIRD; break;
                case 1901: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SNOTLING; break;
                case 1902: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SNOTLING; break;
                case 1903: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SNOTLING; break;
                case 1904: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_GREENSKINS; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_SNOTLING; break;
                case 1905: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_HOUND; break;
                case 1906: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_HOUND; break;
                case 1907: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_HOUND; break;
                case 1908: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_HOUND; break;
                case 1909: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_HOUND; break;
                case 1910: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_HOUND; break;
                case 1911: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_HOUND; break;
                case 1912: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_BEASTS; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_BEASTS_HOUND; break;
                case 1913: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_TZEENTCH; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_TZEENTCH_HORROR; break;
                case 1914: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_TZEENTCH; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_TZEENTCH_HORROR; break;
                case 1915: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_TZEENTCH; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_TZEENTCH_HORROR; break;
                case 1916: CreatureType = (byte)GameData.CreatureTypes.DAEMONS_TZEENTCH; CreatureSubType = (byte)GameData.CreatureSubTypes.DAEMONS_TZEENTCH_HORROR; break;
                case 1917: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_SKAVEN; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_SKAVEN_SKAVEN; break;
                case 1918: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1919: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1920: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1921: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1922: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 1923: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 1924: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 1925: CreatureType = (byte)GameData.CreatureTypes.ANIMALS_REPTILES; CreatureSubType = (byte)GameData.CreatureSubTypes.ANIMALS_REPTILES_COLD_ONE; break;
                case 1926: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1927: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1928: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1929: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1930: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1931: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1932: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1933: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1934: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1935: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1936: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1937: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1938: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1939: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1940: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1941: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1942: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1943: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1944: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1945: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
                case 1946: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_SKAVEN; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_SKAVEN_SKAVEN; break;
                case 1947: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_SKAVEN; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_SKAVEN_SKAVEN; break;
                case 1948: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_SKAVEN; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_SKAVEN_SKAVEN; break;
                case 1949: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_SKAVEN; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_SKAVEN_SKAVEN; break;
                case 1950: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_SKAVEN; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_SKAVEN_SKAVEN; break;
                case 1951: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_SKAVEN; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_SKAVEN_SKAVEN; break;
                case 1952: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_SKAVEN; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_SKAVEN_SKAVEN; break;
                case 1953: CreatureType = (byte)GameData.CreatureTypes.HUMANOIDS_SKAVEN; CreatureSubType = (byte)GameData.CreatureSubTypes.HUMANOIDS_SKAVEN_SKAVEN; break;
                default: CreatureType = (byte)GameData.CreatureTypes.UNCLASSIFIED; CreatureSubType = (byte)GameData.CreatureSubTypes.UNCLASSIFIED; break;
            }
        }

        [PrimaryKey]
        public uint Entry { get; set; }

        private string _name;

        [DataElement(Varchar = 255, AllowDbNull = false)]
        public string Name
        {
            get { return _name; }

            set
            {
                GenderedName = value;

                int caratPos = value.IndexOf("^", StringComparison.Ordinal);

                if (caratPos == -1)
                    _name = value;
                else
                    _name = value.Substring(0, caratPos);
            }
        }

        [DataElement(AllowDbNull = false)]
        public ushort Model1 { get; set; }

        [DataElement(AllowDbNull = false)]
        public ushort Model2 { get; set; }

        [DataElement(AllowDbNull = false)]
        public ushort MinScale { get; set; }

        [DataElement(AllowDbNull = false)]
        public ushort MaxScale { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte MinLevel { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte MaxLevel { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte Faction { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte CreatureType { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte CreatureSubType { get; set; }

        [DataElement(AllowDbNull = false)]
        public ushort Ranged { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte Icone { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte Emote { get; set; }

        private ushort _title;

        [DataElement(AllowDbNull = false)]
        public ushort Title
        {
            get { return _title; }
            set
            {
                _title = value;
                TitleId = (CreatureTitle) value;
            }
        }

        [DataElement(AllowDbNull = false)]
        public ushort Unk
        {
            get { return _Unks[0]; }
            set { if (_Unks == null)_Unks = new ushort[7]; _Unks[0] = value; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort Unk1
        {
            get { return _Unks[1]; }
            set { if (_Unks == null)_Unks = new ushort[7]; _Unks[1] = value; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort Unk2
        {
            get { return _Unks[2]; }
            set { if (_Unks == null)_Unks = new ushort[7]; _Unks[2] = value; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort Unk3
        {
            get { return _Unks[3]; }
            set { if (_Unks == null)_Unks = new ushort[7]; _Unks[3] = value; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort Unk4
        {
            get { return _Unks[4]; }
            set { if (_Unks == null)_Unks = new ushort[7]; _Unks[4] = value; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort Unk5
        {
            get { return _Unks[5]; }
            set { if (_Unks == null)_Unks = new ushort[7]; _Unks[5] = value; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort Unk6
        {
            get { return _Unks[6]; }
            set { if (_Unks == null)_Unks = new ushort[7]; _Unks[6] = value; }
        }

        [DataElement(Varchar = 255, AllowDbNull = false)]
        public string Flag { get; set; }

        [DataElement(Varchar = 255, AllowDbNull = false)]
        public string ScriptName { get; set; }

        [DataElement(AllowDbNull = false)]
        public bool LairBoss { get; set; }

        [DataElement(AllowDbNull = false)]
        public ushort VendorID { get; set; }

        public List<Quest> StartingQuests;
        public List<Quest> FinishingQuests;

        [DataElement(AllowDbNull = false)]
        public string TokUnlock { get; set; }

        public string GenderedName;

        [DataElement]
        public byte[] States { get; set; }

        [DataElement]
        public byte[] FigLeafData { get; set; }

        [DataElement]
        public int BaseRadiusUnits { get; set; }

        [DataElement]
        public byte Career { get; set; }

        private float _powerModifier;

        [DataElement]
        public float PowerModifier
        {
            get { return _powerModifier; }
            set { if (_powerModifier < 0.01f) _powerModifier = 0.01f; _powerModifier = value; }
        }

        private float _woundsModifier;

        [DataElement]
        public float WoundsModifier
        {
            get { return _woundsModifier; }
            set { if (_woundsModifier < 0.01f) _woundsModifier = 0.01f; _woundsModifier = value; }
        }

        [DataElement(AllowDbNull = false)]
        public byte Invulnerable { get; set; }

        [DataElement(AllowDbNull = false)]
        public ushort WeaponDPS { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte ImmuneToCC { get; set; }

        public InteractType InteractType = InteractType.INTERACTTYPE_IDLE_CHAT;

        public CreatureTitle TitleId;

        public byte InteractTrainerType;
    }
}
