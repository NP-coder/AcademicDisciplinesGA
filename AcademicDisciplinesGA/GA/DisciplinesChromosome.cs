using AcademicDisciplinesGA.Models;
using GeneticSharp;
using Microsoft.EntityFrameworkCore;

namespace AcademicDisciplinesGA.GA
{
    public class DisciplinesChromosome
    {
        private readonly ApplicationDbContext _dataContext;
        public List<CourseChromosome> Sequence { get; set; }
        public int Rank { get; set; }
        public int ECTSCount { get; set; }
        public int TeacherFitness { get; set; }
        public int ChairFitness { get; set; }
        public List<Teacher> Teachers { get; set; }
        public List<Chair> Chairs { get; set; }

        public static int _length = 10;

        static Random Random = new Random();

        public DisciplinesChromosome(ApplicationDbContext dataContext, List<Teacher> teachers, List<Chair> chairs)
        {
            _dataContext = dataContext;
            Generate();
            Teachers = teachers;
            Chairs = chairs;
            ECTSCount = GetTotalECTS();
            TeacherFitness = IsTeacherSelected(Teachers);
            ChairFitness = IsChairSelected(Chairs);
        }
        public DisciplinesChromosome(List<CourseChromosome> courses, List<Teacher> teachers, List<Chair> chairs)
        {
            Sequence = courses.ToList();
            Teachers = teachers;
            Chairs = chairs;
            ECTSCount = GetTotalECTS();
            TeacherFitness = IsTeacherSelected(Teachers);
            ChairFitness = IsChairSelected(Chairs);
        }

        public int GetTotalECTS()
        {
            var total = 0;

            for (int i = 0; i < Sequence.Count(); i++)
            {
                var fromCourse = Sequence[i];

                total += fromCourse.ECTS;
            }

            return total;
        }

        public int IsTeacherSelected(List<Teacher> teachers)
        {
            var totalSelected = 0;
            for (int i = 0; i < Sequence.Count(); i++)
            {
                for (int j = 0; j < teachers.Count; j++)
                {
                    if (Sequence[i].TeacherId.Equals(teachers[j].Id))
                    {
                        totalSelected++;
                    }
                }
            }
            return totalSelected;
        }

        public int IsChairSelected(List<Chair> chairs)
        {
            var totalSelected = 0;
            for (int i = 0; i < Sequence.Count(); i++)
            {
                for (int j = 0; j < chairs.Count; j++)
                {
                    if (Sequence[i].ChairId.Equals(chairs[j].Id))
                    {
                        totalSelected++;
                    }

                }
            }
            return totalSelected;
        }

        public void Generate()
        {
            var courses = _dataContext.Courses
                .Include(course => course.Teacher)
                .Include(course => course.Chair).ToList();
            var selectedCoursesIds = new HashSet<int>();
            var result = new List<CourseChromosome>();
            int count = 0;

            for (int i = 0; i < 60; i = count)
            {
                int courseId;
                do
                {
                    courseId = Random.Next(courses.First().Id, courses.Last().Id + 1);
                }
                while (selectedCoursesIds.Contains(courseId));
                selectedCoursesIds.Add(courseId);
                var selectedCourse = courses.Find(c => c.Id.Equals(courseId));
                result.Add(new CourseChromosome()
                {
                    Id = selectedCourse.Id,
                    Title = selectedCourse.Title,
                    ECTS = selectedCourse.ECTS,
                    ChairId = selectedCourse.ChairId,
                    TeacherId = selectedCourse.TeacherId
                });
                count += selectedCourse.ECTS;
            }

            Sequence = result;
        }

    }
}
