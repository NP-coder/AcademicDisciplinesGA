using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AcademicDisciplinesGA.Models
{
    public class Teacher 
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [ValidateNever]
        public List<Course> Courses { get; set; }
    }
}
