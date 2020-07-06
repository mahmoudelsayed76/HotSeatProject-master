using System;
using System.IO;
using SQLite;
using Cointeco;
using SpotOn.DAL.Models;

namespace SpotOn.DAL
{

    public class VoiceIDEnrolmentORM : ORMBase<VoiceIDEnrolment>
    {

        public VoiceIDEnrolmentORM() : base("VoiceIDEnrolment") { }


        /// <summary>
        /// Save a new entry 
        /// </summary>
        /// <returns>
        /// Id of newly inserted item, or -1 if it failed 
        /// </returns>
        public int Save(string userFriendlyName, string profileId)
        {
            int retVal = -1;
            CommonBase.Logger.Information($"VoiceIDEnrolmentORM.Save: Saving settings");

            try
            {
                var newEnrolment = new VoiceIDEnrolment()
                {
                    UserFriendlyName = userFriendlyName,
                    ProfileId = profileId
                };
                var xstg = Get(profileId);
                if (xstg is null)
                {
                    db.Insert(newEnrolment);
                }
                else
                {
                    newEnrolment.Id = xstg.Id;
                    // if an existing record exists for this profile id, we update it if the friendly name has changed 
                    if (newEnrolment.UserFriendlyName != userFriendlyName)
                    {
                        newEnrolment.UserFriendlyName = userFriendlyName;
                        db.Update(newEnrolment);
                    }
                }

                retVal = newEnrolment.Id;
            }
            catch (Exception e)
            {
                CommonBase.Logger.Error($"VoiceIDEnrolmentORM.Save Exception: {e.Message}");
            }
            return retVal;
        }

        public VoiceIDEnrolment Get(string profileId)
        {
            string query = $"Select * from {TableName} Where ProfileId = '{profileId}'";
            var results = db.Query<VoiceIDEnrolment>(query);
            if (results.Count == 0)
                return null;
            else
                return (VoiceIDEnrolment)results[0];
        }
        public VoiceIDEnrolment GetByName(string username)
        {
            string query = $"Select * from {TableName} Where UserFriendlyName = '{username}'";
            var results = db.Query<VoiceIDEnrolment>(query);
            if (results.Count == 0)
                return null;
            else
                return (VoiceIDEnrolment)results[0];
        }

    }
}