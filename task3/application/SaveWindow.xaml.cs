using System.Windows;

/// <summary>
/// Interaction logic for SaveWindow.xaml
/// </summary>

namespace application {
    public partial class SaveWindow : Window
    {
        public string Name { get; set; }

        public SaveWindow() {
            InitializeComponent();
        }

        private void SaveExperiment(object sender, RoutedEventArgs e) {
            Name = experiment_name.Text;
            DialogResult = true;
            Close();
        }

        private void CancelSaveExperiment(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }
    }
}