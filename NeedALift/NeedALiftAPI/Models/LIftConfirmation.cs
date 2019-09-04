using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NeedALiftAPI.Models
{
    public class LiftConfirmation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string UserIdCreated { get; set; }
        public string UserIdRequested { get; set; }

    }
}
