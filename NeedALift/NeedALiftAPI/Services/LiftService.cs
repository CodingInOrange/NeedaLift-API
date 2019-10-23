using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
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

        public List<RequestLift> Get(List<LiftConfirmation> confirmation)
        {
            List<RequestLift> requestLifts = new List<RequestLift>();

            foreach(var lift in confirmation)
            {
                requestLifts.Add(Get(lift.LiftId));
            }

            return requestLifts;
        }


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
            if(request.UserIdRequested == request.UserIdCreated)
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

        //public RequestLift Rating(RequestLift lift)
        //{

        //}


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

        public void UpdateRating(LiftConfirmation lift)
        {
           if(lift.UserIdCreated != null)
            {
                var user = _users.Find(x => x.UserId == lift.UserIdRequested).FirstOrDefault();
                var rating = user.Rating;

                if (user == null)
                    throw new Exception("User not found");

               if(rating == null)
                {
                   // user.Rating = Convert.ToString(lift.RequestedRating);
                   rating = Convert.ToString(lift.RequestedRating);
                }
               else
                {
                    rating = Convert.ToString((Convert.ToDouble(user.Rating) + Convert.ToDouble(lift.RequestedRating)) / 2);
                }
                    

                var filter = Builders<Users>.Filter.Eq("userId", lift.UserIdRequested);
                var update = Builders<Users>.Update.Set("userId", lift.UserIdRequested);
                update = update.Set("Rating", rating);
                _users.UpdateOne(filter, update);
               // _lifts.DeleteOne(lift.LiftId);
            }
            //else if(lift.UserIdRequested != null)
            //{
            //    var user = _users.Find(x => x.UserId == lift.UserIdCreated).FirstOrDefault();

            //    if (user == null)
            //        throw new Exception("User not found");

            //    user.Rating = (user.Rating + lift.RequestedRating) / 2;
            //}

            

        }
        public async Task<IEnumerable<LiftConfirmation>> GetAcceptedLifts(string id)
        {
            try
            {
                var notify =  _requests.Find(x => x.UserIdRequested == id && x.Accepted == "Yes");

                return await notify.ToListAsync();
                

            }
            catch(Exception e)
            {
                throw e;
            }
        }

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
