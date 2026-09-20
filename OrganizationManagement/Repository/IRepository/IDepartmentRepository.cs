using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrganizationManagement.DTOs.Department;
using OrganizationManagement.Repository.IRepository;
using OrganizationManagement.Models;

namespace OrganizationManagement.IRepository
{
    public interface IDepartmentRepository: IGenericRepository<Department>
    {
        
    }
}