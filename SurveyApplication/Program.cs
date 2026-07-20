using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repository;
using Repository.Data;
using Repository.Interface;
using SurveyApplication.Authorization;
using SurveyApplication.Validation.Survey;
using SurveyBusinessLayer;
using SurveyBusinessLayer.Interface;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
// For FluentValidation one is fine the ASP.net will scan the assembly and find all validators 
builder.Services.AddValidatorsFromAssemblyContaining<SurveyCreaterDtoValidation>();
builder.Services.AddValidatorsFromAssemblyContaining<SurveyUpdateDtoValidation>();

builder.Services.AddSingleton<IAuthorizationHandler, EditDeleteResuorseHandler>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("AuthLimiter", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("ResponseLimiter", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // This tells Swagger that our API uses JWT Bearer authentication
    // through the HTTP Authorization header.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        // The name of the HTTP header where the token will be sent.
        Name = "Authorization",

        // Indicates this is an HTTP authentication scheme.
        Type = SecuritySchemeType.Http,

        // Specifies the authentication scheme name.
        // Must be exactly "Bearer" for JWT Bearer tokens.
        Scheme = "Bearer",

        // Optional metadata to describe the token format.
        BearerFormat = "JWT",

        // Specifies that the token is sent in the request header.
        In = ParameterLocation.Header,

        // Text shown in Swagger UI to guide the user.
        Description = "Enter: Bearer {your JWT token}"
    });

    // This tells Swagger that endpoints protected by [Authorize]
    // require the Bearer token defined above.
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                // Reference the previously defined "Bearer" security scheme.
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },


            // No scopes are required for JWT Bearer authentication.
            // This array is empty because JWT does not use OAuth scopes here.
            new string[] {}
        }
    });
});


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");


var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlServer(connectionString)
    .LogTo(Console.WriteLine, LogLevel.Information)
    .EnableSensitiveDataLogging()
    .Options;


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<ISurveyService, SurveyService>();
builder.Services.AddScoped<ISurveyRepository, SurveyRepository>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IResponseService, ResponseService>();
builder.Services.AddScoped<IResponseRepository, ResponseRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService , AuthService>();


// STEP 1 : CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("UserApiCorsPolicy", policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:7217",
                "http://localhost:5215"
            )
            // in production, when need to specify the exact origins that are allowed to access your API
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];


builder.Services.AddAuthentication(options =>
{
    // Force the app to use JWT Bearer tokens by default
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EditDeleteResuorse", policy =>
        policy.Requirements.Add(new EditDeleteResuorseRequirement()));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())        
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 1. Activate the global exception safety net
app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        context.Response.ContentType = "application/json";

        // 2. Grab the actual error that happened somewhere in the app
        var exceptionFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var originalError = exceptionFeature?.Error;

        // 3. Inspect what KIND of error it is, and choose the HTTP status code
        if (originalError is KeyNotFoundException)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(new { error = originalError.Message });
        }
        else if (originalError is ArgumentException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = originalError.Message });
        }
        else if (originalError is InvalidOperationException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = originalError.Message });
        }
        else if (originalError is FluentValidation.ValidationException valEx)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            var errors = valEx.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
            await context.Response.WriteAsJsonAsync(new { error = "Validation failed", details = errors });
        }
        else if(originalError is UnauthorizedAccessException)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = originalError.Message });
        }
        else if (originalError is Exception)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = originalError.Message, details = exceptionFeature?.Error?.InnerException?.Message });
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = "Something went wrong on our end." });
        }
    });
});

app.Use(async (context, next) =>
{
    await next();
    
    if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var path = context.Request.Path.ToString();

        app.Logger.LogWarning(
            "Forbidden access. UserId={UserId}, Path={Path}, IP={IP}",
            userId,
            path,
            ip
        );
    }
});


// APPLAY CORS POLICY AND HTTPS 
app.UseHttpsRedirection();
app.UseCors("UserApiCorsPolicy");

app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();

app.Run();