using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace WPF__BackgroundWorker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private static BackgroundWorker backgroundWorker;
        private static int Sleep = 100;
        ManualResetEvent _shutdownEvent = new ManualResetEvent(false);
        ManualResetEvent _pauseEvent = new ManualResetEvent(true);
        // Check pause state
        public bool IsPaused { get { return !_pauseEvent.WaitOne(0); } }
        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            backgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            backgroundWorker.DoWork += backgroundWorker_DoWork;
            //For the display of operation progress to UI.    
            backgroundWorker.ProgressChanged += backgroundWorker_ProgressChanged;
            //After the completation of operation.    
            backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
            // 
            backgroundWorker.RunWorkerAsync();
            //Console.ReadLine();

            //if (backgroundWorker.IsBusy)
            //{
            //    backgroundWorker.CancelAsync();
            //    //Console.ReadLine();
            //}
        }
        public void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < 100; i++)
            {
                _pauseEvent.WaitOne(Timeout.Infinite);
                if (_shutdownEvent.WaitOne(0))
                    break;

                if (backgroundWorker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                backgroundWorker.ReportProgress(i);

                Thread.Sleep(Sleep);
                e.Result = 100;
            }
        }

        public void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.progress.Value = e.ProgressPercentage;
        }

        public void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            if (e.Cancelled)
            {
                lblStatus.Content = "Cancel";
            }
            else if (e.Error != null)
            {
                lblStatus.Content = e.Error;
            }
            else
            {
                lblStatus.Content = e.Result;
            }
        }
        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            if (IsPaused)
            {
                btnPause.Content = "Pause";
                _pauseEvent.Set();
            }
            else
            {
                btnPause.Content = "Resume";
                _pauseEvent.Reset();
            }
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            // Signal the shutdown event
            //_shutdownEvent.Set();

            // Make sure to resume any paused threads
            _pauseEvent.Set();

            // Wait for the thread to exit
            backgroundWorker.CancelAsync();
            this.progress.Value = 0;
            //_thread.Join();
        }
    }
}