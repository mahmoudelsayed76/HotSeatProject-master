using System;

namespace SpotOn.DAL.Metrics
{
    public class ClarityMetric : SingleMetric
    {
        public const string TITLE = "Clarity";
        public const string DESCRIPTION = "Clarity is a measure of how clear and lucid the speech is. It is derived from Pace, Pitch and Accuracy.";

        public override string Description { get { return DESCRIPTION; } }
        public override string Title { get { return TITLE; } }

        /// <summary>
        /// Clarity is like a Likert (out of 5)
        /// </summary>
        public override double Rating
        {
            get
            {
                return (Value / 5.0) * 100;
            }
        }

        public ClarityMetric(double paceWPM, double accuracyPCT, bool useDemoCalculations)
        {
            // Attenuation Factor gets bigger as the pace gets further from the desired value (AVG_USER_WPM)
            double paceAttenuationFacor = Math.Abs(paceWPM - OPTIMAL_USER_WPM) / OPTIMAL_USER_WPM;

            // Attenuation Factor is limited to 0.75
            // THe larger it is, the greater the effect on Clarity 
            paceAttenuationFacor = paceAttenuationFacor > 0.75 ? 0.75 : paceAttenuationFacor;

            // A high Attenuation Factor reduces clarity.
            double clarityPCT = (1.0 - paceAttenuationFacor) * accuracyPCT;

            //  1 (Clarity < 50 %)
            //  2 (Clarity  50 - 69.99 %)
            //  3 (Clarity  70 - 79.99 %)
            //  4 (Clarity  80 - 89.99 %)
            //  5 (Clarity  90 - 100 %)

            switch (clarityPCT)
            {
                case double d when (d > 90):
                    Value = 5;
                    Commentary = "You did an outstanding job of speaking clearly.";
                    break;
                case double d when (d >= 80 && d < 90):
                    Value = 4;
                    Commentary = "You spoke clearly, at the right pace.";
                    break;
                case double d when (d >= 70 && d < 80):
                    Value = 3;
                    Commentary = "Your speech was not very clear - adjust your pace and focus on the script.";
                    break;
                case double d when (d >= 50 && d < 70):
                    Value = 2;
                    Commentary = "You have low clarity - speak at the right pace and keep to the script.";
                    break;
                default:
                case double d when (d < 50):
                    Value = 1;
                    Commentary = "You are unintelligble. Please check your pace, and FOLLOW THE SCRIPT.";
                    break;
            }

            // SPOT-83 : Demo Mode Calculations 
            // if (AppBase.UseDemoCalculations && Value < 3)
            if (useDemoCalculations && Value < 3)
            {
                Value = 3;
            }
        }

    }
}