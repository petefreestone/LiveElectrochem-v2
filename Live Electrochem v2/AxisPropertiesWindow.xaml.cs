using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;
using Helper.Common;

namespace Live_Electrochem
{
    /// <summary>
    /// Interaction logic for AxisPropertiesWindow.xaml
    /// </summary>
    public partial class AxisPropertiesWindow : Window
    {
        public AxisPropertiesWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }



        //public DoubleRectangle DataRange
        //{
        //    get { return (DoubleRectangle)GetValue(DataRangeProperty); }
        //    set { SetValue(DataRangeProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for DataRange.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty DataRangeProperty =
        //    DependencyProperty.Register("DataRange", typeof(DoubleRectangle), typeof(AxisPropertiesWindow), new PropertyMetadata(null));



        //public DoubleRectangle FullDataRange
        //{
        //    get { return (DoubleRectangle)GetValue(FullDataRangeProperty); }
        //    set { SetValue(FullDataRangeProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for FullDataRange.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty FullDataRangeProperty =
        //    DependencyProperty.Register("FullDataRange", typeof(DoubleRectangle), typeof(AxisPropertiesWindow), new PropertyMetadata(null));




        public PlotViewModelBase PlotModel
        {
            get { return (PlotViewModelBase)GetValue(PlotModelProperty); }
            set { SetValue(PlotModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PlotModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlotModelProperty =
            DependencyProperty.Register("PlotModel", typeof(PlotViewModelBase), typeof(AxisPropertiesWindow), new PropertyMetadata(null));




        //public double Y_Minimum
        //{
        //    get { return (double)GetValue(Y_MinimumProperty); }
        //    set { SetValue(Y_MinimumProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for Y_Minimum.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty Y_MinimumProperty =
        //    DependencyProperty.Register("Y_Minimum", typeof(double), typeof(AxisPropertiesWindow), new PropertyMetadata(0d));



        //public double Y_Maximum
        //{
        //    get { return (double)GetValue(Y_MaximumProperty); }
        //    set { SetValue(Y_MaximumProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for Y_Maximum.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty Y_MaximumProperty =
        //    DependencyProperty.Register("Y_Maximum", typeof(double), typeof(AxisPropertiesWindow), new PropertyMetadata(0d));



        //public double X_Minimum
        //{
        //    get { return (double)GetValue(X_MinimumProperty); }
        //    set { SetValue(X_MinimumProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for X_Minimum.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty X_MinimumProperty =
        //    DependencyProperty.Register("X_Minimum", typeof(double), typeof(AxisPropertiesWindow), new PropertyMetadata(0d));



        //public double X_Maximum
        //{
        //    get { return (double)GetValue(X_MaximumProperty); }
        //    set { SetValue(X_MaximumProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for X_Maximum.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty X_MaximumProperty =
        //    DependencyProperty.Register("X_Maximum", typeof(double), typeof(AxisPropertiesWindow), new PropertyMetadata(0d));

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            base.Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            base.Close();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (this.PlotModel == null)
            {
                Debug.Print("######### Reset_Click called: this.PlotModel == Null therefore do nothing");
                return;
            }

            string axes = ((Button)sender).Tag.ToString().Substring(0, 1);
            bool global = ((Button)sender).Tag.ToString().Substring(1, 1) == "G" ? true : false;
            string mode = ((Button)sender).Tag.ToString().Contains("Max") ? "Maximum" : "Minimum";

            DoubleRectangle fdr = this.PlotModel.FullDataRectangle; //global ? this.PlotModel.FullDataRectangle : this.PlotModel.DataRectangle;

            if (axes == "X")
            {
                if (mode == "Maximum")
                {
                    this.PlotModel.DataRectangle.Right = fdr.Right;
                }
                else
                {
                    this.PlotModel.DataRectangle.Left = fdr.Left;
                }
            }
            else
            {
                if (mode == "Maximum")
                {
                    this.PlotModel.DataRectangle.Top = fdr.Top;
                }
                else
                {
                    this.PlotModel.DataRectangle.Bottom = fdr.Bottom;
                }
            }
        }
    }
}
