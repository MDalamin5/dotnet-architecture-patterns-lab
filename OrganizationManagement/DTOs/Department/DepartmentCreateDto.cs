using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OrganizationManagement.DTOs.Department
{
    public class DepartmentCreateDto
    {
        [Required(ErrorMessage = "Name is Required.")]
        [StringLength(20, MinimumLength =3)]
        public required string Name {get; set;}
    }
}