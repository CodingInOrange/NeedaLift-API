using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using NeedALiftAPI.Models;
    
namespace NeedALiftAPI.Services
{
    public class LiftService
    {
        private readonly IMongoCollection<RequestLift> _lifts;
        private readonly IMongoCollection<LiftConfirmation> _requests;

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

        public RequestLift Create(RequestLift request)
        {
            LiftConfirmation confirmation = new LiftConfirmation
            {
                Id = request.Id,
                UserIdCreated = request.userId
            };

            _lifts.InsertOne(request);
            _requests.InsertOne(confirmation);
            return request;
        }

        public void Update(string id, RequestLift liftIn) =>
            _lifts.ReplaceOne(lift => lift.Id == id,liftIn);

        public void Remove(RequestLift liftIn) =>
            _lifts.DeleteOne(lift => lift.Id == liftIn.Id);

        public void Remove(string id) =>
            _lifts.DeleteOne(lift => lift.Id == id);

        public List<RequestLift> SearchLifts(string from, string to) =>
            _lifts.Find(lift => lift.From == from && lift.To == to).ToList();

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
