using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using SQLite;
using Cointeco;
using SpotOn.DAL.Models;

namespace SpotOn.DAL.Metrics
{

    public class VolumeMetric : SingleMetric
    {

        public override string Description
        {
            get
            {
                return "Sound volume is measured in decibles (db) but audio volume is measured in Levels. " +
                    "To convert levels to decibels, a physical device such as a speaker is used. ";
            }
        }
        public override string Title
        {
            get { return "Volume"; }
        }

        public override double Rating
        {
            get
            {
                var variance = Math.Abs(_volumeFactor - 1.0);
                if (variance > 1.0) variance = 1.0;
                var retVal =  (1.0 - variance) * 100;

                // SPOT-83 : Demo Mode Calculations 
                if (_useDemoCalculations && retVal < 50)
                {
                    retVal = 50 + (new Random()).NextDouble() * 30;
                }
                return retVal;

            }
        }
        double _volumeFactor;
        bool _useDemoCalculations;
        public VolumeMetric(Attempt A, bool useDemoCalculations)
        {

            _useDemoCalculations = useDemoCalculations;
            var wf = new WavFile(A.AudioRecording);

            Value = wf.GetVolumeIndB(0,  // 0 means auto-calculate block size
                true); // true means exclude silences 

            _volumeFactor = Value / AVG_VOLUME_dB;
            double volumeSpread = AVG_VOLUME_dB_SPREAD / AVG_VOLUME_dB;

            if (_volumeFactor < (AVG_VOLUME_dB - 3 * volumeSpread))
                Commentary = "You were talking far too softly. Speak much louder.";
            else if (_volumeFactor >= (AVG_VOLUME_dB - 3 * volumeSpread) && _volumeFactor <= (AVG_VOLUME_dB - 1 * volumeSpread))
                Commentary = "You were talking quite softly. Speak louder.";
            else if (_volumeFactor >= (AVG_VOLUME_dB - 1 * volumeSpread) && _volumeFactor <= (AVG_VOLUME_dB + 1 * volumeSpread))
                Commentary = "You spoke at a normal volume - not too loud or too soft.";
            else if (_volumeFactor >= (AVG_VOLUME_dB + 1 * volumeSpread) && _volumeFactor <= (AVG_VOLUME_dB + 3 * volumeSpread))
                Commentary = "You were talking a little loudly. Speak softer.";
            else if (_volumeFactor >= (AVG_VOLUME_dB + 3 * volumeSpread))
                Commentary = "You were talking far too loudly. Speak a lot softer.";

        }
    }
}