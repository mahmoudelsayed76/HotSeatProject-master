using Cointeco;
using System;
using System.IO;
using SQLite;
using SpotOn.DAL.Models;

namespace SpotOn.DAL
{

    public class SettingsORM : ORMBase<Settings>
    {

        public SettingsORM() : base("Settings") { }  
 
        /// <summary>
        /// Save a new entry 
        /// </summary>
        /// <returns>
        /// Id of newly inserted item, or -1 if it failed 
        /// </returns>
        public int Save(string orgId, User currentUser, string appPersona, long SPTimeoutMS )
        {

            int retVal = -1;
            CommonBase.Logger.Information($"SettingsORM.Save: Saving settings");

            try
            {
                // save user
                currentUser.Id = ORM.User.Upsert(currentUser);

                var singleton = new Settings()
                {
                    UserId = currentUser.Id,
                    OrgId = orgId,
                    AppPersona = appPersona,
                    SpRecogTimeoutMS = SPTimeoutMS
                };

                var xstg = Get();
                if (xstg is null)
                {
                    db.Insert(singleton);
                }
                else
                {
                    singleton.Id = xstg.Id;
                    db.Update(singleton);
                }

                retVal = singleton.Id;
            }
            catch (Exception e)
            {
                CommonBase.Logger.Error($"SettingsORM.Save Exception: {e.Message}");
            }
            return retVal;
        }

        public Settings Get()
        {
            string query = $"Select * from {TableName} ORDER BY _id LIMIT 1";
            var results = db.Query<Settings>(query);
            if (results.Count == 0)
                return null;
            else
                return (Settings) results[0];
        }


    }
}