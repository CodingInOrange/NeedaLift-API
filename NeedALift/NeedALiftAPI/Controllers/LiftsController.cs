using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NeedALiftAPI.Models;
using NeedALiftAPI.Services;
using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;

namespace NeedALiftAPI.Controllers
{
    [Route("api/lifts")]
    [ApiController]
    public class LiftsController : ControllerBase
    {
        private readonly LiftService _liftservice;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly INeedALiftDBSettings _settings;

        public LiftsController(LiftService service, IUserService uService, IMapper mapper, IOptions<NeedALiftDBSettings> settings)
        {
            _liftservice = service;
            _userService = uService;
            _mapper = mapper;
            _settings = settings.Value;
        }

        [HttpGet]
        public ActionResult<List<RequestLift>> Get() =>
            _liftservice.Get();

        //[Authorize]
        [HttpGet,Route("Users")]
        public ActionResult<List<Users>> GetUser() =>
           _userService.Get();

        //[Authorize]
        [HttpPost("{id:length(24)}",Name = "GetLift"),Route("GetLift") ]
        public ActionResult<RequestLift> Get([FromBody]RequestLift id)
        {
            var lift = _liftservice.Get(id.Id);

            if(lift == null)
            {
                return NotFound();
            }

            return lift;
        }
        [AllowAnonymous]
        [HttpGet(template: "{from}/{to}")]
        public async Task<IEnumerable<RequestLift>> Get(string from, string to)
        {
            var lift = _liftservice.Get(from, to);

            return await lift ?? new List<RequestLift>();
        }

        //[Authorize]
        [HttpPost,Route("Notification")]
        public async Task<IEnumerable<LiftConfirmation>> Notification([FromBody]UsersDTO id)
        {
            var notification = _liftservice.Notification(id.UserId);

            return await notification ?? new List<LiftConfirmation>();
        }

        //[Authorize]
        [HttpPost, Route("AcceptedLifts")]
        public IEnumerable<RequestLift> Notification1([FromBody]UsersDTO id)
        {
            var notification = _liftservice.GetAcceptedLifts(id.UserId);
            var noti = notification.Result.ToList();


            return _liftservice.Get(noti);
            //return await notification ?? new List<LiftConfirmation>();
        }

        [HttpPost, Route("RateLifts")]
        public async Task<IEnumerable<LiftConfirmation>> LiftToRate(UsersDTO user)
        {
            var lift = _liftservice.LiftsToRate(user.UserId);
            return await lift ?? new List<LiftConfirmation>();
        }

        [HttpPost,Route("Ratings")]
        public IActionResult Rating(LiftConfirmation lift)
        {
            var request = _liftservice.Get(lift);

            if(request.RequestedRating == "Yes" && request.UserIdRequested == lift.UserIdRequested)
            {
                return Ok(new { message = "You have already rated this user" });
            }
            else if(request.CreatedRating == "Yes" && request.UserIdCreated == lift.UserIdCreated)
            {
                return Ok(new { message = "You have already rated this user" });
            }

            _liftservice.UpdateRating(lift);

            return Ok(new { message = "Thank you for rating the user!" });
        }
        //[Authorize]
        [HttpPost("{userId}"),Route("UserLifts")]
        public async Task<IEnumerable<RequestLift>> UserLifts(RequestLift uId)
        {
            var userlifts = _liftservice.GetUserLifts(uId.userId);
            return await userlifts ?? new List<RequestLift>();
        }

        //[Authorize]
        [HttpPost]
        public ActionResult<RequestLift> Create([FromBody]RequestLift lift)
        {
            _liftservice.Create(lift);

            return CreatedAtRoute("GetLift", new { id = lift.Id.ToString() }, lift);
        }

        //[AllowAnonymous]
        [HttpPost("Register"),Route("Register")]
        public IActionResult Create([FromBody]UsersDTO userdto)
        {
            var user = _mapper.Map<Users>(userdto);
            var exist = _userService.Get(userdto.UserId.ToString());
            try
            {
                if(exist != null)
                {
                    return BadRequest(new { message = "User already exists" });
                }
                _userService.Create(user, userdto.Password);
                return Authenticate(userdto);
                
            }
            catch(Exception e)
            {
                return BadRequest(new { message = "User already exists" });
            }
        }

        //[Authorize]
        [HttpPut,Route("UpdateUser")]
        public IActionResult UpdateUser(UsersDTO userdto)
        {
            var user = _mapper.Map<Users>(userdto);
            try
            {
                var exist = _userService.Get(userdto.UserId.ToString());

                if (_userService.Authenticate(userdto.UserId, userdto.OldPassword) == null)
                {
                    return BadRequest(new { message = "Old password is incorrect" });
                }
                _userService.Update(user, userdto.Password);
            }
            catch
            {
                return BadRequest(new { message = "User does not exist" });
            }

            return Ok(new { message = "Updated"});
        }

        //[AllowAnonymous]
        [HttpPost("authenticate"),Route("Authentication")]
        public IActionResult Authenticate([FromBody]UsersDTO userDto)
        {
            var user = _userService.Authenticate(userDto.UserId, userDto.Password);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_settings.Secret);
            var date = DateTime.UtcNow.AddDays(7);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = date.AddTicks(-(date.Ticks % TimeSpan.TicksPerSecond)),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // return basic user info (without password) and token to store client side
            return Ok(new
            {
                Id = user.Id,
                UserId = user.UserId,
                FName = user.FName,
                LName = user.LName,
                Token = tokenString,
                TokenExpiry = tokenDescriptor.Expires.Value
        }); 
        }

        //[Authorize]
        [HttpPut("{id:length(24)}"),Route("UpdateLift")]
        public IActionResult Update(RequestLift liftIn)
        {
            var lift = _liftservice.Get(liftIn.Id);

            if (lift == null)
            {
                return NotFound();
            }

            _liftservice.Update(liftIn);

            return Ok(new { message = "Lift updated succesfully!" });
        }

        [HttpPut,Route("ConfirmLiftRequest")]
        public IActionResult Update(LiftConfirmation liftIn)
        {
            var lift = _liftservice.Get(liftIn);

            if(liftIn.Requested =="Decline")
            {
                _liftservice.RemoveConf(liftIn.Id);

                return Ok(new { message = "Declined lift request" });
            }

            _liftservice.Update(liftIn);

            if(_liftservice.UpdatePeople(liftIn.LiftId) == null)
            {
                return Ok(new { message = "You do not have any space left to accept this lift" });
            }

            return Ok(new { message = "You have accepted the lift request!" }); 
        }
        //[Authorize]
        [HttpDelete,Route("DeleteLift")]
        public IActionResult Delete(RequestLift id)
        {
            var lift = _liftservice.Get(id.Id);

            if (lift == null)
            {
                return NotFound();
            }

            _liftservice.Remove(lift.Id);
            _liftservice.RemoveEntConf(lift.Id);

            return NoContent();
        }

       // [Authorize]
        [HttpPost,Route("RequestLift")]
        public IActionResult RequestLift(LiftConfirmation lift)
        {
            if (_liftservice.Request(lift) == null)
            {
                return Ok(new { message = "You have already requested this lift, you may also not request your own lift" });
            }
            else
            {
                return Ok(new { message = "Lift request sent!" });
            }

           
        }
    }
}
