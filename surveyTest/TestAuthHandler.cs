using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;




namespace surveyTest
{
    public class TestAuthHandler
    {
        readonly HttpClient _client;
        readonly IConfiguration _configuration;
        public TestAuthHandler(IConfiguration configuration, HttpClient client)
        {
            _configuration = configuration;
            _client = client;
        }
        private string GenerateTestJwtToken(IConfiguration configuration, string userId, string role = "Creator")
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var jwtSettings = configuration.GetSection("JwtSettings");
            var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(20),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string AddAuthHeaderAdmin()
        {
            var fakeUserId = "7b0e14a2-9c3f-42a1-b8d6-5f8e02c1439b";
            var token = GenerateTestJwtToken(_configuration, fakeUserId, "Admin");
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            return fakeUserId;
        }

        public string AddAuthHeaderCreator()
        {
            var fakeUserId = "ca489e13-b5d2-4f77-ae19-8c2a30b49ef4";
            var token = GenerateTestJwtToken(_configuration, fakeUserId, "Creator");
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            return fakeUserId;
        }

        public string AddAuthHeaderRespondent()
        {
            var fakeUserId = "9c3f9e13-b5d2-4f77-ae19-8c2a30b49ef6";
            var token = GenerateTestJwtToken(_configuration, fakeUserId, "Respondent");
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            return fakeUserId;

        }
    }
}
