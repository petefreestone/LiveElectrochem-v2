using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Live_Electrochem
{
    public class AOChannel
    {
        public decimal[] TriangleWaveform { get; set; }
        public int SampleCount
        {
            get
            {
                return TriangleWaveform != null ? TriangleWaveform.Length : 0;
            }
        }
        public double WaveformDuration { get; set; }
        public double WaveformScanRate { get; set; }
        public double WaveformUpdateRate { get; set; }
        public double WaveformOffset { get; set; }
        public double WaveformMultiplier { get; set; }
        public int ChannelNumber { get; private set; }

        public AOChannel(int ChannelNumber)
        {
            this.ChannelNumber = ChannelNumber;
        }
    }
}
