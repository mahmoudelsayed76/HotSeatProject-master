using Cointeco;
using System;
using System.IO;
using SpotOn.DAL.Models;
using SQLite;

namespace SpotOn.DAL
{

    public class UserORM : ORMBase<User>
    {

        public UserORM() : base("User") { }

        public User Get(string username)
        {
            string query = $"Select * from {TableName} Where UserName = '{username}'";
            var users = db.Query<User>(query);
            if (users.Count >= 1) return users[0];
            else return null;
        }

        public string GetName(int id)
        {
            var user = Get(id);
            return (user?.FriendlyName ?? "(null)");
        }

        public int Upsert(User user)
        {
            int retVal = user.Id;

            try
            {
                // look for the user by id (first) and username (next) 
                var xstgUser = this.Get(user.Id) ?? this.Get(user.UserName) ?? this.Get(user.FriendlyName);

                if (xstgUser is null)
                {
                    db.Insert(user); // Insert() sets the Id field in the object and returns it (int)
                }
                else
                {
                    user.Id = xstgUser.Id;
                    db.Update(user);
                }
                retVal = user.Id;
            }
            catch (Exception e)
            {
                CommonBase.Logger.Error($"User.Save Exception: {e.Message}");
            }
            return retVal;
        }
        public static User New(string username, string orgId = null, UserRole role =  UserRole.Learner,
            string password = null, string friendlyname = null, int gender = 0 , int status = 0)
        {
            var user = new  User()
            {
                Id = -1,
                CreatedDate = DateTime.UtcNow,
                UserName = username,
                FriendlyName = friendlyname?? username,
                Password = password,
                Gender = gender,
                Role = role,
                Status = (UserStatus)status 
            };

            // insert it 
            ORM.User.Upsert(user);
            // return user object 
            return user; 
        }

        public int Upsert(int id, string orgId, DateTime createdDate, string username, string password,
            string friendlyname, UserRole role, int gender, int status = 0)
        {
            return Upsert(new User()
            {
                Id = id,
                CreatedDate = createdDate,
                FriendlyName = friendlyname,
                UserName = username,
                Password = password,
                Gender = gender,
                Role = role,
                Status = (UserStatus)status // 0 = active, -1 is disabled 
            });
        }

    }
}