using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrganizationManagement.Models;

namespace OrganizationManagement.Repository.IRepository
{
    public interface IUnitOfWork: IDisposable
    {
        IGenericRepository<Department> Departments {get;}
        IGenericRepository<Designation> Designations {get;}

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        
    }
}