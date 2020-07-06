using System;
using System.IO;

using SQLite;
using SpotOn.DAL.Models;

namespace SpotOn.DAL
{

    /// <summary>
    /// this is an ORM factory
    /// </summary>
    public class ORM
    {
        static LessonORM _lesson;
        public static LessonORM Lesson
        {
            get
            {
                _lesson ??= new LessonORM();
                return _lesson;
            }
        }

        static AssignmentORM _assignment;
        public static AssignmentORM Assignment
        {
            get
            {
                _assignment ??= new AssignmentORM();
                return _assignment;
            }
        }

        static AssessmentORM _assessment;
        public static AssessmentORM Assessment
        {
            get
            {
                _assessment ??= new AssessmentORM();
                return _assessment;
            }
        }


        static AttemptORM _attempt;
        public static AttemptORM Attempt
        {
            get
            {
                _attempt ??= new AttemptORM();
                return _attempt;
            }
        }

        static UserORM _user;
        public static UserORM User
        {
            get
            {
                _user ??= new UserORM();
                return _user;
            }
        }


        static ReviewerRatingORM _reviewerRating;
        public static ReviewerRatingORM ReviewerRating
        {
            get
            {
                _reviewerRating ??= new ReviewerRatingORM();
                return _reviewerRating;
            }
        }

        static SettingsORM _settings;
        public static SettingsORM  Settings
        {
            get
            {
                _settings ??= new SettingsORM();
                return _settings;
            }
        }

        static NotificationORM _notification;
        public static NotificationORM Notification
        {
            get
            {
                _notification ??= new NotificationORM();
                return _notification;
            }
        }
        
        static ArrayTableORM _arrayTable;
        public static ArrayTableORM ArrayTable
        {
            get
            {
                _arrayTable ??= new ArrayTableORM();
                return _arrayTable;
            }
        }

        static TranscriptORM _transcript;
        public static TranscriptORM  Transcript
        {
            get
            {
                _transcript ??= new TranscriptORM();
                return _transcript;
            }
        }

    }
}