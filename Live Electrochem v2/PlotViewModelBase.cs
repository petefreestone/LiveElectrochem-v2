using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
//using OxyPlot.Wpf;
using Helper.Common;
using System.Diagnostics;

namespace Live_Electrochem
{
    public abstract class PlotViewModelBase
    {

        public PlotViewModelBase()
        {
            this.FullDataRectangle = new DoubleRectangle();
            this.DataRectangle = new DoubleRectangle();
        }

        #region OxyPlot MODEL implementation and extensions

        public PlotModel Model { get; set; }

        //public void AddScatterSeries(IEnumerable<double> xSeries, IEnumerable<double> ySeries)
        //{
        //    AddScatterSeries(xSeries, ySeries, OxyColors.Automatic);
        //}

        //public void AddScatterSeries(IEnumerable<double> xSeries, IEnumerable<double> ySeries, OxyColor color)
        //{
        //    var scatterSeries = new ScatterSeries()
        //    {
        //        MarkerFill = color,
        //        MarkerSize = 1,
        //    };

        //    foreach (var item in xSeries.Zip(ySeries, (x, y) => new { x, y }))
        //    {
        //        scatterSeries.Points.Add(new ScatterPoint(item.x, item.y));
        //    }

        //    Model.Series.Add(scatterSeries);
        //}

        public void AddHighlightedPoint(double x, double y)
        {
            AddHighlightedPoint(x, y, OxyColors.Automatic);
        }

        public void AddHighlightedPoint(double x, double y, OxyColor color)
        {
            var scatterSeries = new ScatterSeries()
            {
                MarkerFill = color,
                MarkerType = MarkerType.Triangle,
                MarkerSize = 5,
            };

            scatterSeries.Points.Add(new ScatterPoint(x, y));

            Model.Series.Add(scatterSeries);
        }

        public void AddLineSeries(IEnumerable<double> xSeries, IEnumerable<double> ySeries)
        {
            AddLineSeries(xSeries, ySeries, OxyColors.Automatic);
        }

        public void AddLineSeries(IEnumerable<DateTime> xSeries, IEnumerable<double> ySeries)
        {
            AddLineSeries(xSeries, ySeries, OxyColors.Automatic);
        }

        public void AddLineSeries(IEnumerable<DateTime> xSeries, IEnumerable<double> ySeries, OxyColor color)
        {
            Model.Axes.Add(new DateTimeAxis());
            AddLineSeries(xSeries.Select(x => DateTimeAxis.ToDouble(x)), ySeries);
        }

        public void AddLineSeries(IEnumerable<double> xSeries, IEnumerable<double> ySeries, OxyColor color)
        {
            var lineSeries = new LineSeries();
            foreach (var item in xSeries.Zip(ySeries, (x, y) => new { x, y }))
            {
                lineSeries.Points.Add(new DataPoint(item.x, item.y));
            }

            Model.Series.Add(lineSeries);
            UpdateFullDataRectangle();
        }

        //public void AddColumnSeries(IEnumerable<string> xLabels, IEnumerable<double> ySeries)
        //{
        //    AddColumnSeries(xLabels, ySeries, OxyColors.Automatic);
        //}

        //public void AddColumnSeries(IEnumerable<string> xLabels, IEnumerable<double> ySeries, OxyColor color)
        //{
        //    var axis = new CategoryAxis() { Position = AxisPosition.Bottom };
        //    axis.Labels.AddRange(xLabels);
        //    Model.Axes.Add(axis);

        //    var columnSeries = new ColumnSeries()
        //    {
        //        FillColor = color,
        //    };

        //    columnSeries.Items.AddRange(ySeries.Select(y => new ColumnItem(y)));

        //    Model.Series.Add(columnSeries);
        //}

        #endregion
        public string Title { get; set; }

        private bool AutoScaleX_ = true;
        public bool AutoScaleX
        {
            get { return AutoScaleX_; }
            set { AutoScaleX_ = value; }
        }

        private bool AutoScaleY_ = true;
        public bool AutoScaleY
        {
            get { return AutoScaleY_; }
            set { AutoScaleY_ = value; }
        }

        public DoubleRectangle DataRectangle { get; protected set; }
        public DoubleRectangle FullDataRectangle { get; protected set; }

        public void UpdateFullDataRectangle()
        {
            this.FullDataRectangle.Left = double.MaxValue;
            this.FullDataRectangle.Right = double.MinValue;
            this.FullDataRectangle.Bottom = double.MaxValue;
            this.FullDataRectangle.Top = double.MinValue;


            //LinearAxis axis = (LinearAxis)this.Model.Axes.Where(a => a.Position == AxisPosition.Left).First();

            foreach (LineSeries series in this.Model.Series)
            {
                this.FullDataRectangle.Left = Math.Min(this.FullDataRectangle.Left, series.Points.Min(a => a.X));
                this.FullDataRectangle.Right = Math.Max(this.FullDataRectangle.Right, series.Points.Max(a => a.X));
                this.FullDataRectangle.Bottom = Math.Min(this.FullDataRectangle.Bottom, series.Points.Min(a => a.Y));
                this.FullDataRectangle.Top = Math.Max(this.FullDataRectangle.Top, series.Points.Max(a => a.Y));
            }

            //this.FullDataRectangle = FullDataRectangle;
        }
        //public void SetDataRectangle(DoubleRectangle DataRectangle)
        //{
        //    if (DataRectangle != null)
        //        this.DataRectangle = DataRectangle;
        //    else
        //        this.DataRectangle = FullDataRectangle;
        //}

        /// <summary>
        /// Calls Model.InvalidatePlot(true) after adjusting/scaling the axes
        /// </summary>
        /// <param name="Reset"></param>
        public virtual void ScalePlotAxes(bool Reset = false)
        {
            try
            {
                if (Reset)
                {
                    LinearAxis x_axis = (LinearAxis)Model.Axes.First(a => a.Position == OxyPlot.Axes.AxisPosition.Bottom);
                    x_axis.Minimum = FullDataRectangle.Left;
                    x_axis.Maximum = FullDataRectangle.Right;
                    LinearAxis y_axis = (LinearAxis)Model.Axes.First(a => a.Position == OxyPlot.Axes.AxisPosition.Left);
                    y_axis.Minimum = FullDataRectangle.Bottom;
                    y_axis.Maximum = FullDataRectangle.Top;

                    this.Model.InvalidatePlot(true);        //this might not need the TRUE
                    return;
                }

                //X-axis
                if (AutoScaleX)
                {
                    LinearAxis x_axis = (LinearAxis)Model.Axes.First(a => a.Position == OxyPlot.Axes.AxisPosition.Bottom);
                    x_axis.Minimum = FullDataRectangle.Left;
                    x_axis.Maximum = FullDataRectangle.Right;       //could be optimized to use the First and Last DataPoints
                }
                else
                {
                    LinearAxis x_axis = (LinearAxis)Model.Axes.First(a => a.Position == OxyPlot.Axes.AxisPosition.Bottom);
                    x_axis.Minimum = DataRectangle.Left;
                    x_axis.Maximum = DataRectangle.Right;
                }

                //Y-axis
                if (AutoScaleY)
                {
                    LinearAxis y_axis = (LinearAxis)Model.Axes.First(a => a.Position == OxyPlot.Axes.AxisPosition.Left);
                    y_axis.Minimum = FullDataRectangle.Bottom;
                    y_axis.Maximum = FullDataRectangle.Top;
                }
                else
                {
                    LinearAxis y_axis = (LinearAxis)Model.Axes.First(a => a.Position == OxyPlot.Axes.AxisPosition.Left);
                    y_axis.Minimum = DataRectangle.Bottom;
                    y_axis.Maximum = DataRectangle.Top;
                }
            }
            catch
            {
                Debug.Print("PlotViewModelBase[{0}]::ScalePlotAxes failed", this.Model.Title);
            }
            Model.InvalidatePlot(true);
        }


        public virtual void Clear()
        {
            if (this.Model != null)
            {
                Model.Axes.Clear();
                Model.Series.Clear();
            }
        }
    }
}
