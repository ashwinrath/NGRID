using System;
using NGRID.Storage.MemoryStorage;
using NGRID.Storage.MsSqlStorage;
using NGRID.Storage.MySqlStorage;
using NGRID.Storage.SQLiteStorage;
using NGRID.Settings;

namespace NGRID.Storage
{
    public static class StorageManagerFactory
    {
        /// <summary>
        /// Creates Storage Manager according to StorageType value in Settings file.
        /// </summary>
        /// <returns>Storame Manager</returns>
        public static IStorageManager CreateStorageManager()
        {
            //Get a reference to the settings
            var settings = NGRIDSettings.Instance;

            //Create storage manager according to the settings
            var storageType = settings["StorageType"];
            IStorageManager storageManager;
            if (storageType.Equals("MySQL-ODBC", StringComparison.OrdinalIgnoreCase))
            {
                storageManager = new MySqlOdbcStorageManager { ConnectionString = settings["ConnectionString"] };
            }
            else if (storageType.Equals("MySQL-Net", StringComparison.OrdinalIgnoreCase))
            {
                storageManager = new MySqlNetStorageManager { ConnectionString = settings["ConnectionString"] };
            }
            else if (storageType.Equals("MSSQL", StringComparison.OrdinalIgnoreCase))
            {
                storageManager = new MsSqlStorageManager { ConnectionString = settings["ConnectionString"] };
            }
            else if (storageType.Equals("SQLite", StringComparison.OrdinalIgnoreCase))
            {
                storageManager = new SqliteStorageManager();
            }
            else //Default storage manager
            {
                storageManager = new MemoryStorageManager();
            }

            //Wrap storageManager with FaultToleratedStorageManagerWrapper and return it
            return new FaultToleratedStorageManagerWrapper(storageManager);
        }
    }
}
