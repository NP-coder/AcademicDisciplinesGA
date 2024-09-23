using AcademicDisciplinesGA.GA;
using AcademicDisciplinesGA.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicDisciplinesGA.Helpers
{
    public class PopulationHelper
    {
        private static Random random = new Random();

        public static List<DisciplinesChromosome> SpawnPopulation(
            ApplicationDbContext dataContext, List<Competence> competences)
        {
            var population = new HashSet<DisciplinesChromosome>();

            int remainingCount = GAConfig.PopulationCount;
            while (remainingCount > 0)
            {
                var individuals = Enumerable.Range(0, remainingCount)
                                            .Select(i => new DisciplinesChromosome(dataContext, competences))
                                            .ToList();

                foreach (var individual in individuals)
                {
                    population.Add(individual);
                }

                remainingCount = GAConfig.PopulationCount - population.Count;
            }
            return population.ToList();
        }

        public static (DisciplinesChromosome, DisciplinesChromosome) GetCandidateParents(List<DisciplinesChromosome> population)
        {
            var candidateA = population[random.Next(GAConfig.PopulationCount)];
            var candidateB = population[random.Next(GAConfig.PopulationCount)];

            while (candidateA == candidateB)
            {
                candidateB = population[random.Next(GAConfig.PopulationCount)];
            }

            return (candidateA, candidateB);
        }

        public static DisciplinesChromosome TournamentSelection(DisciplinesChromosome candidateA, DisciplinesChromosome candidateB)
        {
            if (candidateA.Rank <= candidateB.Rank)
            {
                return candidateA;
            }
            else
            {
                return candidateB;
            }

        }

        public static DisciplinesChromosome DoCrossover(DisciplinesChromosome individualA, DisciplinesChromosome individualB, List<Competence> competences, int crossoverPosition = -1)
        {
            crossoverPosition = crossoverPosition == -1
                ? random.Next(1, individualA.Sequence.Count - 1)
                : crossoverPosition;

            var offspringSequence = individualA.Sequence.Take(crossoverPosition).ToList();
            var appeared = offspringSequence.ToHashSet();

            foreach (var course in individualB.Sequence)
            {
                if (appeared.Contains(course))
                {
                    continue;
                }

                if (offspringSequence.Count == individualA.Sequence.Count)
                {
                    break;
                }

                offspringSequence.Add(course);
            }

            return new DisciplinesChromosome(offspringSequence, competences);
        }

        public static DisciplinesChromosome DoMutate(DisciplinesChromosome individual, ApplicationDbContext dataContext, List<Competence> competences)
        {
            var courses = dataContext.Courses
                .Include(course => course.Teacher)
                .Include(course => course.Chair)
                .Include(course => course.Competences)
                .ThenInclude(courseCompetence => courseCompetence.Competence)
                .ToList();

            var sequence = individual.Sequence;
            int randomIndex = random.Next(0, sequence.Count);
            int newCourseId;
            CourseChromosome newCourse;

            do
            {
                newCourseId = random.Next(courses.First().Id, courses.Last().Id + 1);
                var selectedCourse = courses.Find(c => c.Id.Equals(newCourseId));
                newCourse = new CourseChromosome()
                {
                    Id = selectedCourse.Id,
                    Title = selectedCourse.Title,
                    ECTS = selectedCourse.ECTS,
                    ChairId = selectedCourse.ChairId,
                    TeacherId = selectedCourse.TeacherId,
                    RequiredCompetences = selectedCourse.Competences.Select(cc => cc.Competence).ToList(),
                    Prerequisites = selectedCourse.Prerequisites.Select(p => p.PrerequisiteId).ToList()
                };
            }
            while (sequence.Contains(newCourse));

            sequence[randomIndex] = newCourse;

            return new DisciplinesChromosome(sequence, competences);
        }

        public static (DisciplinesChromosome, DisciplinesChromosome) Mutate(DisciplinesChromosome individualA, DisciplinesChromosome individualB, ApplicationDbContext dataContext, List<Competence> competences)
        {
            var newIndividualA = new DisciplinesChromosome(individualA.Sequence, competences);
            var newindividualB = new DisciplinesChromosome(individualB.Sequence, competences);

            if (random.NextDouble() < GAConfig.MutationChance)
            {
                newIndividualA = DoMutate(individualA, dataContext, competences);
            }

            if (random.NextDouble() < GAConfig.MutationChance)
            {
                newindividualB = DoMutate(individualB, dataContext, competences);
            }

            return (newIndividualA, newindividualB);
        }

    }
}
