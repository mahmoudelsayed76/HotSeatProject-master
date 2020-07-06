using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using SQLite;
using SpotOn.DAL.Models;
using Cointeco;

namespace SpotOn.DAL.Metrics
{

    public class PitchMetric : SingleMetric
    {
        public override string Description
        {
            get
            {
                return "Pitch is measured in Hertz (Hz). A typical adult male will have a fundamental frequency from 85 to 180 Hz, " +
                    "and a typical adult female from 165 to 255 Hz.";
            }
        }
        public override string Title
        {
            get { return "Pitch"; }
        }

        double _rating;

        public override double Rating
        {
            get
            {
                return _rating;
            }
        }
        /// <summary>
        /// The voiced speech of a typical adult male will have a fundamental frequency from 85 to 180 Hz, 
        /// and that of a typical adult female from 165 to 255 Hz.
        /// </summary>
        /// <param name="A"></param>
        public PitchMetric(Attempt A)
        {
            if (File.Exists(A.AudioRecording))
            {
                var f0Est = CommonBase.EstimateF0(A.AudioRecording);
                Value = f0Est.AverageHz;
                //ValueMin = f0Est.MinHz;
                //ValueMax = f0Est.MaxHz;
                
                // TODO: make min/max Pitch work ...
                ValueMin = 0.9 * Value;
                ValueMax = 1.1 * Value;

                double maleUpperRange = 180.0;
                double maleLowerRange = 85.0;
                double femaleUpperRange = 255.0;
                double femaleLowerRange = 165.0;
                double upperRange = maleUpperRange;
                double lowerRange = maleLowerRange;

                if (AssignmentORM.IsAssigneeFemale(A.AssignmentId))
                {
                    upperRange = femaleUpperRange;
                    lowerRange = femaleLowerRange;
                }

                if (Value < 0.6 * lowerRange)
                {
                    Commentary = "You were speaking with a very low pitch. Increase your pitch a lot.";
                    _rating = 50;
                }
                else if (Value >= (0.6 * lowerRange) && Value < lowerRange)
                {
                    Commentary = "You were speaking with an unusually low pitch. Increase your pitch a little.";
                    _rating = 75;

                }
                else if (Value >= lowerRange && Value < upperRange)
                {
                    Commentary = "Your voice is in the normal pitch range. ";
                    _rating = 100;

                }
                else if (Value >= upperRange && Value < (1.4 * upperRange))
                {
                    Commentary = "You were speaking with an unusually high-pitched voice. Lower your pitch a little.";
                    _rating = 75;
                }
                else
                {
                    Commentary = "You were screeching at a very high pitch. Lower your voice a lot.";
                    _rating = 50;
                }
            }
            else
            {
                Value = 0.0;
                ValueMin = 0.0;
                ValueMax = 0.0;
                Commentary = "Media Location is invalid. Unable to assess pitch";
                _rating = 0;
            }

            

        }
    }
}