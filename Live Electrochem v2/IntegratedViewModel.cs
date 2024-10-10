using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Axes;
using System.Diagnostics;

namespace Live_Electrochem
{
    public class IntegratedViewModel : PlotViewModelBase
    {
        //public IList<DataPoint> Points1 { get; set; }
        //public IList<DataPoint> Points2 { get; set; }

        public IntegratedViewModel() : base()
        {

        }

        //public void SetData(List<List<DataPoint>> Data)
        //{
        //    //if (Data[0] != null)
        //    //    this.Points1 = Data[0];
        //    //if (Data.Count > 1)
        //    //    this.Points2 = Data[1];
        //    if (Data.Count > 2)
        //        throw new NotImplementedException();

        //    double y_min = double.MaxValue;
        //    double y_max = double.MinValue;
        //    double x_min = double.MaxValue;
        //    double x_max = double.MinValue;


        //    foreach (List<DataPoint> datapoints in Data)
        //    {
        //        y_min = Math.Min(Data.Min(a => a.Min(b => b.Y)), y_min);
        //        y_max = Math.Max(Data.Max(a => a.Max(b => b.Y)), y_max);
        //        x_min = Math.Min(Data.Min(a => a.Min(b => b.X)), x_min);
        //        x_max = Math.Max(Data.Max(a => a.Max(b => b.X)), x_max);

        //    }

        //    FullDataRectangle = new Helper.Common.DoubleRectangle()
        //    {
        //        Left = x_min,
        //        Right = x_max,
        //        Bottom = y_min,
        //        Top = y_max
        //    };
        //}

        public override void Clear()
        {
            base.Clear();
        }

        public override void ScalePlotAxes(bool Reset = false)
        {
            base.ScalePlotAxes(Reset);
        }

        //this.MyModel = new PlotModel { Title = "Example 1" };
        //    this.MyModel.Series.Add(new FunctionSeries(Math.Cos, 0, 10, 0.1, "cos(x)"));
    }
}
