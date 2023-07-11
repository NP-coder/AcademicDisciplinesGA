namespace AcademicDisciplinesGA.Models
{
    public class FormedPool
    {
        public int Id { get; set; }
        public List<Course> FormedCoursesPool { get; set; }
        public int UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
