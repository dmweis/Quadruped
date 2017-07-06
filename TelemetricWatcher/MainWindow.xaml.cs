using System;
using System.Windows;

namespace TelemetricWatcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = new MainViewModel();
            InitializeComponent();
            Closed += OnClosed;
        }

        private void OnClosed(object sender, EventArgs eventArgs)
        {
            (DataContext as MainViewModel)?.Dispose();
        }
    }
}
