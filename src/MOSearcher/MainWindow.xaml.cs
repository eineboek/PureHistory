using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

namespace MOSearcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OpenFileDialog FilePrompt;
        private MOReader moReader;
        private List<MOGridData> DataList;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            FilePrompt = new();
            FilePrompt.Title = "Open File";
            FilePrompt.DefaultExt = ".mo";
            FilePrompt.Filter = "MO Files (*.mo) | *.mo";

            bool? dialogResult = FilePrompt.ShowDialog();

            if (dialogResult == true)
            {
                moReader = new(FilePrompt.FileName);
                MOGrid.Items.Clear();

                DataList = new();

                for (int i = 0; i < moReader.Count; i++)
                {
                    MOLine line = moReader[i];
                    DataList.Add(new MOGridData { index = line.Index, identifier = line.Original, content = line.Translated });
                }
                MOGrid.ItemsSource = DataList;
            }
        }

        private void UnloadData(object sender, RoutedEventArgs e)
        {
            MOGrid.ItemsSource = null;
            DataList.Clear();
            moReader.Dispose();
        }

        private void DoSearch(object sender, RoutedEventArgs e)
        {
        }

        private void PreviousResult(object sender, RoutedEventArgs e)
        {
        }

        private void NextResult(object sender, RoutedEventArgs e)
        {
        }
    }
}