using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using SQLite;

namespace SpotOn.DAL.Models
{
    class FinalScoreComparer : IComparer<AssignmentBundle>
    {
        public int Compare(AssignmentBundle x, AssignmentBundle y)
        {
            if (x is null || y is null || x.Attempt is null || y.Attempt is null)
            {
                return 0;
            }

            // if the difference is less than 1.0, the  
            double diff = 100 * (y.Attempt.FinalScore - x.Attempt.FinalScore);

            return (int)diff;
        }
    }
    public  class AssignmentBundle
    {
        public  Assignment Assignment { get; set; }
        public Attempt Attempt { get; set; }
        public Lesson Lesson { get; set; }
        public Assessment Assessment { get; set; }
        public ReviewerRating ReviewerRating { get; set; }

        /// <summary>
        /// State has the following values:
        ///    0 : nothing loaded
        ///    1 : attempt loaded
        ///    2 : assignment loaded
        ///    3 : lesson loaded
        ///    4 : asessment loaded
        /// </summary>
        public int LoadState { get; set; }

        public bool IsLoaded
        {
            get
            {
                return LoadState == 4;
            }
        }

        /// <summary>
        /// Is the assignment currently in progress ? (as opposed to 'submitted'  / 'reviewed') 
        /// </summary>
        public bool IsWorkInProgress
        {
            get
            {
                return (Assignment?.Status == AssignmentStatus.Assigned || Assignment?.Status == AssignmentStatus.InProgress);
            }
        }

        public bool IsReviewed
        {
            get
            {
                return (Assignment?.Status == AssignmentStatus.Reviewed);
            }
        }

        public bool HasDraft
        {
            get
            {
                return (Assignment?.Status == AssignmentStatus.InProgress);
            }
        }
        /// <summary>
        /// return all completed attempts (status = 3) 
        /// </summary>
        /// <returns></returns>
        public static List<AssignmentBundle> GetReviewedSortedByFinalScore()
        {
            var assignments = ORM.Assignment.GetReviewed();
            List<AssignmentBundle> assgnmtBndls = new List<AssignmentBundle>(assignments.Count);
            foreach (var a in assignments)
            {
                assgnmtBndls.Add(AssignmentBundle.FromAssignmentId(a.Id));
            }
            // sort it by attempt FinalScore 
            assgnmtBndls.Sort(new FinalScoreComparer());
            return assgnmtBndls;
        }

        /// <summary>
        /// return reviewed assignments with the highest score 
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static AssignmentBundle GetHighestScoring()
        {
            var reviewedAssignments = GetReviewedSortedByFinalScore();
            if (reviewedAssignments.Count > 0)
            {
                // the first one has the highest score 
                return reviewedAssignments[0];
            }
            return null;
        }

        /// <summary>
        /// set the attempt to pass and recalculate the scores 
        /// </summary>
        /// <param name="attempt"></param>
        /// <returns></returns>
        public static bool PassOrFail(Attempt attempt, bool setToPass = true)
        {
            var AB = AssignmentBundle.FromAttemptId(attempt.Id);

            // recalculate the score (done inside the Upsert)
            if (AB.Assessment != null && AB.ReviewerRating != null)
                ORM.Attempt.Upsert(AB.Attempt, AB.Assessment);

            // pass the assignment 
            if (setToPass)
                return ORM.Assignment.Pass(AB.Attempt);
            else
                return ORM.Assignment.Fail(AB.Attempt);

        }

        public static AssignmentBundle FromAssignmentId(int assignmentId)
        {
            var attempt = ORM.Attempt.Latest(assignmentId);
            if (attempt is null)
            {
                // just find the assignment 
                var assignment = ORM.Assignment.Get(assignmentId);
                if (assignment is null)
                {
                    return null;
                }
                else
                {
                    return new AssignmentBundle()
                    {
                        Assignment = assignment,
                        Lesson = ORM.Lesson.Get(assignment.LessonId),
                        LoadState = 2, // assignment loaded
                        Attempt = new Attempt() { Id = -1 }, // new 'blank' attempt
                        ReviewerRating = null

                    };
                }
            }
            return new AssignmentBundle(attempt.Id);
        }

        public static AssignmentBundle FromAttemptId(int attemptId)
        {
            return new AssignmentBundle(attemptId);
        }

        public AssignmentBundle()
        {
            LoadState = 0;
        }
        /// <summary>
        /// load all the objects related to this Attempt
        /// </summary>
        /// <param name="attemptId"></param>
        public AssignmentBundle(int attemptId)
        {
            // From the AttemptID provided, we should be able to load the AssignmentID, AssessmentID and LessonID
            // Get Attempt 
            this.Attempt = ORM.Attempt.Get(attemptId);
            if (this.Attempt is null)
            {
                return;
            }
            else
            {
                LoadState = 1; // attempt loaded

                // Get Assignment / Assessment   
                this.Assignment = ORM.Assignment.Get(this.Attempt.AssignmentId);
                if (this.Assignment is null)
                {
                    return;
                }
                else
                {
                    LoadState = 2; //assignment loaded

                    this.Lesson = ORM.Lesson.Get(this.Assignment.LessonId);

                    if (this.Lesson is null)
                    {
                        return;
                    }
                    else
                    {
                        LoadState = 3; // Lesson loaded

                        this.Assessment = ORM.Assessment.Get(this.Attempt.AssessmentId);

                        if (this.Assessment is null)
                        {
                            return;
                        }
                        else
                        {
                            LoadState = 4; // Assessment loaded

                            // And load Reviwwer Ratings 
                            this.ReviewerRating = ORM.ReviewerRating.GetByAttempt(Attempt.Id);

                            // TODO: And Peer Ratings
                            // this.PeerRatings = ORM.ReviewerRating.GetByAttempt(Attempt.Id);

                        }
                    }
                }
            }
        }

        /// <summary>
        ///  delete the draft assignment, if any.
        /// </summary>
        public void DeleteDraft()
        {
            if (Attempt != null && Assignment != null && Assignment.Status == AssignmentStatus.InProgress)
            {
                if (Attempt.Id > 0)
                    ORM.Attempt.Delete(Attempt.Id);
                Attempt = null;
                Assignment.Status = AssignmentStatus.Assigned;
                ORM.Assignment.Upsert(Assignment);
            }
        }
    }
}