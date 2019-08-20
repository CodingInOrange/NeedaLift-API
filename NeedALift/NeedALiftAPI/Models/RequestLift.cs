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

        public string FName { get; set; }//
        public string SName { get; set; }//
        public string StartDest { get; set; }
        public string EndDest { get; set; }

        //date
        public string Time { get; set; }
        public int Space { get; set; }
        public string Cost { get; set; }
        public string Comment { get; set; }

    }
}
