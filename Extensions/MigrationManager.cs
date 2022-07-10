namespace DapperAspNetCore.Extensions;

using DapperAspNetCore.Migrations;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


public static class MigrationManager
{
    public static IHost MigrateDatabase(this IHost host)
    {
        using (var scope = host.Services.CreateScope())
        {
            var databaseService = scope.ServiceProvider.GetRequiredService<Database>();
            var migrationService = scope.ServiceProvider.GetRequiredService<IMigrationRunner>(); // modification 1

            try
            {
                databaseService.CreateDatabase("DapperDemo");
                migrationService.ListMigrations(); // modification 1
                migrationService.MigrateUp(); // modification 1

                // if you want to downgrade
                //migrationService.MigrateDown(202207120001);
            }
            catch
            {
                //log errors or ...
                throw;
            }
        }

        return host;
    }
}

/*
 
 We create an IHost extension method where we get the Database service from a service provider and use it to call our CreateDatabase method
Since we are extending the IHost interface, we can call it inside the Program.cs class to ensure its execution as soon as our app starts

This migration works, but we didn’t use the FluentMigrator library yet. 
This was all Dapper. 
We had to do it that way since FluentMigrator doesn’t support database creation. But now, since we have the database, we can use FluentMigrator to add tables and data to the database.

After creating the Migration class, we need to modify MigrateDatabase method (modification 1)
we get the IMigrationRunner service and then list all migrations with the ListMigrations method, and execute them with the MigrateUp method
The final thing we have to do is to configure FluentMigrator in the Program class:
 */
