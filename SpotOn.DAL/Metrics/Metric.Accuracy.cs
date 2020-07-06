using System;
using Cointeco;

namespace SpotOn.DAL.Metrics
{
    public class AccuracyMetric : SingleMetric
    {
        public override string Description
        {
            get
            {
                return "Accuracy is a measure of how close the user is to the script. It is measured by comparing the transcribed text with the script";
            }
        }
        public override string Title
        {
            get { return "Accuracy"; }
        }

        /// <summary>
        /// The accuracy is already out of 100, so it is the same as the rating 
        /// </summary>
        public override double Rating
        {
            get 
            {
                return Value; 
            }
        }
        /// <summary>
        ///  the difference between the actual and expected text 
        /// </summary>
        public string DiffHTML {get;set;}

        public AccuracyMetric(string actualText, string expectedText)
        {
            DiffHTML = CommonBase.AgroDiffToHtml(expectedText, actualText, out double acc);
            Value = acc;
            Commentary  = AccuracyFeedback(Value);
        }

        public static string AccuracyFeedback(double accuracy)
        {
            // analyse the accuracy 
            string accuracyFeedback = String.Format("Your accuracy was {0:0.0} %. ", accuracy);
            if (accuracy > 95)
                accuracyFeedback += "That was excellent! Well done! ";
            else
            if (accuracy > 85)
                accuracyFeedback += "That was great!";
            else
            if (accuracy > 70)
                accuracyFeedback += "That was OK - please review the words you missed.";
            else
            {
                if (accuracy > 25)
                    accuracyFeedback += "That was not great - please pay attention.";
                else
                    accuracyFeedback += "That was dismal - are you even trying?";
            }

            return accuracyFeedback;
        }
    }
}