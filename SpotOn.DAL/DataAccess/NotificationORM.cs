using Cointeco;
using System;
using System.Collections.Generic;
using SpotOn.DAL.Models;

namespace SpotOn.DAL
{

    public class NotificationORM : ORMBase<Notification>
    {

        public NotificationORM() : base("Notification") { }

        public static Notification New(
                string orgId,
                string title,
                string text,
                NotificationType type = NotificationType.General,
                DateTime dateTime = default,
                int fromUserId = Notification.SYSTEM_USER_ID,
                int toUserId = Notification.SYSTEM_USER_ID
            )
        {
            return new Notification()
            {
                Id = -1,
                OrgId = orgId,
                Title = title,
                Text = text,
                Type = type,
                DateTime = dateTime == default ? DateTime.UtcNow : dateTime,
                ToUserId = toUserId,
                FromUserId = fromUserId,
            };
        }

        /// <summary>
        /// returns id of item 
        /// </summary>
        /// <param name="N"></param>
        /// <returns></returns>
        public int Upsert(Notification N)
        {
            int retVal = -1;
            CommonBase.Logger.Information($"Notification.Upsert: Saving entry {N.Title} ");

            try
            {
                if (base.Get(N.Id) is null)
                    db.Insert(N);
                else
                    db.Update(N);
                retVal = N.Id;
            }
            catch (Exception e)
            {
                CommonBase.Logger.Error($"Notification.Upsert Exception: {e.Message}");
            }
            return retVal;
        }

        /// <summary>
        /// find all the New notifications for the user, sorted by most recent first
        /// </summary>
        /// <param name="userId"></param>
        public  List<Notification> GetNew(int userId)
        {
            string query = $"Select * from {TableName} where  (ToUserId = '{userId}' OR ToUserId = '{Notification.PUBLIC_USER_ID}' OR " +
                $"Type = {(int)NotificationType.General}) AND IsRead = False ORDER BY DateTime DESC";
            return db.Query<Notification>(query);
        }

        /// <summary>
        /// find all the Old notifications for the user, sorted by most recent first
        /// </summary>
        /// <param name="userId"></param>
        public  List<Notification> GetOld(int userId)
        {
            string query = $"Select * from {TableName} where  (ToUserId = '{userId}' OR ToUserId = '{Notification.PUBLIC_USER_ID}' OR " +
                $"Type = {(int)NotificationType.General}) AND IsRead = True ORDER BY DateTime DESC";
            return db.Query<Notification>(query);
        }

        /// <summary>
        /// create a new notification for the intended recipient 
        /// </summary>
        /// <returns>the Notification Id</returns>
        public  int Notify(
                string orgId,
                string title,
                string text,
                NotificationType type,
                int toUserId,
                int fromUserId = Notification.SYSTEM_USER_ID)
        {
            if (toUserId == Notification.PUBLIC_USER_ID || type == NotificationType.General)
            {
                // put the notification into EVERYONE's inbox 
                foreach (var user in ORM.User.GetAll())
                {
                    this.Upsert(NotificationORM.New(orgId, title, text, type, DateTime.UtcNow, fromUserId, user.Id));
                }
                return -1; 
            }
            else
            {
                return this.Upsert(NotificationORM.New(orgId, title, text, type, DateTime.UtcNow, fromUserId, toUserId));
            }
        }
    }
}