using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace NeedALiftAPI.Models
{
    public class Users
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonRequired]
        //[EmailAddress]
        public string UserId { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public string Rating { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
    }

    public class UsersDTO
    {
        [Required]
        [EmailAddress]
        public string UserId { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string OldPassword { get; set; }
    }
}
