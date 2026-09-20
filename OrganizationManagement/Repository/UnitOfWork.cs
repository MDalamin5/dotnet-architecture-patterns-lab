using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using OrganizationManagement.Data;
using OrganizationManagement.Models;
using OrganizationManagement.Repository.IRepository;

namespace OrganizationManagement.Repository
{
    public class UnitOfWork: IUnitOfWork
    {
        private readonly AppDbContext _appDbContext;
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

        public void Dispose()
        {
            _appDbContext.Dispose();
        }
    }
}