using System.Data;
using Dapper;
using DapperAspNetCore.Context;
using DapperAspNetCore.Contracts;
using DapperAspNetCore.Dto;
using DapperAspNetCore.Entities;

namespace DapperAspNetCore.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly DapperContext _context;
    public CompanyRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Company>> GetCompanies()
    {
        //var query = "SELECT * FROM Companies";
        var query = "SELECT Id, Name, Address, Country FROM Companies";
        using (var connection = _context.CreateConnection())
        {
            var companies = await connection.QueryAsync<Company>(query);
            return companies.ToList();
        }
    }

    public async Task<Company> GetCompany(Guid id)
    {
        var query = "SELECT * FROM Companies WHERE Id = @Id";
        using (var connection = _context.CreateConnection())
        {
            var company = await connection.QuerySingleOrDefaultAsync<Company>(query, new { id });
            return company;
        }
    }

    //public async Task CreateCompany(CompanyForCreationDto company)
    //{
    //    var query = "INSERT INTO Companies (Name, Address, Country) VALUES (@Name, @Address, @Country)";
    //    var parameters = new DynamicParameters();
    //    parameters.Add("Name", company.Name, DbType.String);
    //    parameters.Add("Address", company.Address, DbType.String);
    //    parameters.Add("Country", company.Country, DbType.String);
    //    using (var connection = _context.CreateConnection())
    //    {
    //        await connection.ExecuteAsync(query, parameters);
    //    }
    //}

    public async Task<Company> CreateCompany(CompanyForCreationDto company)
    {
        var query = "INSERT INTO Companies (Id, Name, Address, Country) OUTPUT inserted.Id VALUES (@Id, @Name, @Address, @Country)"; // had to change this cos we use Guid ID instead of int. OUTPUT inserted.Id returns the inserted Id



        var parameters = new DynamicParameters();
        parameters.Add("Id", Guid.NewGuid(), DbType.Guid);
        parameters.Add("Name", company.Name, DbType.String);
        parameters.Add("Address", company.Address, DbType.String);
        parameters.Add("Country", company.Country, DbType.String);

        using (var connection = _context.CreateConnection())
        {
            var id = await connection.QuerySingleOrDefaultAsync<Guid>(query, parameters);

            var createdCompany = new Company
            {
                Id = id,
                Name = company.Name,
                Address = company.Address,
                Country = company.Country
            };

            return createdCompany;
        }
    }

    public async Task UpdateCompany(Guid id, CompanyForUpdateDto company)
    {
        var query = "UPDATE Companies SET Name = @Name, Address = @Address, Country = @Country WHERE Id = @Id";
        var parameters = new DynamicParameters();
        parameters.Add("Id", id, DbType.Guid);
        parameters.Add("Name", company.Name, DbType.String);
        parameters.Add("Address", company.Address, DbType.String);
        parameters.Add("Country", company.Country, DbType.String);
        using (var connection = _context.CreateConnection())
        {
            await connection.ExecuteAsync(query, parameters);
        }
    }
    public async Task DeleteCompany(Guid id)
    {
        var query = "DELETE FROM Companies WHERE Id = @Id";
        using (var connection = _context.CreateConnection())
        {
            await connection.ExecuteAsync(query, new { id });
        }
    }


    // NOW ADDING STORED PROCEDURES
    public async Task<Company> GetCompanyByEmployeeId(Guid id)
    {
        var procedureName = "ShowCompanyForProvidedEmployeeId";
        var parameters = new DynamicParameters();
        parameters.Add("Id", id, DbType.Int32, ParameterDirection.Input);
        using (var connection = _context.CreateConnection())
        {
            var company = await connection.QueryFirstOrDefaultAsync<Company>
                (procedureName, parameters, commandType: CommandType.StoredProcedure);
            return company;
        }
    }

    /* 
    this is the stored procedure. need to create in DB :

    USE [DapperDemo]
    GO
    SET ANSI_NULLS ON
    GO
    SET QUOTED_IDENTIFIER ON
    GO
    CREATE PROCEDURE [dbo].[ShowCompanyForProvidedEmployeeId] @Id Guid
    AS
    SELECT c.Id, c.Name, c.Address, c.Country
    FROM Companies c JOIN Employees e ON c.Id = e.CompanyId
    Where e.Id = @Id
    GO

    Here, we create a variable that contains a procedure name and a dynamic parameter object with a single parameter inside. 
    Because our stored procedure returns a value, we use the QueryFirstOrDefaultAsync method to execute it. 
    Pay attention that if your stored procedure doesn’t return a value, you can use the ExecuteAsync method for execution.

    */

    // Executing Multiple SQL Statements with a Single Query
    public async Task<Company> GetCompanyEmployeesMultipleResults(Guid id)
    {
        var query = "SELECT * FROM Companies WHERE Id = @Id;" +
                    "SELECT * FROM Employees WHERE CompanyId = @Id";
        using (var connection = _context.CreateConnection())
        using (var multi = await connection.QueryMultipleAsync(query, new { id }))
        {
            var company = await multi.ReadSingleOrDefaultAsync<Company>();
            if (company != null)
                company.Employees = (await multi.ReadAsync<Employee>()).ToList();
            return company;
        }
    }

    /*
     As you can see, our query variable contains two SELECT statements. 
    First will return a single company and a second one will return all the employees for that company. 
    After that, we are creating a connection and then using that connection to call the QueryMultipleAsync method. 
    Once we get multiple results inside the multi variable, we can extract both results (company and employees per that company) by using the ReadSignleOrDefaultAsync and ReadAsync methods. 
    The first method returns a single result, while the second one returns a collection.
     */

    // Multiple Mapping
    public async Task<List<Company>> GetCompaniesEmployeesMultipleMapping()
    {
        var query = "SELECT * FROM Companies c JOIN Employees e ON c.Id = e.CompanyId";

        using (var connection = _context.CreateConnection())
        {
            var companyDict = new Dictionary<Guid, Company>();

            var companies = await connection.QueryAsync<Company, Employee, Company>(
                query, (company, employee) =>
                {
                    if (!companyDict.TryGetValue(company.Id, out var currentCompany))
                    {
                        currentCompany = company;
                        companyDict.Add(currentCompany.Id, currentCompany);
                    }

                    currentCompany.Employees.Add(employee);
                    return currentCompany;
                }
            );

            return companies.Distinct().ToList();
        }
    }

    /*
     So, we create a query, and inside the using statement a new connection. 
    Then, we create a new dictionary to keep our companies in. 
    To extract data from the database, we are using the QueryAsync method, but this time it has a new syntax we haven’t seen so far.
    We can see three generic types. The first two are the input types we are going to work with, and the third one is the return type. 
    This method accepts our query as a parameter, and also a Func delegate that accepts two parameters of type Company end Employee. 
    Inside the delegate, we try to extract a company by its Id value. If it doesn’t exist, we store it inside the currentCompany variable and add it to the dictionary. 
    Also, we assign all the employees to that current company and return it from a Func delegate as a result.
    */

    // Transactions
    public async Task CreateMultipleCompanies(List<CompanyForCreationDto> companies)
    {
        var query = "INSERT INTO Companies (Name, Address, Country) VALUES (@Name, @Address, @Country)";
        using (var connection = _context.CreateConnection())
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                foreach (var company in companies)
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("Name", company.Name, DbType.String);
                    parameters.Add("Address", company.Address, DbType.String);
                    parameters.Add("Country", company.Country, DbType.String);
                    await connection.ExecuteAsync(query, parameters, transaction: transaction);
                }
                transaction.Commit();
            }
        }
    }

}

/*
 
 So, we create a query string variable where we store our SQL query to fetch all the companies. Then inside the using statement, we use our DapperContext object to create the SQLConnection object (or to be more precise an IDbConnection object) by calling the CreateConnection method. As you can see, as soon as we stop using our connection, we have to dispose of it. Once we create a connection, we can use it to call the QueryAsync method and pass the query as an argument. Since the QueryAsync method returns IEnumerable<T>, we convert it to a list as soon as we want to return a result.

It is important to notice that we use a strongly typed result from the QueryAsync method: QueryAsync<Company>(query). But Dapper supports anonymous results as well: connection.QueryAsync(query). 

 */
