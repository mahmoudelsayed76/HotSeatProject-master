using Cointeco;
using System;
using System.Collections.Generic;
using System.IO;
using SpotOn.DAL.Models;
using SpotOn;
using SQLite;
using System.Security.Cryptography;
// Something
namespace SpotOn.DAL
{

    public static class ORMSettings
    {
        private static string _dbDirectory;

        public static string BaseDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(_dbDirectory))
                {
                    // NOTE: The personal folder is the default location for the Mobile App 
                    // so its kept as a legacy setting. 
                    // Apps should call Initialize() before using the ORM 
                    _dbDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                }

                return _dbDirectory;
            }
        }

        /// <summary>
        /// Initiliaze the Data Subsystem 
        /// </summary>
        /// <param name="dbDirectory"></param>
        public static void Initialize(string dbDirectory)
        {
            _dbDirectory = dbDirectory;
            // check if the directory exists
            if (!Directory.Exists(dbDirectory))
            {
                CommonBase.Logger.Information("ORMSettings.Initialize: DB Folder {f} does not exist. Creating", _dbDirectory);
                // try to create it 
                var newDir = Directory.CreateDirectory(_dbDirectory);
                if (!newDir.Exists)
                {
                    throw new Exception($"ORMSettings.Initialize() unable to create folder {_dbDirectory}");
                }
            }
        }
    }

    public class ORMBase<T>
    {

        protected string TableName
        {
            get; set;
        }
        protected SQLiteConnection db;
        public string DbPath
        {
            get
            {
                return Path.Combine(ORMSettings.BaseDirectory, $"{TableName}.db3");
            }
        }

        public int ItemCount
        {
            get
            {
                int rowcount = -1;
                try
                {
                    rowcount = db.ExecuteScalar<int>($"Select COUNT(*) from [{TableName}]");
                }
                catch (Exception e)
                {

                    CommonBase.Logger.Error($"ORMBase.ItemCount Exception: {e.Message}");
                }
                return rowcount;
            }
        }

        public ORMBase(string tableName)
        {
            // CommonBase.Logger.Information($"ORMBase.ORMBase: Constructor");
            TableName = tableName;
            try
            {
                db = new SQLiteConnection(DbPath);
                db.CreateTable<T>();
            }
            catch (Exception e)
            {
                CommonBase.Logger.Error($"ORMBase Constructor Exception: {e.Message}");
            }
        }

        public virtual bool DeleteAll()
        {

            bool retVal = false;
            //CommonBase.Logger.Information($"ORMBase.DeleteAll: deleting all items");
            try
            {
                var rowcount = db.DeleteAll<T>();
                retVal = (rowcount > 0);
            }
            catch (Exception e)
            {
                CommonBase.Logger.Error($"ORMBase.DeleteAll Exception: {e.Message}");
            }
            return retVal;
        }

        /// <summary>
        /// Attempts to delete the item and returns true if it went well 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool Delete(int id)
        {
            bool retVal = false;
            CommonBase.Logger.Information($"ORMBase.Delete: deleting item with id {id}");
            try
            {
                var rowcount = db.Delete<T>(id);
                retVal = (rowcount == 1);
            }
            catch (Exception e)
            {
                CommonBase.Logger.Error($"ORMBase.Delete Exception: {e.Message}");
            }
            return retVal;
        }


        /// <summary>
        /// Tries to get item from the DB, returns null if no item found  
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual T Get(int id)
        {
            T retVal = default;
            // CommonBase.Logger.Information($"ORMBase.Get: finding item with id {id}");
            try
            {
                TableMapping t = new TableMapping(typeof(T));
                retVal = (T)db.Get(id, t);
            }
            catch (Exception e)
            {
                if (!e.Message.Contains("Sequence contains no elements")) // known error when id is invalid (we just return null)
                    CommonBase.Logger.Error($"ORMBase.Get Exception: {e.Message}");
            }
            return retVal;
        }


        /// <summary>
        /// Tries to get all item from the DB, sorted by sortBy
        /// </summary>
        /// <param name="sortByColumn">specify a SINGLE optional column to sort by.</param>
        /// <returns></returns>
        public List<T> GetAll(string sortByColumn = null)
        {
            List<T> retVal = new List<T>(this.ItemCount);
            try
            {
                if (this.ItemCount == 0)
                    return retVal;
                TableMapping t = new TableMapping(typeof(T));

                var query = $"Select * from [{TableName}] ";
                if (!string.IsNullOrEmpty(sortByColumn))
                    // TODO: need to validate that [sortByColumn] exists!
                    query += $" ORDER BY {sortByColumn}";
                else
                    query += $" ORDER BY _id ";
                var results = db.Query(t, query);
                foreach (var r in results)
                    retVal.Add((T)r);
            }
            catch (Exception e)
            {
                CommonBase.Logger.Error($"ORMBase.GetAll Exception: {e.Message}");
            }
            return retVal;
        }
        public T ScalarQuery(string query)
        {
            T retval = db.ExecuteScalar<T>(query);
            return retval;
        }

    }
}