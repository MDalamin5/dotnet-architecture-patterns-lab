using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrganizationManagement.Repository.IRepository;
using OrganizationManagement.Data;
using OrganizationManagement.DTOs.Designation;
using OrganizationManagement.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace OrganizationManagement.Repository
{
    public class DesignationRepository: GenericRepository<Designation>, IDesignationRepository
        {
            private readonly AppDbContext _appDbContext;
            private readonly IMapper _mapper;
            public DesignationRepository(AppDbContext appDbContext, IMapper mapper)
            : base(appDbContext, mapper)
            {
                
            }

            
        }
}