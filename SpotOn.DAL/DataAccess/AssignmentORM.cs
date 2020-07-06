using Cointeco;
using System;
using System.IO;

using SQLite;
using SpotOn.DAL.Models;
using System.Collections.Generic;

namespace SpotOn.DAL
{

    public class AssignmentORM : ORMBase<Assignment>
    {
        public AssignmentORM() : base("Assignment") { }


        /// <summary>
        /// Save or update a new entry 
        /// </summary>
        /// <returns>
        /// Id of upserted item, or -1 if it failed 
        /// </returns>
        public int Upsert(Assignment assignment)
        {
            int retVal = -1;

            try
            {
                // check lesson ID  foreign key
                if (null == ORM.Lesson.Get(assignment.LessonId))
                {
                    throw new Exception($"Assignment.Upsert() Exception: LessonID {assignment.LessonId} is not valid.");
                }

                if (null == Get(assignment.Id))
                {
                    db.Insert(assignment);
                }
                else
                {
                    db.Update(assignment);
                }

                retVal = assignment.Id;

            }
            catch (Exception e)
            {
                CommonBase.Logger.Error($"Assignment.Upsert Exception: {e.Message}");
            }
            return retVal;
        }

        /// <summary>
        /// Save or update a new entry 
        /// </summary>
        /// <returns>
        /// Id of upserted item, or -1 if it failed 
        /// </returns>
        private int Upsert(int id, int lessonId, int assigneeId, int reviewerId, DateTime date, DateTime dueDate, AssignmentStatus status, bool completed = false, int attempts = 0)
        {

            return Upsert(new Assignment()
            {
                Id = id,
                DueDate = dueDate,
                Date = date,
                AssigneeUserId = assigneeId,
                ReviewerUserId = reviewerId,
                Status = status,
                Attempts = attempts,
                Completed = completed,
                LessonId = lessonId
            });
        }


        /// <summary>
        /// Get All assignments for the lesson / user 
        /// </summary>
        /// <param name="lessonid"></param>
        /// <param name="username">if null, all assignments for this lesson are returned</param>
        /// <returns></returns>
        public  List<Assignment> GetAssignments(int lessonid, int assigneeId = -1)
        {
            string query = $"Select * from {TableName} where LessonID = '{lessonid}'";
            if (assigneeId > -1)
            {
                query += $" and AssigneeUserId = '{assigneeId}'";
            }
            return db.Query<Assignment>(query);
        }



        /// <summary>
        /// get all reviewed assignments
        /// </summary>
        /// <returns></returns>
        public  List<Assignment> GetReviewed()
        {
            string query = $"Select * from {TableName} where Status = 3";
            return db.Query<Assignment>(query);
        }

        public  int AssignTo(int lessonId, int assigneeId, int reviewerId, DateTime utcNow, DateTime dd)
        {
            var assignmentId = Upsert(
                -1, // assignment ID
                lessonId,
                assigneeId,
                reviewerId,
                DateTime.UtcNow, // assigned date
                dd, // due date
                AssignmentStatus.Assigned);

            // also notify recipient
            ORM.Notification.Notify(Organization.PUBLIC_ORG_ID,"New Assignment", $"You have a new assignment by {ORM.User.GetName(reviewerId)}. Please complete it by {dd:d}.",
                NotificationType.NewAssignment, assigneeId, reviewerId);

            return assignmentId;
        }


        /// <summary>
        /// Get All assignments for the assignee or reviewer (set forReviewer=true)
        /// </summary>
        /// <returns></returns>
        public List<Assignment> GetUserAssignmentsInProgress(int userId)
        {
            string query = $"Select * from {TableName} where AssigneeUserId = '{userId}' AND Status in (0,1)"; // 0 = Assigned, 1 = InProgress
            return db.Query<Assignment>(query);
        }

        public List<Assignment> GetUserAssignmentsInReview(int userId)
        {
            string query = $"Select * from {TableName} where (ReviewerUserId = '{userId}' OR AssigneeUserId = '{userId}' ) AND Status = 2"; // 2 = submitted
            return db.Query<Assignment>(query);
        }
        public List<Assignment> GetUserAssignmentsReviewed(int userId)
        {
            string query = $"Select * from {TableName} where AssigneeUserId = '{userId}' AND Status = 3"; // 3 = reviewed
            return db.Query<Assignment>(query);
        }

        /// <summary>
        /// submit the assignment for review 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool SubmitForReview(Attempt attempt, bool notifyRecipient = true)
        {
            return SetNewState(attempt.AssignmentId, AssignmentStatus.Submitted, false, notifyRecipient);
        }

        public bool Pass(Attempt attempt, bool notifyRecipient = true)
        {
            // See who is on the leaderboard 
            var AB1 = AssignmentBundle.GetHighestScoring();

            var retval = SetNewState(attempt.AssignmentId, AssignmentStatus.Reviewed, false, notifyRecipient);

            // if the leaderboard has changed, announce it.
            var AB2 = AssignmentBundle.GetHighestScoring();
            if (AB2 != null)
            {
                if (AB1 is null || AB2.Assignment.Id != AB1.Assignment.Id)
                {
                    // there's a new winner 
                    ORM.Notification.Notify(Organization.PUBLIC_ORG_ID, "New High Score", $"{AB2.Assignment.GetAssigneeName()} has topped the leaderboard with a score {AB2.Attempt.FinalScore:0.0}.",
                        NotificationType.NewLeaderboardEntry,Notification.PUBLIC_USER_ID);
                }
            }

            return retval; 
        }
        public bool Fail(Attempt attempt, bool notifyRecipient = true)
        {
            return SetNewState(attempt.AssignmentId, AssignmentStatus.InProgress, true, notifyRecipient);
        }

        private bool SetNewState(int id, AssignmentStatus status, bool incrementAttempts = false, bool notifyRecipient = false)
        {
            var assignment = this.Get(id);

            if (assignment is null)
                return false;

            // if previous state was Submitted, and now its assigned, 
            // it must have been re-opened 
            bool failed = false;
            if (assignment.Status == AssignmentStatus.Submitted && status == AssignmentStatus.InProgress)
                failed = true;

            // change the state 
            assignment.Status = status;

            // increment attempts 
            if (incrementAttempts)
                assignment.Attempts++;

            // notify recipients 
            if (notifyRecipient)
            {
                if (failed)
                {
                    ORM.Notification.Notify(Organization.PUBLIC_ORG_ID,"Assignment Failed", $"You have an assignment that was failed by {assignment.GetReviewerName()}. Please try it again.",
                        NotificationType.FailedAssignment, assignment.AssigneeUserId, assignment.ReviewerUserId);
                }
                else switch (status)
                    {
                        case AssignmentStatus.Reviewed:

                            ORM.Notification.Notify(Organization.PUBLIC_ORG_ID,"Assignment Passed", $"Your assignment was reviewed and passed by {assignment.GetReviewerName()}. Well done.",
                                NotificationType.PassedAssignment, assignment.AssigneeUserId, assignment.ReviewerUserId);

                            break;
                        case AssignmentStatus.Assigned:

                            ORM.Notification.Notify(Organization.PUBLIC_ORG_ID,"New Assignment", $"You have a new assignment by {assignment.GetReviewerName()}. Please complete it by {assignment.DueDate:d}.",
                                NotificationType.NewAssignment, assignment.AssigneeUserId, assignment.ReviewerUserId);

                            break;
                        case AssignmentStatus.Submitted:

                            ORM.Notification.Notify(Organization.PUBLIC_ORG_ID, "Assignment Ready For Review", $"{assignment.GetAssigneeName()} submitted an assignment for your review.",
                                NotificationType.AssignmentForReview, assignment.ReviewerUserId, assignment.AssigneeUserId);

                            break;
                        default:
                            CommonBase.Logger.Warning("AssignmentORM.SetNewState() Error: Invalid Notification Condition.");
                            break;
                    }
            }

            // save it 
            Upsert(assignment);

            return true;
        }
        /// <summary>
        /// Delete the assignment, by first deleting all Attempts
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override bool Delete(int id)
        {
            var attempts = ORM.Attempt.GetByAssignmentId(id);
            bool retVal = true;
            foreach (var attempt in attempts)
            {
                if (!ORM.Attempt.Delete(attempt.Id))
                {
                    CommonBase.Logger.Warning("AssignmentORM.Delete() Error: Unable to delete Attempt {id} from Assignment {assid}. ", attempt.Id, id);
                    retVal = false;
                }
            }

            // this deletes the assignment 
            retVal &= base.Delete(id);

            return retVal;
        }


        public override bool DeleteAll()
        {
            var allItems = this.GetAll();
            foreach (var item in allItems)
            {
                Delete(item.Id);
            }
            return true;
        }

        /// <summary>
        /// return whether the person taking this assignment is female
        /// </summary>
        /// <param name="id"></param>
        public static bool IsAssigneeFemale(int assignmentId)
        {
            var assgmt = ORM.Assignment.Get(assignmentId);
            var user = ORM.User.Get(assgmt?.AssigneeUserId ?? -1);
            return (true == user?.IsFemale);
        }

    }
}