using System;

namespace SpotOn.DAL.Metrics
{
    public class PaceMetric : SingleMetric
    {
        public override string Description
        {
            get
            {
                return "Pace is measured in Words Per Minute (wpm). A good speaking pace that optimizes clarity is 110 wpm.";
            }
        }
        public override string Title
        {
            get { return "Pace"; }
        }
        /// <summary>
        /// a value of 1.0 means the pace was perfect
        /// </summary>
        private  double PaceFactor { get; set; }

        /// <summary>
        /// Depends on PaceFactor which has an optimal value of 1.0 
        /// </summary>
        public override double Rating
        {
            get
            {
                var pfVariance = Math.Abs(PaceFactor - 1.0);
                if (pfVariance > 1.0) pfVariance = 1.0;
                var retVal = (1.0 - pfVariance) * 100;

                // SPOT-83 : Demo Mode Calculations 
                if (_useDemoCalculations && retVal < 50)
                {
                    retVal = 50 + (new Random()).NextDouble()*30;
                }

                return retVal;
            }
        }
        private bool _useDemoCalculations;
        public PaceMetric(double durationMS, string actualText, bool useDemoCalculations)
        {
            _useDemoCalculations = useDemoCalculations;

            // count number of words 
            double numWords = actualText.Split(' ').Length;

            if (numWords <= 0)
            {
                Value = 0;
                Commentary = "Unable to determine pace as Actual Transcript was empty. ";
            }
            else // user's speech rate  = number of words / time (convert ms to minutes by multiplying result by 6000 )
            {
                Value = 1000 * 60 * numWords / (durationMS); // this is PaceWPM

                // analyse speed 
                double expectedDurationSec = numWords / AVG_USER_WPS;

                // speed factor. 
                //      < 1.0 : user speaking faster than expected 
                //      = 1.0 : it took exactly the amount of time expected
                //      > 1.0 : user speaking slower than expected 
                PaceFactor = durationMS / (expectedDurationSec * 1000);

                if (PaceFactor < 0.5)
                    Commentary = "You were talking too fast. Slow down a lot.";
                else if (PaceFactor >= 0.5 && PaceFactor < 0.9)
                    Commentary = "You were talking a little fast. Slow down a little.";
                else if (PaceFactor >= 0.9 && PaceFactor <= 1.1)
                    Commentary = "Your pace was great.";
                else if (PaceFactor >= 1.1 && PaceFactor < 1.5)
                    Commentary = "You were a little slow. Try speaking faster.";
                else if (PaceFactor >= 1.5)
                    Commentary = "Your pace was way too slow. You'll need to speak much faster.";
            }

        }

    }
}