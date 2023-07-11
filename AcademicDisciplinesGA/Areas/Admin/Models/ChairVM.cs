using AcademicDisciplinesGA.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AcademicDisciplinesGA.Areas.Admin.Models
{
    public class ChairVM
    {
        public Chair Chair { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> FacultyList { get; set; }
    }
}
