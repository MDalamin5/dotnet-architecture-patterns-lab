using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Storage;
using OrganizationManagement.Data;
using OrganizationManagement.Models;
using OrganizationManagement.Repository.IRepository;

namespace OrganizationManagement.Repository
{
    public class UnitOfWork: IUnitOfWork
    {
        private readonly AppDbContext _appDbContext;
        private IDbContextTransaction _transaction;
        private readonly IMapper _mapper;

        public IGenericRepository<Department> Departments {get;}
        public IGenericRepository<Designation> Designations {get;}


        public UnitOfWork(AppDbContext appDbContext, IMapper mapper)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;

            Departments = new GenericRepository<Department>(_appDbContext, _mapper);
            Designations = new GenericRepository<Designation>(_appDbContext, _mapper);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _appDbContext.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _appDbContext.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if(_transaction == null)
                throw new InvalidOperationException("No Active Transaction.");

            try
            {
                await _appDbContext.SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            catch
            {
                await _transaction.RollbackAsync();
                throw;
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if(_transaction == null)
                return;

            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }









        public void Dispose()
        {
            _appDbContext.Dispose();
        }
    }
}