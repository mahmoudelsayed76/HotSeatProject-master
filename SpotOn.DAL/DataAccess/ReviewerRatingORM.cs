using System;
using System.IO;
using SQLite;
using Cointeco;
using SpotOn.DAL.Models;

namespace SpotOn.DAL
{

    public class ReviewerRatingORM : ORMBase<ReviewerRating>
    {

        public ReviewerRatingORM() : base("ReviewerRating") { }

        /// <summary>
        /// returns id of item 
        /// </summary>
        /// <param name="ReviewerRating"></param>
        /// <returns></returns>
        public int Upsert(ReviewerRating ReviewerRating)
        {
            int retVal = -1;
            CommonBase.Logger.Information($"ReviewerRating.Upsert: Saving entry from reviewer {ReviewerRating.GetReviewerName()} on {ReviewerRating.Date}");

            try
            {
                if (Get(ReviewerRating.Id) is null)
                    db.Insert(ReviewerRating);
                else
                    db.Update(ReviewerRating);
                retVal = ReviewerRating.Id;
            }
            catch (Exception e)
            {
                CommonBase.Logger.Error($"ReviewerRating.Upsert Exception: {e.Message}");
            }
            return retVal;
        }

        ///// <summary>
        ///// Select a Reviewer Rating object by the attempt. 
        ///// If none is found, return a new object
        ///// </summary>
        ///// <param name="attemptId"></param>
        ///// <returns></returns>
        //public ReviewerRating CreateOrGetByAttempt(int attemptId)
        //{
        //    var rr = GetByAttempt(attemptId);
        //    rr ??= New(attemptId);
        //    return rr; 
        //}

        public static ReviewerRating New(int attemptId)
        {
            var attemptBundle = AssignmentBundle.FromAttemptId(attemptId);

            if (attemptBundle is null)
                throw new Exception($"Unable to create ReviewerRating object from invalid Attempt having Id {attemptId} ");
            var rr = new ReviewerRating()
            {
                Id = -1,
                AssigneeId = attemptBundle.Assignment.AssigneeUserId,
                ReviewerId = attemptBundle.Assignment.ReviewerUserId,
                AttemptId = attemptBundle.Attempt.Id,
                // also copy App scores from Assessment
                AppClarity = (int)attemptBundle.Assessment.ClarityLikert,
                AppConfidence = (int)attemptBundle.Assessment.ConfidenceLikert,
                AppCredibility = (int)attemptBundle.Assessment.CredibilityLikert,
                Date = DateTime.UtcNow
            };

            return rr;
        }


        public ReviewerRating GetByAttempt(int attemptId)
        {
            string query = $"Select * from {TableName} where AttemptId = '{attemptId}' ORDER BY Date desc";
            var rrs = db.Query<ReviewerRating>(query);
            if (rrs.Count >= 1)
                return rrs[0];
            else
                return null;
        }
    }
}