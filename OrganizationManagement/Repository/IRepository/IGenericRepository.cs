using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganizationManagement.Repository.IRepository
{
    public interface IGenericRepository<TEntity> where TEntity: class
    {
        Task<List<TEntity>> GetAllAsync();
        Task<TEntity> GetByIdAsync(Guid id);
        Task CreateAsync(TEntity model);
        Task UpdateAsync(TEntity model);
        Task DeleteAsync(TEntity model);
    }
}