using System.Windows;

/// <summary>
/// Interaction logic for LoadWindow.xaml
/// </summary>

namespace application {
    public partial class LoadWindow : Window
    {
        public string ExperimentName { get; set; }

        public LoadWindow(List<string> Experiments) {
            InitializeComponent();

            experiments_list_box.ItemsSource = Experiments;
        }

        private void LoadExperiment(object sender, RoutedEventArgs e) {
            if (experiments_list_box.SelectedItem != null) {
                ExperimentName = experiments_list_box.SelectedItem.ToString();
                DialogResult = true;
                Close();
            }
        }

        private void CancelLoadExperiment(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }
    }
}