using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MiniChatApp2.Data;
using MiniChatApp2.Model;

namespace MiniChatApp2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly MiniChatApp2Context _context;
        private readonly string _jwtSecretKey = "your_secret_key_here";
        private readonly IConfiguration _configuration;
        public UsersController(MiniChatApp2Context context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/Users
        /* [HttpGet]
         public async Task<ActionResult<IEnumerable<User>>> GetUser()
         {
             if (_context.User == null)
             {
                 return NotFound();
             }
             return await _context.User.ToListAsync();
         }*/
        [HttpGet]
       public async Task<ActionResult<IEnumerable<User>>> GetUserList()
        {
            // Get the current user's email from the claims
            var currentUser = HttpContext.User;
            var currentUserEmail = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;

            var id = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return Unauthorized(new { message = "Unauthorized access" });
            }
            // Retrieve the user list excluding the current user
            var users = await _context.User
          .Where(u => u.Email != currentUserEmail) // Exclude the current user
          .Select(u => new
          {
              id = u.Id,
              name = u.Name,
              email = u.Email
          })
          .ToListAsync();

            // Return the user list
            return Ok(users);
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            if (_context.User == null)
            {
                return NotFound();
            }
            var user = await _context.User.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            if (_context.User == null)
            {
                return Problem("Entity set 'MiniChatApp2Context.User'  is null.");
            }
            _context.User.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (_context.User == null)
            {
                return NotFound();
            }
            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.User.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpPost("register")]
        public IActionResult Register(User model)
        {
            // Check if the model is valid
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "Registration failed due to validation errors" });
            }

            // Check if the email is already registered
            if (_context.User.Any(u => u.Email == model.Email))
            {
                return Conflict(new { error = "Registration failed because the email is already registered" });
            }

            // Generate a unique user ID (using auto-incrementing int)
            // Since EF Core and the database handle this, we don't need to manually generate it.
            // The UserId property will be auto-generated in the database.

            // Hash the password securely before storing it in the database
            var hashedPassword = HashPassword(model.Password);

            // Create the user object
            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                Password = hashedPassword // Store the hashed password in the database.
            };

            // Add the user to the database using EF Core
            _context.User.Add(user);
            _context.SaveChanges();

            // Return successful response
            return Ok(new
            {
                userId = user.Id,
                name = user.Name,
                email = user.Email,
                hashedPassword = hashedPassword
            });
        }

        [HttpPost("login")]
        public IActionResult Login(User model)
        {
            // Check if the model is valid
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "Login failed due to validation errors" });
            }

            // Find the user with the given email
            var user = _context.User.SingleOrDefault(u => u.Email == model.Email);

            // Check if the user exists and the password matches
            if (user == null || !VerifyPassword(model.Password, user.Password))
            {
                return Unauthorized(new { error = "Login failed due to incorrect credentials" });
            }

            // Generate and return the JWT token
            var token = GenerateJwtToken(user.Id, user.Name, user.Email);

            // Return successful response with the JWT token and user profile details
            return Ok(new
            {
                token,
                profile = new
                {
                    id = user.Id,
                    name = user.Name,
                    email = user.Email
                }
            });
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var hashedPassword = Convert.ToBase64String(hashedBytes);
                return string.Equals(hashedPassword, passwordHash);
            }
        }

       /* private string GenerateJwtToken(int id, string name, string email)
        {
            var claims = new List<Claim>
    {
        new Claim("id", id.ToString()),
        new Claim("name", name),
        new Claim("email", email)
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: signIn);

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenString;
        }*/


        private string GenerateJwtToken(int id, string name, string email)
        {
            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Email, email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: signIn);


            string Token = new JwtSecurityTokenHandler().WriteToken(token);

            return Token;
        }

        private string HashPassword(string password)
        {
            // Use a secure hashing algorithm, such as SHA-256 or bcrypt.
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
        private bool UserExists(int id)
        {
            return (_context.User?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
