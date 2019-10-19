using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using Abp.Notifications;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using NeedALiftAPI.Models;
    
namespace NeedALiftAPI.Services
{
    public class LiftService
    {
        private readonly IMongoCollection<RequestLift> _lifts;
        private readonly IMongoCollection<LiftConfirmation> _requests;
        private readonly IMongoCollection<Users> _users;
       // private readonly INotificationSubscriptionManager;



        public LiftService(INeedALiftDBSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DBName);

            _lifts = database.GetCollection<RequestLift>(settings.CollectionName1);
            _requests = database.GetCollection<LiftConfirmation>(settings.CollectionName2);
        }

        public List<RequestLift> Get() =>
            _lifts.Find(lift => true).ToList();

        public RequestLift Get(string id) =>
            _lifts.Find(lift => lift.Id == id).FirstOrDefault();

        public async Task<IEnumerable<RequestLift>> GetUserLifts(string id)
        {
            try
            {
                var query = _lifts.Find(lift => lift.userId == id);
                return await query.ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public RequestLift Create(RequestLift request)
        {
            _lifts.InsertOne(request);
            return request;
        }

        public LiftConfirmation Request(LiftConfirmation request)
        {
            var lift = _lifts.Find(request.Id);
            if (lift == null)
            {
                return null;
            }

            _requests.InsertOne(request);
            return request;
        }



        public void Update(RequestLift liftIn) =>
            _lifts.ReplaceOne(lift => lift.Id == liftIn.Id,liftIn);

        public void Remove(RequestLift liftIn) =>
            _lifts.DeleteOne(lift => lift.Id == liftIn.Id);

        public void Remove(string id) =>
            _lifts.DeleteOne(lift => lift.Id == id);

        public async Task<IEnumerable<RequestLift>> Get(string from, string to)
        {
            try
            {
                var query = _lifts.Find(lift => lift.From.Contains(from) && lift.To.Contains(to));
                return await query.ToListAsync();
            }
            catch(Exception e)
            {
                throw e;
            }
        }
    }
}
