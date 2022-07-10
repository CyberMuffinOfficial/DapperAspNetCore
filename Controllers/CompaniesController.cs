using DapperAspNetCore.Contracts;
using DapperAspNetCore.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DapperAspNetCore.Controllers;
[Route("api/[controller]")]
[ApiController]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyRepository _companyRepo;
    public CompaniesController(ICompanyRepository companyRepo)
    {
        _companyRepo = companyRepo;
    }
    [HttpGet]
    public async Task<IActionResult> GetCompanies()
    {
        try
        {
            var companies = await _companyRepo.GetCompanies();
            return Ok(companies);
        }
        catch (Exception ex)
        {
            //log error
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("{id}", Name = "CompanyById")]
    public async Task<IActionResult> GetCompany(Guid id)
    {
        try
        {
            var company = await _companyRepo.GetCompany(id);
            if (company == null)
                return NotFound();
            return Ok(company);
        }
        catch (Exception ex)
        {
            //log error
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateCompany(CompanyForCreationDto company)
    {
        try
        {
            var createdCompany = await _companyRepo.CreateCompany(company);
            return CreatedAtRoute("CompanyById", new { id = createdCompany.Id }, createdCompany);
        }
        catch (Exception ex)
        {
            //log error
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCompany(Guid id, CompanyForUpdateDto company)
    {
        try
        {
            var dbCompany = await _companyRepo.GetCompany(id);
            if (dbCompany == null)
                return NotFound();
            await _companyRepo.UpdateCompany(id, company);
            return NoContent();
        }
        catch (Exception ex)
        {
            //log error
            return StatusCode(500, ex.Message);
        }
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCompany(Guid id)
    {
        try
        {
            var dbCompany = await _companyRepo.GetCompany(id);
            if (dbCompany == null)
                return NotFound();
            await _companyRepo.DeleteCompany(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            //log error
            return StatusCode(500, ex.Message);
        }
    }
}
