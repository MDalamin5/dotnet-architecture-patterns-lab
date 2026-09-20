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

        
    }
}