using AcademicDisciplinesGA.Areas.Admin.Models;
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
        public int CompetenceFitness { get; set; }
        public int LoadFitness { get; set; }
        public List<Competence> StudentCompetences { get; set; }
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

        public DisciplinesChromosome(ApplicationDbContext dataContext, List<Competence> studentInterests)
        {
            _dataContext = dataContext;

            // Генерація курсів на основі компетенцій студента
            Generate(studentInterests);

            // Алокація курсів з пререквізитами і балансуванням навантаження
            GenerateCoursesWithPrerequisitesAndDistributeLoad();

            // Підсумування ECTS після встановлення усіх курсів 
            ECTSCount = GetTotalECTS();
            // Оцінювання за критеріями
            //TeacherFitness = IsTeacherSelected(Teachers);
            //ChairFitness = IsChairSelected(Chairs);
            CompetenceFitness = CalculateCompetenceFitness();
        }

        public DisciplinesChromosome(List<CourseChromosome> courses, List<Competence> studentInterests)
        {
            Sequence = courses.ToList();
            StudentCompetences = studentInterests;
            ECTSCount = GetTotalECTS();
            //TeacherFitness = IsTeacherSelected(Teachers);
            //ChairFitness = IsChairSelected(Chairs);
            CompetenceFitness = CalculateCompetenceFitness();
        }

        public void RecalculateCompetenceFitness()
        {
            var allCourses = _dataContext.Courses
                .Include(course => course.Competences)
                .ThenInclude(cc => cc.Competence)
                .ToList();

            for (int i = 0; i < Sequence.Count; i++)
            {
                var updatedCourse = allCourses.Find(c => c.Id == Sequence[i].Id);
                if (updatedCourse != null)
                {
                    var newCourse = new CourseChromosome()
                    {
                        Id = updatedCourse.Id,
                        Title = updatedCourse.Title,
                        ECTS = updatedCourse.ECTS,
                        ChairId = updatedCourse.ChairId,
                        TeacherId = updatedCourse.TeacherId,
                        RequiredCompetences = updatedCourse.Competences.Select(cc => cc.Competence).ToList(),
                        Prerequisites = updatedCourse.Prerequisites.Select(p => p.PrerequisiteId).ToList()
                    };

                    Sequence[i] = newCourse;
                }
            }

            CompetenceFitness = CalculateCompetenceFitness();
        }

        private int CalculateCompetenceFitness()
        {
            int totalFitness = 0;

            foreach (var course in Sequence)
            {
                var matchedCompetences = course.RequiredCompetences.Intersect(StudentCompetences).Count();
                totalFitness += matchedCompetences * 5; // Припущення: кожний співпадаючий курс добавляє 5 до фітнесу
            }

            return totalFitness;
        }

        public void GenerateCoursesWithPrerequisitesAndDistributeLoad()
        {
            // Крок 2.1: Розподіл дисциплін із пререквізитами
            AllocateCoursesWithPrerequisites();

            // Крок 2.2: Розподіл навантаження по семестрах
            CalculateLoadDistribution();
        }

        public int CalculateLoadDistribution()
        {
            // Крок 2.2: Розрахунок штрафу за розходження між фактичним і ідеальним навантаженням
            var semesterECTS = new Dictionary<string, int>();
            foreach (var course in Sequence)
            {
                string key = $"{course.Year}-{course.Semester}";
                if (!semesterECTS.ContainsKey(key))
                    semesterECTS[key] = 0;

                semesterECTS[key] += course.ECTS;
            }

            // Визначення ідеального розподілу навантаження
            int idealECTS = semesterECTS.Values.Sum() / semesterECTS.Count;
            int loadVariancePenalty = semesterECTS.Values.Select(x => Math.Abs(x - idealECTS)).Sum();

            return -loadVariancePenalty; // Мінус означає штраф за велике відхилення від ідеального розподілу
        }

        public void AllocateCoursesWithPrerequisites()
        {
            var graph = new Dictionary<int, List<int>>();
            var inDegree = new Dictionary<int, int>();
            //var allCourses = _dataContext.Courses.ToList();

            // Побудова графу залежностей і розрахунок ступеня входу для кожного курсу
            foreach (var course in Sequence) //allCourses
            {
                if (!graph.ContainsKey(course.Id))
                {
                    graph[course.Id] = new List<int>();
                    inDegree[course.Id] = 0;
                }
                foreach (var prereq in course.Prerequisites)
                {
                    if (!graph.ContainsKey(prereq))
                    {
                        graph[prereq] = new List<int>();
                        inDegree[prereq] = 0;
                    }
                    graph[prereq].Add(course.Id);
                    inDegree[course.Id]++;
                }
            }

            // Застосування алгоритму топологічного сортування
            var queue = new Queue<int>();
            foreach (var key in inDegree.Keys)
            {
                if (inDegree[key] == 0) queue.Enqueue(key);
            }

            int semester = 1, year = 1;
            while (queue.Count > 0)
            {
                int size = queue.Count;
                for (int i = 0; i < size; i++)
                {
                    var courseId = queue.Dequeue();
                    //var course = allCourses.Find(c => c.Id == courseId);
                    var course = Sequence.Find(c => c.Id == courseId);
                    course.Year = year;
                    course.Semester = semester;

                    foreach (var neighbor in graph[courseId])
                    {
                        inDegree[neighbor]--;
                        if (inDegree[neighbor] == 0)
                        {
                            queue.Enqueue(neighbor);
                        }
                    }
                }

                semester++;
                if (semester > 2) { semester = 1; year++; }
            }
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

        //public void Generate(List<Competence> studentInterests)
        //{
        //    var allCourses = _dataContext.Courses
        //        .Include(course => course.Teacher)
        //        .Include(course => course.Chair)
        //        .Include(course => course.RequiredCompetences)
        //        .Include(course => course.Prerequisites)  // Завантажуємо пререквізити курсів
        //        .ToList();

        //    var filteredCourses = allCourses.Where(course =>
        //        studentInterests.Any(sc => course.RequiredCompetences.Contains(sc))).ToList();

        //    var selectedCoursesIds = new HashSet<int>();
        //    var result = new List<CourseChromosome>();
        //    int count = 0;

        //    for (int i = 0; i < 60; i++)
        //    {
        //        if (filteredCourses.Count == 0)
        //            break;

        //        // Выбираємо курс випадковим чином
        //        int index = Random.Next(0, filteredCourses.Count);
        //        var selectedCourse = filteredCourses[index];

        //        // Перевіряємо, чи всі необхідні пререквізити містяться у вже вибраних курсах
        //        var prerequisiteIds = selectedCourse.Prerequisites.Select(p => p.PrerequisiteId).ToList();
        //        if (prerequisiteIds.All(pid => selectedCoursesIds.Contains(pid)))
        //        {
        //            // Додаємо вибраний курс до результативної послідовності
        //            selectedCoursesIds.Add(selectedCourse.Id);
        //            result.Add(new CourseChromosome()
        //            {
        //                Id = selectedCourse.Id,
        //                Title = selectedCourse.Title,
        //                ECTS = selectedCourse.ECTS,
        //                ChairId = selectedCourse.ChairId,
        //                TeacherId = selectedCourse.TeacherId,
        //                RequiredCompetences = selectedCourse.RequiredCompetences,
        //                Prerequisites = prerequisiteIds  // Заповнюємо список ID пререквізитів
        //            });
        //            count += selectedCourse.ECTS;

        //            // Після додавання курсу видаляємо його з вихідного списку
        //            filteredCourses.RemoveAt(index);
        //        }
        //    }

        //    Sequence = result.OrderBy(c => c.Prerequisites.Count).ThenBy(c => c.Id).ToList();
        //    // Спроба впорядкування курсів по количеству пререквізитів і ID, що допоможе дотримуватися логічного порядку
        //}

        public void Generate(List<Competence> studentInterests)
        {
            var allCourses = _dataContext.Courses
                .Include(course => course.Teacher)
                .Include(course => course.Chair)
                .Include(course => course.Competences)
                .ThenInclude(courseCompetence => courseCompetence.Competence)
                .ToList();

            var filteredCourses = allCourses.Where(course =>
                course.Competences.Any(cc => studentInterests.Contains(cc.Competence))).ToList();

            var selectedCoursesIds = new HashSet<int>();
            var result = new List<CourseChromosome>();

            while (selectedCoursesIds.Count < _length && filteredCourses.Count > 0)
            {
                int index = Random.Next(0, filteredCourses.Count);
                var selectedCourse = filteredCourses[index];

                if (!selectedCoursesIds.Contains(selectedCourse.Id))
                {
                    selectedCoursesIds.Add(selectedCourse.Id);
                    result.Add(new CourseChromosome()
                    {
                        Id = selectedCourse.Id,
                        Title = selectedCourse.Title,
                        ECTS = selectedCourse.ECTS,
                        ChairId = selectedCourse.ChairId,
                        TeacherId = selectedCourse.TeacherId,
                        RequiredCompetences = selectedCourse.Competences.Select(cc => cc.Competence).ToList(),
                        Prerequisites = selectedCourse.Prerequisites.Select(p => p.PrerequisiteId).ToList()  // Зберігаємо пререквізити для наступного використання
                    });
                }
            }

            Sequence = result;
        }

    }
}
