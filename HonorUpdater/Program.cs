using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;

namespace HonorUpdater
{


    class Program
    {
        private static Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private static string ConnectionString = System.Configuration.ConfigurationManager.AppSettings["WAR.ConnectionString"];


        static void Main(string[] args)
        {
            Logger.Info($"Begin processing Honor Updates....");

            // Reduce Honor Points for each character, recalculate their Honor Level
            var connection = new MySqlConnection(ConnectionString);
            try
            {
                connection.Open();
                var characterList = connection.Query<Character>($"select CharacterId, Name, AccountId, HonorPoints, HonorRank from war_characters.characters where HonorPoints > 0");

                foreach (var character in characterList)
                {
                    var currentHonorPoints = character.HonorPoints;
                    // Reduce honor points by 10%, unless they are < 10 - in which case make it 0
                    var newHonorPoints = 0;
                    if (currentHonorPoints < 10)
                        newHonorPoints = 0;
                    else 
                        newHonorPoints = (int) (currentHonorPoints * 0.90f);

                    // Recalculate Honor Rank
                    var honorLevel = new Common.HonorCalculation().GetHonorLevel((int) newHonorPoints);

                    Logger.Debug($"Character : {character.Name}  Honor {currentHonorPoints} => {newHonorPoints}");

                    connection.ExecuteScalar(
                        $"update war_characters.characters set HonorPoints = {newHonorPoints}, HonorRank = {honorLevel} where CharacterId = {character.CharacterId}");

                }

                Logger.Info($"Finish processing Honor Updates....");

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
    }
}
