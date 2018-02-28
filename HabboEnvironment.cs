using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using Emulator.Core;
using Emulator.HabboHotel;
using Emulator.HabboHotel.GameClients;
using Emulator.HabboHotel.Users;
using Emulator.Utilities;
using log4net;
using Emulator.HabboHotel.Cache.Type;
using Emulator.HabboHotel.Users.UserData;
using Emulator.Messages.Net;
using System.Collections.Concurrent;
using Emulator.Communication.Packets.Outgoing.Moderation;
using Emulator.Communication.Encryption.Keys;
using Emulator.Communication.Encryption;
using Emulator.Database.Interfaces;
using Emulator.Database;
using Emulator.Communication.RCON;
using Emulator.Core.FigureData;
using Emulator.Core.Language;
using System.Net;
using Emulator.HabboHotel.Security;

namespace Emulator
{
    public static class HabboEnvironment
    {
        private static readonly ILog log = LogManager.GetLogger("Emulator.HabboEnvironment");
     //   internal static bool IsLive;
        public const string PrettyVersion = "Habbo Emulator";
        public const string PrettyBuild = "3.4.3.0";
        private static bool DoubleCurrency = false;
        private static ConfigurationData _configuration;
        private static Encoding _defaultEncoding;
        private static ConnectionHandling _connectionManager;
        private static Game _game;
        private static Staff _staff;
        private static LanguageManager _languageManager;
        public static string UpdateBuild = "v1.1";
        private static DatabaseManager _manager;
        public static ConfigData ConfigData;
        public static RCONSocket _rcon;
        private static FigureDataManager _figureManager;

        public static CultureInfo CultureInfo;

        public static bool Event = false;
        public static DateTime lastEvent;
        public static DateTime ServerStarted;

        private static readonly List<char> Allowedchars = new List<char>(new[]
            {
                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l',
                'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
                'y', 'z', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '-', '.'
            });


        
        private static ConcurrentDictionary<int, Habbo> _usersCached = new ConcurrentDictionary<int, Habbo>();

        public static string SWFRevision = "PRODUCTION-201709052204-426856518";

        public static void Initialize()
        {         
            ServerStarted = DateTime.Now;
            Console.ForegroundColor = ConsoleColor.White;                 
            Console.Title = "Loading Habbo Emulator";
            _defaultEncoding = Encoding.Default; 
            CultureInfo = CultureInfo.CreateSpecificCulture("en-GB");
            try
            {

                _configuration = new ConfigurationData(Path.Combine(Application.StartupPath, @"config.ini"));

                var connectionString = new MySqlConnectionStringBuilder
                {
                    ConnectionTimeout = 10,
                    Database = GetConfig().data["db.name"],
                    DefaultCommandTimeout = 30,
                    Logging = false,
                    MaximumPoolSize = uint.Parse(GetConfig().data["db.pool.maxsize"]),
                    MinimumPoolSize = uint.Parse(GetConfig().data["db.pool.minsize"]),
                    Password = GetConfig().data["db.password"],
                    Pooling = true,
                    Port = uint.Parse(GetConfig().data["db.port"]),
                    Server = GetConfig().data["db.hostname"],
                    UserID = GetConfig().data["db.username"],
                    AllowZeroDateTime = true,
                    ConvertZeroDateTime = true,
                };

                _manager = new DatabaseManager(connectionString.ToString());

                if (!_manager.IsConnected())
                {
                    log.Error("Failed to connect to the specified MySQL server.");
                    Console.ReadKey(true);
                    Environment.Exit(1);
                    return;
                }

                log.Info("Connected to Database!");

                //Reset our statistics first.
                using (IQueryAdapter dbClient = GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("TRUNCATE `catalog_marketplace_data`");
                    dbClient.RunQuery("UPDATE `rooms` SET `users_now` = '0' WHERE `users_now` > '0';");
                    dbClient.RunQuery("UPDATE `users` SET `online` = '0' WHERE `online` = '1'");
                    dbClient.RunQuery("UPDATE `server_status` SET `users_online` = '0', `loaded_rooms` = '0'");
                }

                //Get the configuration & Game set.
                ConfigData = new ConfigData();
                _game = new Game();
                _languageManager = new LanguageManager();

                _staff = new Staff();
                _staff.Initilize();

               // _figureManager = new FigureDataManager();
                //_figureManager.Init();

                //Have our encryption ready.
                HabboEncryptionV2.Initialize(new RSAKeys());

                //Make sure RCON is connected before we allow clients to connect.
                _rcon = new RCONSocket(GetConfig().data["rcon.tcp.bindip"], int.Parse(GetConfig().data["rcon.tcp.port"]), GetConfig().data["rcon.tcp.allowedaddr"].Split(Convert.ToChar(";")));

                //Accept connections.
                _connectionManager = new ConnectionHandling(int.Parse(GetConfig().data["game.tcp.port"]), int.Parse(GetConfig().data["game.tcp.conlimit"]), int.Parse(GetConfig().data["game.tcp.conperip"]), GetConfig().data["game.tcp.enablenagles"].ToLower() == "true");
                _connectionManager.Init();


               

                _game.StartGameLoop();

                TimeSpan TimeUsed = DateTime.Now - ServerStarted;

                Console.WriteLine();

                log.Info("EMULATOR -> READY! (" + TimeUsed.Seconds + " s, " + TimeUsed.Milliseconds + " ms)");
            }
            catch (KeyNotFoundException e)
            {
                Logging.WriteLine("Please check your configuration file - some values appear to be missing.", ConsoleColor.Red);
                Logging.WriteLine("Press any key to shut down ...");
                Logging.WriteLine(e.ToString());
                Console.ReadKey(true);
                Environment.Exit(1);
                return;
            }
            catch (InvalidOperationException e)
            {
                Logging.WriteLine("Failed to initialize HabboEmulator: " + e.Message, ConsoleColor.Red);
                Logging.WriteLine("Press any key to shut down ...");
                Console.ReadKey(true);
                Environment.Exit(1);
                return;
            }
            catch (Exception e)
            {
                Logging.WriteLine("Fatal error during startup: " + e, ConsoleColor.Red);
                Logging.WriteLine("Press a key to exit");

                Console.ReadKey();
                Environment.Exit(1);
            }
        }

        public static RCONSocket GetRCONSocket()
        {
            return _rcon;
        }
     
        internal static DateTime UnixToDateTime(double unixTimeStamp)
        {
            DateTime result = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            result = result.AddSeconds(unixTimeStamp).ToLocalTime();
            return result;
        }
        internal static int DateTimeToUnix(DateTime target)
        {
            DateTime d = new DateTime(1970, 1, 1, 0, 0, 0, target.Kind);
            return Convert.ToInt32((target - d).TotalSeconds);
        }
        public static bool EnumToBool(string Enum)
        {
            return (Enum == "1");
        }

        public static string BoolToEnum(bool Bool)
        {
            return (Bool == true ? "1" : "0");
        }
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
        public static int GetRandomNumber(int Min, int Max)
        {
            return RandomNumber.GenerateNewRandom(Min, Max);
        }

        public static int GetRandom(int Min, int Max)
        {
            Random rand3 = new Random();
            return rand3.Next(Min, Max);
        }

        public static double GetUnixTimestamp()
        {
            TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return ts.TotalSeconds;
        }

        internal static int GetIUnixTimestamp()
        {
            var ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            var unixTime = ts.TotalSeconds;
            return Convert.ToInt32(unixTime);
        }

        public static bool _doubleCurrency
        {
            get { return DoubleCurrency; }
            set { DoubleCurrency = value; }
        }

        public static long Now()
        {
            TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            double unixTime = ts.TotalMilliseconds;
            return (long)unixTime;
        }

        public static string FilterFigure(string figure)
        {
            foreach (char character in figure)
            {
                if (!IsValid(character))
                    return "sh-3338-93.ea-1406-62.hr-831-49.ha-3331-92.hd-180-7.ch-3334-93-1408.lg-3337-92.ca-1813-62";
            }

            return figure;
        }

        private static bool IsValid(char character)
        {
            return Allowedchars.Contains(character);
        }

        public static bool CheckUserForDupe(GameClient Session)
        {
            string timestampnew2 = DateTime.Today.ToString("MM/dd");
            string IPAddress = string.Empty;
            string GetRewardStamp = string.Empty;

            using (IQueryAdapter dbClient = GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `dailyGift` FROM `users` WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("id", Session.GetHabbo().Id);
                timestampnew2 = dbClient.getString();

                dbClient.SetQuery("SELECT `ip_last` FROM `users` WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("id", Session.GetHabbo().Id);
                IPAddress = dbClient.getString();

                dbClient.SetQuery("SELECT `ip_last`,`dailyGift` FROM `users` WHERE `ip_last` = @ip AND `dailyGift` = @time LIMIT 1");
                dbClient.AddParameter("time", timestampnew2);
                dbClient.AddParameter("ip", IPAddress);
                GetRewardStamp = dbClient.getString();
            }

            if (GetRewardStamp.Length != 0)
                return false;

            return true;
        }

        public static bool CheckDupeUsers(GameClient User1, GameClient User2 = null)
        {
            String User1IP = String.Empty;
            string FirstUser = User1.GetHabbo().Username;
            using (IQueryAdapter dbClient = GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `ip_last` FROM `users` WHERE `id` = '" + User1.GetHabbo().Id + "' LIMIT 1");
                User1IP = dbClient.getString();
            }

            String User2IP = String.Empty;
            string SecondUser = User2.GetHabbo().Username;
            using (IQueryAdapter dbClient = GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `ip_last` FROM `users` WHERE `id` = '" + User2.GetHabbo().Id + "' LIMIT 1");
                User2IP = dbClient.getString();
            }
            if (User1IP != User2IP)
            {
                return false;
            }
            return true;
        }
        
        public static bool IsValidAlphaNumeric(string inputStr)
        {
            inputStr = inputStr.ToLower();
            if (string.IsNullOrEmpty(inputStr))
            {
                return false;
            }

            for (int i = 0; i < inputStr.Length; i++)
            {
                if (!IsValid(inputStr[i]))
                {
                    return false;
                }
            }

            return true;
        }


        
        public static string GetUsernameById(int UserId)
        {
            string Name = "Unknown User";

            GameClient Client = GetGame().GetClientManager().GetClientByUserID(UserId);
            if (Client != null && Client.GetHabbo() != null)
                return Client.GetHabbo().Username;

            UserCache User = HabboEnvironment.GetGame().GetCacheManager().GenerateUser(UserId);
            if (User != null)
                return User.Username;

            using (IQueryAdapter dbClient = HabboEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `username` FROM `users` WHERE id = @id LIMIT 1");
                dbClient.AddParameter("id", UserId);
                Name = dbClient.getString();
            }

            if (string.IsNullOrEmpty(Name))
                Name = "Unknown User";

            return Name;
        }

        public static Habbo GetHabboById(int UserId)
        {
            try
            {
                GameClient Client = GetGame().GetClientManager().GetClientByUserID(UserId);
                if (Client != null)
                {
                    Habbo User = Client.GetHabbo();
                    if (User != null && User.Id > 0)
                    {
                        if (_usersCached.ContainsKey(UserId))
                            _usersCached.TryRemove(UserId, out User);
                        return User;
                    }
                }
                else
                {
                    try
                    {
                        if (_usersCached.ContainsKey(UserId))
                            return _usersCached[UserId];
                        else
                        {
                            UserData data = UserDataFactory.GetUserData(UserId);
                            if (data != null)
                            {
                                Habbo Generated = data.user;
                                if (Generated != null)
                                {
                                    Generated.InitInformation(data);
                                    _usersCached.TryAdd(UserId, Generated);
                                    return Generated;
                                }
                            }
                        }
                    }
                    catch { return null; }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }



        public static Habbo GetHabboByUsername(String UserName)
        {
            try
            {
                using (IQueryAdapter dbClient = GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT `id` FROM `users` WHERE `username` = @user LIMIT 1");
                    dbClient.AddParameter("user", UserName);
                    int id = dbClient.getInteger();
                    if (id > 0)
                        return GetHabboById(Convert.ToInt32(id));
                }
                return null;
            }
            catch { return null; }
        }
       


        public static void PerformShutDown()
        {
            Console.Clear();
            log.Info("Server shutting down...");
            Console.Title = "HABBO EMULATOR: SHUTTING DOWN!";

            HabboEnvironment.GetGame().GetClientManager().SendMessage(new BroadcastMessageAlertComposer(HabboEnvironment.GetGame().GetLanguageLocale().TryGetValue("shutdown_alert")));
            GetGame().StopGameLoop();
            Thread.Sleep(2500);
            GetConnectionManager().Destroy();//Stop listening.
            GetGame().GetPacketManager().UnregisterAll();//Unregister the packets.
            GetGame().GetPacketManager().WaitForAllToComplete();
            GetGame().GetClientManager().CloseAll();//Close all connections
            GetGame().GetRoomManager().Dispose();//Stop the game loop.
           

            using (IQueryAdapter dbClient = _manager.GetQueryReactor())
            {
                dbClient.RunQuery("TRUNCATE `catalog_marketplace_data`");
                dbClient.RunQuery("UPDATE `users` SET `online` = '0', `auth_ticket` = ''");
                dbClient.RunQuery("UPDATE `rooms` SET `users_now` = '0' WHERE `users_now` > '0'");
                dbClient.RunQuery("UPDATE `server_status` SET `users_online` = '0', `loaded_rooms` = '0'");
            }

            log.Info("Habbo Emulator has successfully shutdown.");

            Thread.Sleep(1000);
            Environment.Exit(0);
        }

        internal static bool ShutdownStarted { get; set; }
       
        public static ConfigurationData GetConfig()
        {
            return _configuration;
        }

        public static ConfigData GetDBConfig()
        {
            return ConfigData;
        }

        public static Encoding GetDefaultEncoding()
        {
            return _defaultEncoding;
        }

        public static ConnectionHandling GetConnectionManager()
        {
            return _connectionManager;
        }


        public static Staff GetStaff()
        {
            return _staff;
        }
        public static Game GetGame()
        {
            return _game;
        }

        public static LanguageManager GetLanguageManager()
        {
            return _languageManager;
        }
        public static DatabaseManager GetDatabaseManager()
        {
            return _manager;
        }

        public static ICollection<Habbo> GetUsersCached()
        {
            return _usersCached.Values;
        }

        public static bool RemoveFromCache(int Id, out Habbo Data)
        {
            return _usersCached.TryRemove(Id, out Data);
        }
    }
}