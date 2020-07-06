using System;

namespace SpotOn.DAL.Metrics
{
    public class InflectionMetric : SingleMetric
    {
        const double OPTIMAL_INFLECTION_SD = 1.5;
        public override string Description
        {
            get
            {
                return "Inflection is a measure of the variance of tone when speaking. It is measured in standard deviations (sd)";
            }
        }
        public override string Title
        {
            get { return "Inflection"; }
        }

        public override double Rating
        {
            get
            {
                // 1.5 is the optimal. 
                // if Value = 0.0, then Rating = (1.0 - CEIL(ABS(-1.5),1.0)) x 100 = 0
                // if Value = 1.0, then Rating = (1.0 - CEIL(ABS(-0.5),1.0)) x 100 = 50
                // if Value = 2.0, then Rating = (1.0 - CEIL(ABS(0.5),1.0)) x 100 = 50
                // if Value = 3.0, then Rating = (1.0 - CEIL(ABS(1.5),1.0)) x 100 = 0

                var inflectionVariance = Math.Abs(Value - OPTIMAL_INFLECTION_SD);
                if (inflectionVariance > 1.0) inflectionVariance = 1.0;
                return (1.0 - inflectionVariance) * 100; 
            }
        }

        public InflectionMetric(string mediaLocation)
        {
            // TODO:  anaylse media (A.MediaLocation)
            Value = 1.234; 

            // out of 6 or so 
            if (Value < 1)
                Commentary = "Your are droning monotonously. Please vary your pitch more.";
            else if (Value >= 1 && Value < 2)
                Commentary = "You have a confident yet energetic inflection when speaking.";
            else if (Value >= 2 && Value <= 3)
                Commentary = "You're inflecting somewhat dramatically - calm down.";
            else if (Value >= 3 )
                Commentary = "Your inflections are wildly artificial - please speak normally.";
        }

    }
}