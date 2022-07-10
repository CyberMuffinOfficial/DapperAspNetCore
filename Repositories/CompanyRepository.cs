using Dapper;
using DapperAspNetCore.Context;
using DapperAspNetCore.Contracts;
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

    public async Task<Company> GetCompany(int id)
    {
        var query = "SELECT * FROM Companies WHERE Id = @Id";
        using (var connection = _context.CreateConnection())
        {
            var company = await connection.QuerySingleOrDefaultAsync<Company>(query, new { id });
            return company;
        }
    }
}

/*
 
 So, we create a query string variable where we store our SQL query to fetch all the companies. Then inside the using statement, we use our DapperContext object to create the SQLConnection object (or to be more precise an IDbConnection object) by calling the CreateConnection method. As you can see, as soon as we stop using our connection, we have to dispose of it. Once we create a connection, we can use it to call the QueryAsync method and pass the query as an argument. Since the QueryAsync method returns IEnumerable<T>, we convert it to a list as soon as we want to return a result.

It is important to notice that we use a strongly typed result from the QueryAsync method: QueryAsync<Company>(query). But Dapper supports anonymous results as well: connection.QueryAsync(query). 

 */
