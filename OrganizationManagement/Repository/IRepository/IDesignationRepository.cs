using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrganizationManagement.DTOs.Designation;
using OrganizationManagement.Models;

namespace OrganizationManagement.Repository.IRepository
{
    public interface IDesignationRepository: IGenericRepository<Designation>
    {
        // Task<bool> CreateDesignation(CreateDesignationDto model);
        // Task<List<ReadDesignationDto>> getAllDesignation();
        // Task<ReadDesignationDto> getDesignationById(Guid id);
        // Task<bool> updateDesignationById(Guid id, UpdateDesignationDto model);
        // Task<bool> deleteDesignationById(Guid id);
    }
}