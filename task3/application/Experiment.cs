namespace application {
    public class Experiment
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int PopulationSize { get; set; }
        public double MutationProbability { get; set; }
        public int EvolutionIteration { get; set; }
        public List<Rect> Rects { get; set; } = new List<Rect>();
        public List<Individual> Individuals { get; set; } = new List<Individual>();
    }
}