using System;

namespace EvolutionaryAlgorithm
{
    class Program
    {
        static void Main(string[] args)
        {
            var geneticAlgorithm = new GeneticAlgorithm(20, new int[] { 1, 2, 3});

            geneticAlgorithm.PrintIterationInfo();

            while (!Console.KeyAvailable)
            {
                geneticAlgorithm.Run();
                geneticAlgorithm.PrintIterationInfo();
                System.Threading.Thread.Sleep(10);
            }

            Console.WriteLine("\nSolution");
            geneticAlgorithm.PrintIterationInfo();

            Console.ReadLine();
        }
    }
}