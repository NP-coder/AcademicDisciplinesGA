using AcademicDisciplinesGA.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AcademicDisciplinesGA.Areas.User.Models
{
    public class CourseVM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int ECTS { get; set; }
        public Teacher Teacher { get; set; }
        public Chair Chair { get; set; }
        public List<CourseCompetence> Competences { get; set; }
        public List<CoursePrerequisite> Prerequisites { get; set; }
        public int Year { get; set; }
        public int Semester { get; set; }
    }
}
