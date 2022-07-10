using FluentMigrator;

namespace DapperAspNetCore.Migrations;

// this is the date
[Migration(202207120001)]
public class InitialTables_202207120001 : Migration
{
    public override void Down()
    {
        Delete.Table("Employees");
        Delete.Table("Companies");
    }
    public override void Up()
    {
        Create.Table("Companies")
            .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("Name").AsString(50).NotNullable()
            .WithColumn("Address").AsString(60).NotNullable()
            .WithColumn("Country").AsString(50).NotNullable();

        Create.Table("Employees")
            .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("Name").AsString(50).NotNullable()
            .WithColumn("Age").AsInt32().NotNullable()
            .WithColumn("Position").AsString(50).NotNullable()
            .WithColumn("CompanyId").AsGuid().NotNullable().ForeignKey("Companies", "Id");
    }
}

/*
 
In the Up method, we are creating our two tables by using different methods that FluentMigrator provides. 
As you can see, the names of the methods describe what each method does, which is great. 
Also, once we create the Employees table, we configure the CompanyId column as a foreign key by calling the ForeignKey method and providing the primary table name and the primary column name parameters.

In the Down method, we revert our changes if we revert this migration. So, we just remove both tables, first Employees, and then Companies.

Now, to be able to start this migration as soon as our app starts, we have to modify the MigrationManager class:

 */
