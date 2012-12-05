using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ExtractorLib;

namespace Dump_Extractor_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private StreamWriter outputFile;
        private ExtractorLib.ExtractorLib sty;
        private DataLayer dataLayer;
        private string[] tables;
        private Thread runner;

        public MainWindow()
        {
            InitializeComponent();
            outputFile = null;
            sty = null;
            dataLayer = null;
            runner = null;
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "YAML Files (*.yaml)|*.yaml|All Files (*.*)|*.*";
            dlg.Title = "YAML Save Location";
            dlg.ShowDialog();

            if (dlg.FileName != "")
            {
                if (outputFile != null)
                {
                    outputFile.Close();
                }
                outputFile = new StreamWriter(dlg.OpenFile());
                txtPath.Text = dlg.FileName;
                btnExtract.IsEnabled = lstTables.SelectedItems.Count > 0;
            }
        }

        private void updateProgress(int progress)
        {
            prgProgress.Value += progress;
        }

        private void completeProgress()
        {
            prgProgress.Value = 100;
        }

        public delegate void UpdateProgressCallback(int progress);
        public delegate void CompleteProgressCallback();

        private void btnExtract_Click(object sender, RoutedEventArgs e)
        {
            if (runner != null) return;
            prgProgress.Value = 0;
            sty = new ExtractorLib.ExtractorLib(String.Join(",", lstTables.SelectedItems.OfType<string>().ToArray<string>()),
                                outputFile, dataLayer, 5);
            sty.MadeProgress += (o, v) => {
                this.Dispatcher.Invoke(new UpdateProgressCallback(this.updateProgress), new object[] {5});
            };
            sty.ExtractionFinished += (o, v) => {
                this.Dispatcher.Invoke(new CompleteProgressCallback(this.completeProgress));
                System.Windows.Forms.MessageBox.Show("Extraction Complete!");
                runner = null;
            };
            runner = new Thread(sty.Convert);
            runner.Start();
        }

        private void btnLoadTables_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dataLayer = new DataLayer(txtConnection.Text);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
                return;
            }
            tables = dataLayer.TableList;
            Array.Sort<string>(tables);
            lstTables.ItemsSource = tables;
            lstTables.SelectAll();
            btnExtract.IsEnabled = txtPath.Text != "";
        }

        private void lstTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnExtract.IsEnabled = (txtPath.Text != "" && lstTables.SelectedItems.Count > 0);
        }
    }
}
