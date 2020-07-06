using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using SQLite;

namespace SpotOn.DAL.Metrics
{

    public abstract class SingleMetric
    {
        public const double OPTIMAL_USER_WPM = 110.0;
        public const double WPM_SPREAD = 10.0;
        public const double AVG_USER_WPS = (OPTIMAL_USER_WPM / 60.0);
        public const double AVG_VOLUME_dB = 58;
        public const double AVG_VOLUME_dB_SPREAD = 5; // this gives a range of 53-63 as the average volume of speech
        public const double MAX_PITCH_SPREAD_HZ = 50;  // pitch spread in Hz of "Confident" speech (a made up metric)

        public abstract string Description { get; }
        public abstract string Title { get; }
        public abstract double Rating { get; }
        public double Value { get; set; }

        private const double MAGIC_VALUE = -99999.99999;
        double _vMax = MAGIC_VALUE;
        double _vMin = MAGIC_VALUE;
        public virtual double ValueMax
        {
            get
            {
                if (_vMax == MAGIC_VALUE) return Value;
                else return _vMax;
            }
            set
            {
                _vMax = value;
            }
        }
        public virtual double ValueMin
        {
            get
            {
                if (_vMin == MAGIC_VALUE) return Value;
                else return _vMin;
            }
            set
            {
                _vMin = value;
            }
        }

        public string Commentary { get; set; }
        public SingleMetric() { }
         
    }

}