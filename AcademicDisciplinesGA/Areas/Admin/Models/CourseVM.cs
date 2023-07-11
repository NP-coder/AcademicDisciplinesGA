using AcademicDisciplinesGA.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AcademicDisciplinesGA.Areas.Admin.Models
{
    public class CourseVM
    {
        public Course Course { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> TeacherList { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> ChairList { get; set; }
    }
}
