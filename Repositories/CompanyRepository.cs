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
}

/*
 
 So, we create a query string variable where we store our SQL query to fetch all the companies. Then inside the using statement, we use our DapperContext object to create the SQLConnection object (or to be more precise an IDbConnection object) by calling the CreateConnection method. As you can see, as soon as we stop using our connection, we have to dispose of it. Once we create a connection, we can use it to call the QueryAsync method and pass the query as an argument. Since the QueryAsync method returns IEnumerable<T>, we convert it to a list as soon as we want to return a result.

It is important to notice that we use a strongly typed result from the QueryAsync method: QueryAsync<Company>(query). But Dapper supports anonymous results as well: connection.QueryAsync(query). 

 */
