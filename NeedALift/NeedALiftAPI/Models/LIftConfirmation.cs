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
        public string LiftId { get; set; }
        public string UserIdRequested { get; set; }
        public string UserIdCreated { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public string Accepted { get; set; }
        public string Requested { get; set; }
        public string Seen { get; set; }
    }

    
}
