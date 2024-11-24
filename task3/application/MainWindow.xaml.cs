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
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>

namespace application {
    public class Experiment
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int PopulationSize { get; set; }
        public double MutationProbability { get; set; }
        public List<Rect> Rects { get; set; } = new List<Rect>();
       // public List<PopulationState> PopulationStates { get; set; }
    }

    public class Rect
    {
        public int Id { get; set; }
        public int Size { get; set; }
        public Experiment Experiment { get; set; }
    }
    
    // public class PopulationState
    // {
    //     //public int Id { get; set; }
    //     //public int ExperimentId { get; set; }
    //     // /public int[] Squares { get; set; }
    //     // public int Loss { get; set; }
    //     // public int EvolutionIteration { get; set; }
    //     // public Experiment Experiment { get; set; }
    // }

    public class ApplicationDbContext : DbContext
    {
        public DbSet<Experiment> Experiments { get; set; }
        public DbSet<Rect> Rects { get; set; }
        //public DbSet<PopulationState> PopulationStates { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(
                "server=localhost; port=3306; database=genetic_algorithm; user=root; password=;", 
                new MySqlServerVersion(new Version(10, 4, 32))
            );
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Experiment>()
                .HasKey(exp => exp.Id);
            
            modelBuilder.Entity<Rect>()
                .HasKey(rect => rect.Id);

            modelBuilder.Entity<Rect>()
                .HasOne(rect => rect.Experiment)
                .WithMany(exp => exp.Rects);

            // modelBuilder.Entity<PopulationState>()
            //     .HasKey(ps => ps.Id);

            // modelBuilder.Entity<PopulationState>()
            //     .HasOne(ps => ps.Experiment)
            //     .WithMany(exp => exp.PopulationStates)
            //     .HasForeignKey(ps => ps.ExperimentId);
        }
    }

    public partial class MainWindow : Window
    {
        private GeneticAlgorithm geneticAlgorithm;
        private Random random = new Random();
        public int[] rectQuantities = new int[5];
        int[] rectSizes = new int[0];
        int populationSize = 100;

        double mutationProbability = 0.5;
        Brush[] rectColors = new Brush[0];
        int totalQuantity = 0;
        public double scale = 8;
        public bool findingSolution = false;

        private CancellationTokenSource cancellationTokenSource;
        
        public MainWindow()
        {
            InitializeComponent();
            //LoadExperimentList();
        }

        private void InitializeObjectsData() {
            int index = 0;
            
            rectQuantities[0] = int.TryParse(rect_quantity_1.Text, out var quantity_1) ? quantity_1 : 0;
            rectQuantities[1] = int.TryParse(rect_quantity_2.Text, out var quantity_2) ? quantity_2 : 0;
            rectQuantities[2] = int.TryParse(rect_quantity_3.Text, out var quantity_3) ? quantity_3 : 0;
            rectQuantities[3] = int.TryParse(rect_quantity_4.Text, out var quantity_4) ? quantity_4 : 0;
            rectQuantities[4] = int.TryParse(rect_quantity_5.Text, out var quantity_5) ? quantity_5 : 0;
        
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

        private void OutputBestMetric(int metric)
        {
            best_metric.Text = $"{metric}";
        }

        private void OutputAlgorithmProgress(int progress)
        {
            algorithm_progress.Text = $"iteration {progress}";
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

        private void ClearObjectsData(object sender, RoutedEventArgs e)
        {
            rect_size_1.Text = string.Empty;
            rect_size_2.Text = string.Empty;
            rect_size_3.Text = string.Empty;
            rect_size_4.Text = string.Empty;
            rect_size_5.Text = string.Empty;
            rect_quantity_1.Text = string.Empty;
            rect_quantity_2.Text = string.Empty;
            rect_quantity_3.Text = string.Empty;
            rect_quantity_4.Text = string.Empty;
            rect_quantity_5.Text = string.Empty;
            population_size.Text = string.Empty;
            mutation_probability.Text = string.Empty;

            StopFindingSolution(sender, e);

            canvas.Children.Clear();
        }

        private async void StartFindingSolution(object sender, RoutedEventArgs e)
        {   
            InitializeObjectsData();
            
            if (totalQuantity > 0) {
                geneticAlgorithm = new GeneticAlgorithm(rectSizes, populationSize, mutationProbability);
            
                findingSolution = true;
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
        }

        private void StopFindingSolution(object sender, RoutedEventArgs e)
        {
            findingSolution = false;
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
        }

        private async void LoadExperiment(object sender, RoutedEventArgs e)
        {
            // using (var context = new ApplicationDbContext())
            // {
            //     var experiment = await context.Experiments
            //         .Include(e => e.PopulationStates)
            //         .FirstOrDefaultAsync(e => e.Name == "experiment1");

            //     if (experiment != null)
            //     {
            //         populationSize = experiment.PopulationSize;
            //         mutationProbability = experiment.MutationProbability;
            //         rectSizes = experiment.RectSizes;

            //         geneticAlgorithm = new GeneticAlgorithm(rectSizes, populationSize, mutationProbability);
            //        // geneticAlgorithm._population[0].Squares = experiment.PopulationStates[0].Squares.Select(s => new Square(s)).ToArray();
            //         geneticAlgorithm._population[0].Loss = experiment.PopulationStates[0].Loss;
            //         geneticAlgorithm._evolutionIter = experiment.PopulationStates[0].EvolutionIteration;

            //         StartFindingSolution(sender, e);
            //     }
            // }
        }

        private async void SaveExperiment(object sender, RoutedEventArgs e)
        {         
            try 
            {
                using (var context = new ApplicationDbContext())
                {
                    InitializeObjectsData();

                    var rects = new List<Rect>();

                    foreach (var size in rectSizes)
                    {
                        rects.Add(new Rect { Size = size });
                    }
                    
                    Experiment experiment = new Experiment
                    {
                        Name = "experiment1",
                        PopulationSize = populationSize,
                        MutationProbability = mutationProbability,
                        Rects = rects
                        //PopulationStates = new List<PopulationState>
                        // {
                        //     new PopulationState
                        //     {
                        //         //Squares = geneticAlgorithm._population[0].Squares.Select(s => s.ToArray()).ToArray(),
                        //         Loss = geneticAlgorithm._population[0].Loss,
                        //         EvolutionIteration = geneticAlgorithm._evolutionIter
                        //     }
                        // }s
                    };

                    context.Experiments.Add(experiment);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении эксперимента: {ex.Message} {ex.InnerException?.Message}");
            }
        }
    }
}