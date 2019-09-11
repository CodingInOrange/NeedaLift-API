using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NeedALiftAPI.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace NeedALiftAPI.Services
{
    public interface IUserService
    {
        Users Authenticate(string username, string password);
        List<Users> Get();
        Users Get(string id);
        Users Create(Users user, string password);
        void Update(Users user, string password = null);
        void Delete(string id);
    }
    public class UserService : IUserService
    {
        private readonly IMongoCollection<Users> _users;

        public UserService(INeedALiftDBSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DBName);
            _users = database.GetCollection<Users>(settings.CollectionName3);
        }

        public Users Authenticate(string userId, string password)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password))
                return null;

            var user = _users.Find(x => x.UserId == userId).FirstOrDefault();

            // check if username exists
            if (user == null)
                return null;

            // check if password is correct
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            // authentication successful
            return user;
        }

        public List<Users> Get() =>
            _users.Find(lift => true).ToList();

        public Users Get(string id) =>
          _users.Find(user => user.UserId == id).FirstOrDefault();

        public Users Create(Users user, string password)
        {
            // validation
            if (string.IsNullOrWhiteSpace(password))
                throw new Exception("Password is required");

            //if (_users.Find(x => x.UserId == user.UserId) != null)
            //    throw new Exception("Username \"" + user.UserId + "\" already exists");

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _users.InsertOne(user);

            return user;
        }

        public void Update(Users userParam, string password = null)
        {
            var user = _users.Find(x => x.UserId == userParam.UserId).FirstOrDefault();

            if (user == null)
                throw new Exception("User not found");

            if (userParam.UserId != user.UserId)
            {
                // username has changed so check if the new username is already taken
                if (_users.Find(x => x.UserId == user.UserId) != null)
                    throw new Exception("Username \"" + user.UserId + "\" already exists");
            }

            // update user properties
            user.FName = userParam.FName;
            user.LName = userParam.LName;
            user.UserId = userParam.UserId;

            // update password if it was entered
            if (!string.IsNullOrWhiteSpace(password))
            {
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash(password, out passwordHash, out passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }

            _users.ReplaceOne(x => x.UserId == userParam.UserId, user);
        }

        public void Delete(string id)
        {
            var user = _users.Find(x => x.UserId == id);
            if (user != null)
            {
                           _users.DeleteOne(lift => lift.Id == id);
            }
        }

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
            if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
            if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }
    }
}
