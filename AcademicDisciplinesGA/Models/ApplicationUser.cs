using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademicDisciplinesGA.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Name { get; set; }

        [Display(Name = "Chair")]
        public int? ChairId { get; set; }
        [ForeignKey("ChairId")]
        [ValidateNever]
        public Chair Chair { get; set; }
    }
}
