using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using SQLite;

namespace SpotOn.DAL.Models
{

    public enum UserStatus
    {
        Active = 0,
        Disabled = -1
    }

    public enum UserRole
    {
        Learner = 0,
        Supervisor = 1,
        Admin = 9, // admin for current tenant 
        Superuser = 10, // super admin (admin for all tenants)
    }
    public enum UserSource
    {
        Local = 0,
        Google = 1,
        Microsoft = 2, 
        Facebook = 3,
        Twitter = 4, 
        Apple = 5
    }

    [Table("User")]
    public class User : IModel
    {
        public const string SYSTEM_USER = "SYSTEM";

        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; } = -1;

        public string OrgId { get; set; }

        public string FriendlyName { get; set; }
        public string FirstName { get; set; }
        public string LasttName { get; set; }

        public string UserName { get; set; }
        public string Title { get; set; }

        public string Password { get; set; }

        /// <summary>
        /// if null, the source is local.
        /// Otherwise, it is 
        /// </summary>
        public UserSource Source { get; set; } = UserSource.Local;


        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// 0 : male
        /// 1 : female
        /// </summary>
        public int Gender { get; set; }

        [Ignore]
        public bool IsMale { get { return Gender == 0; } }
        [Ignore]
        public bool IsFemale { get { return Gender == 1; } }

        /// <summary>
        /// Learner / Supervisor 
        /// </summary>
        public UserRole Role { get; set; } = UserRole.Learner;


        public UserStatus Status { get; set; } = UserStatus.Active;

        /// <summary>
        /// This is here so that SyncFusion controls can bind ... 
        /// </summary>
        //[Ignore]
        //public int IntStatus
        //{
        //    get
        //    {
        //        return (int)this.Status;
        //    }
        //    set
        //    {
        //        // this goes crazy if value is not a member of the UserStatus enums...
        //        Status = (UserStatus)value;
        //    }
        //}

        public User Clone()
        {
            return new User()
            {
                CreatedDate = this.CreatedDate,
                FriendlyName = this.FriendlyName,
                Gender = this.Gender,
                Id = this.Id,
                Title = this.Title,
                Password = this.Password,
                Role = this.Role,
                Status = this.Status,
                UserName = this.UserName
            };
        }
    }

}