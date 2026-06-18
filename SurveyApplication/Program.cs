using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Interface;
using Repository;

using SurveyBusinessLayer.Interface;
using SurveyBusinessLayer;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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
//builder.Services.AddScoped<IQuestionService, QuestionService>();
//builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
//builder.Services.AddScoped<IChoiceService, ChoiceService>();
//builder.Services.AddScoped<IChoiceRepository, ChoiceRepository>();
//builder.Services.AddScoped<IResponseService, ResponseService>();
//builder.Services.AddScoped<IResponseRepository, ResponseRepository>();
//builder.Services.AddScoped<IAnswerService, AnswerService>();
//builder.Services.AddScoped<IAnswerRepository, AnswerRepository>();

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


// ... before app.UseAuthorization()
app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();