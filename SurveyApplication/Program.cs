using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Interface;
using Repository;

using SurveyBusinessLayer.Interface;
using SurveyBusinessLayer;
using FluentValidation;
using SurveyApplication.Validation.Survey;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
// For FluentValidation one is fine the ASP.net will scan the assembly and find all validators 
builder.Services.AddValidatorsFromAssemblyContaining<SurveyCreaterDtoValidation>();
builder.Services.AddValidatorsFromAssemblyContaining<SurveyUpdateDtoValidation>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
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
        else if (originalError is Exception)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = originalError.Message });
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = "Something went wrong on our end." });
        }
    });
});




app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();