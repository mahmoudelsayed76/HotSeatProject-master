using Cointeco;
using System;

namespace SpotOn.DAL.Metrics
{
    public class CredibilityMetric : SingleMetric
    {
        public const string TITLE = "Credibility";
        public const string DESCRIPTION = "Credibility is a measure of how truthful and trustworthy the speaker is perceived to be. " +
                    "It is proxied by Pace.";

        public override string Description { get { return DESCRIPTION; } }
        public override string Title { get { return TITLE; } }

        /// <summary>
        /// The Confidence Value is % , so it is the same as the Rating 
        /// </summary>
        public override double Rating
        {
            get
            {
                // Values are either 1,2,3 
                return (Value / 5.0) * 100;
            }
        }


        /// <summary>
        /// Calculate credibility as a function of pace 
        /// </summary>
        /// <param name="paceWPM"></param>
        public CredibilityMetric(double paceWPM, bool useDemoCalculations)
        {
            // see https://docs.google.com/spreadsheets/d/169LhmM94_uk6qBPsByeS8m09Qvgf65bAKz3QLotQ7Po/edit?ts=5e87d7f2#gid=1160429215
            // Implemented as the distance of the actual WPM from the optimal WPM 
            //   1 (Distance > 50   Wpm)
            //   2 (Distance  40-  50  Wpm)
            //   3 (Distance  30 - 40  Wpm)
            //   4 (Distance  20 - 30  Wpm)
            //   5 (Distance < 20 Wpm)"
            var diffWPM = Math.Abs(paceWPM - OPTIMAL_USER_WPM);
            string adjective = paceWPM - OPTIMAL_USER_WPM > 0 ? "fast" : "slow";
            switch (diffWPM)
            {
                case double d when (d < 20):
                    Value = 5;
                    Commentary = $"Your pace is optimal and this enhances your credibility.";
                    break;
                case double d when (d >= 20 && d < 30):
                    Value = 4;
                    Commentary = $"Your pace is a little {adjective} and this reduces your credibility a bit.";
                    break;
                case double d when (d >= 30 && d < 40):
                    Value = 3;
                    Commentary = $"Your pace is {adjective} and this reduces your credibility.";
                    break;
                case double d when (d >= 40 && d < 50):
                    Value = 2;
                    Commentary = $"Your pace is a bit too {adjective} and this reduces your credibility a lot.";
                    break;
                default: 
                case double d when (d >= 50):
                    Value = 1;
                    Commentary = $"Your pace is much too {adjective} and this really hurts credibility.";
                    break;
            }

            // SPOT-83 : Demo Mode Calculations 
            if (useDemoCalculations && Value < 3)
            {
                Value = 3;
            }

        }

    }
}