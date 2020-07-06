using System;

namespace SpotOn.DAL.Metrics
{
    public class ConfidenceMetric : SingleMetric
    {
        public const string TITLE = "Confidence";
        public const string DESCRIPTION = "Confidence is a measure of how well the speaker is  projecting self-belief and assurance " +
                "about what is being said. It is proxied by Pitch.";

        public override string Description { get { return DESCRIPTION; } }
        public override string Title { get { return TITLE; } }

        /// <summary>
        /// The Confidence Value is either 1 or 2 
        /// </summary>
        public override double Rating
        {
            get
            {
                return (Value / 5.0) * 100;
            }
        }

        public ConfidenceMetric(double pitchHz, double pitchMinHz, double pitchMaxHz, bool useDemoCalculations)
        {
            // see https://docs.google.com/spreadsheets/d/169LhmM94_uk6qBPsByeS8m09Qvgf65bAKz3QLotQ7Po/edit?ts=5e87d7f2#gid=1160429215
            // 1 (Diff > 90 Hz)
            // 2 (Diff 70 - 90 Hz)
            // 3 (Diff 50 - 70 Hz)
            // 4 (Diff  40 - 50 Hz)
            // 5 (Diff < 40 Hz)

            var diffHz = Math.Abs(pitchMaxHz - pitchMinHz);

            switch (diffHz)
            {
                case double d when (d < 40):
                    Value = 5;
                    Commentary = $"Your confidence shines through.";
                    break;
                case double d when (d >= 40 && d < 50):
                    Value = 4;
                    Commentary = $"You confidence (as measured by pitch variation) could be better.";
                    break;
                case double d when (d >= 50 && d < 70):
                    Value = 3;
                    Commentary = $"You confidence (as measured by pitch variation) is low.";
                    break;
                case double d when (d >= 70 && d < 90):
                    Value = 2;
                    Commentary = $"You confidence (as measured by pitch variation) is very low.";
                    break;
                default:
                case double d when (d >= 90):
                    Value = 1;
                    Commentary = $"You confidence (as measured by pitch variation) is nearly zero.";
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