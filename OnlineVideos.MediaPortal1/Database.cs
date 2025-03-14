using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.NET;
using MediaPortal.Configuration;
using MediaPortal.Database;

namespace OnlineVideos.MediaPortal1
{
    public class Database: IDisposable
    {
        public SQLiteClient Client { get; private set; }

        private static Database _Instance;

        public static Database Instance
        {
            get
            {
                if (_Instance == null) 
                    _Instance = new Database();

                return _Instance;
            }
        }

        private Database()
        {
            try
            {
                this.Client = new SQLiteClient(Config.GetFile(Config.Dir.Database, "OnlineVideoDatabase.db3"));
                DatabaseUtility.SetPragmas(this.Client);
            }
            catch (SQLiteException ex)
            {
                Log.Instance.Error("database exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
            }
        }

        public void Dispose()
        {
            if (this.Client != null)
            {
                this.Client.Close();
                this.Client.Dispose();
                this.Client = null;
            }
        }
    }
}
