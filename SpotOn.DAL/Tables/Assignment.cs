using System;
using SQLite;
namespace SpotOn.DAL.Models
{

    public enum AssignmentStatus
    {
        Assigned = 0,
        InProgress = 1,
        Submitted = 2,
        Reviewed = 3
    }

    [Table("Assignment")]
    public class Assignment
    {

        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public string OrgId { get; set; }

        /// <summary>
        /// foreign key to lesson id 
        /// </summary>
        [NotNull]
        public int LessonId { get; set; }

        /// <summary>
        /// Learner who takes the assingment
        /// </summary>
        public int AssigneeUserId { get; set; }
        
        /// <summary>
        /// Manager who reviews the assingment
        /// </summary>
        public int ReviewerUserId { get; set; }

        /// <summary>
        /// Manager who reviews the assingment
        /// </summary>
        public AssignmentStatus Status { get; set; }

        /// <summary>
        /// time assigned 
        /// </summary>
        public DateTime Date { get; set; }
        public DateTime DueDate { get; set; }

        public bool Completed { get; set; } = false;

        ///// <summary>
        ///// Assignee User Name
        ///// </summary>
        public string GetAssigneeName()
        {
            
            return ORM.User.GetName(this.AssigneeUserId);
        }

        ///// <summary>
        ///// Assignee User Name
        ///// </summary>
        public string GetReviewerName()
        {
            return ORM.User.GetName(this.ReviewerUserId);
        }
        /// <summary>
        /// Number of times the assigment has been attempted 
        /// </summary>
        public int Attempts { get; set; }

    }


}