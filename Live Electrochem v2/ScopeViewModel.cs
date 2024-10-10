using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Diagnostics;

namespace Live_Electrochem
{
    public class ScopeViewModel : PlotViewModelBase
    {
        public ScopeViewModel() : base()
        {

        }

        //public void SetData(List<List<DataPoint>> Data, IList<DataPoint> WaveformData)
        //{
        //    //if (Data.Count > 2)
        //    //    throw new NotImplementedException();

        //    //double y_min = double.MaxValue;
        //    //double y_max = double.MinValue;

        //    //foreach (List<DataPoint> datapoints in Data)
        //    //{
        //    //    y_min = Math.Min(Data.Min(a => a.Min(b => b.Y)), y_min);
        //    //    y_max = Math.Max(Data.Max(a => a.Max(b => b.Y)), y_max);
        //    //}

        //    //FullDataRectangle = new Helper.Common.DoubleRectangle()
        //    //{
        //    //    Left = WaveformPoints.Select(a => a.Y).Min(),
        //    //    Right = WaveformPoints.Select(a => a.Y).Max(),
        //    //    Bottom = y_min,
        //    //    Top = y_max
        //    //};
        //}

        public static double YMajorStep
        {
            get
            {
                //if (Points1 == null)
                //    return 10;
                //double range = AutoScaleY ? Points1.Max(a => a.Y) - Points1.Min(b => b.Y) : DataRectangle.Top - DataRectangle.Bottom;       //only uses the first data set
                //if (range > 1000)
                //    return 250;
                //if (range > 100)
                //    return 25;
                //if (range > 10)
                //    return 5;
                //if (range > 1)
                //    return 0.5;
                //if (range > 0.1)
                //    return 0.05;
                return 1;
            }
        }

        public override void Clear()
        {
            base.Clear();
        }

        public override void ScalePlotAxes(bool Reset = false)
        {
            base.ScalePlotAxes(Reset);
        }
            
    }
}
