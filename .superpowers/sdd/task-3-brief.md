### Task 3: Implement AuthController for Admin JWT Token Generation
**Files:**
- Create: `FTD.Api/Controllers/AuthController.cs`

**Interfaces:**
- Consumes: Identity `SignInManager<IdentityUser>`, Identity `UserManager<IdentityUser>`
- Produces: API endpoint `POST /api/auth/login` returning token.

- [ ] **Step 1: Create AuthController**
  Create `FTD.Api/Controllers/AuthController.cs`:
  ```csharp
  using Microsoft.AspNetCore.Identity;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.IdentityModel.Tokens;
  using System.IdentityModel.Tokens.Jwt;
  using System.Security.Claims;
  using System.Text;

  namespace FTD.Api.Controllers
  {
      [ApiController]
      [Route("api/[controller]")]
      public class AuthController : ControllerBase
      {
          private readonly UserManager<IdentityUser> _userManager;
          private readonly SignInManager<IdentityUser> _signInManager;
          private readonly IConfiguration _config;

          public AuthController(
              UserManager<IdentityUser> userManager,
              SignInManager<IdentityUser> signInManager,
              IConfiguration config)
          {
              _userManager = userManager;
              _signInManager = signInManager;
              _config = config;
          }

          [HttpPost("login")]
          public async Task<IActionResult> Login([FromBody] LoginRequest request)
          {
              if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                  return BadRequest("البريد الإلكتروني وكلمة المرور مطلوبة");

              var user = await _userManager.FindByEmailAsync(request.Email);
              if (user == null)
                  return Unauthorized("البريد الإلكتروني أو كلمة المرور غير صحيحة");

              var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
              if (!result.Succeeded)
                  return Unauthorized("البريد الإلكتروني أو كلمة المرور غير صحيحة");

              var roles = await _userManager.GetRolesAsync(user);
              if (!roles.Contains("Admin"))
                  return Forbid("غير مصرح بالدخول لغير المسؤولين");

              var token = GenerateJwtToken(user, roles);
              return Ok(new
              {
                  token,
                  email = user.Email,
                  roles
              });
          }

          private string GenerateJwtToken(IdentityUser user, IList<string> roles)
          {
              var secret = _config["JwtSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured.");
              var issuer = _config["JwtSettings:Issuer"];
              var audience = _config["JwtSettings:Audience"];
              var expiryMinutesStr = _config["JwtSettings:ExpiryMinutes"];
              var expiryMinutes = double.TryParse(expiryMinutesStr, out var mins) ? mins : 120;

              var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
              var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

              var claims = new List<Claim>
              {
                  new Claim(ClaimTypes.NameIdentifier, user.Id),
                  new Claim(ClaimTypes.Email, user.Email ?? "")
              };

              foreach (var role in roles)
                  claims.Add(new Claim(ClaimTypes.Role, role));

              var tokenDescriptor = new SecurityTokenDescriptor
              {
                  Subject = new ClaimsIdentity(claims),
                  Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
                  SigningCredentials = creds,
                  Issuer = issuer,
                  Audience = audience
              };

              var tokenHandler = new JwtSecurityTokenHandler();
              var token = tokenHandler.CreateToken(tokenDescriptor);
              return tokenHandler.WriteToken(token);
          }

          public class LoginRequest
          {
              public string Email { get; set; } = "";
              public string Password { get; set; } = "";
          }
      }
  }
  ```

- [ ] **Step 2: Verify Build**
  Run: `dotnet build FTD.Api/FTD.Api.csproj`
  Expected: Build succeeds with 0 errors.

- [ ] **Step 3: Commit Changes**
  Run:
  ```bash
  git add FTD.Api/Controllers/AuthController.cs
  git commit -m "feat: implement AuthController with JWT token generation for Admin users"
  ```
