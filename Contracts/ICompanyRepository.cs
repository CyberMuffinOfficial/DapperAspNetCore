﻿using DapperAspNetCore.Dto;
using DapperAspNetCore.Entities;

namespace DapperAspNetCore.Contracts;

public interface ICompanyRepository
{
    public Task<IEnumerable<Company>> GetCompanies();

    public Task<Company> GetCompany(Guid id);

    public Task<Company> CreateCompany(CompanyForCreationDto company);

    public Task UpdateCompany(Guid id, CompanyForUpdateDto company);

    public Task DeleteCompany(Guid id);

    // NOW ADDING STORED PROCEDURES
    public Task<Company> GetCompanyByEmployeeId(Guid id);

    //Executing Multiple SQL Statements with a Single Query
    public Task<Company> GetCompanyEmployeesMultipleResults(Guid id);

    // Multiple Mapping
    public Task<List<Company>> GetCompaniesEmployeesMultipleMapping();

}
