using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Aareon.Business.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Aareon.Api.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IPersonService _service;
        private readonly ILogger<AuthController> _logger;
        private readonly AuthSettings _auth;

        public AuthController(IPersonService service, ILogger<AuthController> logger, IOptions<AuthSettings> auth)
        {
            _service = service;
            _logger = logger;
            _auth = auth.Value;
        }

        [HttpPost]
        [Route("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(string surname)
        {
            var log = $"Login attempt ({DateTime.Now.ToLongTimeString()})";
            var user = await _service.GetBySurname(surname);
            if (user == null)
            {
                _logger.LogWarning($"{surname}: Unsuccessful {log}");
                return NotFound();
            }
            
            // build JWT
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_auth.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var jwtValidity = DateTime.Now.AddMinutes(20);
            const string issuer = "https://localhost:5001";
            
            var claims = new List<Claim>
            {
                new (ClaimTypes.Sid, user.Id.ToString()),
                new (ClaimTypes.Name, user.FullName)
            };
            if (user.IsAdmin)
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
 
            var token = new JwtSecurityToken(issuer,
                issuer,
                expires: jwtValidity,
                signingCredentials: creds,
                claims: claims);
 
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            _logger.LogTrace($"{user.FullName} ({user.Id}): Successful {log}");
            return Ok(jwt);
        }
    }
}