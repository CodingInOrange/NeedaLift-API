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

        [HttpGet,Route("Users")]
        public ActionResult<List<Users>> GetUser() =>
           _userService.Get();

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

        [HttpGet(template: "{from}/{to}")]
        public async Task<IEnumerable<RequestLift>> Get(string from, string to)
        {
            var lift = _liftservice.Get(from, to);

            return await lift ?? new List<RequestLift>();
        }

        [HttpGet("{userId}"),Route("UserLifts")]
        public async Task<IEnumerable<RequestLift>> UserLifts(RequestLift uId)
        {
            var userlifts = _liftservice.GetUserLifts(uId.userId);
            return await userlifts ?? new List<RequestLift>();
        }

        [HttpPost]
        public ActionResult<RequestLift> Create([FromBody]RequestLift lift)
        {
            _liftservice.Create(lift);

            return CreatedAtRoute("GetLift", new { id = lift.Id.ToString() }, lift);
        }

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

        [HttpPut("{id:length(24)}"),Route("UpdateLift")]
        public IActionResult Update(string id, RequestLift liftIn)
        {
            var lift = _liftservice.Get(id);

            if (lift == null)
            {
                return NotFound();
            }

            _liftservice.Update(id, liftIn);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}"),Route("DeleteLift")]
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
