namespace application {    
    public class Individual
    {
        public int Id { get; set; }
        public int ExperimentId { get; set; }
        public int Loss { get; set; }
        public List<IndividualSquare> IndividualSquares { get; set; } = new List<IndividualSquare>();
        public Experiment Experiment { get; set; }
    }
}