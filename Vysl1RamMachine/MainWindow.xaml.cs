using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Vysl1RamMachine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ControlUnit ControlUnit { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            const string funProgramFilepath = "fun.txt";

            if (File.Exists(funProgramFilepath))
                txtInstructions.Text = File.ReadAllText(funProgramFilepath);
        }

        private void BtnLoadFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            var result = dialog.ShowDialog();
            if ((bool)result)
            {
                var filename = dialog.FileName;
                string content = File.ReadAllText(filename);

                txtInstructions.Text = content;
            }
        }

        private void BtnRunMachine_Click(object sender, RoutedEventArgs e)
        {
            string instructionsText = txtInstructions.Text,
                inputText = txtInputTape.Text;

            lstRegister.Items.Clear();

            if (string.IsNullOrWhiteSpace(instructionsText))
            {
                ShowWarning("There are no instructions in the input.", "Empty input");
                return;
            }
            
            if (string.IsNullOrEmpty(inputText))
            {
                ShowWarning("The input tape is empty.", "Empty input");
                return;
            }

            string[] lines = instructionsText.Split('\n');

            IList<Operation> operations = new List<Operation>();
            foreach (var line in lines)
            {
                operations.Add(new Operation(line));
            }

            var validOperations = operations.Where(o => o.IsValid).ToList();

            if (validOperations.Count == 0)
            {
                ShowError("There are no valid operations in the input.", "No valid operations in input");
                return;
            }
                
            string result = "";
            
            try
            {
                ControlUnit = new ControlUnit(inputText, validOperations, (bool)chcLinesFromZero.IsChecked);

                result = ControlUnit.Run();

                lstRegister.Items.Clear();
                ControlUnit.ExecutionHistory.ForEach(info => lstRegister.Items.Add(info));
            }
            catch (Exception ex)
            {
                ShowError(
                    $"Exception happened during RAM execution: {ex.Message} Maybe you need a '#' at the end?", 
                    "Unknown error");
                return;
            }

            txtResult.Text = $"Result: {result}";
            txtResult.Foreground = Brushes.Green;
            txtResult.FontWeight = FontWeights.Bold;
        }

        private void ShowError(string messageBoxText, string caption)
        {
            MessageBox.Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Error);
            txtResult.Foreground = Brushes.Red;
            txtResult.FontWeight = FontWeights.Bold;
        }

        private void ShowWarning(string messageBoxText, string caption)
        {
            MessageBox.Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
