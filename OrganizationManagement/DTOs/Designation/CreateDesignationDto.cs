using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace OrganizationManagement.DTOs.Designation
{
    public class CreateDesignationDto
    {
        [Required(ErrorMessage = "Name is Required.")]
        [StringLength(20, MinimumLength =3)]
        public string Name {get; set;}
    }
}