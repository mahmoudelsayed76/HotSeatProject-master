using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using SQLite;

namespace SpotOn.DAL.Models
{

    [Table("ReviewerRating")]
    public class ReviewerRating
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }

        public string OrgId { get; set; }

        public DateTime Date { get; set; }

        public int ReviewerId { get; set; }
        public int AssigneeId { get; set; }
        public int AttemptId { get; set; }


        // --------------------------------
        // App Vocal Power (Range is 1 - 5)
        // NOT REVIEWER EDITABLE 
        // --------------------------------
        public int AppConfidence { get; set; } = 3;
        public int AppClarity { get; set; } = 3;
        public int AppCredibility { get; set; } = 3;

        // --------------------------------
        // TONE (Range is 1 - 5)
        // --------------------------------
        public int Confidence { get; set; } = 3;
        public int Clarity { get; set; } = 3;
        public int Energetic { get; set; } = 3;

        // --------------------------------
        // Demeanor (Range is 1 - 5)
        // --------------------------------
        public int Friendly { get; set; } = 3;
        public int Sincere { get; set; } = 3;
        public int Interest { get; set; } = 3;


        // --------------------------------
        // Goal (Range is 1 - 5)
        // --------------------------------
        public int FollowedScript { get; set; } = 3;
        public int KeyPoints { get; set; } = 3;


        // ----------------
        // Unused
        // ----------------
        public int XArticulation { get; set; } = 3;
        public int XEngagement { get; set; } = 3;


        public string Comments { get; set; }

        public double GetRating()
        {
            // Rating is out of 100, so we divide by total rating (= 11 x 5).
            return 100 * (
                AppConfidence + AppClarity + AppCredibility + 
                Clarity + Energetic +  Confidence + 
                Friendly + Sincere +  Interest + 
                FollowedScript +  KeyPoints) / 55;
        }

        public string GetReviewerName()
        {
            return ORM.User.GetName(this.ReviewerId);
        }

        public string GetAssigneeName()
        {
            return ORM.User.GetName(this.AssigneeId);
        }
    }


}