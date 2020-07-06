using System;

namespace SpotOn.DAL.Metrics
{
    public class FluencyMetric : SingleMetric
    {
        public override string Description
        {
            get
            {
                return "Fluency is a measure of smooth content delivery through mastery of the language. This metric is proxied by Speed X Accuracy.";
            }
        }
        public override string Title
        {
            get { return "Fluency"; }
        }

        /// <summary>
        /// a linear scale that maxes out at 200 if Fluency.Value > 200
        /// </summary>
        public override double Rating
        {
            get
            {
                // SPOT-83 : Demo Mode Calculations 
                if (_useDemoCalculations && Value < 60)
                {
                    Value = 60;
                }
                return Value > 200 ? 100 : Value / 2.0;
            }
        }
        
        bool _useDemoCalculations;

        public FluencyMetric(double paceWPM, double accuracyPCT, bool useDemoCalculations)
        {
            _useDemoCalculations = useDemoCalculations;
            Value = accuracyPCT  * (paceWPM / 100);

            if (Value < 25)
                Commentary = "Very Low Fluency.";
            else if (Value >= 25 && Value < 50)
                Commentary = "Low Fluency.";
            else if (Value >= 50 && Value <= 100)
                Commentary = "Moderately Fluent.";
            else if (Value >= 100 && Value < 200)
                Commentary = "Very Fluent Speech.";
            else if (Value >= 200)
                Commentary = "Fluency Rating off the charts.";
        }

    }
}