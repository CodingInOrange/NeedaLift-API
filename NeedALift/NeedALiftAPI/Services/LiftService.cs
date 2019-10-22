using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public LiftConfirmation Get(LiftConfirmation liftIn) =>
            _requests.Find(lift => lift.Id == liftIn.Id).FirstOrDefault();

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
           // var lift = Get(request.LiftId);
           // if (lift == null)
           // {
           //     return null;
           // }
            if(request.UserIdRequested != request.UserIdCreated)
            {
                return null;
            }
            if(_requests.Find(x => x.UserIdRequested == request.UserIdRequested && x.LiftId == request.LiftId).FirstOrDefault()  == null)
            {
                _requests.InsertOne(request);
                return request;
            }
            else
            {
                return null;
            }
        }



        public void Update(RequestLift liftIn) =>
            _lifts.ReplaceOne(lift => lift.Id == liftIn.Id,liftIn);

        public void Update(LiftConfirmation confirmation) =>
            _requests.ReplaceOne(lift => lift.Id == confirmation.Id,confirmation);

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

        public async Task<IEnumerable<LiftConfirmation>> Notification(string id)
        {
            try
            {
                var notify = _requests.Find(x => x.UserIdCreated == id);
                return await notify.ToListAsync();
            }
            catch(Exception e)
            {
                throw e;
            }

        }


        //public async Task<IEnumerable<LiftConfirmation>> AcceptedLift(LiftConfirmation confirmation)
        //{
        //    try
        //    {
        //        var lift = _requests.Find(x => x.Id == confirmation.Id).FirstOrDefault();
        //        if(lift == null)
        //        {
        //            return null;
        //        }

                
        //    }
        //}

        //public async Task<IEnumerable<LiftConfirmation>> RatingNotification(LiftConfirmation liftConf)
        //{
        //    var user = _requests.Find(x => x.UserIdCreated == liftConf.UserIdCreated).FirstOrDefault();

        //    if (Convert.ToDateTime(liftConf.Date) < DateTime.Now)
        //    {
        //        return null;
        //    }



        //}


    }
}
