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
        private readonly IUnitOfWork _unitOfWork;
        

        public DesignationController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
                await _unitOfWork.Designations.CreateAsync(dbObj);
                await _unitOfWork.SaveChangesAsync();
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
            var allDepartments = await _unitOfWork.Designations.GetAllAsync();
            return Ok(allDepartments);
        }

        // Get a Department by ID
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> getDepartmentById(Guid id)
        {
            var dbObj = await _unitOfWork.Designations.GetByIdAsync(id);
            
            return Ok(dbObj);
        }

        // Update departmentByID
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> updateDepartmentById(Guid id, [FromBody] UpdateDesignationDto model)
        {
            var dbObj = await _unitOfWork.Designations.GetByIdAsync(id);

            dbObj.Name = model.Name;

            await _unitOfWork.Designations.UpdateAsync(dbObj);
            await _unitOfWork.SaveChangesAsync();
            

            return Ok(dbObj);
        }
        

        // Delete the Department
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> deleteDepartmentById(Guid id)
        {
            var dbObj = await _unitOfWork.Designations.GetByIdAsync(id);
            await _unitOfWork.Designations.DeleteAsync(dbObj);
            await _unitOfWork.SaveChangesAsync();
            

            return Ok("Data deleted successfully.");
        }
    }
}