using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace AcademicDisciplinesGA.Models
{
    public class Chair 
    {
        public int Id { get; set; }
        public string Title { get; set; }
        [Display(Name ="Faculty")]
        public int FacultyId { get; set; }
        [ValidateNever]
        public Faculty Faculty { get; set; }
    }
}
