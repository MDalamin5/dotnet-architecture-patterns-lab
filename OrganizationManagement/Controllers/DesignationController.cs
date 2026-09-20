using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrganizationManagement.Data;
using OrganizationManagement.Repository.IRepository;
using OrganizationManagement.DTOs.Designation;
using OrganizationManagement.Models;

namespace OrganizationManagement.Controllers
{
    [ApiController]
    [Route("/api/v1/designations")]
    public class DesignationController: ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly IDesignationRepository _designationRepository;

        public DesignationController(IDesignationRepository designationRepository, AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
            _designationRepository = designationRepository;
        }

        // Department Create Endpoint
        [HttpPost]
        public async Task<IActionResult> createCategories([FromBody] CreateDesignationDto model)
        {
            var dbObj = new Designation
            {
                Id = Guid.NewGuid(),
                Name = model.Name
            };
           

            try
            {
                await _designationRepository.CreateAsync(dbObj);
            }
            catch (Exception)
            {
                return Ok("Designation is not Created successfully.");
                
            }
            return Ok("Designation Created Successfully.");

        }

        // Get All Department
        [HttpGet]
        public async Task<IActionResult> getAllDepartments()
        {
            var allDepartments = await _designationRepository.GetAllAsync();
            return Ok(allDepartments);
        }

        // Get a Department by ID
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> getDepartmentById(Guid id)
        {
            var dbObj = await _designationRepository.GetByIdAsync(id);
            
            return Ok(dbObj);
        }

        // Update departmentByID
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> updateDepartmentById(Guid id, [FromBody] UpdateDesignationDto model)
        {
            var dbObj = await _designationRepository.GetByIdAsync(id);

            dbObj.Name = model.Name;

            await _designationRepository.UpdateAsync(dbObj);
            

            return Ok(dbObj);
        }
        

        // Delete the Department
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> deleteDepartmentById(Guid id)
        {
            var dbObj = await _designationRepository.GetByIdAsync(id);
            await _designationRepository.DeleteAsync(dbObj);
            

            return Ok("Data deleted successfully.");
        }
    }
}