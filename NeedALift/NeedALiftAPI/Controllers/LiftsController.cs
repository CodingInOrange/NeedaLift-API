using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NeedALiftAPI.Models;
using NeedALiftAPI.Services;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NeedALiftAPI.Controllers
{
    [Route("api/lifts")]
    [ApiController]
    public class LiftsController : ControllerBase
    {
        private readonly LiftService _liftservice;

        public LiftsController(LiftService service)
        {
            _liftservice = service;
        }

        [HttpGet]
        public ActionResult<List<RequestLift>> Get() =>
            _liftservice.Get();

        [HttpGet("{id:length(24)}",Name = "GetLift")]
        public ActionResult<RequestLift> Get(string id)
        {
            var lift = _liftservice.Get(id);

            if(lift == null)
            {
                return NotFound();
            }

            return lift;
        }

       // [Route("api/lifts/")]
        [HttpPost]
        public ActionResult<RequestLift> Create([FromBody]RequestLift lift)
        {
            _liftservice.Create(lift);

            return CreatedAtRoute("GetLift", new { id = lift.Id.ToString() }, lift);
        }


        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, RequestLift liftIn)
        {
            var book = _liftservice.Get(id);

            if (book == null)
            {
                return NotFound();
            }

            _liftservice.Update(id, liftIn);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            var lift = _liftservice.Get(id);

            if (lift == null)
            {
                return NotFound();
            }

            _liftservice.Remove(lift.Id);

            return NoContent();
        }
    }
}
