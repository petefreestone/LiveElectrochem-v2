using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Helper.Analysis;
using Helper.Analysis.Enums;

namespace Live_Electrochem
{
    public class AIChannel
    {
        public Int16[][] RawData { get; set; }
        public decimal[][] Data { get; set; }
        public Int16[] RawSubtractData { get; set; }
        public decimal[] SubtractData { get; set; }
        public decimal[] IntegratedData { get; set; }

        public double SamplingPeriod { get; set; }

        public double SamplingFrequency => 1 / SamplingPeriod;
        public int ChannelNumber { get; private set; }
        public bool IsActive { get; set; }
        public bool IsDisabled { get; set; } = true;
        public decimal nA_V { get; set; }
        public decimal ADGain { get; set; }
        public decimal GainMultiplier { get; set; }
        public System.Windows.Media.Brush Stroke { get; private set; }

        public AIChannel(int ChannelNumber)
        {
            this.ChannelNumber = ChannelNumber;
            if (ChannelNumber == 0)
                Stroke = System.Windows.Media.Brushes.Red;
            if (ChannelNumber == 1)
                Stroke = System.Windows.Media.Brushes.Blue;
        }


        public int ScanCount
        {
            get
            {
                return RawData != null ? RawData.Length : 0;
            }
        }
        public int SampleCount
        {
            get
            {
                return RawData != null ? RawData[0].Length : 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal[] GetScaledData(
            int Index,
            bool IsFilterEnabled,
            double FilterCutOffFrequency,
            int BackgroundSubtractionSweepCount = 0)
        {
            //Debug.Print("LVBinFile::GetScaledData: Index {0}", Index);
            //Stopwatch sw = Stopwatch.StartNew();

            if (Data == null)
                throw new ArgumentNullException();
            if (Index < 0)
                throw new IndexOutOfRangeException();
            if (Data[Index] == null)
            {
                Data[Index] = new decimal[RawData[Index].Length];

                //Parallel.For(0, RawData[Index].Length,
                //    i =>
                //    {
                //        Data[Index][i] = (RawData[Index][i] * GainMultiplier);
                //    });

                for (int i = 0; i < RawData[Index].Length; i++)
                {
                    Data[Index][i] = RawData[Index][i] * GainMultiplier;      //GainMultiplier
                }

                if (IsFilterEnabled)
                    Data[Index] = DataAnalysisHelper.Butterworth(Data[Index], SamplingPeriod, FilterCutOffFrequency);
            }

            if (BackgroundSubtractionSweepCount > 0)
            {
                if (SubtractData == null)
                {
                    //generate the Subtract Data first
                    SubtractData = new decimal[RawData[Index].Length];
                    RawSubtractData = Array.ConvertAll(DataAnalysisHelper.CrossFunctionArrays(RawData.Take(BackgroundSubtractionSweepCount).Select(a => Array.ConvertAll<short, double>(a, f => f)), CrossFunctionModeEnum.Average), s => (short)s);      //PF 6/10/2021     surely all this casting is unneccesary
                    //Parallel.For(0, RawData[Index].Length,
                    //i =>
                    //{
                    //    SubtractData[i] = (RawSubtractData[i] * GainMultiplier);
                    //});

                    for (int i = 0; i < RawData[Index].Length; i++)
                    {
                        SubtractData[i] = RawSubtractData[i] * GainMultiplier;        //GainMultiplier
                    }
                    if (IsFilterEnabled)
                        SubtractData = DataAnalysisHelper.Butterworth(SubtractData, SamplingPeriod, FilterCutOffFrequency);
                }
                //sw.Stop();
                //times.Add(sw.ElapsedMilliseconds);
                //averagetime = times.Average();
                //Debug.Print(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>  SUBTRACTED {0} ms      {1} ms AVERAGE", times.Last(), averagetime);

                return Data[Index].SubtractArray(SubtractData);         //this could be optimized further
            }
            else
            {
                //sw.Stop();
                //times.Add(sw.ElapsedMilliseconds);
                //averagetime = times.Average();
                //Debug.Print(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> REAL {0} ms      {1} ms AVERAGE", times.Last(), averagetime);
                return Data[Index];
            }
        }
    }
}
