using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NeedALiftAPI.Models
{
    public class RequestLift
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string userId { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public string From { get; set; }
        public string To { get; set; }

        public string Date { get; set; }
        public string Time { get; set; }
        public int People { get; set; }
        public string Cost { get; set; }
        public string Comment { get; set; }
        public string FromLatitude { get; set; }
        public string FromLongitude { get; set; }
        public string ToLatitude { get; set; }
        public string ToLongitude { get; set; }
    }
}
