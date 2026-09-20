using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrganizationManagement.Data;
using OrganizationManagement.Models;
using OrganizationManagement.DTOs.Department;
using OrganizationManagement.IRepository;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace OrganizationManagement.Repository
{
    public class DepartmentRepository: GenericRepository<Department>, IDepartmentRepository
    {
        private readonly AppDbContext _appDbContext;
        public DepartmentRepository(AppDbContext appDbContext, IMapper mapper):base(appDbContext, mapper)
        {
            
        }

        // public async Task<bool> createDepartments(DepartmentCreateDto model)
        // {
        //     var data = new Department
        //     {
        //         Id = Guid.NewGuid(),
        //         Name = model.Name
        //     };

        //     Console.WriteLine($"Name: {data.Name}, Id: {data.Id}");
            
        //     await _appDbContext.Departments.AddAsync(data);
        //     await _appDbContext.SaveChangesAsync();

        //     return true;
        // }

        // public async Task<List<DepartmentReadDto>> getAllDepartments()
        // {
        //      var allDepartments = await _appDbContext.Departments.Select(d => new DepartmentReadDto
        //      {
        //          Name = d.Name
        //      }).ToListAsync();
        //     return allDepartments;
        // }

        // public async Task<DepartmentReadDto> getDepartmentById(Guid id)
        // {
        //     var dbObj = await _appDbContext.Departments.FirstOrDefaultAsync(d => d.Id == id);

        //     return new DepartmentReadDto
        //     {
        //         Name = dbObj.Name
        //     };
        // }

        // public async Task<bool> updateDepartmentById(Guid id, DepartmentUpdateDto model)
        // {
        //     var dbObj = await _appDbContext.Departments.FirstOrDefaultAsync(d => d.Id == id);
        //     dbObj.Name = model.Name;
        //     await _appDbContext.SaveChangesAsync();

        //     return true;
        // }

        // public async Task<bool> deleteDepartmentById(Guid id)
        // {
        //     var dbObj = await _appDbContext.Departments.FirstOrDefaultAsync(d => d.Id == id);
        //     _appDbContext.Departments.Remove(dbObj);
        //     await _appDbContext.SaveChangesAsync();

        //     return true;
        // }
    }
}