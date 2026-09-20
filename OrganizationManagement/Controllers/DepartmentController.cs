using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizationManagement.Data;
using OrganizationManagement.DTOs.Department;
using OrganizationManagement.IRepository;
using OrganizationManagement.Models;
using OrganizationManagement.Repository.IRepository;

namespace OrganizationManagement.Controllers
{
    [ApiController]
    [Route("/api/v1/departments")]
    public class DepartmentController: ControllerBase
    {
        
        private readonly IUnitOfWork _unitOfWork;

        public DepartmentController(IUnitOfWork unitOfWork )
        {
            _unitOfWork = unitOfWork;
        }

        // Department Create Endpoint
        [HttpPost]
        public async Task<IActionResult> createDepartment([FromBody] DepartmentCreateDto model)
        {
            var dbObj = new Department
            {
                Id = Guid.NewGuid(),
                Name = model.Name
            };

            try
            {
                await _unitOfWork.Departments.CreateAsync(dbObj);
                await _unitOfWork.SaveChangesAsync();
            }
            catch(Exception)
            {
                return Ok("Category is not created.");
            }
            return Ok("Category created Successfully.");
        }

        // Get All Department
        [HttpGet]
        public async Task<IActionResult> getAllDepartments()
        {
            var allDepartments = await _unitOfWork.Departments.GetAllAsync();
            return Ok(allDepartments);
        }

        // Get a Department by ID
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> getDepartmentById(Guid id)
        {
            var dbObj = await _unitOfWork.Departments.GetByIdAsync(id);
            
            return Ok(dbObj);
        }

        // Update departmentByID
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> updateDepartmentById(Guid id, [FromBody] DepartmentUpdateDto model)
        {
            var dbObj = await _unitOfWork.Departments.GetByIdAsync(id);

            dbObj.Name = model.Name;

            await _unitOfWork.Departments.UpdateAsync(dbObj);
            await _unitOfWork.SaveChangesAsync();
            

            return Ok(dbObj);
        }
        

        // Delete the Department
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> deleteDepartmentById(Guid id)
        {
            var dbObj = await _unitOfWork.Departments.GetByIdAsync(id);
            await _unitOfWork.Departments.DeleteAsync(dbObj);
            await _unitOfWork.SaveChangesAsync();
            

            return Ok("Data deleted successfully.");
        }
    }
}