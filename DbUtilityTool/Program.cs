using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;
using NLog;

namespace DbUtilityTool
{
    class Program
    {
        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static string ConnectionString;

        static void Main(string[] args)
        {
            logger.Info("Start Utility Extract");
            Console.WriteLine("Utility to do complex queries....");

            ConnectionString = System.Configuration.ConfigurationManager.AppSettings["WAR.ConnectionString"];
            var outputFileName = System.Configuration.ConfigurationManager.AppSettings["OutputFile"];


            logger.Info($"Connecting to {ConnectionString}, writing to {outputFileName}");

            logger.Info("Start Utility Extract");
            var connection = new MySqlConnection(ConnectionString);
            try
            {

                connection.Open();
                //435224:Sovereign Steadkeeps of the Unbreakable|435236:Sovereign Irongaunts of the Unbreakable|435248:Sovereign Warclasp of the Unbreakable|435260:Sovereign Ironmantle of the Unbreakable|435272:Sovereign Kladgird of the Unbreakable|435284:Sovereign Greath...

                var itemSets = connection.Query<ItemSet>("select *, Name as SetName from war_world.item_sets ").ToList();

                logger.Info($"Found {itemSets.Count} item sets");

                foreach (var itemSet in itemSets)
                {
                    Console.WriteLine($"Investigating [{itemSet.Entry}] {itemSet.SetName}...");
                    logger.Debug($"Investigating [{itemSet.Entry}] {itemSet.SetName}...");

                    System.IO.File.AppendAllText(outputFileName, $"Investigating Set : [{itemSet.Entry}] {itemSet.SetName} ... \n");

                    if (itemSet.ItemsString.Length == 0)
                    {
                        logger.Debug($"ItemSet contains no elements {itemSet.SetName}...");
                        System.IO.File.AppendAllText(outputFileName, $"ItemSet contains no elements {itemSet.SetName}... \n");
                        continue;
                    }

                    var itemSetList = itemSet.ItemsString.Split('|');
                    var missingItemList = string.Empty;
                    foreach (var item in itemSetList)
                    {
                        if (String.IsNullOrEmpty(item))
                            continue;
                        var setItemId = item.Split(':')[0];
                        var setItemName = item.Split(':')[1];
                        var itemAvailable = connection.QueryFirstOrDefault<ItemInfos>($"select * from war_world.Item_Infos where Entry= {setItemId}");
                        if (itemAvailable != null)
                            logger.Debug(
                                $"Item : {setItemId} {itemAvailable.Name} {itemAvailable.Description} ModelId : {itemAvailable.ModelId} SlotId : {itemAvailable.SlotId} Type : {itemAvailable.Type}");
                        else
                        {
                            System.IO.File.AppendAllText(outputFileName, $"Item : [{setItemId}] {setItemName} does not exist in Set : [{itemSet.Entry}] {itemSet.SetName} \n");
                            logger.Debug($"Item : [{setItemId}] {setItemName} does not exist in Set : [{itemSet.Entry}] {itemSet.SetName}");
                            // Update the description field in item_sets
                            missingItemList += $"Item : [{setItemId}] {setItemName} not in set : [{itemSet.Entry}] \n";

                        }
                    }
                    if (!String.IsNullOrEmpty(missingItemList))
                        UpdateItemSetDescription(missingItemList, itemSet.Entry);

                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                logger.Info($"Exception : {e.Message}");
                throw;
            }
            finally
            {
                connection.Close();
            }


            logger.Info("Finish Utility Extract");

        }

        private static void UpdateItemSetDescription(string missingItemList, int setEntryId)
        {
            var connection = new MySqlConnection(ConnectionString);
            try
            {
                connection.Open();
                var result = connection.Execute($"update war_world.item_sets set Comments = @description where Entry = @EntryId",
                    new {description = $"{missingItemList} ", EntryId = setEntryId});
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                connection.Close();
            }

        }
    }
}