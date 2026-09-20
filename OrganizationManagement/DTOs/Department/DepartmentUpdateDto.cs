using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace OrganizationManagement.DTOs.Department
{
    public class DepartmentUpdateDto
    {
        [Required(ErrorMessage = "Name is Required.")]
        [StringLength(20, MinimumLength =3)]
        public string Name {get; set;}
    }
}