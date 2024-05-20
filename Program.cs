using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System;
using TaskMongoDb.Data;
using TaskMongoDb.Middlewares;
using TaskMongoDb.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string databaseName = "TaskMongoDb";
string connectionString = "mongodb://localhost:27017/";

builder.Services.AddSingleton(new MongoDbContext(connectionString, databaseName));

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
         .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>
         (
             connectionString, databaseName
         );

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseWhen(context => context.Request.Path.StartsWithSegments("/api/items"), apiApp =>
{
    apiApp.UseMiddleware<TokenValidationMiddleware>();
});

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
