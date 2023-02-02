using System;
using System.IO;

namespace MessengerStorage
{
    public class RemoteStorage
    {
        public RemoteStorage(string appDataPath, string entryPoint = default, int eraseAfterDays = 90, string key = null)
        {
            if (entryPoint== default)
            {
                entryPoint = "test.cloudservices.agency";
            }
            AppDataPath = appDataPath;
            if (string.IsNullOrEmpty(AppDataPath))
            {
                var appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName, nameof(RemoteStorage));
            }
            Directory.CreateDirectory(AppDataPath);

#if RELEASE
            //string privateKey = @"EMV90BxC+eClZ2C+FpbqFoH91Buc4jfGa/JQ+d1vDxg=";
            //string pubKey = @"ApkrRQUe7qbaKY05Lbs5z+o001UNzXlfHgm+9KEN41vE";
            string privateKey = @"ZU6nmZ2GkKK1oGPgtWXp5XO5Zrmx0GjEM+ZjvJwlzig=";
            string pubKey = @"A4f7EZyD/lVQd5P4r0H3haPCdQJNOU/6sm7LsZoIT+XH";

#elif DEBUG
            //string entryPoint = TelegraphChannel.Utility.GetLocalIPAddress(); // Used for release
            //string privateKey = @"EMV90BxC+eClZ2C+FpbqFoH91Buc4jfGa/JQ+d1vDxg=";
            //string pubKey = @"ApkrRQUe7qbaKY05Lbs5z+o001UNzXlfHgm+9KEN41vE";
            string privateKey = @"usQ5cMu+zmlId4qDP/ZzZPz+NcaM+Og1g/uZJLUtQAM=";
            string pubKey = @"AoT0MTjFnw4bVT1ma74N/dK3Za5A1Iv5TDC4vKtJ6Ukm";
#elif DEBUG_RAM
            string privateKey = @"dpzifhe4Iyun4S/31EnD/c2Fjci3D1PADmDdRjtyXAk=";
            string pubKey = @"AunBfxTIY2JykNZOXdpEW5+1a6+9IQ7ZgdWIgRlHbT9X";
#else
            // Key generators!
            var _mnemo = new Mnemonic(Wordlist.English, WordCount.Twelve);
            var _hdRoot = _mnemo.DeriveExtKey();
            var _privateKey = _hdRoot.PrivateKey;
            var privateKey = Convert.ToBase64String(_privateKey.ToBytes());
            var pubKey = Convert.ToBase64String(_privateKey.PubKey.ToBytes());
#endif
            if (!string.IsNullOrEmpty(key))
                privateKey = key;
            // CloudAppData = @"E:\";
            // don't forget to copy users' data from root to app folder
            Cloud = new Cloud(this, privateKey, entryPoint, appDataPath);
            if (eraseAfterDays != 0)
                EraseNotUsedAccountTimer = new System.Threading.Timer(new System.Threading.TimerCallback((state) => EraseNotUsedAccount()), null, new TimeSpan(24, 0, 0), new TimeSpan(24, 0, 0));
#if DEBUG_RAM
            EraseNotUsedAccountTimer = new System.Threading.Timer(new System.Threading.TimerCallback((state) => EraseNotUsedAccount()), null, new TimeSpan(0, 1, 0), new TimeSpan(0, 1, 0));
#endif
        }
        private string AppDataPath;
        private readonly System.Threading.Timer EraseNotUsedAccountTimer;
        /// <summary>
        /// path to folder where data will be stored
        /// </summary>
        public Cloud Cloud;

        private bool EraseRunning;
        /// <summary>
        /// After how many days of inactivity an account data will be removed from the cloud
        /// </summary>
        private double EraseAfterDays;

        /// <summary>
        /// Removes account data of innactive users from the cloud storage. uses EraseAfterDays specified earlier to check if data needs to be deleted, 
        /// also removes newly created accounts( mainly used for testing purposes) which havent been used.
        /// </summary>
        private  void EraseNotUsedAccount()
        {
            if (!EraseRunning)
            {
                EraseRunning = true;
                var dirs = new DirectoryInfo(AppDataPath);
                if (dirs.Exists)
                {
                    foreach (var dir in dirs.GetDirectories())
                    {
                        var files = dir.GetFiles("*.*", SearchOption.AllDirectories);
                        FileInfo newer = null;
                        foreach (var file in files)
                        {
                            if (newer == null)
                                newer = file;
                            else if (file.LastWriteTimeUtc > newer.LastWriteTimeUtc)
                                newer = file;
                        }
                        if (newer != null)
                        {
                            var lastWriteTimeInDays = (DateTime.UtcNow - newer.LastWriteTimeUtc).TotalDays;
                            if (lastWriteTimeInDays > EraseAfterDays || files.Length <= 2 && lastWriteTimeInDays >= 10) // Delete accounts that have not been used for more than a year OR clear data of newly created accounts that have not been used (usually these are test accounts to test the application)
                            {
                                try { dir.Delete(true); }
                                catch (Exception ex) { Console.WriteLine(ex.Message); }
                            }
                        }
                    }
                }
                EraseRunning = false;
            }
        }
    }
}
