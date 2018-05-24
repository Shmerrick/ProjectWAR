using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.LayoutRenderers;

namespace CharacterUtility
{
    class Program
    {
        private static Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private static string ConnectionString = System.Configuration.ConfigurationManager.AppSettings["WAR.ConnectionString"];
        private static string AccountUserCode = System.Configuration.ConfigurationManager.AppSettings["AccountUserCode"];
        private static IEnumerable<ItemBonus> ItemBonusList;
        private static IEnumerable<NdClass> ClassList;

        static int Main(string[] args)
        {
            Logger.Info("Character Utility");

            return CommandLine.Parser.Default.ParseArguments<CommandLineOptions.ImportOptions, CommandLineOptions.ExportOptions, CommandLineOptions.CreateOptions, CommandLineOptions.ItemSetOptions>(args)
            .MapResult(
                (CommandLineOptions.ImportOptions opts) => ImportCharacters(opts),
                (CommandLineOptions.ExportOptions opts) => ExportCharacters(opts),
                (CommandLineOptions.CreateOptions opts) => CreateCharacters(opts),
                (CommandLineOptions.ItemSetOptions opts) => ManageItemSetOptions(opts),
                errs => 1);
        }

        private static int ManageItemSetOptions(CommandLineOptions.ItemSetOptions opts)
        {
            if ((opts.RebuildItemsetFlag == false) && (opts.ViewItemSetFlag == false))
                return 1;

            if (opts.RebuildItemsetFlag == true)
            {
                ItemBonusList = GetItemBonusList();
                ClassList = GetNDClassList();

                return RebuildItemSet();
            }
            if (opts.ViewItemSetFlag == true)
            {
                return ViewItemSet(GetItemSetList(), opts.ItemsetOutputFile);
            }

            return 1;

        }

        private static IEnumerable<ItemBonus> GetItemBonusList()
        {
            var connection = new MySqlConnection(ConnectionString);
            try
            {
                connection.Open();
                var itemsetList = connection.Query<ItemBonus>($"select * from war_world.item_bonus");
                return itemsetList;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Logger.Error($"Exception : {e.Message}");
                return null;
            }
            finally
            {
                connection.Close();
            }
        }

        private static IEnumerable<NdClass> GetNDClassList()
        {
            var connection = new MySqlConnection(ConnectionString);
            try
            {
                connection.Open();
                var classList = connection.Query<NdClass>($"select * from war_world.classes");
                return classList;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Logger.Error($"Exception : {e.Message}");
                return null;
            }
            finally
            {
                connection.Close();
            }
        }

        private static IEnumerable<ItemSet> GetItemSetList()
        {
            var connection = new MySqlConnection(ConnectionString);
            try
            {
                connection.Open();
                var itemsetList = connection.Query<ItemSet>($"select * from war_world.item_sets");
                return itemsetList;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Logger.Error($"Exception : {e.Message}");
                return null;
            }
            finally
            {
                connection.Close();
            }
        }

        private static int ViewItemSet(IEnumerable<ItemSet> itemSets, string outputFile)
        {
            var jsonOutput = JsonConvert.SerializeObject(itemSets);
            Logger.Debug($"Writing ItemSet details to {outputFile}");
            File.WriteAllText(outputFile, jsonOutput);

            return 1;
        }

        private static int RebuildItemSet()
        {


            ClearExistingItemSetValues();

            UpdateItemSetLists();

            UpdateItemSetFullDescriptions();

            return 0;
        }

        /// <summary>
        /// Create Full Description json and save to the DB.
        /// </summary>
        private static void UpdateItemSetFullDescriptions()
        {
            var itemSetObjects = GetItemSetList();
            var totalCount = itemSetObjects.Count();
            var i = 0;

            // This Set.
            foreach (var itemSetObject in itemSetObjects)
            {
                i++;
                var returnJson = new JObject(
                    new JProperty("entry", itemSetObject.Entry),
                    new JProperty("set-name", itemSetObject.Name));

                // List of Bonus Objects + Values for this set.
                var setBonusList = GetSetBonus(itemSetObject);

                returnJson.Add(new JProperty("set-bonus",
                    JArray.FromObject(setBonusList)));

                // Items in the Set.
                var itemSetList = itemSetObject.ItemsString.Split('|');


                var itemArray = new JArray();
                var classId = 0;

                foreach (var item in itemSetList)
                {
                    if (String.IsNullOrEmpty(item))
                        continue;
                    var itemId = item.Split(':')[0];
                    var itemName = item.Split(':')[1];

                    var itemDetails = GetItemDetails(itemId);
                    if (itemDetails == null)
                    {
                        Logger.Warn($"Could not locate item details for Item {itemId} {itemName}");
                        continue;
                    }
                    classId = itemDetails.Career;

                    if (itemDetails != null)
                    {
                        var item1 = GetItemDetails(itemDetails.Entry.ToString());
                        var jitem = JObject.FromObject(item1);

                        itemArray.Add(jitem);
                    }
                }
                returnJson["items"] = itemArray;

                var classObject = ClassList.SingleOrDefault(x => x.ClassId == classId);
                if (classObject != null)
                {
                    returnJson.Add(new JProperty("class-restriction", classObject.ClassName));
                }
                else
                {
                    Logger.Warn($"Could not find class {classId} for ItemSet {itemSetObject.Entry} {itemSetObject.Name}");
                }

                UpdateItemSetFullDescription(itemSetObject.Entry, returnJson.ToString());
                Logger.Debug($"Processed : {i / totalCount} records");
            }

        }

        /// <summary>
        /// From an Item Set object, return a "Set Bonus Json object"
        /// </summary>
        /// <param name="itemSetObject"></param>
        /// <returns></returns>
        private static List<SetBonus> GetSetBonus(ItemSet itemSetObject)
        {
            // List of strings in the form A:B,C,D (or A:B)
            var listBonus = itemSetObject.BonusString.Split('|');
            var setBonusList = new List<SetBonus>();

            var count = 1;

            foreach (var bonus in listBonus)
            {
                var setBonus = new SetBonus().CalculateSetBonus(bonus, ItemBonusList);


                if (setBonus != null)
                {
                    setBonus.NumberPieces = ++count;
                    setBonusList.Add(setBonus);
                }

            }
            return setBonusList;
        }

        /// <summary>
        /// Create a list of items in ItemSetList (just a Json array).
        ///         1700:Dominator Steadkeeps| 1712:Dominator Irongaunts| 1724:Dominator Kladgird| 1736:Dominator Ironmantle| 1748:Dominator Greathelm| 1760:Dominator Klad|
        ///         =>>;  "name": "Invader ItemSet for Chosen",
        ///         "items": [
        ///         1700,
        ///         1712,
        ///         1724,
        ///         1736,
        ///         1748,
        ///         1760
        ///         ] 
        /// </summary>
        private static void UpdateItemSetLists()
        {

            var itemSetObjects = GetItemSetList();
            var totalCount = itemSetObjects.Count();
            var i = 0;

            foreach (var itemSetObject in itemSetObjects)
            {
                var itemSetList = itemSetObject.ItemsString.Split('|');
                i++;

                var returnJson = new JObject(
                    new JProperty("name", itemSetObject.Name),
                    new JProperty("items",
                        new JArray()));

                var itemArray = new JArray();
                var classId = 0;

                foreach (var item in itemSetList)
                {
                    if (String.IsNullOrEmpty(item))
                        continue;
                    var itemId = item.Split(':')[0];
                    var itemName = item.Split(':')[1];

                    var itemDetails = GetItemDetails(itemId);
                    if (itemDetails == null)
                    {
                        Logger.Warn($"Could not locate item details for Item {itemId} {itemName}");
                        continue;
                    }
                    classId = itemDetails.Career;

                    if (itemDetails != null)
                    {
                        itemArray.Add(itemDetails.Entry);
                    }
                }
                returnJson["items"] = itemArray;

                Logger.Debug($"Updating Item Set List with {JsonConvert.SerializeObject(returnJson, Formatting.Indented)}");
                Logger.Debug($"Updating Item Set Class with {classId}");
                UpdateItemSetList(itemSetObject.Entry, JsonConvert.SerializeObject(returnJson, Formatting.Indented), classId);
                
                Logger.Debug($"Processed : {i/ totalCount} records");
            }


        }

        

        private static void UpdateItemSetList(int setEntryId, string jsonSetList, int classId)
        {
            var connection = new MySqlConnection(ConnectionString);
            try
            {
                connection.Open();
                var result = connection.Execute($"update war_world.item_sets set ItemSetList =  @json, ClassId = @newClassId where Entry = @EntryId", new { json = jsonSetList, EntryId = setEntryId, newClassId = classId });
                Logger.Debug($"Rows Effected {result}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Logger.Error($"Exception : {e.Message}");
            }
            finally
            {
                connection.Close();
            }
        }


        private static void UpdateItemSetFullDescription(int setEntryId, string jsonFullDescription)
        {
            var connection = new MySqlConnection(ConnectionString);
            try
            {
                connection.Open();
                var result = connection.Execute($"update war_world.item_sets set ItemSetFullDescription =  @json where Entry = @EntryId", new { json = jsonFullDescription, EntryId = setEntryId });
                Logger.Debug($"Rows Effected {result}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Logger.Error($"Exception : {e.Message}");
            }
            finally
            {
                connection.Close();
            }
        }

        private static ItemInfo GetItemDetails(string setItemId)
        {

            var connection = new MySqlConnection(ConnectionString);
            try
            {
                connection.Open();
                var itemAvailable = connection.QueryFirstOrDefault<ItemInfo>($"select * from war_world.Item_Infos where Entry= {setItemId}");

                var individualStatItems = itemAvailable.Stats.Split(';');

                foreach (var item in individualStatItems)
                {
                    if ((item == "0:0") || (item == ""))
                        continue;
                    itemAvailable.StatsList.Add(new ItemInfoStats(item, ItemBonusList));
                }

                if (itemAvailable != null)
                {
                    Logger.Debug($"Item : {setItemId} {itemAvailable.Name} {itemAvailable.Description} Type : {itemAvailable.Type}");
                    return itemAvailable;
                }
                else
                {
                    Logger.Debug($"Item : {setItemId} {itemAvailable.Name} {itemAvailable.Description} Type : {itemAvailable.Type} does not exist in Item_infos");
                    return null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Logger.Error($"Exception : {e.Message}");
                return null;
            }
            finally
            {
                connection.Close();
            }


        }

        private static void ClearExistingItemSetValues()
        {
            // Clear the values of ItemSetList and ItemSetFullDescription
            Logger.Debug($"Clear the values of ItemSetList and ItemSetFullDescription");
            var connection = new MySqlConnection(ConnectionString);
            try
            {
                connection.Open();
                var result = connection.Execute($"update war_world.item_sets set ItemSetList = null, ItemSetFullDescription=null, ClassId=null");
                Logger.Debug($"Rows Effected {result}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Logger.Error($"Exception : {e.Message}");
            }
            finally
            {
                connection.Close();
            }
        }

        private static void HandleParseError(IEnumerable<Error> opts)
        {
            throw new NotImplementedException();
        }

        private static int CreateCharacters(CommandLineOptions.CreateOptions opts)
        {

            var templateFile = opts.TemplateFileName;
            var character = CreateCharacterFromTemplate(templateFile);
            SaveCharacter(character);

            return 0;
        }

        private static void SaveCharacter(Character character)
        {
            var connection = new MySqlConnection(ConnectionString);
            try
            {
                connection.Open();

                var characterObject = connection.QueryFirstOrDefault($"select max(CharacterId) as MaxId from war_characters.characters");
                var slotObject = connection.QueryFirstOrDefault($"select max(SlotId) as MaxSlotId from war_characters.characters where AccountId = {character.AccountId}");

                Logger.Info($"Creating Character : {character.Name}");

                // Check toon doesnt exist already
                if (DoesCharacterExist(character.Name))
                {
                    Logger.Warn($"Character with the name : {character.Name} already exists!");
                    return;
                }



                var newCharacterSql =
                    $"insert into war_characters.characters (CharacterId, Name, RealmId, AccountId, SlotId, ModelId, Career, CareerLine, Realm,  HeldLeft, Race, Traits, Sex, Surname, Anonymous, Hidden, OldName, PetName, PetModel) " +
                    $"values ({characterObject.MaxId + 1}, '{character.Name}', {character.RealmId}, {character.AccountId}, {slotObject.MaxSlotId + 1}, {character.ModelId}, {character.Career}, {character.CareerLine}, {character.Realm}, 0, {character.Race}, '', {character.Sex}, '{character.Surname}', 0, 0, '', '', 0)";

                Logger.Debug(newCharacterSql);

                var newCharacterValueSql =
                    $"insert into war_characters.characters_value (CharacterId, Level, Xp, XpMode, RestXp, Renown, RenownRank, Money, Speed, RegionId, ZoneId, WorldX, WorldY, WorldZ, WorldO, RallyPoint, BagBuy, Skills, Online, PlayedTime, LastSeen, BankBuy, GearShow, TitleId, " +
                    $"RenownSkills, MasterySkills, GatheringSkill, GatheringSkillLevel, CraftingSkill, CraftingSkillLevel, ExperimentalMode, RVRKills, RVRDeaths, CraftingBags, Lockouts, DisconcetTime) " +
                    $"values ({characterObject.MaxId + 1}, '{character.Level}', 0, 0, 0, 0, {character.RenownRank}, 500000, 100, " +
                    $"{character.BaseCharacterInfo.Region}, {character.BaseCharacterInfo.ZoneId}, {character.BaseCharacterInfo.WorldX}, " +
                    $"{character.BaseCharacterInfo.WorldY}, {character.BaseCharacterInfo.WorldZ}, {character.BaseCharacterInfo.WorldO}, " +
                    $"{character.BaseCharacterInfo.RallyPt}, 0, {character.BaseCharacterInfo.Skills}, 0, 0, 0, 0, 0, 0, '', '', 0, 0, 0, 0, 0, 0, 0, 0, '', 0 )";

                Logger.Debug(newCharacterValueSql);

                connection.Execute(newCharacterSql);
                connection.Execute(newCharacterValueSql);

                Logger.Info($"Done creating character : {character.Name}");

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Logger.Error($"Exception : {e.Message}");
                throw;
            }
            finally
            {
                connection.Close();
            }
        }

        private static Character CreateCharacterFromTemplate(string templateFile)
        {
            Logger.Debug($"Reading template from {templateFile}");
            var json = JObject.Parse(File.ReadAllText(templateFile));

            var character = new Character();

            JToken value;
            if (json.TryGetValue("name", out value))
                character.Name = (string)value;
            if (json.TryGetValue("level", out value))
                character.Level = (int)value;
            if (json.TryGetValue("renownrank", out value))
                character.RenownRank = (int)value;
            if (json.TryGetValue("surname", out value))
                character.Surname = (string)value;

            if (json.TryGetValue("race", out value))
                character.Race = (int)value;
            if (json.TryGetValue("sex", out value))
                character.Sex = (int)value;
            if (json.TryGetValue("realm", out value))
                character.Realm = (int)value;
            if (json.TryGetValue("realmId", out value))
                character.RealmId = (int)value;

            if (json.TryGetValue("class", out value))
            {
                var characterInfo = GetCharacterInfo((string)value);
                character.Career = characterInfo.Career;

                character.BaseCharacterInfo = characterInfo;
                character.CareerLine = characterInfo.CareerLine;
                character.ModelId = 0;
            }

            character.AccountId = GetAccountId(AccountUserCode);

            Logger.Debug($"Character created : {character}");

            return character;
        }

        private static int GetAccountId(string accountUserCode)
        {
            var connection = new MySqlConnection(ConnectionString);
            try
            {
                connection.Open();
                var account = connection.QueryFirstOrDefault($"select * from war_accounts.accounts where Username = '{accountUserCode}'");

                return account.AccountId;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Logger.Error($"Exception : {e.Message}");
                throw;
            }
            finally
            {
                connection.Close();

            }
        }

        private static bool DoesCharacterExist(string name)
        {
            var connection = new MySqlConnection(ConnectionString);
            try
            {
                connection.Open();
                var character = connection.QueryFirstOrDefault($"select * from war_characters.characters where Name = '{name}'");

                return (character != null);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Logger.Error($"Exception : {e.Message}");
                throw;
            }
            finally
            {
                connection.Close();

            }
        }

        private static CharacterInfo GetCharacterInfo(string career)
        {
            var connection = new MySqlConnection(ConnectionString);
            try
            {
                var information = new CharacterInfo();
                connection.Open();
                var characterInfo = connection.Query<CharacterInfo>($"select * from war_world.characterinfo where CareerName = '{career}'");

                return characterInfo.FirstOrDefault();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Logger.Error($"Exception : {e.Message}");
                throw;
            }
            finally
            {
                connection.Close();
                Logger.Debug("Done GetCharacterInfo");
            }
        }

        private static int ExportCharacters(CommandLineOptions.ExportOptions opts)
        {
            var exportFileName = System.Configuration.ConfigurationManager.AppSettings["DefaultOutputFile"];

            if (!String.IsNullOrEmpty(opts.ExportFileName))
            {
                exportFileName = opts.ExportFileName;
            }

            Logger.Debug($"Command Line Options : {exportFileName}");

            var connection = new MySqlConnection(ConnectionString);

            try
            {
                Logger.Info($"Connecting to {ConnectionString}. Exporting to {exportFileName}");
                Logger.Info("Start Character Export");

                var exportTables = new List<string> { "characters", "character_bag_pools", "character_client_data", "character_influences", "character_saved_buffs", "characters_items", "characters_mails", "characters_quests", "characters_socials", "characters_toks", "characters_toks_kills", "characters_value" };

                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        using (MySqlBackup mb = new MySqlBackup(cmd))
                        {
                            cmd.Connection = conn;
                            conn.Open();
                            mb.ExportInfo.TablesToBeExportedList = exportTables;
                            mb.ExportToFile(exportFileName);
                            conn.Close();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Logger.Error($"Exception : {e.Message}");
                throw;
            }
            finally
            {
                connection.Close();
                Logger.Info("Done Character Export");
            }

            return 0;
        }

        private static int ImportCharacters(CommandLineOptions.ImportOptions opts)
        {
            var importFileName = System.Configuration.ConfigurationManager.AppSettings["DefaultOutputFile"];

            if (!String.IsNullOrEmpty(opts.ImportFileName))
            {
                importFileName = opts.ImportFileName;
            }

            Logger.Debug($"Command Line Options : {importFileName}");

            var connection = new MySqlConnection(ConnectionString);

            try
            {
                Logger.Info($"Connecting to {ConnectionString}. Exporting to {importFileName}");
                Logger.Info("Start Character Import");


                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        using (MySqlBackup mb = new MySqlBackup(cmd))
                        {
                            cmd.Connection = conn;
                            conn.Open();
                            mb.ImportFromFile(importFileName);
                            conn.Close();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Logger.Error($"Exception : {e.Message}");
                throw;
            }
            finally
            {
                connection.Close();
                Logger.Info("Done Character Export");
            }

            return 0;
        }


        //private static void HandleParseError(IEnumerable<Error> errs)
        //{
        //    Logger.Warn($"Parser errors : {errs.ToString()}");
        //}

        private static void RunOptionsAndReturnExitCode(CommandLineOptions opts)
        {
            Logger.Debug($"Command Line Options : {opts.ToString()}");
        }
    }
}
