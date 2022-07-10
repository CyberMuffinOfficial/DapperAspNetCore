using System.Reflection;
using DapperAspNetCore.Context;
using DapperAspNetCore.Extensions;
using DapperAspNetCore.Migrations;
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddSingleton<DapperContext>();
builder.Services.AddSingleton<Database>();

builder.Services.AddLogging(c => c.AddFluentMigratorConsole())
        .AddFluentMigratorCore()
        .ConfigureRunner(c => c.AddSqlServer()
        .WithGlobalConnectionString(builder.Configuration.GetConnectionString("SqlConnectionForFluentMigrator"))
        .ScanIn(Assembly.GetExecutingAssembly()).For.Migrations());


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MigrateDatabase(); // Since we are extending the IHost interface, we can call it here


app.Run();

/*
    "DapperAspNetCore": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "launchUrl": "swagger",
      "applicationUrl": "https://localhost:7149;http://localhost:5149",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }

Install-Package Dapper

Install-Package FluentMigrator.Runner
This library has the dependency on the core FluentMigrator package and also on the FluentMigrator.Runner.SqlServer package, so we don’t have to install them separately. 
Additionally, the FluentMigrator.Runner.SqlServer package has the dependency on the Microsoft.Data.SqlClient package, which we are going to need while creating the SqlConnection in the DapperContext class


Add connection string to appsettings.json - 

Should be - "Server=NIGELDESKTOP\\NGDESKTOPSERVER;Integrated Security=true;TrustServerCertificate=True"
and not - "Server=NIGELDESKTOP\\NGDESKTOPSERVER;Integrated Security=true;TrustServerCertificate=True;Initial Catalog=DapperDemo"
Exclude InitialCatalog for Dapper I think

Create entities and context. Register context in DI

Create Migrations/Database and register that too. This is responsible for creating the DB if it doesnt exist.

Run the app and the db should be created - DapperDemo

To start adding tables to the database, we are going to create a new InitialTables_202207120001 class under the Migrations folder. We use the {fileName_version} pattern for the full file name
We have to decorate our migration file with the [Migration] attribute from the FluentMigrator namespace, and provide a version number as a parameter. Also, our class must derive from the abstract Migration class.

We add the FluentMigrator logs to the console, configure the migration runner with the AddFluentMigratorCore method, and configure that runner with the ConfigureRunner method. 
We have to provide the SQL server support, the connection string, and the assembly to search types from. 

For the fluentmigrator connection string, I had to create a new one that included the initial catalog, otherwise it wouldnt work

 */
