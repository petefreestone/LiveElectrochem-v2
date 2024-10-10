using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Helper.Common;
using Helper.Extensions;
using Helper.Analysis;
using MathNet.Numerics.Statistics;
using System.Xml.Serialization;
using System.Xml;
using System.ComponentModel;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using System.Net.Mail;
using System.Net;
using Plotting;
using OxyPlot.Series;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Wpf;
using System.Windows.Media;
using System.Reflection;
using Helper.Analysis.Enums;
using System.Windows.Media.Animation;
using OxyPlot.Legends;
using OxyPlot.Annotations;
using System.Runtime.CompilerServices;

namespace Live_Electrochem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

#if DEBUG
            SetValue(SourceFolderProperty, @"C:\Users\pfre017\Desktop\#17-3 20th June Calib\");
            //SetValue(SourceFolderProperty, @"P:\Software Development\Live_Electrochem\Live_Electrochem\test folder\");
            //SetValue(SourceFolderProperty, @"C: \Users\pfre017\Desktop\Live_Electrochem\Live_Electrochem\test folder\");
#else
            SetValue(SourceFolderProperty, @"U:\LipskiLab\Shared\");
#endif

            SetValue(LogsProperty, new ObservableCollection<string>());
            SetValue(FilesProperty, new ObservableCollection<LVBinFile>());
            SetValue(CalibrationStepsProperty, new ObservableCollection<Event>());

            SetValue(AllIntegratedModelProperty, new IntegratedViewModel()
            {
                Title = "Temporal Charge",
                Model = new PlotModel()
                {
                    Title = "Temporal Charge",
                },
            });

            SetValue(ScopeModelProperty, new ScopeViewModel()
            {
                Title = "Voltammogram",
                Model = new PlotModel()
                {
                    Title = "Voltammogram",
                },
            });

            SetValue(ScopeRawModelProperty, new ScopeViewModel()
            {
                Title = "Voltammograms Raw",
                Model = new PlotModel()
                {
                    Title = "Voltammograms Raw",
                    IsLegendVisible = true,
                },
            });

            SetValue(IntegratedModelProperty, new IntegratedViewModel()
            {
                Title = "Charge",
                Model = new PlotModel()
                {
                    Title = "Charge",
                },
            });

            //SetValue(ScopeModelProperty, new ScopeViewModel() { AutoScaleX = true, AutoScaleY = true });

            string title = "Live Electrochemistry - Pete Freestone 2016 (single+multi-channel version; 12th December 2019)";
            this.Title = title;
            AddLog(title);

            //CalculateGainMultiplier(this.nA_V, this.AD_Gain);

            LoadSettings();
        }

        private System.IO.FileSystemWatcher watcher;

        #region Dependency Properties

        public ObservableCollection<Event> CalibrationSteps
        {
            get { return (ObservableCollection<Event>)GetValue(CalibrationStepsProperty); }
            set { SetValue(CalibrationStepsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CalibrationSteps.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CalibrationStepsProperty =
            DependencyProperty.Register("CalibrationSteps", typeof(ObservableCollection<Event>), typeof(MainWindow), new PropertyMetadata(null));

        public int CalibrationVoltammogramPeakIndex
        {
            get { return (int)GetValue(CalibrationVoltammogramPeakIndexProperty); }
            set { SetValue(CalibrationVoltammogramPeakIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CalibrationVoltammogramPeakIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CalibrationVoltammogramPeakIndexProperty =
            DependencyProperty.Register("CalibrationVoltammogramPeakIndex", typeof(int), typeof(MainWindow), new PropertyMetadata(617));

        public int CalibrationRMSWindowCentre
        {
            get { return (int)GetValue(CalibrationRMSWindowCentreProperty); }
            set { SetValue(CalibrationRMSWindowCentreProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CalibrationRMSWindowCentre.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CalibrationRMSWindowCentreProperty =
            DependencyProperty.Register("CalibrationRMSWindowCentre", typeof(int), typeof(MainWindow), new PropertyMetadata(1000));

        public int CalibrationRMSWindowWidth
        {
            get { return (int)GetValue(CalibrationRMSWindowWidthProperty); }
            set { SetValue(CalibrationRMSWindowWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CalibrationRMSWindowWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CalibrationRMSWindowWidthProperty =
            DependencyProperty.Register("CalibrationRMSWindowWidth", typeof(int), typeof(MainWindow), new PropertyMetadata(100));

        public string CalibrationSource
        {
            get { return (string)GetValue(CalibrationSourceProperty); }
            set { SetValue(CalibrationSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CalibrationSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CalibrationSourceProperty =
            DependencyProperty.Register("CalibrationSource", typeof(string), typeof(MainWindow), new PropertyMetadata(null));

        public AIChannel SelectedAIChannel
        {
            get { return (AIChannel)GetValue(SelectedAIChannelProperty); }
            set { SetValue(SelectedAIChannelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedAIChannel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedAIChannelProperty =
            DependencyProperty.Register("SelectedAIChannel", typeof(AIChannel), typeof(MainWindow), new PropertyMetadata(null));

        public AOChannel SelectedAOChannel
        {
            get { return (AOChannel)GetValue(SelectedAOChannelProperty); }
            set { SetValue(SelectedAOChannelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedAOChannel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedAOChannelProperty =
            DependencyProperty.Register("SelectedAOChannel", typeof(AOChannel), typeof(MainWindow), new PropertyMetadata(null));

        public bool PurgeLayoutOnExit
        {
            get { return (bool)GetValue(PurgeLayoutOnExitProperty); }
            set { SetValue(PurgeLayoutOnExitProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PurgeLayoutOnExit.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PurgeLayoutOnExitProperty =
            DependencyProperty.Register("PurgeLayoutOnExit", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public bool IsFilterEnabled
        {
            get { return (bool)GetValue(IsFilterEnabledProperty); }
            set { SetValue(IsFilterEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsFilterEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFilterEnabledProperty =
            DependencyProperty.Register("IsFilterEnabled", typeof(bool), typeof(MainWindow), new PropertyMetadata(true, OnIsFilterEnabledChanged));

        private static void OnIsFilterEnabledChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            MainWindow w = (MainWindow)o;
            w.Reset();
        }

        public double FilterCutOffFrequency
        {
            get { return (double)GetValue(FilterCutOffFrequencyProperty); }
            set { SetValue(FilterCutOffFrequencyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FilterCutOffFrequency.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilterCutOffFrequencyProperty =
            DependencyProperty.Register("FilterCutOffFrequency", typeof(double), typeof(MainWindow), new PropertyMetadata(5000d, OnFilterCutOffFrequencyChanged));

        private static void OnFilterCutOffFrequencyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            MainWindow w = (MainWindow)o;
            w.Reset();              //not sure if this truly does reset the files and cause the new CutOff Frequency to be used
        }

        public bool IsExtensionFilterEnabled
        {
            get { return (bool)GetValue(IsExtensionFilterEnabledProperty); }
            set { SetValue(IsExtensionFilterEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsExtensionFilterEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsExtensionFilterEnabledProperty =
            DependencyProperty.Register("IsExtensionFilterEnabled", typeof(bool), typeof(MainWindow), new PropertyMetadata(true));

        public string ExtensionFilterString
        {
            get { return (string)GetValue(ExtensionFilterStringProperty); }
            set { SetValue(ExtensionFilterStringProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ExtensionFilterString.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExtensionFilterStringProperty =
            DependencyProperty.Register("ExtensionFilterString", typeof(string), typeof(MainWindow), new PropertyMetadata("bin"));

        public decimal SelectedCharge
        {
            get { return (decimal)GetValue(SelectedChargeProperty); }
            set { SetValue(SelectedChargeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedChargeProperty =
            DependencyProperty.Register("SelectedCharge", typeof(decimal), typeof(MainWindow), new PropertyMetadata(0m));

        public DateTime ExperimentStart
        {
            get { return (DateTime)GetValue(ExperimentStartProperty); }
            set { SetValue(ExperimentStartProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExperimentStartProperty =
            DependencyProperty.Register("ExperimentStart", typeof(DateTime), typeof(MainWindow), new PropertyMetadata(null));

        public bool IsExperimentStarted
        {
            get { return (bool)GetValue(IsExperimentStartedProperty); }
            set { SetValue(IsExperimentStartedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsExperimentStartedProperty =
            DependencyProperty.Register("IsExperimentStarted", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public decimal Offset
        {
            get { return (decimal)GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register("Offset", typeof(decimal), typeof(MainWindow), new PropertyMetadata(0m));

        public int BackgroundSubtractionSweepCount
        {
            get { return (int)GetValue(BackgroundSubtractionSweepCountProperty); }
            set { SetValue(BackgroundSubtractionSweepCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundSubtractionSweepCountProperty =
            DependencyProperty.Register("BackgroundSubtractionSweepCount", typeof(int), typeof(MainWindow), new PropertyMetadata(1));

        public double COV
        {
            get { return (double)GetValue(COVProperty); }
            set { SetValue(COVProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty COVProperty =
            DependencyProperty.Register("COV", typeof(double), typeof(MainWindow), new PropertyMetadata(0d));

        public int COV_window
        {
            get { return (int)GetValue(COV_windowProperty); }
            set { SetValue(COV_windowProperty, value); }
        }

        // Using a DependencyProperty as the backing store for COV_window.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty COV_windowProperty =
            DependencyProperty.Register("COV_window", typeof(int), typeof(MainWindow), new PropertyMetadata(3));


        public ObservableCollection<LVBinFile> Files
        {
            get { return (ObservableCollection<LVBinFile>)GetValue(FilesProperty); }
            set { SetValue(FilesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilesProperty =
            DependencyProperty.Register("Files", typeof(ObservableCollection<LVBinFile>), typeof(MainWindow), new PropertyMetadata(null));

        public LVBinFile SelectedFile
        {
            get { return (LVBinFile)GetValue(SelectedFileProperty); }
            set { SetValue(SelectedFileProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedFileProperty =
            DependencyProperty.Register("SelectedFile", typeof(LVBinFile), typeof(MainWindow), new PropertyMetadata(null, SelectedFileChanged));

        private static void SelectedFileChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            //I don't think is necessary as it happens when the file is processed
            MainWindow window = (MainWindow)o;
            int old = window.SelectedSweep;
            window.SelectedSweep = 0;
            window.SelectedSweep = old;

            LVBinFile file = (LVBinFile)e.NewValue;

            if (file == null)
                return;
            if (file.AIChannels == null)
                return;
            if (file.AIChannels.Count == 0)
                return;

            window.IntegratedModel.Clear();

            double[] x_data = DataAnalysisHelper.FillSeries<double>(file.AIChannels.First().IntegratedData.Length, 1, 0);
            //x_data.FillSeriesArray(1);

            foreach (AIChannel channel in file.AIChannels)
            {
                window.IntegratedModel.AddLineSeries(x_data, channel.IntegratedData.Select(a => (double)a));
                ((OxyPlot.Series.LineSeries)window.IntegratedModel.Model.Series.Last()).Color = channel.Stroke.ToOxyColor();
            }

            //int oldSweep = window.SelectedSweep;
            //window.SelectedSweep = 0;
            //window.SelectedSweep = oldSweep;

            window.IntegratedModel.Model.InvalidatePlot(true);
            window.IntegratedModel.ScalePlotAxes(false);

            window.CalculateSD();
        }

        public int LowerSweepBound
        {
            get { return (int)GetValue(LowerSweepBoundProperty); }
            set { SetValue(LowerSweepBoundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LowerSweepBoundProperty =
            DependencyProperty.Register("LowerSweepBound", typeof(int), typeof(MainWindow), new PropertyMetadata(1209));

        public int UpperSweepBound
        {
            get { return (int)GetValue(UpperSweepBoundProperty); }
            set { SetValue(UpperSweepBoundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UpperSweepBoundProperty =
            DependencyProperty.Register("UpperSweepBound", typeof(int), typeof(MainWindow), new PropertyMetadata(1211));

        public ObservableCollection<string> Logs
        {
            get { return (ObservableCollection<string>)GetValue(LogsProperty); }
            set { SetValue(LogsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LogsProperty =
            DependencyProperty.Register("Logs", typeof(ObservableCollection<string>), typeof(MainWindow), new PropertyMetadata(null));

        public string SourceFolder
        {
            get { return (string)GetValue(SourceFolderProperty); }
            set { SetValue(SourceFolderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceFolderProperty =
            DependencyProperty.Register("SourceFolder", typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));

        public bool IsMonitoring
        {
            get { return (bool)GetValue(IsMonitoringProperty); }
            set { SetValue(IsMonitoringProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMonitoringProperty =
            DependencyProperty.Register("IsMonitoring", typeof(bool), typeof(MainWindow), new PropertyMetadata(false, IsMonitoringChanged));

        public double LowerVoltageBound
        {
            get { return (double)GetValue(LowerVoltageBoundProperty); }
            set { SetValue(LowerVoltageBoundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LowerVoltageBoundProperty =
            DependencyProperty.Register("LowerVoltageBound", typeof(double), typeof(MainWindow), new PropertyMetadata(0.4d));

        public double UpperVoltageBound
        {
            get { return (double)GetValue(UpperVoltageBoundProperty); }
            set { SetValue(UpperVoltageBoundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UpperVoltageBoundProperty =
            DependencyProperty.Register("UpperVoltageBound", typeof(double), typeof(MainWindow), new PropertyMetadata(0.9d));

        public int FileCount
        {
            get { return (int)GetValue(FileCountProperty); }
            set { SetValue(FileCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FileCountProperty =
            DependencyProperty.Register("FileCount", typeof(int), typeof(MainWindow), new PropertyMetadata(0));

        public int WindowLowerBound
        {
            get { return (int)GetValue(WindowLowerBoundProperty); }
            set { SetValue(WindowLowerBoundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WindowLowerBound.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WindowLowerBoundProperty =
            DependencyProperty.Register("WindowLowerBound", typeof(int), typeof(MainWindow), new PropertyMetadata(0, OnWindowLowerBoundChanged));

        private static void OnWindowLowerBoundChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            MainWindow mw = (MainWindow)o;
            mw.UpdateOutputPlot((int)e.NewValue, mw.WindowUpperBound);
        }

        public int WindowUpperBound
        {
            get { return (int)GetValue(WindowUpperBoundProperty); }
            set { SetValue(WindowUpperBoundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WindowUpperBound.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WindowUpperBoundProperty =
            DependencyProperty.Register("WindowUpperBound", typeof(int), typeof(MainWindow), new PropertyMetadata(2, OnWindowUpperBoundChanged));

        private static void OnWindowUpperBoundChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            MainWindow mw = (MainWindow)o;
            mw.UpdateOutputPlot(mw.WindowLowerBound, (int)e.NewValue);
        }

        public AggregateFunctionEnum OutputAggregateFunction
        {
            get { return (AggregateFunctionEnum)GetValue(OutputAggregateFunctionProperty); }
            set { SetValue(OutputAggregateFunctionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OutputAggregateFunction.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OutputAggregateFunctionProperty =
            DependencyProperty.Register("OutputAggregateFunction", typeof(AggregateFunctionEnum), typeof(MainWindow), new PropertyMetadata(AggregateFunctionEnum.Average, OnOutputAggregateFunctionChanged));

        private static void OnOutputAggregateFunctionChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            MainWindow mw = (MainWindow)o;
            mw.UpdateOutputPlot(mw.WindowLowerBound, mw.WindowUpperBound);
        }

        #endregion

        #region Commands + UI input

        private void OpenSourceFolder_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFolderDialog d = new OpenFolderDialog();

            if (d.ShowDialog(this).GetValueOrDefault(false) == true)
            {
                this.SourceFolder = d.FolderName;
                SaveSettings();
            }
        }

        private void ClearLogs_Click(object sender, RoutedEventArgs e)
        {
            this.Logs.Clear();
        }

        private async void Analyze_Click(object sender, RoutedEventArgs e)
        {
            await Analyze();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void COV_Settings_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        //private void PurgeLayout_Click(object sender, RoutedEventArgs e)
        //{
        //    this.PurgeLayoutOnExit = true;
        //    MessageBox.Show("Window Layout Data will be cleared on exit.\nPlease restart application");
        //}

        private void FileProperties_Click(object sender, RoutedEventArgs e)
        {
            FilePropertiesWindow w = new()
            {
                File = this.SelectedFile
            };
            w.Show();
        }

        private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }


        #endregion

        #region Folder Watching

        private static void IsMonitoringChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            MainWindow window = (MainWindow)o;
            window.AddLog("Live Monitoring {0}", (bool)e.NewValue ? "ENABLED" : "DISABLED");
            window.IsMonitoringChangedInternal(e);
        }

        private async void IsMonitoringChangedInternal(DependencyPropertyChangedEventArgs e)    //PF16/10/2021 removed first parameter 'DependencyObject o' since it was unused
        {
            if (this.watcher != null)
            {
                this.watcher.Created -= Watcher_Created;
                this.watcher.Dispose();
                this.watcher = null;
            }

            if (string.IsNullOrEmpty(this.SourceFolder))
            {
                MessageBox.Show("SourceFolder not set");
                return;
            }

            if (timers != null)
                foreach (System.Timers.Timer t in timers.Values)
                {
                    t.Enabled = false;
                    t.Elapsed -= Timer_Elapsed;
                    t.Dispose();
                }
            timers.Clear();

            this.watcher = new FileSystemWatcher(this.SourceFolder);
            this.watcher.Created += Watcher_Created;
            this.watcher.EnableRaisingEvents = (bool)e.NewValue;

            if ((bool)e.NewValue == false)
                IsExperimentStarted = false;
            else
            {
                FileSystemInfo[] files = new DirectoryInfo(SourceFolder).GetFileSystemInfos();
                if (files != null && files.Length > 0)
                {
                    FileSystemInfo fileinfo = files.OrderByDescending(F => F.CreationTime).First();
                    ExperimentStart = fileinfo.CreationTime;
                    IsExperimentStarted = true;
                    await Analyze();            //analyzes exisiting files in the Folder
                }
            }
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            Debug.Print("Watcher_Created::File Created {1} {0}", sender.ToString(), e.FullPath);

            Dispatcher.BeginInvoke((Action)(() =>
            {
                AddLog("\tFileWatcher::File Created '{0}'", e.FullPath);

                if (IsExperimentStarted == false)
                {
                    //ExperimentStart = DateTime.Now;
                    //find oldest file in the folder

                    try
                    {
                        FileSystemInfo fileinfo = new DirectoryInfo(System.IO.Path.GetDirectoryName(e.FullPath)).GetFileSystemInfos().OrderByDescending(F => F.CreationTime).First();
                        ExperimentStart = fileinfo.CreationTime;
                    }
                    catch
                    {
                        ExperimentStart = DateTime.Now;
                    }

                    IsExperimentStarted = true;
                }

                if (IsFileLocked(e.FullPath))
                {
                    AddLog("\tFileWatcher::File is locked. Will start Timer to monitor when it is available.");

                    ExtTimer t = new(2000);
                    t.Elapsed += Timer_Elapsed;
                    t.Tag = e.FullPath;
                    t.AutoReset = true;
                    t.Start();
                    timers.Add(e.FullPath, t);
                }
                else
                {
                    this.ProcessFile(e.FullPath, true);
                }
            }));
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Debug.Print("T_Elapsed sender={0}     e={1}", sender, e);

            ExtTimer t = (ExtTimer)sender;

            if (IsFileLocked((string)t.Tag) == false)
            {
                t.Stop();
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.ProcessFile((string)t.Tag, true);      //ProcessFile with UpdatePlots = true
                    timers.Remove((string)(t.Tag));
                }));
                t.Elapsed -= Timer_Elapsed;
                t.Dispose();
            }
        }

        private readonly Dictionary<string, ExtTimer> timers = new();

        #endregion

        #region THE WORK

        private async Task Analyze()
        {
            Reset();

            AddLog("Analyzing files in {0}", SourceFolder);
            AddLog("Filter: {0}, Cut-off Freqeuncy {1}Hz", IsFilterEnabled ? "ON" : "OFF", FilterCutOffFrequency);
            AddLog("LowerSweepBound: {0}, UpperSweepBound: {1}", LowerSweepBound, UpperSweepBound);
            AddLog("LowerVoltageBound: {0}V, UpperVoltageBound: {1}V", LowerVoltageBound, UpperVoltageBound);

            if (System.IO.Directory.Exists(SourceFolder) == false)
            {
                AddLog("!!! Cannot find folder '{0}'", SourceFolder);
                return;
            }

            //analyze exisiting files first
            Debug.Print("processing exisiting files in folder {0}", SourceFolder);
            Debug.Print("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^");
            DirectoryInfo folder = new(SourceFolder);
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                //Process/Analyze only the first file (useful for debugging a whole folder)
                await ProcessFileSync(folder.GetFiles().OrderBy(a => a.LastWriteTime).First().FullName);
                return;
            }


            foreach (FileInfo f in folder.GetFiles().OrderBy(a => a.LastWriteTime))
            {
                bool validfile = false;

                if (IsExtensionFilterEnabled == false)
                    validfile = true;   //all files are valid when Extension Filtering is disabled
                else if (System.IO.Path.GetExtension(f.FullName) == (ExtensionFilterString == null ? string.Empty : ExtensionFilterString.WrapInStrings(".", null, true)))
                    validfile = true;

                if (validfile)
                {
                    if (IsFileLocked(f.FullName) == false)
                    {
                        Debug.Print("Analyze. About to call ProcessFileSync for {0}", f.FullName);
                        await ProcessFileSync(f.FullName, false);
                        //if (IsExtensionFilterEnabled == false)
                        //    await ProcessFileSync(f.FullName, false);
                        //else if (System.IO.Path.GetExtension(f.FullName) == (ExtensionFilterString == null ? string.Empty : ExtensionFilterString.EnsureStartsWith(".")))
                        //{
                        //    Debug.Print("System.IO.Path.GetExtension(f.FullName) = '{0}' == '{1}'", System.IO.Path.GetExtension(f.FullName), ExtensionFilterString == null ? string.Empty : ExtensionFilterString.EnsureStartsWith("."));
                        //    await ProcessFileSync(f.FullName, false);
                        //}
                    }
                    else
                    {
                        AddLog("\tFileWatcher::File is locked. Will start Timer to monitor when it is available.");

                        ExtTimer t = new(2000);
                        t.Elapsed += Timer_Elapsed;
                        t.Tag = f.FullName;
                        t.AutoReset = true;
                        t.Start();
                        timers.Add(f.FullName, t);
                    }
                }
            }

            //Debug.Print("Average GetScaledData time = {0} ms", Files.Average(a => a.averagetime));


            if (Files.Count == 0 && folder.GetFiles().Length > 0)
            {
                MessageBox.Show("No files were Analyzed yet there are files in the Folder. Consider changing Extension Filter");
            }

            UpdateOutputPlot(this.WindowLowerBound, this.WindowUpperBound);
            CalculateCOV();


            Debug.Print("finsihed processing exisiting files in folder {0}", SourceFolder);
            Debug.Print("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^");
        }

        //public static void MyParallelFor(int inclusiveLowerBound, int exclusiveUpperBound, Action<int> body)
        //{
        //    // Get the number of processors, initialize the number of remaining
        //    // threads, and set the starting point for the iteration.
        //    int numProcs = Environment.ProcessorCount;
        //    int remainingWorkItems = numProcs;
        //    int nextIteration = inclusiveLowerBound;
        //    using (ManualResetEvent mre = new ManualResetEvent(false))
        //    {
        //        // Create each of the work items.
        //        for (int p = 0; p < numProcs; p++)
        //        {
        //            ThreadPool.QueueUserWorkItem(delegate
        //            {
        //                int index;
        //                while ((index = Interlocked.Increment(ref nextIteration) - 1) < exclusiveUpperBound)
        //                    body(index);

        //                if (Interlocked.Decrement(ref remainingWorkItems) == 0)
        //                    mre.Set();
        //            });
        //        }
        //        // Wait for all threads to complete.
        //        mre.WaitOne();
        //    }
        //}

        private void ProcessFile(string Filename, bool UpdatePlots = false)
        {
            AddLog("\tProcessing File '{0}'", System.IO.Path.GetFileName(Filename));

            //open the file
            LVBinFile file = OpenLabViewBinFile(Filename);

            if (file == null)
            {
                AddLog($"!!! File is Null (Unable to Open File), Unable to Process File '{Filename}'");
                return;
            }

            //AddLog("\t\t{0}", file.InfoString);           //too much information

            if (LowerSweepBound <= 0 || LowerSweepBound > file.AIChannels.First().RawData.Length)
            {
                AddLog($"!!! LowerSweepBound is invalid (file.AIChannels.First().RawData.Length = {file.AIChannels.First().RawData.Length}");
                return;
            }

            if (UpperSweepBound < 0 || UpperSweepBound > file.AIChannels.First().RawData.Length)
            {
                AddLog($"!!! UpperSweepBound is invalid (file.AIChannels.First().RawData.Length = {file.AIChannels.First().RawData.Length}");
                return;
            }

            if (UpperSweepBound < LowerSweepBound)
            {
                AddLog("!!! UpperSweepBound cannot be less than LowerSweepBound");
                return;
            }

            try
            {
                long LowerBound = FindClosestIndex(file.AOChannels.First().TriangleWaveform, (decimal)LowerVoltageBound);
                long UpperBound = FindClosestIndex(file.AOChannels.First().TriangleWaveform, (decimal)UpperVoltageBound, ExpandDirection.Down);
                if (UpperBound == -1 || LowerBound == -1)
                {
                    AddLog("!!! FindClosest function returned -1. Check Lower and UpperSweepBound values");
                    return;
                }

                List<List<DataPoint>> list_datapoints = new();

                foreach (AIChannel channel in file.AIChannels)
                {
                    channel.IntegratedData = new decimal[(UpperSweepBound - LowerSweepBound) + 1];
                    List<DataPoint> datapoints = new();

                    decimal a = (decimal)((file.AOChannels.First().WaveformDuration / 1000) / file.AOChannels.First().SampleCount);

                    int backgroundSubtractionSweepCount = BackgroundSubtractionSweepCount;
                    int lowerSweepBound = LowerSweepBound;
                    int upperSweepBound = UpperSweepBound;

                    for (int i = lowerSweepBound - 1; i < upperSweepBound; i++)
                    {
                        channel.IntegratedData[i - lowerSweepBound + 1] = CalculateCharge(channel.GetScaledData(i, file.IsFilterEnabled, file.FilterCutOffFrequency, backgroundSubtractionSweepCount), LowerBound, UpperBound, (decimal)channel.SamplingPeriod);
                        datapoints.Add(new DataPoint(i - lowerSweepBound + 1, (double)channel.IntegratedData[i - lowerSweepBound + 1]));
                    }
                    list_datapoints.Add(datapoints);
                }
                if (Files.Contains(file) == false)
                    Files.Add(file);

                SelectedFile = Files.Last();
                SelectedAIChannel = SelectedFile.AIChannels.First();

                if (UpdatePlots)
                {
                    UpdateOutputPlot(WindowLowerBound, WindowUpperBound);
                    CalculateCOV();
                }
            }
            catch (DivideByZeroException e1)
            {
                AddLog($"DivideByZeroException thrown. Probably erroneous calculation in Butterworth filter\n\n{e1.StackTrace}");
            }
            catch (OverflowException e2)
            {
                AddLog($"OverflowException thrown. Probably erroneous (calculation in Butterworth filter\n\n{e2.StackTrace}");
            }
            catch (Exception e3)
            {
                AddLog($"Exception thrown: {e3.Message}. \n\n{e3.StackTrace}");
            }
        }

        private void UpdateOutputPlot(int WindowLowerBound, int WindowUpperBound)
        {
            if (Files == null | Files.Count == 0)
            {
                AddLog("There are no files (UpdateOutputPlot)");
                return;
            }
            if (WindowUpperBound <= WindowLowerBound)
            {
                //MessageBox.Show("WindowUpperBound is smaller or equal to WindowLowerBound", "Error");
                AddLog("WindowUpperBound is smaller or equal to WindowLowerBound (UpdateOutputPlot)");
                return;
            }
            if (WindowLowerBound >= WindowUpperBound)
            {
                //MessageBox.Show("WindowLowerBound is greater or equal to WindowUpperBound", "Error");
                AddLog("WindowLowerBound is greater or equal to WindowUpperBound (UpdateOutputPlot)");
                return;
            }
            if ((WindowUpperBound - WindowLowerBound) > Files.First().AIChannels.First().IntegratedData.Length)
            {
                AddLog("Window (Upper - Lower) exceeds number of Intergrated data points (UpdateOutputPlot)");
                return;
            }

            AllIntegratedModel.Clear();

            //OxyPlot.Wpf.OxyColorConverter oxyColorConverter = new OxyColorConverter();

            double[] x_data = DataAnalysisHelper.FillSeries(Files.Count, Files.First().Duration.TotalSeconds, 0);
            double[] y_data = new double[Files.Count];
            for (int channelnumber = 0; channelnumber < Files.First().AIChannels.Count; channelnumber++)
            {
                int j = 0;
                foreach (LVBinFile file in Files)
                {
                    y_data[j++] = DataAnalysisHelper.AnalyzeData(file.AIChannels[channelnumber].IntegratedData, OutputAggregateFunction, WindowLowerBound, WindowUpperBound);
                }

                AllIntegratedModel.AddLineSeries(x_data, y_data, OxyColors.MediumPurple);
                ((OxyPlot.Series.LineSeries)AllIntegratedModel.Model.Series.Last()).MarkerSize = 3;
                ((OxyPlot.Series.LineSeries)AllIntegratedModel.Model.Series.Last()).Color = Files.First().AIChannels[channelnumber].Stroke.ToOxyColor();
            }
            AllIntegratedModel.Model.InvalidatePlot(true);
            AllIntegratedModel.ScalePlotAxes(false);
        }

        private async Task ProcessFileSync(string Filename, bool UpdatePlots = false)
        {
            await Task.Run(() =>
            Dispatcher.Invoke(() => ProcessFile(Filename, UpdatePlots)));
        }

        private LVBinFile OpenLabViewBinFile(string Filename)
        {
            Debug.Print("OpenLabViewBinFile called: {0}", Filename);
            //read header
            FileStream stream = null;
            LVBinaryReader reader = null;
            try
            {
                LVBinFile file = new()
                {
                    FullFilename = Filename
                };

                stream = new FileStream(Filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                reader = new LVBinaryReader(stream);

                file.ActualFileSize = stream.Length;
                //DebugFileStructure(reader);

                reader.BaseStream.Seek(0, SeekOrigin.Begin);

                int d1 = reader.ReadInt32();
                int aochannel_count = reader.ReadInt32();

                int aosample_count = reader.ReadInt32();

                reader.BaseStream.Seek(12 + (aosample_count * 0), SeekOrigin.Begin);

                file.AOChannels = new ObservableCollection<AOChannel>();
                for (int channel_number = 0; channel_number < aochannel_count; channel_number++)
                {
                    file.AOChannels.Add(new AOChannel(channel_number));
                    file.AOChannels.Last().TriangleWaveform = new decimal[aosample_count];
                    for (int x = 0; x < aosample_count; x++)
                    {
                        file.AOChannels.Last().TriangleWaveform[x] = (decimal)reader.ReadDouble();
                    }
                }
                long fileoffset = 12 + (file.AOChannels.Count * 8 * aosample_count);       //modern version
                //13324

                //Section: Waveform
                reader.BaseStream.Seek(fileoffset, SeekOrigin.Begin);
                int read_repeat = reader.ReadInt32();
                for (int i = 0; i < read_repeat; i++)
                {
                    reader.ReadInt32();  //samplecount repeated as many times as channels
                }
                read_repeat = reader.ReadInt32();
                for (int i = 0; i < read_repeat; i++)
                {
                    file.AOChannels[i].WaveformUpdateRate = reader.ReadDouble();  //samplecount repeated as many times as channels
                }
                read_repeat = reader.ReadInt32();
                for (int i = 0; i < read_repeat; i++)
                {
                    file.AOChannels[i].WaveformDuration = reader.ReadDouble();  //samplecount repeated as many times as channels
                }
                read_repeat = reader.ReadInt32();
                for (int i = 0; i < read_repeat; i++)
                {
                    file.AOChannels[i].WaveformScanRate = reader.ReadDouble();  //samplecount repeated as many times as channels
                }
                read_repeat = reader.ReadInt32();
                for (int i = 0; i < read_repeat; i++)
                {
                    file.AOChannels[i].WaveformMultiplier = reader.ReadDouble();  //samplecount repeated as many times as channels
                }
                read_repeat = reader.ReadInt32();
                for (int i = 0; i < read_repeat; i++)
                {
                    file.AOChannels[i].WaveformOffset = reader.ReadDouble();  //samplecount repeated as many times as channels
                }
                file.WaveformFrequency = reader.ReadDouble();               //22868

                reader.ReadInt32();         //not sure what this is?
                read_repeat = reader.ReadInt32();
                reader.ReadInt32();         //not sure what this is?

                //Section: Stimulus
                file.StimulationFrequency = reader.ReadInt32();
                file.StimulationPulses = reader.ReadInt32();
                file.StimulationPulseWidth = reader.ReadDouble();
                file.StimulationAmplitude = reader.ReadDouble();
                file.StimulationPolarity = (StimulationPolarityEnum)reader.ReadInt16();
                file.StimulusToScanDelay = reader.ReadDouble();
                file.StimulationUpdateRate = reader.ReadInt32();
                file.StimulationMultiplier = reader.ReadDouble();           //22926

                //Section: Collection
                file.CollectionStimOrTTL = reader.ReadInt32();
                file.CollectionPreEventScanCount = reader.ReadInt32();
                file.CollectionPostEventScanCount = reader.ReadInt32();
                file.CollectionNumberChannels = reader.ReadInt32();


                reader.ReadInt32();         //2
                reader.ReadInt32();         //1

                Int16 activechannel = reader.ReadInt16();
                file.CollectionActiveChannel = (activechannel / 256) - 48;  //safe way of avoiding DivideByZero error
                Int32 eventmode = reader.ReadInt32();       //need to check if this value actually changes with eventmode or not

                //this is where the filetypes start to differ.
                if (eventmode == 2111)
                    file.FileVersion = 1.0;     //old FSCAV file version (widely used in the Lipski lab in 2016 till 2018+
                else if (eventmode == 305)
                    file.FileVersion = 2.0;     //multi-channel support version of Knowmad Technologies WCCV (3.7?? 2018)
                else if (eventmode == 306)
                    file.FileVersion = 3.0;     //added 20/8/2019
                else
                {
                    throw new FileFormatException(string.Format("Unrecognized FileVersion (={0})", eventmode));
                }

                file.CollectionEventMode = LVBinFile.GetEventMode(eventmode);   //PF 6/11/2018 Is this really EventMode? It doesn't look right at all.

                if (file.FileVersion == 3.0)
                    read_repeat = reader.ReadInt32();
                else if (file.FileVersion == 2.0)
                    read_repeat = reader.ReadInt32();
                else if (file.FileVersion == 1.0)
                    read_repeat = 8;            //forced value of 8, and not advancing the reader
                else
                    throw new Exception();

                file.nA_Vs = new decimal[read_repeat];
                UInt16[] ADGain_ = new UInt16[read_repeat];
                file.ADGains = new decimal[read_repeat];
                file.GainMultipliers = new decimal[read_repeat];    //read directly from file for FileVersion 2.0

                //BLOCK #1
                for (int i = 0; i < read_repeat; i++)
                {
                    if (file.FileVersion == 3.0)
                        file.GainMultipliers[i] = (decimal)reader.ReadDouble();
                    else if (file.FileVersion == 2.0)
                        file.GainMultipliers[i] = (decimal)reader.ReadDouble();
                    else if (file.FileVersion == 1.0)
                    {
                        ADGain_[i] = reader.ReadUInt16();
                        reader.BaseStream.Seek(6, SeekOrigin.Current);
                    }
                }
                reader.ReadInt16().ToString();         //not sure what this is (=3)
                reader.ReadInt16().ToString();         //not sure what this is (=0)
                reader.ReadInt16().ToString();         //not sure what this is (=1000)

                if (file.FileVersion == 3.0)
                    read_repeat = reader.ReadInt32();
                else if (file.FileVersion == 2.0)
                    read_repeat = reader.ReadInt32();
                else if (file.FileVersion == 1.0)
                {
                    reader.ReadInt32();
                    read_repeat = 8;            //forced value of 8, and not advancing the reader
                }
                else
                    throw new Exception();

                //BLOCK #2
                for (int i = 0; i < read_repeat; i++)
                {
                    if (file.FileVersion == 3.0)
                    {
                        file.nA_Vs[i] = (decimal)reader.ReadSingle();
                        file.ADGains[i] = (file.GainMultipliers[i] / (20m / 65536m)) / file.nA_Vs[i];
                    }
                    else if (file.FileVersion == 2.0)
                    {
                        file.nA_Vs[i] = (decimal)reader.ReadSingle();
                        file.ADGains[i] = (file.GainMultipliers[i] / (20m / 65536m)) / file.nA_Vs[i];
                    }
                    else if (file.FileVersion == 1.0)
                    {
                        file.nA_Vs[i] = LVBinFile.Get_nA_V(reader.ReadUInt16());
                        file.ADGains[i] = LVBinFile.Get_AD_Gain(ADGain_[i], file.nA_Vs[i]);
                        file.GainMultipliers[i] = (file.nA_Vs[i] / file.ADGains[i]) * (20m / 65536m);
                        reader.BaseStream.Seek(2, SeekOrigin.Current);
                    }
                }

                //Main Analog Input (pA) data reading section
                reader.BaseStream.Seek(1000000, SeekOrigin.Begin);

                //Debug.Print("Reading Analog Input Data from '{0}'", file.FullFilename);
                int scan_offset = (file.AOChannels.First().SampleCount * 2) * (file.CollectionNumberChannels - 1);
                //Debug.Print(">>>scan_offset = {0} (AOChannel.SampleCount = {1})", scan_offset, file.AOChannels.First().SampleCount);

                //check for corrupt file (incomplete recording)
                if (file.ActualFileSize != file.ExpectedFileSize)
                {
                    throw new FileFormatException(string.Format("File length ({0} bytes) less than expected ({1} bytes). File corrupted or recording terminated early", file.ActualFileSize, file.ExpectedFileSize));
                }


                //New reading logic. Reads a sample from each channel sequentially until the end of the scan, then advances to the next scan
                //add channels first
                for (int channel_number = 0; channel_number < file.CollectionNumberChannels; channel_number++)
                {
                    //Debug.Print("\t Adding channel #{0} of {1}", channel_number, file.CollectionNumberChannels);
                    //reader.BaseStream.Seek(1000000 + (channel_number * scan_offset), SeekOrigin.Begin);

                    file.AIChannels.Add(new AIChannel(channel_number));
                    file.AIChannels.Last().RawData = new short[file.TotalScanCount][];
                    file.AIChannels.Last().Data = new decimal[file.TotalScanCount][];
                    file.AIChannels.Last().SamplingPeriod = (file.AOChannels.First().WaveformDuration / 1000d) / file.AOChannels.First().SampleCount;
                    file.AIChannels.Last().nA_V = file.nA_Vs[channel_number];
                    file.AIChannels.Last().ADGain = file.ADGains[channel_number];
                    file.AIChannels.Last().GainMultiplier = file.GainMultipliers[channel_number];
                    if (channel_number == file.CollectionActiveChannel)
                        file.AIChannels.Last().IsActive = true;
                    file.AIChannels.Last().IsDisabled = false;

                    for (int scan = 0; scan < file.TotalScanCount; scan++)
                        file.AIChannels.Last().RawData[scan] = new short[file.AOChannels.First().SampleCount];
                }

                reader.BaseStream.Seek(1000000, SeekOrigin.Begin);

                for (int scan = 0; scan < file.TotalScanCount; scan++)
                {
                    //Debug.Print("\t\t scan {0} of {1}", scan.ToString().PadRight(5), file.TotalScanCount);

                    for (int y = 0; y < file.AOChannels.First().SampleCount; y++)
                    {
                        for (int channel = 0; channel < file.CollectionNumberChannels; channel++)
                        {
                            file.AIChannels[channel].RawData[scan][y] = reader.ReadInt16();
                        }
                    }
                    //reader.BaseStream.Seek(scan_offset, SeekOrigin.Current);
                }


                //Below For-Loop was used until the 15th November. This is sequential: reading a whole scan for a channel, then a whole scan of the next channel, then advancing to the next scan
                //for (int channel_number = 0; channel_number < file.CollectionNumberChannels; channel_number++)
                //{
                //    Debug.Print("\t channel #{0} of {1}", channel_number, file.CollectionNumberChannels);
                //    reader.BaseStream.Seek(1000000 + (channel_number * scan_offset), SeekOrigin.Begin);

                //    file.AIChannels.Add(new AIChannel(channel_number));
                //    file.AIChannels.Last().RawData = new short[file.TotalScanCount][];
                //    file.AIChannels.Last().Data = new decimal[file.TotalScanCount][];
                //    file.AIChannels.Last().SamplingPeriod = (file.AOChannels.First().WaveformDuration / 1000d) / file.AOChannels.First().SampleCount;
                //    file.AIChannels.Last().nA_V = file.nA_Vs[channel_number];
                //    file.AIChannels.Last().ADGain = file.ADGains[channel_number];
                //    file.AIChannels.Last().GainMultiplier = file.GainMultipliers[channel_number];
                //    if (channel_number == file.CollectionActiveChannel)
                //        file.AIChannels.Last().IsActive = true;
                //    file.AIChannels.Last().IsDisabled = false;

                //    for (int scan = 0; scan < file.TotalScanCount; scan++)
                //    {
                //        //Debug.Print("\t\t scan {0} of {1}", scan.ToString().PadRight(5), file.TotalScanCount);
                //        file.AIChannels.Last().RawData[scan] = new short[file.AOChannels.First().SampleCount];

                //        for (int y = 0; y < file.AOChannels.First().SampleCount; y++)
                //        {
                //            file.AIChannels.Last().RawData[scan][y] = reader.ReadInt16();
                //        }
                //        reader.BaseStream.Seek(scan_offset, SeekOrigin.Current);
                //    }
                //}


                file.IsFilterEnabled = this.IsFilterEnabled;
                file.FilterCutOffFrequency = this.FilterCutOffFrequency;
                return file;

            }
            catch (Exception e)
            {
                AddLog("!!!  Unable to open file '{0}'", System.IO.Path.GetFileName(Filename));
                AddLog("!!!  {0}", e.Message);
                return null;
            }
            finally
            {
                stream?.Close();
                reader?.Close();
            }


        }

        #endregion

        #region Plotting

        public IntegratedViewModel AllIntegratedModel
        {
            get { return (IntegratedViewModel)GetValue(AllIntegratedModelProperty); }
            set { SetValue(AllIntegratedModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AllIntegratedModelProperty =
            DependencyProperty.Register("AllIntegratedModel", typeof(IntegratedViewModel), typeof(MainWindow), new PropertyMetadata(null));

        public IntegratedViewModel IntegratedModel
        {
            get { return (IntegratedViewModel)GetValue(IntegratedModelProperty); }
            set { SetValue(IntegratedModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IntegratedModelProperty =
            DependencyProperty.Register("IntegratedModel", typeof(IntegratedViewModel), typeof(MainWindow), new PropertyMetadata(null));

        public ScopeViewModel ScopeModel
        {
            get { return (ScopeViewModel)GetValue(ScopeModelProperty); }
            set { SetValue(ScopeModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScopeModelProperty =
            DependencyProperty.Register("ScopeModel", typeof(ScopeViewModel), typeof(MainWindow), new PropertyMetadata(null));

        public ScopeViewModel ScopeRawModel
        {
            get { return (ScopeViewModel)GetValue(ScopeRawModelProperty); }
            set { SetValue(ScopeRawModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScopeRawModelProperty =
            DependencyProperty.Register("ScopeRawModel", typeof(ScopeViewModel), typeof(MainWindow), new PropertyMetadata(null));


        public int SelectedSweep
        {
            get { return (int)GetValue(SelectedSweepProperty); }
            set { SetValue(SelectedSweepProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedSweepProperty =
            DependencyProperty.Register("SelectedSweep", typeof(int), typeof(MainWindow), new PropertyMetadata(0, SelectedSweepChanged));

        private static void SelectedSweepChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            MainWindow window = (MainWindow)o;

            //Debug.Print($"SelectedSweepChanged {e.NewValue}");


            if (window.SelectedFile == null)
                return;
            if ((int)e.NewValue == -1)
                return;
            if (window.SelectedFile.AIChannels == null)
                return;
            if (window.SelectedFile.AIChannels.Count == 0)
                return;


            //Debug.Print("integrated sweep {0} > {1}     {2}     {3}", sweep, trap, wedge, window.SelectedFile.IntegratedData[counter]);
            //IList<DataPoint> waveformpoints = new List<DataPoint>();
            //List<List<DataPoint>> list_currentpoints = new List<List<DataPoint>>();
            int j = 0;

            //foreach (double value in window.SelectedFile.AOChannels.First().TriangleWaveform)
            //{
            //    waveformpoints.Add(new DataPoint(j, value));
            //    j++;
            //}

            double[] x_data = DataAnalysisHelper.FillSeries<double>(window.SelectedFile.AOChannels.First().SampleCount, 1, 0);
            double[] x_dataRaw = DataAnalysisHelper.FillSeries<double>(window.SelectedFile.AOChannels.First().SampleCount, 1, 0);

            double[] y_data = new double[window.SelectedFile.AOChannels.First().SampleCount];
            double[] y_dataRaw = new double[window.SelectedFile.AOChannels.First().SampleCount];
            double[] y_dataBackgroundRaw = new double[window.SelectedFile.AOChannels.First().SampleCount];

            window.ScopeModel.Clear();
            window.ScopeModel.Model.Axes.Clear();
            window.ScopeModel.Model.Annotations.Clear();
            window.ScopeModel.Model.Axes.Add(new OxyPlot.Axes.LinearAxis() { Key = "primary", Position = AxisPosition.Left, Title = "Current (substracted)" });
            window.ScopeModel.Model.Axes.Add(new OxyPlot.Axes.LinearAxis() { Key = "secondary", Position = AxisPosition.Right, Title = "Scan Voltage" });
            window.ScopeModel.AddLineSeries(x_data, window.SelectedFile.AOChannels.First().TriangleWaveform.Select(a => (double)a));
            ((OxyPlot.Series.LineSeries)window.ScopeModel.Model.Series.Last()).YAxisKey = "secondary";

            window.ScopeRawModel.Clear();
            window.ScopeRawModel.Model.Axes.Clear();
            window.ScopeRawModel.Model.Annotations.Clear();
            window.ScopeRawModel.Model.Axes.Add(new OxyPlot.Axes.LinearAxis() { Key = "primaryRaw", Position = AxisPosition.Left, Title = "Current" });
            window.ScopeRawModel.Model.Axes.Add(new OxyPlot.Axes.LinearAxis() { Key = "secondaryRaw", Position = AxisPosition.Right, Title = "Scan Voltage" });
            window.ScopeRawModel.AddLineSeries(x_dataRaw, window.SelectedFile.AOChannels.First().TriangleWaveform.Select(a => (double)a));
            ((OxyPlot.Series.LineSeries)window.ScopeRawModel.Model.Series.Last()).YAxisKey = "secondaryRaw";
            window.ScopeRawModel.Model.Legends.Clear();
            window.ScopeRawModel.Model.Legends.Add(new Legend()
            {
                LegendTitle = "Legend",
                LegendPosition = LegendPosition.RightTop,
                IsLegendVisible = true,
                ShowInvisibleSeries = true,
                LegendPlacement = LegendPlacement.Inside
            });



            foreach (AIChannel channel in window.SelectedFile.AIChannels)
            {
                decimal[] data = channel.GetScaledData((int)e.NewValue, window.SelectedFile.IsFilterEnabled, window.SelectedFile.FilterCutOffFrequency, window.BackgroundSubtractionSweepCount);

                decimal[] dataRaw = channel.GetScaledData((int)e.NewValue, window.SelectedFile.IsFilterEnabled, window.SelectedFile.FilterCutOffFrequency, 0);

                //The first sweep (sweep = 0; obviously with no background subtraction)
                decimal[] dataBackgroundRaw = channel.GetScaledData(0, window.SelectedFile.IsFilterEnabled, window.SelectedFile.FilterCutOffFrequency, 0);

                //List<DataPoint> currentpoints = new List<DataPoint>();

                j = 0;
                foreach (decimal value in data)
                {
                    //this is sample #/time (x-axis) against current (y axis)
                    //currentpoints.Add(new DataPoint(j, (double)value));
                    //j++;

                    //this is voltage (x) against current (y)
                    y_data[j] = (double)value;
                    y_dataRaw[j] = (double)dataRaw[j];
                    y_dataBackgroundRaw[j] = (double)dataBackgroundRaw[j];
                    j++;
                }

                int lowerVoltageBoundIndex = FindClosestIndex(window.SelectedFile.AOChannels.First().TriangleWaveform, (decimal)window.LowerVoltageBound);
                int upperVoltageBoundIndex = FindClosestIndex(window.SelectedFile.AOChannels.First().TriangleWaveform, (decimal)window.UpperVoltageBound);


                window.ScopeModel.AddLineSeries(x_data, y_data);
                ((OxyPlot.Series.LineSeries)window.ScopeModel.Model.Series.Last()).YAxisKey = "primary";
                ((OxyPlot.Series.LineSeries)window.ScopeModel.Model.Series.Last()).Color = channel.Stroke.ToOxyColor();
                ((OxyPlot.Series.LineSeries)window.ScopeRawModel.Model.Series.Last()).RenderInLegend = true;
                ((OxyPlot.Series.LineSeries)window.ScopeModel.Model.Series.Last()).Title = $"sweep {(int)e.NewValue}";

                window.ScopeModel.Model.Annotations.Add(new LineAnnotation()
                {
                    X = lowerVoltageBoundIndex,
                    Type = LineAnnotationType.Vertical,
                    LineStyle = LineStyle.Dash,
                    Color = OxyColors.LightGray
                });
                window.ScopeModel.Model.Annotations.Add(new LineAnnotation()
                {
                    X = upperVoltageBoundIndex,
                    Type = LineAnnotationType.Vertical,
                    LineStyle = LineStyle.Dash,
                    Color = OxyColors.LightGray
                });
                window.ScopeModel.Model.Annotations.Add(new LineAnnotation()
                {
                    Y = window.LowerVoltageBound,
                    YAxisKey = "secondary",
                    Type = LineAnnotationType.Horizontal,
                    LineStyle = LineStyle.Dash,
                    Color = OxyColors.LightGray,
                    Text = $"{window.LowerVoltageBound} V"
                });
                window.ScopeModel.Model.Annotations.Add(new LineAnnotation()
                {
                    Y = window.UpperVoltageBound,
                    YAxisKey = "secondary",
                    Type = LineAnnotationType.Horizontal,
                    LineStyle = LineStyle.Dash,
                    Color = OxyColors.LightGray,
                    Text = $"{window.UpperVoltageBound} V"
                });


                window.ScopeRawModel.AddLineSeries(x_dataRaw, y_dataRaw);
                ((OxyPlot.Series.LineSeries)window.ScopeRawModel.Model.Series.Last()).YAxisKey = "primaryRaw";
                ((OxyPlot.Series.LineSeries)window.ScopeRawModel.Model.Series.Last()).Color = channel.Stroke.ToOxyColor();
                ((OxyPlot.Series.LineSeries)window.ScopeRawModel.Model.Series.Last()).RenderInLegend = true;
                ((OxyPlot.Series.LineSeries)window.ScopeModel.Model.Series.Last()).Title = $"sweep {(int)e.NewValue}";


                window.ScopeRawModel.AddLineSeries(x_dataRaw, y_dataBackgroundRaw);
                ((OxyPlot.Series.LineSeries)window.ScopeRawModel.Model.Series.Last()).YAxisKey = "primaryRaw";
                ((OxyPlot.Series.LineSeries)window.ScopeRawModel.Model.Series.Last()).Color = OxyColor.FromRgb(0, 0, 255);
                ((OxyPlot.Series.LineSeries)window.ScopeRawModel.Model.Series.Last()).RenderInLegend = true;
                ((OxyPlot.Series.LineSeries)window.ScopeModel.Model.Series.Last()).Title = $"background";

                #region vertical and horizontal line (Lower and Upper Voltage Bounds)

                window.ScopeRawModel.Model.Annotations.Add(new LineAnnotation()
                {
                    X = lowerVoltageBoundIndex,
                    Type = LineAnnotationType.Vertical,
                    LineStyle = LineStyle.Dash,
                    Color = OxyColors.LightGray
                });
                window.ScopeRawModel.Model.Annotations.Add(new LineAnnotation()
                {
                    X = upperVoltageBoundIndex,
                    Type = LineAnnotationType.Vertical,
                    LineStyle = LineStyle.Dash,
                    Color = OxyColors.LightGray
                });
                window.ScopeRawModel.Model.Annotations.Add(new LineAnnotation()
                {
                    Y = window.LowerVoltageBound,
                    YAxisKey = "secondaryRaw",
                    Type = LineAnnotationType.Horizontal,
                    LineStyle = LineStyle.Dash,
                    Color = OxyColors.LightGray,
                    Text = $"{window.LowerVoltageBound} V"
                });
                window.ScopeRawModel.Model.Annotations.Add(new LineAnnotation()
                {
                    Y = window.UpperVoltageBound,
                    YAxisKey = "secondaryRaw",
                    Type = LineAnnotationType.Horizontal,
                    LineStyle = LineStyle.Dash,
                    Color = OxyColors.LightGray,
                    Text = $"{window.UpperVoltageBound} V"
                });

                #endregion

                #region integral annotation
                // showing the integral on the Voltammogram plot

                List<DataPoint> points = new List<DataPoint>();

                points.Add(new DataPoint(upperVoltageBoundIndex, (double)data[upperVoltageBoundIndex]));

                for (int i = lowerVoltageBoundIndex; i < upperVoltageBoundIndex; i++)
                {
                    points.Add(new DataPoint(i, (double)data[i]));
                }

                PolylineAnnotation integralAnnotation = new()
                {
                    YAxisKey = "primary",
                    LineStyle = LineStyle.Solid,
                    StrokeThickness = 2,
                    Color = OxyColors.Purple,
                    EdgeRenderingMode = EdgeRenderingMode.PreferSharpness,
                    Text = $"integral"
                };
                integralAnnotation.Points.AddRange(points);
                window.ScopeModel.Model.Annotations.Add(integralAnnotation);

                #endregion

                #region peak

                double peakValue = y_data.MaxWithIndex<double, double>(out long peakIndex, (long)lowerVoltageBoundIndex, (long)upperVoltageBoundIndex);

                window.ScopeModel.Model.Annotations.Add(new ArrowAnnotation()
                {
                    EndPoint = new DataPoint(peakIndex, peakValue),
                    StartPoint = new DataPoint(peakIndex, peakValue - (peakValue * 0.1)),
                    YAxisKey = "primary",
                    StrokeThickness = 2,
                    Color = OxyColors.Purple,
                    Text = $"Peak {peakValue:#.0} nA\n@ {window.SelectedFile.AOChannels.First().TriangleWaveform[peakIndex]:0.00} V"
                });

                window.ScopeRawModel.Model.Annotations.Add(new ArrowAnnotation()
                {
                    EndPoint = new DataPoint(peakIndex, y_dataRaw[peakIndex]),
                    StartPoint = new DataPoint(peakIndex, y_dataRaw[peakIndex] + (100)),
                    YAxisKey = "primaryRaw",
                    StrokeThickness = 2,
                    Color = OxyColors.Purple,
                    Text = $"Peak {peakValue:#.0} nA\n@ {window.SelectedFile.AOChannels.First().TriangleWaveform[peakIndex]:0.00} V"
                });

                #endregion
            }

            window.ScopeModel.Model.InvalidatePlot(true);
            window.ScopeModel.ScalePlotAxes(false);

            window.ScopeRawModel.Model.InvalidatePlot(true);
            window.ScopeRawModel.ScalePlotAxes(false);
        }

        #endregion

        #region Private and Protected Functions

        private void Reset()
        {
            AddLog("----------------------------------------------------");
            AddLog("Resetting now {0}", DateTime.Now);
            AddLog("----------------------------------------------------");

            FileCount = 0;
            Files?.Clear();

            SelectedFile = null;
            SelectedSweep = -1;

            ScopeModel?.Clear();
            ScopeRawModel?.Clear();
            IntegratedModel?.Clear();
            AllIntegratedModel?.Clear();
        }

        protected virtual bool IsFileLocked(string Filename)
        {
            FileStream stream = null;

            try
            {
                FileInfo file = new(Filename);

                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                stream?.Close();
            }

            //file is not locked
            return false;
        }

        private static int FindClosestIndex(decimal[] Data, decimal Value, System.Windows.Controls.ExpandDirection Direction = ExpandDirection.Up)
        {
            ArgumentNullException.ThrowIfNull(Data, nameof(Data));
            ArgumentNullException.ThrowIfNull(Value, nameof(Value));

            switch (Direction)
            {
                case ExpandDirection.Up:
                    {
                        for (int i = 0; i < Data.Length; i++)
                        {
                            if (Data[i] > Value)
                                return i;
                        }
                        break;
                    }
                case ExpandDirection.Down:
                    {
                        for (int i = 0; i < Data.Length; i++)
                        {
                            if (Data[i] > Value)
                                return i - 1;
                        }
                        break;
                    }
                default:
                    {
                        for (int i = 0; i < Data.Length; i++)
                        {
                            if (Data[i] > Value)
                                return i;
                        }
                        break;
                    }
            }
            return -1;
        }

        private static void DebugFileStructure(LVBinaryReader reader)
        {
            long pos;
            double d_;
            float f_;
            Int32 i32_;
            Int16 i16_;
            UInt16 ui16_;

            long start = 0;
            reader.BaseStream.Seek(start, SeekOrigin.Begin);

            int column_width = 15;

            do
            {
                pos = reader.BaseStream.Position;

                d_ = reader.ReadDouble();
                reader.BaseStream.Position = pos;
                f_ = reader.ReadSingle();
                reader.BaseStream.Position = pos;
                i32_ = reader.ReadInt32();
                reader.BaseStream.Position = pos;
                i16_ = reader.ReadInt16();
                reader.BaseStream.Position = pos;
                ui16_ = reader.ReadUInt16();
                reader.BaseStream.Position = pos;
                Debug.Print("{0}>\tDouble64:{1}\tFloat32:{2}\tInt32:{3}\tInt16:{4}\tUInt16:{5}",
                    pos,
                    d_.ToString("0.00E+00").PadRight(column_width),
                    f_.ToString("0.00E+00").PadRight(column_width),
                    i32_.ToString().PadRight(column_width),
                    i16_.ToString().PadRight(column_width),
                    ui16_.ToString().PadRight(column_width));
                //Debug.Print("{0}>    Double64:{1}    Float32:{2}    Int32:{3}     Int16:{4}", pos, d_.ToString("0.00E+00"), f_.ToString("0.00E+00"), i32_, i16_);

                reader.BaseStream.Seek(2, SeekOrigin.Current);
            } while (pos < 25350);
            //for (long i = start; i < 11652; i++)
            //{
            //    //Debug.Print(i.ToString());
            //    //reader.BaseStream.Seek(i, SeekOrigin.Begin);


            //}
        }

        private void AddLog(string Message, params object[] Items)
        {
            int i = this.Logs.Count + 1;
            string s = i + ": " + string.Format(Message, Items);

            Dispatcher.Invoke((Action)(() =>
            {
                Logs.Add(s);
                Debug.Print(s);
            }));

        }

        private void CalculateCOV()
        {
            if (COV_window < 2)
                AddLog("COV window size is invalid");

            double mean = 0;
            double stdev = 1;
            if (Files.Count > COV_window)
            {
                mean = Files.TakeLast(COV_window).SelectMany(a => a.AIChannels[SelectedAIChannel.ChannelNumber].IntegratedData).Select(b => Convert.ToDouble(b)).Average();
                //MathNet.Numerics.Statistics.Statistics.StandardDeviation()
                //Files.TakeLast(10).SelectMany(a => a.Key.IntegratedData).St     MathNet
                stdev = Files.TakeLast(COV_window).SelectMany(a => a.AIChannels[SelectedAIChannel.ChannelNumber].IntegratedData).Select(b => Convert.ToDouble(b)).StandardDeviation();
                COV = stdev / mean;
            }
            else
                COV = 0.0d;
        }

        /// <summary>
        /// Analyze the FSCV trace to get some values of interest
        /// </summary>
        private void CalculateSD()
        {
            if (Files == null | Files.Count == 0)
                return;

            AnalysisOutputLB.Items.Clear();
            AnalysisOutputLB.Items.Add("CalculateSD");

            //double baseline_SD = 0;
            //double evoked_Max = 0;

            //LVBinFile file = this.Files.First();

            try
            {
                //baseline_SD = DataAnalysisHelper.AnalyzeData(IntegratedModel.Model.Series.First().CastTo<LineSeries>().Points.Select(a => a.Y).ToArray(), AggregateFunctionEnum.StandardDeviation, 0, file.CollectionPreEventScanCount);
                //evoked_Max = DataAnalysisHelper.AnalyzeData(IntegratedModel.Model.Series.First().CastTo<LineSeries>().Points.Select(a => a.Y).ToArray(), AggregateFunctionEnum.Maximum, file.CollectionPreEventScanCount, file.CollectionPreEventScanCount + 10);

                //AnalysisOutputLB.Items.Add(string.Format("Baseline SD: {0}", baseline_SD.ToString("0.000")));
                //AnalysisOutputLB.Items.Add(string.Format("Maximum: {0}", evoked_Max.ToString("0.000")));
                //AnalysisOutputLB.Items.Add(string.Format("Ratio: {0}", (evoked_Max / baseline_SD).ToString("0.00")));
                //AnalysisOutputLB.Items.Add(string.Format("Passed Threshold: {0}", (evoked_Max / baseline_SD) > 2 ? "TRUE" : "FALSE"));
            }
            catch
            {
                AnalysisOutputLB.Items.Add("FAILED");
            }
            //file.DebugDump();
        }

        private static string GetSettingsFolder()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (Directory.Exists(folder) == false)
                Directory.CreateDirectory(folder);
            return folder;
        }

        private void JumpToSweep_Click(object sender, RoutedEventArgs e)
        {
            SelectedSweep = LowerSweepBound;
        }


        //private void CalculateGainMultiplier(decimal nA_V, decimal AD_Gain)
        //{
        //    Gain = AD_Gain;     //works with new multi-channel version of file 23/10/2018
        //    //if (AD_Gain > 0m)
        //    //    Gain = (20m / 65536) * (nA_V / AD_Gain);
        //}

        #endregion

        #region Integration

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="LowerBound"></param>
        /// <param name="UpperBound"></param>
        /// <param name="a">The Sample Period (= sweep duration / # of samples) in Seconds</param>
        /// <returns>Result is in pColoumbs (I think)</returns>
        private decimal CalculateCharge(decimal[] Data, long LowerBound, long UpperBound, decimal a)
        {
            decimal curve = IntegrateRange(Data, LowerBound, UpperBound, a);

            //Data = Data.MultiplyArray(1e-9m);

            decimal wedge = Integrate(Data, LowerBound, UpperBound, a * (UpperBound - LowerBound));
            return (curve - wedge) * 1e6m; // * 1e12m;
        }

        private decimal IntegrateRange(decimal[] Data, long LowerBound, long UpperBound, decimal a)
        {
            decimal result = 0m;
            for (long i = LowerBound; i < UpperBound; i++)
            {
                result += Integrate(Data, i, i + 1, a);
            }
            return result;
        }

        private static decimal Integrate(decimal[] Data, long LowerBound, long UpperBound, decimal a)
        {
            return ((Data[LowerBound] + Data[UpperBound]) * 0.5m) * a;

            //return (0.5m * a) * (Data[LowerBound] + Data[UpperBound]);
        }

        #endregion

        #region Copy data

        private void CopyTemporalToClipboard_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("Will copy the Selected AIChannel Integrated Temporal data only");
            if (SelectedAIChannel == null)
            {
                MessageBox.Show("No AIChannel selected");
                return;
            }
            if (OutputAggregateFunction == AggregateFunctionEnum.MaximumEffect)
            {
                MessageBox.Show("OutputAggregateFunction.MaximumEffect data cannot be exported currently");
                return;
            }
            try
            {
                StringBuilder sb = new();
                sb.AppendLine($"Filename, Time (min), Time (sec), {string.Join(",", this.Files.First().AIChannels.Select((a, i) => $"Channel #{i}"))}");

                int j = 0;
                foreach (LVBinFile file in this.Files)
                {
                    string output = $"{file.Filename}, {file.Duration.TotalMinutes * j}, {file.Duration.TotalSeconds * j}";


                    j++;

                    foreach (AIChannel channel in file.AIChannels)
                    {
                        output += $", {channel.IntegratedData.AnalyzeData(this.OutputAggregateFunction)}";
                    }
                    sb.AppendLine(output);
                    //sb.AppendLine(string.Format("{0},{1},{2}",
                    //    file.Filename,
                    //    file.AIChannels[0].IntegratedData != null ? file.AIChannels[0].IntegratedData.AnalyzeData(this.OutputAggregateFunction).ToString() : "0",
                    //    file.AIChannels[1]?.IntegratedData != null ? file.AIChannels[1].IntegratedData.AnalyzeData(this.OutputAggregateFunction).ToString() : "0"));
                }
                //if (SelectedAIChannel.ChannelNumber == 0)
                //    AllIntegratedModel.Points1.Select(a => a.Y).CopyToClipboard();
                //else if (SelectedAIChannel.ChannelNumber == 1)
                //    AllIntegratedModel.Points2.Select(a => a.Y).CopyToClipboard();
                Clipboard.SetText(sb.ToString(), TextDataFormat.CommaSeparatedValue);
                AddLog("Integrated Data copied to Clipboard ({0} files)", Files.Count);
            }
            catch
            {
                AddLog("Unable to copy Integrated Data to Clipboard");
            }
        }

        //private void CopyVoltageWaveform_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        SelectedFile.TriangleWaveform.CopyToClipboard();
        //        AddLog("Voltage Waveform copied to Clipboard");
        //    }
        //    catch
        //    {
        //        AddLog("Unable to copy Votlage Waveform to Clipboard");
        //    }
        //}

        //private void CopyCurrentResponse_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        SelectedFile.GetScaledData(SelectedSweep, Gain, Offset, nA_V, BackgroundSubtractionSweepCount).CopyToClipboard();
        //        AddLog("Current Waveform copied to Clipboard (array index {0})", SelectedSweep);
        //    }
        //    catch
        //    {
        //        AddLog("Unable to copy Current Waveform to Clipboard");
        //    }
        //}

        private void CopyChargeData_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFile == null)
                return;
            if (SelectedAIChannel == null)
            {
                MessageBox.Show("First select an AIChannel");
                return;
            }
            try
            {
                SelectedAIChannel.IntegratedData.CopyToClipboard();
                AddLog("Charge data copied to Clipboard");
            }
            catch
            {
                AddLog("Unable to copy Charge data to Clipboard");
            }
        }

        private void CopyBackgroundResponse_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFile == null)
                return;
            try
            {
                SelectedFile.AIChannels.First().SubtractData.CopyToClipboard();
                AddLog("Background Current Waveform copied to Clipboard (array index {0})", SelectedSweep);
            }
            catch
            {
                AddLog("Unable to copy Background Current Waveform to Clipboard");
            }
        }

        #endregion

        //--------------------------------------------------------------------------
        // This function returns the data filtered. Converted to C# 2 July 2014.
        // Original source written in VBA for Microsoft Excel, 2000 by Sam Van
        // Wassenbergh (University of Antwerp), 6 june 2007.
        //--------------------------------------------------------------------------

        #region Settings

        private readonly string RegistryPathBase = "Software\\Wow6432Node\\Silicon Nervous System\\Live Electrochemistry";          //if changed, update in SNS_Post_Install_Scripting


        private object GetRegistryValue(string Value, bool CurrentUser = false)
        {
            if (CurrentUser == false)
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(RegistryPathBase))
                {
                    if (key != null)
                    {
                        return key.GetValue(Value);
                    }
                    else
                        Debug.Print("Unable to GetRegistryValue '{0}'", Value);
                }
            }
            else
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPathBase))
                {
                    if (key != null)
                    {
                        return key.GetValue(Value);
                    }
                    else
                        Debug.Print("Unable to GetRegistryValue '{0}'", Value);
                }
            }
            return null;
        }

        private void SaveSettings()
        {
            XmlWriter writer = XmlWriter.Create(GetSettingsFolder() + "\\Live Electrochemistry Settings.xml");
            XmlSerializer serializer = new(typeof(Settings));

            Settings s = new();

            //s.CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            var result = GetRegistryValue("InstallerVersion");      //this will cause problems when running from Visual Studio (i.e. not properly installed)

            if (result == null)
            {
                if (Debugger.IsAttached)
                    s.CurrentVersion = new Version(0, 0, 0, 1).ToString();
                else
                    s.CurrentVersion = new Version(0, 0, 0, 0).ToString();
            }
            else if (!string.IsNullOrWhiteSpace(result.ToString()))
                s.CurrentVersion = result.ToString();
            else
                s.CurrentVersion = new Version(0, 0, 0, 0).ToString();

            s.RecentFolder = this.SourceFolder;
            s.IsExtensionFilterEnabled = this.IsExtensionFilterEnabled;
            s.ExtensionFilterString = this.ExtensionFilterString;
            serializer.Serialize(writer, s);

            writer.Close();

            AddLog("SaveSettings::Filename = {0}", GetSettingsFolder() + "\\Live Electrochemistry Settings.xml");
        }

        private void LoadSettings()
        {
            Settings s = new()
            {
                RecentFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (File.Exists(GetSettingsFolder() + "\\Live Electrochemistry Settings.xml"))
            {
                XmlReader reader = null;
                Debug.Print("Settings::{0}", GetSettingsFolder() + "\\Live Electrochemistry Settings.xml");

                try
                {
                    reader = XmlReader.Create(GetSettingsFolder() + "\\Live Electrochemistry Settings.xml");
                    XmlSerializer Serializer = new(typeof(Settings));
                    s = (Settings)Serializer.Deserialize(reader);
                }
                catch
                {
                    AddLog("Error occured trying to Deserialize Live Electrochemistry Settings.xml");
                }
                finally
                {
                    reader?.Close();

                    //check if the Settings file was created with a different version of the Application (using InstallerVersion as a proxy for Application Version since I can control InstallerVersion easier)
                    var result = GetRegistryValue("InstallerVersion");
                    string CurrentVersion = result == null ? string.Empty : result.ToString();
                    //if (Debugger.IsAttached)
                    //{
                    //    CurrentVersion = new Version(0, 0, 0, 1).ToString();
                    //}
                    if (s.CurrentVersion != CurrentVersion)
                    {
                        MessageBox.Show(string.Format("Settings File was created with version {0}, which is not the current version {1}\n\nYou should delete the Settings and Layout .xml files from My Documents", s.CurrentVersion, CurrentVersion), "Incorrect Version", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    AddLog(string.Format("Settings File was created with Version {0} (Current Version {1})", s.CurrentVersion, CurrentVersion));
                }
            }
            else
            {
                //create empty Settings, since the file cannot be found (ie. running for the first time)
            }

            this.SourceFolder = s.RecentFolder;
            this.IsExtensionFilterEnabled = s.IsExtensionFilterEnabled;
            this.ExtensionFilterString = s.ExtensionFilterString;
        }

        #endregion
        private void File_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                var path = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
                if (Directory.Exists(path))
                {
                    this.SourceFolder = path;
                    return;
                }

                LVBinFile file;

                foreach (string filename in files)
                {

                    file = OpenLabViewBinFile(filename);


                    if (file == null)
                    {
                        MessageBox.Show("Unable to open the file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    file.AOChannels.First().TriangleWaveform.CopyToClipboard();


                    decimal[] scaleddata = file.AIChannels.First().GetScaledData(0, file.IsFilterEnabled, file.FilterCutOffFrequency, BackgroundSubtractionSweepCount);


                    IList<DataPoint> waveformpoints = new List<DataPoint>(file.TotalScanCount);
                    IList<DataPoint> currentpoints = new List<DataPoint>(file.TotalScanCount);
                    int j = 0;


                    foreach (decimal value in scaleddata)
                    {
                        waveformpoints.Add(new DataPoint(j, (double)file.AOChannels.First().TriangleWaveform[j]));
                        currentpoints.Add(new DataPoint(j, (double)value));
                        j++;
                    }

                    //QuickViewModel = new ScopeViewModel(currentpoints, waveformpoints);

                    decimal[] integrateddata = new decimal[file.TotalScanCount];

                    int LowerBound = FindClosestIndex(file.AOChannels.First().TriangleWaveform, (decimal)LowerVoltageBound);
                    int UpperBound = FindClosestIndex(file.AOChannels.First().TriangleWaveform, (decimal)UpperVoltageBound, ExpandDirection.Down);
                    //decimal a = (decimal)((file.SweepDuration / 1000) / file.SampleCount);

                    //decimal gain = Gain;
                    decimal offset = Offset;
                    //decimal na_v = nA_V;
                    int backgroundsubstactionsweepcount = BackgroundSubtractionSweepCount;

                    Parallel.For(0, file.TotalScanCount,
                        index =>
                        {
                            scaleddata = file.AIChannels.First().GetScaledData(index, file.IsFilterEnabled, file.FilterCutOffFrequency, backgroundsubstactionsweepcount);
                            integrateddata[index] = CalculateCharge(scaleddata, LowerBound, UpperBound, (decimal)file.AIChannels.First().SamplingPeriod);

                        });


                    //for (int i = 0; i < file.TotalScanCount; i++)
                    //{
                    //    scaleddata = file.GetScaledData(i, Gain, Offset, nA_V, BackgroundSubtractionSweepCount);

                    //    integrateddata[i] = CalculateCharge(scaleddata, LowerBound, UpperBound, (decimal)file.SamplingPeriod);

                    //}



                    //Helper.Extensions.Extensions.CopyToClipboard<decimal>(integrateddata);

                    //########################## The follow 1 line of code is disabled as it is not compatible with multichannel yet
                    //file.IntegratedData = integrateddata;
                    Files.Add(file);
                }
            }


            //############################## The following block could possoibly be updated to use the generic Files and SelectedFile objects
            //decimal[] averageintegrateddata = new decimal[quickfile.TotalScanCount];

            //if (quickfiles.Any(a => a.IntegratedData.Length != quickfiles.First().IntegratedData.Length))
            //{
            //    MessageBox.Show("Not all QuickFiles have the same duration and cannot be averaged together.\n\nPrevious files will be ignored");
            //    quickfiles.RemoveRange(0, quickfiles.Count - 2);
            //    return;
            //}

            //averageintegrateddata = Helper.Analysis.DataAnalysisHelper.CrossAverageArrays(quickfiles.Select(a => a.IntegratedData));

            //IList<DataPoint> integratedpoints = new List<DataPoint>(quickfile.TotalScanCount);
            //int p = 0;
            //foreach (decimal value in averageintegrateddata)
            //{
            //    integratedpoints.Add(new DataPoint(p, (double)value));
            //    p++;
            //}

            //QuickViewIntergratedModel = new IntegratedViewModel(integratedpoints);
            //######################################

            SelectedFile = Files.Last();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(ExportSweepSelection.Text, out int sweep) == false)
            {
                MessageBox.Show("Invalid ExportSweepSelection value (must be a number). Unable to continue");
                return;
            }

            if (SelectedFile == null)
            {
                MessageBox.Show("SelectedFile is null. Unable to continue");
                return;
            }

            //if (sweep < 0)            //the try..catch handle this now
            //{
            //    MessageBox.Show("Invalid Sweep #)");
            //    return;
            //}
            //if (sweep > SelectedFile.TotalScanCount)
            //{
            //    MessageBox.Show("Invalid Sweep #");
            //    return;
            //}

            try
            {
                SelectedFile.AIChannels.First().GetScaledData(sweep != -1 ? sweep : SelectedSweep, SelectedFile.IsFilterEnabled, SelectedFile.FilterCutOffFrequency, BackgroundSubtractionSweepCount).CopyToClipboard();
            }
            catch (IndexOutOfRangeException iore)
            {
                AddLog($"Unable to copy Voltammogram to Clipboard. IndexOutOfRangeException '{iore.Message}'. Check ExportSweepSelection");
            }
            catch (Exception ex)
            {
                AddLog($"Unable to copy Voltammogram to Clipboard. Exception '{ex.Message}'");
            }
        }
        private void CopyTriangleWaveform_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFile == null)
            {
                MessageBox.Show("SelectedFile is null. Unable to continue");
                return;
            }

            SelectedFile.AOChannels.First().TriangleWaveform.CopyToClipboard();
        }
        private void CopyVoltammogram_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(ExportSweepSelection.Text, out int sweep) == false)
            {
                MessageBox.Show("Invalid ExportSweepSelection value (must be a number). Unable to continue");
                return;
            }

            if (SelectedFile == null)
            {
                MessageBox.Show("SelectedFile is null. Unable to continue");
                return;
            }
            //if (SelectedAOChannel == null)            //just do all channels
            //{
            //    MessageBox.Show("Selected AOChannel is null. First select an AOChannel.  Unable able to continue");
            //    return;
            //}
            try
            {
                List<decimal[]> channeldata = new();
                List<string> headings = new() { "Voltage (V)" };

                foreach (AIChannel channel in SelectedFile.AIChannels)
                {
                    channeldata.Add(channel.GetScaledData(sweep != -1 ? sweep : SelectedSweep, SelectedFile.IsFilterEnabled, SelectedFile.FilterCutOffFrequency, BackgroundSubtractionSweepCount));
                    headings.Add($"Channel {channel.ChannelNumber} Current (nA?)");
                }



                SelectedFile.AOChannels.First().TriangleWaveform.CopyToClipboard(channeldata.First().ItemAsEnumerable(), headings);            //WARNING WARNING WARNING This still only uses the first AIChannel. Need to update CopyToClipboard to accept IEnumerable of IEnumerable<decimal>
                //SelectedFile.AOChannels.First().TriangleWaveform.CopyToClipboard(
                //    SelectedFile.AIChannels.First().GetScaledData(sweep != -1 ? sweep : SelectedSweep, SelectedFile.IsFilterEnabled, SelectedFile.FilterCutOffFrequency, BackgroundSubtractionSweepCount),
                //new List<string>() { "Voltage (V)", "Current (nA?)" });
                AddLog(string.Format("Voltammogram copied to Clipboard (sweep #{0} of file {1}, Channels {2})", sweep, SelectedFile.Filename, string.Join(",", SelectedFile.AIChannels.Select(a => a.ChannelNumber))));
            }

            catch (IndexOutOfRangeException iore)
            {
                AddLog($"Unable to copy Voltammogram to Clipboard. IndexOutOfRangeException '{iore.Message}'. Check ExportSweepSelection");
            }
            catch (Exception ex)
            {
                AddLog($"Unable to copy Voltammogram to Clipboard. Exception '{ex.Message}'");
            }
        }

        #region Debug

        private void DebugExportRaw_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFile == null)
                return;

            SelectedAIChannel.RawData[SelectedSweep].CopyToClipboard(AdditionalArrays: null, ("Raw").ItemAsEnumerable());

            MessageBox.Show(string.Format("Raw Data (I16) exported to clipboard\n\nSweep #{0} (zero indexed?)", SelectedSweep));
        }

        private void DebugExport_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFile == null)
                return;

            SelectedAIChannel.GetScaledData(SelectedSweep, SelectedFile.IsFilterEnabled, SelectedFile.FilterCutOffFrequency, 0).CopyToClipboard(AdditionalArrays: null, ("Data").ItemAsEnumerable());

            MessageBox.Show(string.Format("Data (decimal) exported to clipboard\n\nSweep #{0} (zero indexed?)\n\nNot background subtracted\n\nGain = {1}    Offset = {2}   nA/V = {3}",
                SelectedSweep, SelectedAIChannel.ADGain, Offset, SelectedAIChannel.nA_V));
        }

        private void DebugExportRawBackground_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFile == null)
                return;

            SelectedAIChannel.RawSubtractData.CopyToClipboard(AdditionalArrays: null, ("Raw Background").ItemAsEnumerable());

            MessageBox.Show(string.Format("Raw Background Data (I16) exported to clipboard\n\n{0} sweeps (from 0)", BackgroundSubtractionSweepCount));
        }

        private void DebugExportBackground_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFile == null)
                return;

            SelectedAIChannel.SubtractData.CopyToClipboard(AdditionalArrays: null, ("Background").ItemAsEnumerable());

            MessageBox.Show(string.Format("Background Data (decimal) exported to clipboard\n\n{0} sweeps (from 0)", BackgroundSubtractionSweepCount));

        }
        private void DebugExportVoltageTriangle_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFile == null)
                return;

            var stuff = SelectedFile.AOChannels.First().TriangleWaveform.CopyToClipboard(null, ("Voltage Triangle").ItemAsEnumerable());

            //SelectedFile.AOChannels.First().TriangleWaveform.AsEnumerable().CopyToClipboardXXX(AdditionalArrays: null, ("Voltage Triangle").ItemAsEnumerable());

            MessageBox.Show(string.Format("Voltage Triangle Command (decimal) exported to clipboard\n\n{0} data points", SelectedFile.AOChannels.Count));
        }

        #endregion
        private void FindPeak_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Feature disabled at the moment");
            //throw new NotImplementedException("This feature is not compatible with Multi-channel recording yet");
            //try
            //{
            //    double peak = (double)SelectedAIChannel.IntegratedData.Max();
            //    //Double peak = this.IntegratedModel.Points.Max(a => a.Y);
            //    DataPoint point;
            //    if (SelectedAIChannel.ChannelNumber == 0)
            //    {
            //        point = this.IntegratedModel.Points1.First(a => a.Y == peak);
            //    }
            //    else
            //        point = this.IntegratedModel.Points2.First(a => a.Y == peak);

            //    PeakValue.Text = peak.ToString("0.##");
            //    PeakValueTime.Text = "@ " + (point.X / SelectedFile.WaveformFrequency).ToString("0.0 s");

            //    SelectedSweep = (int)point.X;
            //}
            //catch
            //{
            //    MessageBox.Show("Unable to Find Peak");
            //}
        }

        #region Open and Close application, Load/Save Layout

        protected override void OnClosing(CancelEventArgs e)
        {
            SaveSettings();
            base.OnClosing(e);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        #endregion
        private void ScalePlot_Click(object sender, RoutedEventArgs e)
        {
            PlotViewModelBase pvmb = (PlotViewModelBase)((Button)sender).Tag;
            AxisPropertiesWindow apw = new()
            {
                Owner = this,
                PlotModel = pvmb
            };
            if (apw.ShowDialog().GetValueOrDefault(false) == true)
            {
                apw.PlotModel.ScalePlotAxes();
            }
        }

        private void ScaleRawPlot_Click(object sender, RoutedEventArgs e)
        {
            PlotViewModelBase pvmb = (PlotViewModelBase)((Button)sender).Tag;
            AxisPropertiesWindow apw = new()
            {
                Owner = this,
                PlotModel = pvmb
            };
            if (apw.ShowDialog().GetValueOrDefault(false) == true)
            {
                apw.PlotModel.ScalePlotAxes();
            }
        }

        #region Default Settings
        private void Default_FSCV_Click(object sender, RoutedEventArgs e)
        {
            WindowLowerBound = 0;
            WindowUpperBound = 9;
            OutputAggregateFunction = AggregateFunctionEnum.Maximum;

            //should try to automatically set Lower and Upper Window Bound from the files (or First example file)

        }

        private void Default_FSCAV_Click(object sender, RoutedEventArgs e)
        {
            WindowLowerBound = 0;
            WindowUpperBound = 2;
            OutputAggregateFunction = AggregateFunctionEnum.Average;
        }
        #endregion
        private static DoubleRectangle GetFullDataRange(OxyPlot.Wpf.PlotView Plot)
        {
            //DoubleRectangle result = new DoubleRectangle();

            //PlotModel pm = Plot.ActualModel;

            double ymin = double.MaxValue;
            double ymax = double.MinValue;
            double xmin = double.MaxValue;
            double xmax = double.MinValue;


            foreach (OxyPlot.Series.LineSeries series in Plot.Model.Series)
            {
                foreach (DataPoint item in series.Points)
                {
                    Debug.Print("object = {0}", item);
                    ymin = Math.Min(ymin, item.Y);
                    ymax = Math.Max(ymax, item.Y);
                    xmin = Math.Min(xmin, item.X);
                    xmax = Math.Max(xmax, item.X);
                }
                //for (int i = 0; i <= series.Points.Count() - 1; i++)
                //{
                //    ymin = Math.Min(ymin, series.Points[i].Y);
                //    ymax = Math.Max(ymax, series.Points[i].Y);
                //    xmin = Math.Min(ymin, series.Points[i].X);
                //    xmax = Math.Max(ymax, series.Points[i].X);
                //}
            }

            DoubleRectangle result = new()
            {
                Bottom = ymin,
                Top = ymax,
                Left = xmin,
                Right = xmax
            };
            return result;
        }
        private void EmailLog_Click(object sender, RoutedEventArgs e)
        {




            SmtpClient client = new("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("siliconnervoussystem@gmail.com", "cqbe mhfz oelv egux"), //new NetworkCredential(GetSNSEmail(), "eaqcxrxgndrzasnc", ""),
                EnableSsl = true
            };

            MailMessage message = new();
            message.To.Add(new MailAddress("peter.s.freestone@gmail.com"));
            message.From = new MailAddress("siliconnervoussystem@gmail.com");
            message.Body = string.Join("\n", Logs);
            message.Subject = string.Format("Live Electrochem Debug: {0}", DateTime.Now.ToString());

            //foreach (string filename in this.Attachments)
            //{
            //    message.Attachments.Add(new Attachment(filename));
            //}

            //NetworkCredential credential = new("siliconnervoussystem@gmail.com", @"wP39Q[20", "");

            //client.Credentials = credential;
            client.SendCompleted += new SendCompletedEventHandler(Client_SendCompleted);
            //client.SendAsync(message, string.Format("EmailFile_{0}", File.FileId));
            try
            {
                client.Send(message);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(string.Format("Local error occurred trying to send email\n\n{0}", ex.Message), "Error Emailing Message", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                client.Dispose();
            }
        }
        static void Client_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
                System.Windows.MessageBox.Show("SendEmail was cancelled");
            if (e.Error != null)
                System.Windows.MessageBox.Show(string.Format("SendEmail Failed\n\n{0}", e.Error.Message));
            else
            {
                System.Windows.MessageBox.Show("Email sent ok");
                //add audit entry
            }
        }

        private void CalibrationSource_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog d = new()
            {
                Multiselect = false
            };        //eventually support opening a folder so that it works for FSCAV calibrations (not just FSCV)

            if (d.ShowDialog(this) == true)
            {
                this.CalibrationSource = d.FileName;
            }
            else
                return;

            LVBinFile calibrationfile = OpenLabViewBinFile(this.CalibrationSource);
            if (calibrationfile == null)
            {
                AddLog("Unable to open Calibration File");
                return;
            }
            AddLog("Calibration analysis started");

            long LowerBound = FindClosestIndex(calibrationfile.AOChannels.First().TriangleWaveform, (decimal)LowerVoltageBound);
            long UpperBound = FindClosestIndex(calibrationfile.AOChannels.First().TriangleWaveform, (decimal)UpperVoltageBound, ExpandDirection.Down);
            int RMS_lb = CalibrationRMSWindowCentre - (int)(0.5 * (CalibrationRMSWindowWidth - 1));
            int RMS_ub = CalibrationRMSWindowCentre + (int)(0.5 * (CalibrationRMSWindowWidth - 1));

            if (RMS_lb < 0)
            {
                AddLog("CalibrationRMSWindowCentre and/or CalibrationRMSWindowWidth are invalid (out of range)");
                return;
            }
            if (RMS_ub >= calibrationfile.AIChannels.First().SampleCount)
            {
                AddLog("CalibrationRMSWindowCentre and/or CalibrationRMSWindowWidth are invalid (out of range)");
                return;
            }
            CalibrationSteps.Clear();
            CalibrationSteps.AddRangeUnique(ExtractCalibrationSteps(calibrationfile));

            //            return;           //PF 6/9/2021 Removed this line - not sure why I didn't want the code below to execute?


            AddLog("Peak search Window {0} ({1} V) to {2} ({3} V)", LowerBound, LowerVoltageBound, UpperBound, UpperVoltageBound);

            foreach (AIChannel aichannel in calibrationfile.AIChannels)
            {
                double[] values_a = new double[aichannel.ScanCount - 1];
                double[] values_b = new double[aichannel.ScanCount - 1];
                double value_a; //this is the value (Current) taken from the voltamogram at a certain/fixed INDEX/VOLTAGE
                double value_b; //this is the PEAK value (Current) taken from the voltamogram (within a window)
                double value_RMS;

                List<double> performance_GetScaledData = new(aichannel.ScanCount);
                List<double> performance_Remaining = new(aichannel.ScanCount);

                Stopwatch sw = new();



                Stopwatch overall = new();
                overall.Restart();
                for (int scan = 0; scan < aichannel.ScanCount - 1; scan++)
                {
                    sw.Restart();
                    //double[] data = Array.ConvertAll<decimal, double>(aichannel.GetScaledData(i, calibrationfile.IsFilterEnabled, calibrationfile.FilterCutOffFrequency, BackgroundSubtractionSweepCount), new Converter<decimal, double>(DecimalToDouble));

                    decimal[] data = aichannel.GetScaledData(scan, calibrationfile.IsFilterEnabled, calibrationfile.FilterCutOffFrequency, BackgroundSubtractionSweepCount);
                    performance_GetScaledData.Add(sw.ElapsedMilliseconds);
                    sw.Stop();

                    sw.Restart();
                    value_a = (double)data[CalibrationVoltammogramPeakIndex];
                    value_b = data.MaxWithIndex<decimal, double>(out long IndexOfPeak, LowerBound, UpperBound);

                    values_a[scan] = value_a;
                    values_b[scan] = value_b;
                    value_RMS = data.CalculateRMS(RMS_lb, RMS_ub);

                    Debug.Print("{0}\t{1} (@{6})\t{2} (@{3})\t{4}\t{5}", scan, value_a, value_b, IndexOfPeak, value_RMS, value_b / value_RMS, CalibrationVoltammogramPeakIndex);

                    sw.Stop();
                    performance_Remaining.Add(sw.ElapsedMilliseconds);

                }
                TemporalTrace tt = new(values_a, 1, DateTime.Now);
                CalibrationPlot.AddTrace(tt, this, true);

                Debug.Print("Average performance for performance_GetScaledData = {0}±{1} ms", performance_GetScaledData.Average(), Helper.Analysis.DataAnalysisHelper.CalculateError(performance_GetScaledData.ToArray(), null, ErrorTypesEnum.SEM));
                Debug.Print("Average performance for performance_Remaining = {0}±{1} ms", performance_Remaining.Average(), Helper.Analysis.DataAnalysisHelper.CalculateError(performance_Remaining.ToArray(), null, ErrorTypesEnum.SEM));
                overall.Stop();
                Debug.Print("Overall time = {0} ms", overall.ElapsedMilliseconds);
            }


            CalibrationPlot.InvalidateVisual();
            CalibrationPlot.UpdateLayout();


        }

        private IEnumerable<Event> ExtractCalibrationSteps(LVBinFile CalibrationFile)
        {
            IEnumerable<Event> calibrationsteps = null;

            //initial scan to detect where concentrations are
            foreach (AIChannel aichannel in CalibrationFile.AIChannels)
            {
                decimal[] rawdata = new decimal[aichannel.ScanCount - 1];
                decimal[] rawdata_0 = new decimal[aichannel.ScanCount - 1];
                decimal[] rawdata_1 = new decimal[aichannel.ScanCount - 1];
                decimal[] rawdata_2 = new decimal[aichannel.ScanCount - 1];
                decimal[] rawdata_3 = new decimal[aichannel.ScanCount - 1];
                decimal[] rawdata_4 = new decimal[aichannel.ScanCount - 1];

                for (int scan = 0; scan < aichannel.ScanCount - 1; scan++)
                {

                    ///################################ ATTENTION - Currently uses the UNFILTERED data to improve performance
                    //decimal[] xxx = aichannel.GetScaledData(scan, false, calibrationfile.FilterCutOffFrequency, BackgroundSubtractionSweepCount);
                    //Debug.Print("scan #{0}\txxx is {1}", scan, xxx != null);
                    //Debug.Print("\t\t\t{0}", xxx[CalibrationVoltamogramPeakIndex]);
                    rawdata[scan] = aichannel.GetScaledData(scan, true, CalibrationFile.FilterCutOffFrequency, BackgroundSubtractionSweepCount)[CalibrationVoltammogramPeakIndex];

                    //Debug.Print("{0}\t{1}", scan, data[CalibrationVoltamogramPeakIndex]);
                }
                //filter the rawdata
                rawdata = DataAnalysisHelper.Butterworth(rawdata, 1 / CalibrationFile.WaveformFrequency, 0.1);
                //rawdata_0 = DataAnalysisHelper.Butterworth(rawdata, 1 / calibrationfile.WaveformFrequency, 0.1);      //these are for debugging only
                //rawdata_1 = DataAnalysisHelper.Butterworth(rawdata, 1 / calibrationfile.WaveformFrequency, 0.5);
                //rawdata_2 = DataAnalysisHelper.Butterworth(rawdata, 1 / calibrationfile.WaveformFrequency, 1);
                //rawdata_3 = DataAnalysisHelper.Butterworth(rawdata, 1 / calibrationfile.WaveformFrequency, 2);
                //rawdata_4 = DataAnalysisHelper.Butterworth(rawdata, 1 / calibrationfile.WaveformFrequency, 4);


                double[] xdata = DataAnalysisHelper.FillSeries<double>(aichannel.ScanCount - 1, 1 / CalibrationFile.WaveformFrequency, 0); //new double[aichannel.ScanCount - 1];
                //DataAnalysisHelper.FillSeries<double>(xdata, 1 / CalibrationFile.WaveformFrequency);
                double[] data = Array.ConvertAll(rawdata, new Converter<decimal, double>(DecimalToDouble));
                double[] differentiated = data.Differentiate(xdata);


                EventDetectorProfile normalprofile = EventDetectorProfile.EmptyProfile("Normal");
                EventDetectorProfile differentiatedprofile = new("Differentiated")
                {
                    EventPolarity = EventPolarityEnum.Up,
                    IsMinimumValueEnabled = true,
                    MinimumValue = new CalculatedValue(0.2)
                };

                calibrationsteps = DataAnalysisHelper.DetectEvents(data, xdata, 1 / CalibrationFile.WaveformFrequency, normalprofile, differentiatedprofile).Select(a => (Event)a).ToList();

                //debug output to testing
                for (int scan = 0; scan < aichannel.ScanCount - 1; scan++)
                {
                    //Debug.Print("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", rawdata[scan], rawdata_0[scan], rawdata_1[scan], rawdata_2[scan], rawdata_3[scan], rawdata_4[scan]);


                    //Debug.Print("{0}\t{1}\t{2}", scan, rawdata[scan], differentiated[scan]);
                }
            }

            return calibrationsteps;
        }

        public static double DecimalToDouble(decimal value)
        {
            return (double)value;
        }

    }

}
