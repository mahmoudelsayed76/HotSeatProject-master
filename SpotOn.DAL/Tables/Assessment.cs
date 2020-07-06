using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using SQLite;

namespace SpotOn.DAL.Models
{

    [Table("Assessment")]
    public class Assessment
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }

        public string OrgId { get; set; }

        public DateTime Date { get; set; }

        /// <summary>
        /// Use Attempt.User.Name, if available 
        /// </summary>
        [MaxLength(50)]
        public string Speaker { get; set; }

        /// <summary>
        /// Use Attempt.ActualTranscript, if available 
        /// </summary>
        public string Transcript { get; set;  }

        // Pitch and Volume level are derived from the audio recording at RecordingPath
        public double PitchAvgHz { get; set; }
        public double PitchMinHz { get; set; }
        public double PitchMaxHz { get; set; }
        public string PitchComment { get; set; }

        // Pitch and Volume level are derived from the audio recording at RecordingPath
        public double VolumeLevel { get; set; }
        public string VolumeComment { get; set; }

        // Pace is derived from DurationMs and ActualTranscript
        public double PaceWPM { get; set; }
        public string PaceComment { get; set; }

        // Accuracy is derived from ActualTranscript and ExpectedTranscript
        public double AccuracyPct { get; set; }
        public string AccuracyComment { get; set; }

        // Clarity is derived from  Pace and Accuracy 
        public double ClarityLikert { get; set; }
        public string ClarityComment { get; set; }

        // Confidence is derived from Pitch
        public double ConfidenceLikert { get; set; }
        public string ConfidenceComment { get; set; }

        // Credibility is derived from Pace
        public double CredibilityLikert { get; set; }
        public string CredibilityComment { get; set; }

        public double Score { get; set; }

        //// Fluency is derived from  Pace and Accuracy (it has no unit)
        //public double Fluency { get; set; }
        //public string FluencyComment { get; set; }

        //// Inflection is derived from Pitch (measured in standard deviations)
        //public double InflectionSD { get; set; }
        //public string InflectionComment { get; set; }

        /// <summary>
        /// Tone, Vocal Quality, etc 
        /// </summary>
        public string SubjectiveMetric { get; set; }

        /// <summary>
        /// Differences from original in HTML
        /// </summary>
        public string DiffHTML { get; set; }

    }


}