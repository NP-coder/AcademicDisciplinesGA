using AcademicDisciplinesGA.Helpers;
using AcademicDisciplinesGA.Models;

namespace AcademicDisciplinesGA.GA
{
    public class DisciplinesPopulation
    {
        private readonly ApplicationDbContext _dataContext;
        public List<DisciplinesChromosome> Population { get; set; }
        public List<Teacher> Teachers { get; set; }
        public List<Chair> Chairs { get; set; }
        public int GenerationCount { get; set; } = 0;
        public List<int> FitnessOverTime { get; private set; }
        public int NoImprovementCount { get; private set; } = 0;
        public bool HasConverged =>
              GenerationCount > GAConfig.MaxGenerations
            || NoImprovementCount > GAConfig.MaxNoImprovementCount;

        private float previousConvergenceArea = float.MaxValue;

        public DisciplinesPopulation(ApplicationDbContext dbContext, List<Teacher> teachers, List<Chair> chairs)
        {
            _dataContext = dbContext;
            Teachers = teachers;
            Chairs = chairs;
            FitnessOverTime = new List<int>();
            Spawn();
        }

        public void Spawn()
        {
            var result = PopulationHelper.SpawnPopulation(_dataContext, Teachers, Chairs);
            Population = result;
        }

        public void DoGeneration()
        {
            GenerationCount++;

            var offspring = new List<DisciplinesChromosome>();

            while (offspring.Count < GAConfig.PopulationCount)
            {
                var mother = GetParent();
                var father = GetParent();

                while (mother == father)
                {
                    father = GetParent();
                }

                var (offspringA, offspringB) = GetOffspring(mother, father);

                (offspringA, offspringB) = Mutate(offspringA, offspringB);

                offspring.Add(offspringA);
                offspring.Add(offspringB);
            }

            Population.AddRange(offspring);

            MultiObjectiveHelper.UpdatePopulationFitness(Population);

            var newPopulation = new List<DisciplinesChromosome>();

            foreach (var individual in Population.OrderBy(i => i.Rank))
            {
                if (!newPopulation.Contains(individual))
                {
                    newPopulation.Add(individual);
                }
            }

            newPopulation = newPopulation.Take(GAConfig.PopulationCount).ToList();

            Population.Clear();

            newPopulation.ForEach(i => Population.Add(i));

            var firstRank = Population.OrderBy(c => c.ChairFitness).ThenBy(t => t.TeacherFitness);
            var currentArea = MultiObjectiveHelper.CalculateArea(firstRank);

            if (Math.Abs(previousConvergenceArea - currentArea) < 0.1)
            {
                NoImprovementCount++;
            }
            else
            {
                NoImprovementCount = 0;
                previousConvergenceArea = currentArea;
            }
        }

        public DisciplinesChromosome GetBestIndividual()
        {
            DisciplinesChromosome bestChromosome = null;
            int maxFitness = int.MinValue;

            foreach (var chromosome in Population)
            {
                if (chromosome.TeacherFitness + chromosome.ChairFitness > maxFitness)
                {
                    maxFitness = chromosome.TeacherFitness + chromosome.ChairFitness;
                    bestChromosome = chromosome;
                }
            }

            return bestChromosome;

            //var firstRank = Population.GroupBy(i => i.Rank).First().ToList();
            //return firstRank;
        }

        private (DisciplinesChromosome, DisciplinesChromosome) Mutate(DisciplinesChromosome individualA, DisciplinesChromosome individualB)
        {
            return PopulationHelper.Mutate(individualA, individualB, _dataContext, Teachers, Chairs);
        }

        private (DisciplinesChromosome, DisciplinesChromosome) GetOffspring(DisciplinesChromosome individualA, DisciplinesChromosome individualB)
        {
            var offspringA = DoCrossover(individualA, individualB);
            var offspringB = DoCrossover(individualB, individualA);

            return (offspringA, offspringB);
        }

        private DisciplinesChromosome DoCrossover(DisciplinesChromosome individualA, DisciplinesChromosome individualB)
        {
            return PopulationHelper.DoCrossover(individualA, individualB, Teachers, Chairs);
        }

        private DisciplinesChromosome GetParent()
        {
            var (candidate1, candidate2) = PopulationHelper.GetCandidateParents(Population);

            return PopulationHelper.TournamentSelection(candidate1, candidate2);
        }

    }
}
