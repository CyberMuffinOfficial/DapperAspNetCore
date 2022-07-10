namespace DapperAspNetCore.Context;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;


public class DapperContext
{
    private readonly IConfiguration _configuration;

    public DapperContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IDbConnection CreateConnection()
        => new SqlConnection(_configuration.GetConnectionString("SqlConnectionForDapper"));

    public IDbConnection CreateMasterConnection()
        => new SqlConnection(_configuration.GetConnectionString("MasterConnectionForDapper"));
}
