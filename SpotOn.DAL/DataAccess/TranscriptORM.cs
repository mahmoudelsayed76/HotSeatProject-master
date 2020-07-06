using Cointeco;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Newtonsoft.Json.Serialization;
using SpotOn.DAL.Models;


namespace SpotOn.DAL
{

    public class TranscriptORM : ORMBase<Transcript>
    {

        public TranscriptORM() : base("Transcript") { }

        /// <summary>
        /// returns id of item 
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        public int Upsert(Transcript T)
        {
            int retVal = -1;

            try
            {
                // update date time
                if (T.DateTime == default)
                    T.DateTime = DateTime.UtcNow;

                if (base.Get(T.Id) is null)
                    db.Insert(T);
                else
                    db.Update(T);
                retVal = T.Id;
            }
            catch (Exception e)
            {
                CommonBase.Logger.Error($"TranscriptORM.Upsert()  Exception: {e.Message}");
            }
            return retVal;
        }

        public Transcript NextFrom(int TranscriptId)
        {
            var xstg = Get(TranscriptId);
            if (xstg is null)
                return null;
            else
                return xstg.Next();
        }

        /// <summary>
        /// Return all recorded transcripts for the given userid by checking for rows where 
        /// SpeakerId = userId and TranscriptType = Header
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<Transcript> GetForUser(int userId, bool timeAscending = false, TranscriptModule module =  TranscriptModule.Roleplay)
        {
            try
            {
                string query = $"Select * from {TableName} where (SpeakerId = '{userId}' AND Type = {(int)TranscriptType.Header} AND Module = {(int)module}) ";
                if (timeAscending)
                    query += "ORDER BY DateTime ASC";
                else
                    query += "ORDER BY DateTime DESC";

                return db.Query<Transcript>(query);
            }
            catch (Exception ex)
            {
                CommonBase.Logger.Error("TranscriptORM.GetForUser({userId}) Exception : ", userId, ex.Message);
            }
            return null;
        }

        /// <summary>
        /// return all the entries related to a given conversation id 
        /// </summary>
        /// <param name="conversationId"></param>
        /// <returns></returns>
        public List<Transcript> GetConversation(string conversationId)
        {
            try
            {
                string query = $"Select * from {TableName} where ( ConversationId = '{conversationId}') ORDER BY Sequence ASC";
                return db.Query<Transcript>(query);
            }
            catch (Exception ex)
            {
                CommonBase.Logger.Error("TranscriptORM.Get(conversationId) Exception : ", ex.Message);
            }
            return null;
        }

        /// <summary>
        /// delete all entries related to the given conversation id 
        /// </summary>
        /// <param name="ConversationId"></param>
        /// <returns></returns>
        public bool DeleteConversation(string conversationId)
        {
            try
            {
                var transcripts = GetConversation(conversationId);
                foreach (var t in transcripts)
                    Delete(t.Id);
                return true;
            }
            catch (Exception ex)
            {
                CommonBase.Logger.Error("TranscriptORM.DeleteConversation() Exception : ", ex.Message);
            }
            return false;
        }


        /// <summary>
        /// delete all entries related to the given conversation id 
        /// </summary>
        /// <param name="ConversationId"></param>
        /// <returns></returns>
        public bool SaveConversation(int ownerId, string orgId, string description, List<Transcript> transcripts, TranscriptModule module)
        {
            try
            {
                string conversationId = Guid.NewGuid().ToString();
                int seq = 0;
                var hdr = Transcript.New(ownerId, Organization.PUBLIC_ORG_ID, description, 0, TranscriptType.Header, module,  DateTime.UtcNow, conversationId);
                var hdrId = Upsert(hdr);

                foreach (var t in transcripts)
                {
                    // override some items
                    t.ConversationId = conversationId;
                    t.OrgId = orgId;
                    t.Type = TranscriptType.Detail;
                    t.Sequence = ++seq; // increment before using 
                    t.Id = -1;
                    Upsert(t);
                }

                return true;
            }
            catch (Exception ex)
            {
                CommonBase.Logger.Error("TranscriptORM.DeleteConversation() Exception : ", ex.Message);
            }
            return false;
        }
    }
}