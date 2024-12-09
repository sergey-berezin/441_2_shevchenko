namespace application {
    public class Rect
    {
        public int Id { get; set; }
        public int ExperimentId { get; set; }
        public int Size { get; set; }
        public int Quantity { get; set; }
        public Experiment Experiment { get; set; }
    }
}