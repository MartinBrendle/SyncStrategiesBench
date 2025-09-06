using System.IO;
using System.Text.Json;
using System.Windows;
using AIO.Utils;

namespace ParallelDeviceUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BaselineMessungTest_Click(object sender, RoutedEventArgs e)
        {
            var window = new BaselineMessungTest(LadeExperimenteAusDatei());
            window.Show();
        }

        private void SemaphoreTest_Click(object sender, RoutedEventArgs e)
        {
          var window = new SemaphoreTest(LadeExperimenteAusDatei());
          window.Show();
        }

        private void TaskQueueTest_Click(object sender, RoutedEventArgs e)
        {
            var window = new TaskQueueTest(LadeExperimenteAusDatei());
            window.Show();
        }

        private void CopyTest_Click(object sender, RoutedEventArgs e)
        {
            var window = new CopyTest(LadeExperimenteAusDatei());
            window.Show();
        }

        private void DomainCopyTest_Click(object sender, RoutedEventArgs e)
        {
          var window = new DomainCopyTest(LadeExperimenteAusDatei());
          window.Show();
        }

        private void SessionCopyTest_Click(object sender, RoutedEventArgs e)
        {
          var window = new SessionCopyTest(LadeExperimenteAusDatei());
          window.Show();
        }

        private void CASTest_Click(object sender, RoutedEventArgs e)
        {
          var window = new CASTest(LadeExperimenteAusDatei());
          window.Show();
        }
        
        private static List<ExperimentConfig> LadeExperimenteAusDatei()
        {
            string trunkPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", ".."));
            string dateipfad = Path.Combine(trunkPath, "experimente_config.json");

            if (!File.Exists(dateipfad))
                throw new FileNotFoundException($"Konfigurationsdatei nicht gefunden: {dateipfad}");

            string json = File.ReadAllText(dateipfad);
            ExperimentConfigWrapper? wrapper = JsonSerializer.Deserialize<ExperimentConfigWrapper>(json);

            if (wrapper == null || wrapper.Experimente == null || wrapper.Experimente.Count == 0)
                throw new InvalidOperationException("Keine Experimente gefunden!");

            return wrapper.Experimente;
        }

    }
}