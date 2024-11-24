using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EvolutionaryAlgorithm
{
    public class Square
    {
        public int Side { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class Individual
    {
        public List<Square> Squares { get; set; }
        public int Loss { get; set; }

        public Individual(List<Square> squares)
        {
            Squares = squares;
            CalculateLoss();
        }

        public void CalculateLoss()
        {
            int minX = Squares.Min(s => s.X);
            int minY = Squares.Min(s => s.Y);
            int maxX = Squares.Max(s => s.X + s.Side);
            int maxY = Squares.Max(s => s.Y + s.Side);

            Loss = (maxX - minX) * (maxY - minY);
        }

        public bool IsPositionValid() {
            foreach (var square in Squares)
            {
                foreach (var otherSquare in Squares)
                {
                    if (square != otherSquare && (IsSquaresIntersect(square, otherSquare)  || IsSquareUnbounding(square)))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool IsSquaresIntersect(Square square1, Square square2)
        {
            return square1.X < square2.X + square2.Side &&
                   square2.X < square1.X + square1.Side &&
                   square1.Y < square2.Y + square2.Side &&
                   square2.Y < square1.Y + square1.Side;
        }

        private bool IsSquareUnbounding(Square square) {
            return square.X + square.Side > 100 || square.Y + square.Side > 100;
        }
    }

    public class GeneticAlgorithm
    {
        private Random _random = new Random();
        public List<Individual> _population;
        private int _populationSize;
        private int _numSquares;
        private int[] _sideLengths;
        private double _mutationProbability;
        public int _evolutionIter; 

        public GeneticAlgorithm(int[] sideLengths, int populationSize, double mutationProbability)
        {
            _populationSize = populationSize;
            _sideLengths = sideLengths;
            _numSquares = sideLengths.Length;
            _mutationProbability = mutationProbability;
            _evolutionIter = 0;
            _population = InitializePopulation();
            GetBestIndividual();
        }

        private List<Individual> InitializePopulation()
        {
            List<Individual> population = new List<Individual>();

            Parallel.For(0, _populationSize, i =>
            {
                Individual individual;
                do 
                {
                    List<Square> squares = new List<Square>();

                    for (int j = 0; j < _numSquares; j++)
                    {
                        int side = _sideLengths[j];
                        int x = _random.Next(0, 100);
                        int y = _random.Next(0, 100);

                        squares.Add(new Square { Side = side, X = x, Y = y });
                    }

                    individual = new Individual(squares);
                } while (!individual.IsPositionValid());

                lock (population)
                {
                    population.Add(individual);
                }
            });

            return population;
        }

        public void Evolve ()
        {
            Parallel.For(0, _populationSize, i =>
            {
                Individual child;
                do 
                {
                    child = Mutate(Crossover(i, _random.Next(0, _populationSize)));
                } while (!child.IsPositionValid());

                lock (_population)
                {
                    _population.Add(child);
                }
            });

            GetBestIndividual();
            _evolutionIter++;
        }
        
        private Individual Crossover(int parent1, int parent2)
        {
            List<Square> squares = new List<Square>();

            for (int i = 0; i < _numSquares; i++)
            {
                if (_random.NextDouble() < 0.5)
                {
                    squares.Add(_population[parent1].Squares[i]);
                }
                else
                {
                    squares.Add(_population[parent2].Squares[i]);
                }
            }

            return new Individual(squares);
        }

        private Individual Mutate(Individual individual)
        {
            int maxCoordinate = (int)Math.Sqrt(individual.Squares.Sum(s => s.Side * s.Side));

            foreach (Square square in individual.Squares)
            {
                if (_random.NextDouble() < _mutationProbability)
                {
                    square.X += _random.Next(0, 1);
                    square.Y += _random.Next(0, 1);
                }
            }

            return individual;
        }

        private void GetBestIndividual() {
            _population = _population.OrderBy(i => i.Loss).Take(_populationSize).ToList();
        }

        public void PrintIterationInfo() {
            Console.WriteLine(string.Format("Iteration {0}: Area {1}", _evolutionIter, _population[0].Loss));
        }
    }
}