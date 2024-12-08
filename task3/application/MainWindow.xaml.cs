using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using EvolutionaryAlgorithm;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>

namespace application {    
    public partial class MainWindow : Window
    {
        private Random random = new Random();
        private GeneticAlgorithm geneticAlgorithm;
        public int[] rectQuantities = new int[5];
        int[] rectSizes = new int[0];
        int populationSize = 100;

        double mutationProbability = 0.5;
        Brush[] rectColors = new Brush[0];
        int totalQuantity = 0;
        public double scale = 8;
        public bool findingSolution = false;
        private CancellationTokenSource cancellationTokenSource;
        private bool isLoadExperiment = false;
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private TextBox GetSizeTextBox(int index)
        {
            switch (index)
            {
                case 0: return rect_size_1;
                case 1: return rect_size_2;
                case 2: return rect_size_3;
                case 3: return rect_size_4;
                case 4: return rect_size_5;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private TextBox GetQuantityTextBox(int index)
        {
            switch (index)
            {
                case 0: return rect_quantity_1;
                case 1: return rect_quantity_2;
                case 2: return rect_quantity_3;
                case 3: return rect_quantity_4;
                case 4: return rect_quantity_5;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private void InitializeObjectsData() {
            int index = 0;
            
            for (int i = 0; i < 5; i++)
            {
                rectQuantities[i] = int.TryParse(GetQuantityTextBox(i).Text, out var quantity) ? quantity : 0;
            }
        
            totalQuantity = rectQuantities.Sum();
            rectSizes = new int[totalQuantity];
            rectColors = new Brush[totalQuantity];

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < rectQuantities[i]; j++)
                {
                    rectSizes[index] = int.TryParse(GetSizeTextBox(i).Text, out var size) ? size : 0;
                    rectColors[index] = GenerateColor();
                    index++;
                }
            }

            populationSize = int.TryParse(population_size.Text, out var population) ? population : 100;
            mutationProbability = double.TryParse(mutation_probability.Text, out var probability) ? probability : 0.5;
        }

        private Brush GenerateColor() {
            byte r = (byte)random.Next(256);
            byte g = (byte)random.Next(256);
            byte b = (byte)random.Next(256);

            Brush fill = new SolidColorBrush(Color.FromRgb(r, g, b));
            return fill;
        }

        private void DrawSquare(double x, double y, double side, Brush fill)
        {
            double scaledX = x * scale;
            double scaledY = y * scale;
            double scaledSide = side * scale;

            if (x < 0 || y < 0 || scaledX + scaledSide > canvas.ActualWidth || scaledY + scaledSide > canvas.ActualHeight)
            {
                return;
            }

            Rectangle square = new Rectangle
            {
                Width = scaledSide,
                Height = scaledSide,
                Stroke = Brushes.Black,
                Fill = fill
            };

            Canvas.SetLeft(square, scaledX);
            Canvas.SetTop(square, scaledY);

            canvas.Children.Add(square);
        }

        private void OutputBestMetric(int metric)
        {
            best_metric.Text = $"{metric}";
        }

        private void OutputAlgorithmProgress(int progress)
        {
            algorithm_progress.Text = $"iteration {progress}";
        }

        private void ClearObjectsData(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 5; i++)
            {
                GetQuantityTextBox(i).Text = string.Empty;
                GetSizeTextBox(i).Text = string.Empty;
            }

            population_size.Text = string.Empty;
            mutation_probability.Text = string.Empty;
        }

        private async void RunFindingSolution() {
            cancellationTokenSource = new CancellationTokenSource();

            await Task.Factory.StartNew(() =>
            {
                while (findingSolution && !cancellationTokenSource.Token.IsCancellationRequested)
                {
                    int index = 0;
                    geneticAlgorithm.Evolve();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        canvas.Children.Clear();

                        foreach (var item in geneticAlgorithm._population[0].Squares)
                        {
                            DrawSquare(item.X, item.Y, item.Side, rectColors[index]);                            
                            index++;
                        }

                        OutputBestMetric(geneticAlgorithm._population[0].Loss);
                        OutputAlgorithmProgress(geneticAlgorithm._evolutionIter);
                    });
                }
            }, cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private async void StartFindingSolution(object sender, RoutedEventArgs e)
        {   
            if (!isLoadExperiment) {
                InitializeObjectsData();

                if (totalQuantity == 0) {
                    return;
                }   

                geneticAlgorithm = new GeneticAlgorithm(rectSizes, populationSize, mutationProbability);
            } else {
                isLoadExperiment = false;
            }

            findingSolution = true;
            RunFindingSolution();
        }

        private void StopFindingSolution(object sender, RoutedEventArgs e)
        {
            findingSolution = false;
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
        }

        private async void LoadExperiment(object sender, RoutedEventArgs e)
        {
            try 
            {
                using (var context = new ApplicationDbContext())
                {
                    var loadWindow = new LoadWindow(context.Experiments.Select(exp => exp.Name).ToList());

                    if (loadWindow.ShowDialog() == true) {
                        var experiment = (context.Experiments.ToArray())[loadWindow.ExperimentIndex];
                        var rects = context.Rects.Where(rect => rect.ExperimentId == experiment.Id).ToArray();
                        var individuals = context.Individuals.Where(ind => ind.ExperimentId == experiment.Id).ToArray();

                        for (int i = 0; i < rects.Length; i++)
                        {
                            GetQuantityTextBox(i).Text = rects[i].Quantity.ToString();
                            GetSizeTextBox(i).Text = rects[i].Size.ToString();
                        }  

                        population_size.Text = experiment.PopulationSize.ToString();
                        mutation_probability.Text = experiment.MutationProbability.ToString();

                        InitializeObjectsData();

                        geneticAlgorithm = new GeneticAlgorithm(rectSizes, populationSize, mutationProbability);

                        geneticAlgorithm._evolutionIter = experiment.EvolutionIteration;
                        geneticAlgorithm._population.Clear();

                        foreach (var individual in individuals) {
                            var squares = context.IndividualSquares
                                .Where(
                                    inds => inds.IndividualId == individual.Id
                                )
                                .Select(
                                    square => new EvolutionaryAlgorithm.Square {
                                        Side = square.Side, 
                                        X = square.X, 
                                        Y = square.Y
                                    }
                                )
                                .ToList();

                            var newIndividual = new EvolutionaryAlgorithm.Individual(squares);
                            newIndividual.Loss = individual.Loss;

                            geneticAlgorithm._population.Add(newIndividual);
                        }
                        
                        isLoadExperiment = true;
                        
                        MessageBox.Show($"Эксперимент: {experiment.Name} успешно загружен");
                    }
                }
            } catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке эксперимента: {ex.Message} {ex.InnerException?.Message}");
            }    
        }

        private async void SaveExperiment(object sender, RoutedEventArgs e)
        {   
            try 
            {
                var saveWindow = new SaveWindow();

                if (saveWindow.ShowDialog() == true) {
                    using (var context = new ApplicationDbContext())
                    {
                        InitializeObjectsData();

                        var rects = new List<Rect>();
                        var individuals = new List<Individual>();
                        var totalQuantity = 0;

                        foreach (var rectQuantity in rectQuantities)
                        {
                            rects.Add(
                                new Rect { 
                                    Size = rectSizes[totalQuantity], 
                                    Quantity = rectQuantity
                                }
                            );

                            if ((totalQuantity += rectQuantity) >= rectSizes.Length) {
                                break;
                            }
                        }

                        foreach (var individual in geneticAlgorithm._population) {
                            var individualSquares = new List<IndividualSquare>();

                            foreach (var individualSquare in individual.Squares) {
                                individualSquares.Add(
                                    new IndividualSquare {
                                        Side = individualSquare.Side,
                                        X = individualSquare.X,
                                        Y = individualSquare.Y
                                    }
                                );
                            }

                            individuals.Add(
                                new Individual {
                                    Loss = individual.Loss,
                                    IndividualSquares = individualSquares
                                }
                            );
                        }
                        
                        Experiment experiment = new Experiment
                        {
                            Name = saveWindow.Name,
                            PopulationSize = populationSize,
                            MutationProbability = mutationProbability,
                            EvolutionIteration = geneticAlgorithm._evolutionIter,
                            Rects = rects,
                            Individuals = individuals
                        };

                        context.Experiments.Add(experiment);
                        await context.SaveChangesAsync();
                        MessageBox.Show($"Эксперимент {saveWindow.Name} успешно сохранен");
                    }
                }
            } catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении эксперимента: {ex.Message} {ex.InnerException?.Message}");
            }    
        }
    }
}