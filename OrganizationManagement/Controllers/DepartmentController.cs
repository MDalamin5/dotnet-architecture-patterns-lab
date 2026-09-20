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

namespace OrganizationManagement.Controllers
{
    [ApiController]
    [Route("/api/v1/departments")]
    public class DepartmentController: ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly IDepartmentRepository _departmentRepository;

        public DepartmentController(IDepartmentRepository departmentRepository, AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
            _departmentRepository = departmentRepository;
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
                await _departmentRepository.CreateAsync(dbObj);
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
            var allDepartments = await _departmentRepository.GetAllAsync();
            return Ok(allDepartments);
        }

        // Get a Department by ID
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> getDepartmentById(Guid id)
        {
            var dbObj = await _departmentRepository.GetByIdAsync(id);
            
            return Ok(dbObj);
        }

        // Update departmentByID
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> updateDepartmentById(Guid id, [FromBody] DepartmentUpdateDto model)
        {
            var dbObj = await _departmentRepository.GetByIdAsync(id);

            dbObj.Name = model.Name;

            await _departmentRepository.UpdateAsync(dbObj);
            

            return Ok(dbObj);
        }
        

        // Delete the Department
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> deleteDepartmentById(Guid id)
        {
            var dbObj = await _departmentRepository.GetByIdAsync(id);
            await _departmentRepository.DeleteAsync(dbObj);
            

            return Ok("Data deleted successfully.");
        }
    }
}