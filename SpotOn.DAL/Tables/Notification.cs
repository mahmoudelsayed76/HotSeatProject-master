using SQLite;
using System;

namespace SpotOn.DAL.Models
{
    public enum NotificationType
    {
        General = 1,
        PrivateMessage = 2,
        NewAssignment = 10,
        FailedAssignment = 20,
        PassedAssignment = 30,
        AssignmentForReview = 40,
        NewLesson = 100,
        NewLeaderboardEntry = 200,
        NewFeature = 300,
        SystemNotification = 400
    }
    [Table("Notification")]
    public class Notification
    {
        public const int SYSTEM_USER_ID = -1;
        public const int APP_USER_ID = -2;
        public const int PUBLIC_USER_ID = -10;

        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public string OrgId { get; set; }

        public string Title { get; set; }
        public string Text { get; set; }
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public bool IsRead { get; set; }
        public NotificationType Type { get; set; }
        public DateTime DateTime { get; set; }
    }
}