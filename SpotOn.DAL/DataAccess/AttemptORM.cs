using System;
using System.IO;
using Cointeco;
using SQLite;
using SpotOn.DAL.Models;
using System.Collections.Generic;

namespace SpotOn.DAL
{

    public class AttemptORM : ORMBase<Attempt>
    {
        public AttemptORM() : base("Attempt") { }

        /// <summary>
        /// Save or update a new entry 
        /// </summary>
        /// <returns>
        /// Id of upserted item, or -1 if it failed 
        /// </returns>
        //public int Upsert(int assessmentId, int assignmentId, DateTime date = default, string mediaLocation = null, double mediaDurationMS = 0.0, int id = -1)
        //{
        //    var newAttempt = new Attempt()
        //    {
        //        Id = id,
        //        Date = date,
        //        AssessmentId = assessmentId,
        //        AssignmentId = assignmentId,
        //        MediaLocation = mediaLocation,
        //        MediaDurationMS = mediaDurationMS
        //    };
        //    return Upsert(newAttempt);
        //}

        public int Upsert(Attempt attempt, Assessment assessment = null)
        {
            CommonBase.Logger.Information($"Attempt.Upsert: Saving entry with id {attempt.Id} for assignment id {attempt.AssignmentId} on {attempt.Date}");

            try
            {
                // check foreign keys : assignment is required but assessment is optional 
                if (ORM.Assignment.Get(attempt.AssignmentId) is null)
                    throw new Exception($"Attempt.Upsert() Exception: AssignmentID {attempt.AssignmentId} is not valid.");

                // if assessment is provided, we automatically update the assessmentID to the new one 
                if (assessment != null)
                {
                    attempt.AssessmentId = ORM.Assessment.Upsert(assessment);
                }
                else // no assessment provided - look for one 
                {
                    assessment = ORM.Assessment.Get(attempt.AssessmentId);
                    if (attempt.AssessmentId >= 0 && assessment is null)
                        throw new Exception($"Attempt.Upsert() Exception: AssessmentID {attempt.AssessmentId} is not valid.");
                }

                if (assessment != null)
                {
                    attempt.FinalScore = assessment.Score/2.0;

                    // look for reviewer rating (if any)  to calculate final score
                    var rr = ORM.ReviewerRating.GetByAttempt(attempt.Id);
                    if (rr != null)
                        attempt.FinalScore += rr.GetRating()/2.0;
                }

                // All Good
                if (Get(attempt.Id) is null)
                {
                    db.Insert(attempt);
                }
                else
                {
                    db.Update(attempt);
                }

                return attempt.Id;

            }
            catch (Exception e)
            {
                CommonBase.Logger.Error($"Attempt.Upsert Exception: {e.Message}");
            }
            return -1;
        }

        public  List<Attempt> GetByAssignmentId(int assignmentId)
        {
            string query = $"Select * from {TableName} where AssignmentID = '{assignmentId}' ORDER BY Date desc";
            return db.Query<Attempt>(query);
        }

        /// <summary>
        /// get latest attempt by assignment id. returns NULL if none found.
        /// </summary>
        /// <param name="assignmentId"></param>
        /// <returns></returns>
        public  Attempt Latest(int assignmentId)
        {
            var attempts = this.GetByAssignmentId(assignmentId);
            return (attempts.Count > 0) ? attempts[0] : null;
        }

        /// <summary>
        /// Delete the attempt, after deleting 
        ///     PeerRating 
        ///     SupervisorRating
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override bool Delete(int id)
        {
            // this deletes the Attempt and attached stuff 
            var rr = ORM.ReviewerRating.GetByAttempt(id);
            if (rr != null)
                ORM.ReviewerRating.Delete(rr.Id);
            return base.Delete(id);
        }
    }
}