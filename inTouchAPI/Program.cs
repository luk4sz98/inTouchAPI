global using inTouchAPI.DataContext;
global using Microsoft.EntityFrameworkCore;
global using inTouchAPI.Models;
global using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
global using inTouchAPI.Dtos;
global using Microsoft.AspNetCore.Identity;
global using SendGrid;
global using AutoMapper;
global using inTouchAPI.Services;
global using Response = inTouchAPI.Helpers.Response;
global using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("DatabaseConnection")));
builder.Services.AddIdentityCore<User>(opt => opt.SignIn.RequireConfirmedAccount = true)
    .AddDefaultTokenProviders()
    .AddUserManager<UserManager<User>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddErrorDescriber<IdentityErrorDescriber>();
builder.Services.AddDataProtection();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddScoped((serviceProvider) =>
{
    return new SendGridClient(builder.Configuration.GetSection("SendGridCredentials").GetValue<string>("ApiKey"));
});

builder.Services.AddScoped<IEmailSenderService, EmailSenderService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
