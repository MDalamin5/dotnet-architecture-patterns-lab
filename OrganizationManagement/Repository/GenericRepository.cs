using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OrganizationManagement.Data;
using OrganizationManagement.Repository.IRepository;

namespace OrganizationManagement.Repository
{
    public class GenericRepository<TEntity>: IGenericRepository<TEntity> where TEntity: class
    {
        private readonly AppDbContext _appDbContext;
        private readonly DbSet<TEntity> _dbSet;
        private readonly IMapper _mapper;

        public GenericRepository(AppDbContext appDbContext, IMapper mapper)
        {
            _mapper = mapper;
            _appDbContext = appDbContext;
            _dbSet = appDbContext.Set<TEntity>();
        }

        public async Task<List<TEntity>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<TEntity> GetByIdAsync(Guid id)
        {
            var dbObj = _dbSet.FindAsync(id);
            if(dbObj == null)
                return null;
            return _mapper.Map<TEntity>(dbObj);
        }

        public async Task CreateAsync(TEntity model)
        {
            // var newObj = _mapper.Map<TEntity>(model);
            await _dbSet.AddAsync(model);
            await _appDbContext.SaveChangesAsync();

            // _mapper.Map<TEntity>(model);
        }

        public async Task UpdateAsync(TEntity model)
        {
            _dbSet.Update(model);
            await _appDbContext.SaveChangesAsync();
            
        }

        public async Task DeleteAsync(TEntity model)
        {
            _dbSet.Remove(model);
            await _appDbContext.SaveChangesAsync();
        }



    }
}