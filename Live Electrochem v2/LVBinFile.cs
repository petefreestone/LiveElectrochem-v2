using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Helper;
using Helper.Analysis;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Live_Electrochem
{
    public enum EventModeEnum
    {
        EventTrigger = 0,
        Stimulation_Internal = 1,
        Stimulation_External = 2,
        NoEvent = 3
    }

    public enum StimulationPolarityEnum
    {
        Biphasic = 0,
        Monophasic_Positive = 1,
        Monophasic_Negative = 2,
        Sine = 3
    }

    public class LVBinFile
    {
        public LVBinFile()
        {
            AOChannels = new ObservableCollection<AOChannel>();
            AIChannels = new ObservableCollection<AIChannel>();
            //TriangleWaveforms = new List<decimal[]>();
            //WaveformDurations = new List<double>();
            //WaveformMultipliers = new List<double>();
            //WaveformOffsets = new List<double>();
            //WaveformScanRates = new List<double>();
            //WaveformUpdateRates = new List<double>();
        }

        private int BytesPerSample = 2;

        [Category("Information")]
        public double FileVersion { get; set; }
        [Category("Information")]
        public long ExpectedFileSize
        {
            get
            {
                if (AOChannels == null)
                    return 0;
                return (((AOChannels.First().SampleCount * BytesPerSample) * CollectionNumberChannels) * TotalScanCount) + 1000000;
            }
        }
        [Category("Information")]
        public long ActualFileSize { get; set; }
        private string Filename_;

        //public int ChannelCount;
        [Category("Data")]
        public ObservableCollection<AOChannel> AOChannels { get; set; }
        [Category("Data")]
        public ObservableCollection<AIChannel> AIChannels { get; set; }

        //Section: Waveform
        [Category("Waveform")]
        public double WaveformFrequency { get; set; }           //reader from file

        //Section: Stimulation
        [Category("Stimulation")]
        public int StimulationFrequency { get; set; }
        [Category("Stimulation")]
        public int StimulationPulses { get; set; }
        [Category("Stimulation")]
        public double StimulationPulseWidth { get; set; }
        [Category("Stimulation")]
        public double StimulationAmplitude { get; set; }
        [Category("Stimulation")]
        public StimulationPolarityEnum StimulationPolarity { get; set; }
        [Category("Stimulation")]
        public double StimulusToScanDelay { get; set; }
        [Category("Stimulation")]
        public double StimulationUpdateRate { get; set; }
        [Category("Stimulation")]
        public double StimulationMultiplier { get; set; }

        //Section: Collection
        [Category("Collection")]
        public int CollectionStimOrTTL { get; set; }
        [Category("Collection")]
        public int CollectionPreEventScanCount { get; set; }
        [Category("Collection")]
        public int CollectionPostEventScanCount { get; set; }
        [Category("Collection")]
        public int CollectionNumberChannels { get; set; }
        [Category("Collection")]
        public int CollectionActiveChannel { get; set; }
        [Category("Collection")]
        public EventModeEnum CollectionEventMode { get; set; }
        [Category("Collection")]
        public decimal[] nA_Vs { get; set; }        //the result of the lookup
        [Category("Collection")]
        public decimal[] ADGains { get; set; }
        [Category("Collection")]
        public decimal[] GainMultipliers { get; set; }

        /// <summary>
        /// Returns the nA_V value using a lookup table (based on analysis of files - see excel file)
        /// </summary>
        /// <param name="Value">Read from BLOCK #2 starting immediately after Int32 value 2115</param>
        /// <returns></returns>
        public static decimal Get_nA_V(UInt16 Value)
        {
            //Debug.Print("Get_nA_V: Value = {0}", Value);

            if (Value == 64000)
                return 500M;
            if (Value == 18432)
                return 200M;
            if (Value == 51200)
                return 100M;
            if (Value == 0)
                return 0;

            throw new ArgumentException(string.Format("Value '{0}' (unit16) for Get_nA_V is invalid", Value));
        }

        /// <summary>
        /// Returns the AD_Gain value (not Gain Multiplier) using a calculation (based on analysis of files - see excel file)
        /// </summary>
        /// <param name="Value">Read from BLOCK #1</param>
        /// <param name="nA_V">Obtained using Get_nA_V(unit16 value from BLOCK #2)</param>
        /// <returns></returns>
        public static decimal Get_AD_Gain(UInt16 Value, decimal nA_V)
        {
            if (nA_V == 0)
                return 1;
            decimal result1 = (decimal)Math.Exp(((Value - 13320.2190310034) / 5937.0862541531));
            decimal result2 = nA_V / result1;
            //Debug.Print("Get_AD_Gain: Value = {0}, nA_V = {1}, AD_Gain = {2}", Value, nA_V, result2);

            return Math.Round(result2, 0);
        }

        public static EventModeEnum GetEventMode(int Value)
        {
            if (Value == 768)
                return EventModeEnum.NoEvent;
            if (Value == 512)
                return EventModeEnum.Stimulation_External;
            if (Value == 256)
                return EventModeEnum.Stimulation_Internal;
            return EventModeEnum.EventTrigger;
        }

        public int TotalScanCount
        {
            get
            {
                return CollectionPreEventScanCount + CollectionPostEventScanCount;
            }
        }

        public TimeSpan Duration
        {
            get
            {
                return TimeSpan.FromSeconds(TotalScanCount * (1 / WaveformFrequency));
            }
        }

        public string FullFilename
        {
            get { return Filename_; }
            set { Filename_ = value; }
        }

        public string Filename
        {
            get
            {
                return System.IO.Path.GetFileName(FullFilename);
            }
        }

        

        //private List<long> times = new List<long>(2000);
        //public double averagetime;

        public string InfoString
        {
            get
            {
                int i = 0;
                StringBuilder sb = new();
                foreach (AOChannel c in this.AOChannels)
                {
                    sb.AppendLine(string.Format("Channel #{5}: {0} sweeps, Waveform {1} > {2} V, {3} samples per sweep, {4} ms sweep duration",
                        this.AIChannels[0].RawData == null ? "null" : this.AIChannels[0].RawData.Length.ToString(),
                        c.TriangleWaveform == null ? "null" : c.TriangleWaveform.Min().ToString("0.000"),
                        c.TriangleWaveform == null ? "null" : c.TriangleWaveform.Max().ToString("0.000"),
                        c.TriangleWaveform == null ? "null" : c.TriangleWaveform.Length.ToString(),
                        c.WaveformDuration.ToString("0.00"), i));
                    i++;
                }
                return sb.ToString();
            }
        }

        public bool IsFilterEnabled;
        //public double SamplingPeriod;
        public double FilterCutOffFrequency = 1660;        //Hz from Knowmad Analysis application

        public static void DebugDump()
        {
            DebugDump();
            //DebugWriter w = new DebugWriter();
            //Dumper.Dump(this, "LVBinFile", w);
            //w.Close();
        }
    }
}
