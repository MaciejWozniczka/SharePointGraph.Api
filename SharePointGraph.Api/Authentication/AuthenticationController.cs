namespace SharePointGraph.Api.Authentication;

public class AuthenticationController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly SignInManager<User> _signInManager;
    private readonly DataContext _db;
    public AuthenticationController(IConfiguration configuration, SignInManager<User> signInManager, DataContext db)
    {
        _configuration = configuration;
        _signInManager = signInManager;
        _db = db;
    }

    public class TokenAccess
    {
        public string? Token { get; set; }
    }

    [HttpPost("authenticate")]
    public async Task<ActionResult<TokenAccess>> Authenticate([FromBody] AuthenticationRequestBody authenticationRequestBody)
    {
        var user = await _db.Users
            .Where(u => u.UserName == authenticationRequestBody.UserName)
            .FirstOrDefaultAsync();

        if (user == null)
            return Unauthorized();

        var validationResult = await _signInManager.CheckPasswordSignInAsync(user, authenticationRequestBody.Password, false);

        if (!validationResult.Succeeded)
        {
            return Forbid();
        }

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Authentication:SecretKey"]));
        var signingCredentials = new SigningCredentials(
            securityKey, SecurityAlgorithms.HmacSha256);

        var claimsForToken = new List<Claim>();
        claimsForToken.Add(new Claim("sub", user.Email.ToString()));

        var jwtSecurityToken = new JwtSecurityToken(
            _configuration["Authentication:Issuer"],
            _configuration["Authentication:Audience"],
            claimsForToken,
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(1),
            signingCredentials);

        var tokenToReturn = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

        return new TokenAccess
        {
            Token = tokenToReturn
        };
    }
}