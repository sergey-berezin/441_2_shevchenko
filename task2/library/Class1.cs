using System;
using System.Collections.Generic;
using System.Linq;

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

        private void CalculateLoss()
        {
            int minX = Squares.Min(s => s.X);
            int minY = Squares.Min(s => s.Y);
            int maxX = Squares.Max(s => s.X + s.Side);
            int maxY = Squares.Max(s => s.Y + s.Side);

            Loss = (maxX - minX) * (maxY - minY);

            foreach (var square in Squares)
            {
                foreach (var otherSquare in Squares)
                {
                    if (square != otherSquare && DoSquaresIntersect(square, otherSquare))
                    {
                        Loss = int.MaxValue;
                        return;
                    }
                }
            }
        }

        private bool DoSquaresIntersect(Square square1, Square square2)
        {
            return square1.X < square2.X + square2.Side &&
                   square2.X < square1.X + square1.Side &&
                   square1.Y < square2.Y + square2.Side &&
                   square2.Y < square1.Y + square1.Side;
        }
    }

    public class GeneticAlgorithm
    {
        private Random _random = new Random();
        public List<Individual> _population;
        private int _populationSize;
        private int _numSquares;
        private int[] _sideLengths;
        public int _evolutionIter; 

        public GeneticAlgorithm(int populationSize, int[] sideLengths)
        {
            _populationSize = populationSize;
            _sideLengths = sideLengths;
            _numSquares = sideLengths.Length;
            _evolutionIter = 0;
            _population = InitializePopulation();
            GetBestIndividual();
        }

        public void Run()
        {
            Evolve();
        }

        private List<Individual> InitializePopulation()
        {
            List<Individual> population = new List<Individual>();

            for (int i = 0; i < _populationSize; i++)
            {
                List<Square> squares = new List<Square>();

                for (int j = 0; j < _numSquares; j++)
                {
                    int side = _sideLengths[j];
                    int x = _random.Next(0, 100);
                    int y = _random.Next(0, 100);

                    squares.Add(new Square { Side = side, X = x, Y = y });
                }

                population.Add(new Individual(squares));
            }

            return population;
        }

        private void Evolve ()
        {
            for (int i = 0; i < _populationSize; i++)
            {
                Mutate(i);
                Crossover(i, _random.Next(0, _populationSize));
            }

            GetBestIndividual();
            _evolutionIter++;
        }

        private void Crossover(int parent1, int parent2)
        {
            List<Square> childSquares = new List<Square>();

            for (int i = 0; i < _numSquares; i++)
            {

                if (_random.NextDouble() < 0.5)
                {
                    childSquares.Add(_population[parent1].Squares[i]);
                }
                else
                {
                    childSquares.Add(_population[parent2].Squares[i]);
                }

            }

            _population.Add(new Individual(childSquares));
        }

        private void Mutate(int individual)
        {
            int squareIndex = _random.Next(0, _population[individual].Squares.Count);
            int totalArea = _population[individual].Squares.Sum(s =>s.Side * s.Side);

            int maxX = (int)Math.Sqrt(totalArea) * 2;
            int maxY = maxX;

            _population[individual].Squares[squareIndex].X = _random.Next(0, maxX);
            _population[individual].Squares[squareIndex].Y = _random.Next(0, maxY);
        }

        private void GetBestIndividual() {
            _population = _population.OrderBy(i => i.Loss).Take(_populationSize).ToList();
        }

        public void PrintIterationInfo() {
            Console.WriteLine(string.Format("Iteration {0}: Area {1}", _evolutionIter, _population[0].Loss));
        }
    }
}