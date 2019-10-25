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
            _users = database.GetCollection<Users>(settings.CollectionName3);
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

                foreach (var user in requestLifts)
                {
                    var userinfo = _users.Find(x => x.UserId == lift.UserIdCreated || x.UserId == lift.UserIdRequested).FirstOrDefault();
                    user.PhoneNum = userinfo.PhoneNum;
                    user.Rating = userinfo.Rating;
                }
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
            var user = _users.Find(x => x.UserId == request.userId).FirstOrDefault();
            request.Rating = user.Rating;

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

        //public void UpdatePeople(string id)
        //{
        //    var lift = _lifts.Find(x => x.Id == id).FirstOrDefault();
        //}
        public void Remove(RequestLift liftIn) =>
            _lifts.DeleteOne(lift => lift.Id == liftIn.Id);

        public void RemoveConf(string liftIn) =>
        _requests.DeleteOne(lift => lift.Id == liftIn);

        public void Remove(string id) =>
            _lifts.DeleteOne(lift => lift.Id == id);

        public async Task<IEnumerable<RequestLift>> Get(string from, string to)
        {
            var rated = new List<RequestLift>();
            try
            {
                var query = await _lifts.Find(lift => lift.From.Contains(from) && lift.To.Contains(to)).ToListAsync();

                foreach(var item in query)
                {
                    var user = _users.Find(x => x.UserId == item.userId).FirstOrDefault();

                    item.Rating = user.Rating;

                    rated.Add(item);
                }

                return rated;
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
                var notify = _requests.Find(x => x.UserIdCreated == id).ToListAsync();
                return await notify;
            }
            catch(Exception e)
            {
                throw e;
            }

        }

        public async Task<IEnumerable<LiftConfirmation>> LiftsToRate(string id)
        {
            var lift = new List<LiftConfirmation>();

            try
            {
                var notify = await _requests.Find(x => x.UserIdCreated == id || x.UserIdRequested == id ).ToListAsync();


                    foreach (var item in notify)
                    {

                        if ((item.Date != null && Convert.ToDateTime(item.Date) < DateTime.UtcNow ) && item.Accepted == "Yes")
                        {
                            if(item.UserIdRequested == id && item.CreatedRating != "Yes")
                            {
                                var user = _users.Find(x => x.UserId == item.UserIdCreated).FirstOrDefault();
                                item.FName = user.FName;
                                item.LName = user.LName;
                                item.UserIdRequested = null;
                                lift.Add(item);
                        }
                            else if(item.UserIdCreated == id  && item.RequestedRating != "Yes" )
                            {
                                item.UserIdCreated = null;
                                lift.Add(item);
                            }
                            
                        }
                    }
                return lift;
            }
            catch (Exception e)
            {
                throw e;
            }

        }


        public void UpdateRating(LiftConfirmation lift)
        {
           if (_requests.Find(x => x.Id == lift.Id && x.RequestedRating == "Yes" && x.CreatedRating == "Yes").FirstOrDefault() == null)
            {
                if (lift.RequestedRating != null)
                {
                    var user = _users.Find(x => x.UserId == lift.UserIdRequested).FirstOrDefault();
                    var rating = user.Rating;

                    if (user == null)
                        throw new Exception("User not found");

                    if (rating == null)
                    {
                        // user.Rating = Convert.ToString(lift.RequestedRating);
                        rating = Convert.ToString(lift.RequestedRating);
                    }
                    else
                    {
                        rating = Convert.ToString((Convert.ToDouble(user.Rating) + Convert.ToDouble(lift.RequestedRating)) / 2);
                    }


                    var filter = Builders<Users>.Filter.Eq("UserId", lift.UserIdRequested);
                    var update = Builders<Users>.Update.Set("Rating", rating);
                    _users.UpdateOne(filter, update);

                    var filter1 = Builders<LiftConfirmation>.Filter.Eq("Id", lift.Id);
                    var update1 = Builders<LiftConfirmation>.Update.Set("RequestedRating", "Yes");
                    _requests.UpdateOne(filter1, update1);
                }
                else if (lift.CreatedRating != null)
                {
                    var user = _users.Find(x => x.UserId == lift.UserIdCreated).FirstOrDefault();
                    var rating = user.Rating;

                    if (user == null)
                        throw new Exception("User not found");

                    if (rating == null)
                    {
                        rating = Convert.ToString(lift.CreatedRating);
                    }
                    else
                    {
                        rating = Convert.ToString((Convert.ToDouble(user.Rating) + Convert.ToDouble(lift.CreatedRating)) / 2);
                    }


                    var filter = Builders<Users>.Filter.Eq("UserId", lift.UserIdCreated);
                    var update = Builders<Users>.Update.Set("Rating", rating);
                    _users.UpdateOne(filter, update);

                    var filter1 = Builders<LiftConfirmation>.Filter.Eq("Id", lift.Id);
                    var update1 = Builders<LiftConfirmation>.Update.Set("CreatedRating", "Yes");
                    _requests.UpdateOne(filter1, update1);
                }
            }
           else
            {
                RemoveConf(lift.Id);
                Remove(lift.LiftId);
            }

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



    }
}
