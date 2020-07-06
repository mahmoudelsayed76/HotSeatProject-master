using System;
using System.IO;
using SQLite;
using Cointeco;

namespace SpotOn.DAL.Models
{

    [Table("Attempt")]
    public class Attempt
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public string OrgId { get; set; }

        /// <summary>
        /// date attempted 
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// foreign key to Assignment Id  
        /// </summary>
        public int AssignmentId { get; set; }

        /// <summary>
        /// foreign key to Assessment Id   (results of attempt)
        /// </summary>
        public int AssessmentId { get; set; }

        /// <summary>
        /// location of the audio/vidoe recordings of the attempt 
        /// </summary>
        public string AudioRecording { get; set; }
        public string VideoRecording { get; set; }

        /// <summary>
        /// number of milliseconds of the media
        /// </summary>
        public double MediaDurationMS { get; set; }

        /// <summary>
        /// what should have been said
        /// </summary>
        public string ExpectedTranscript { get; set; }

        /// <summary>
        /// What was said
        /// </summary>
        public string ActualTranscript { get; set; }

        /// <summary>
        /// Aggegrate Score
        /// </summary>
        public double FinalScore { get; set; }

        /// <summary>
        /// Comments by the person attempting it 
        /// </summary>
        public string Comments { get; set; }

        public string GetVideoCover()
        {
            string cover = null;
            if (File.Exists(VideoRecording))
            {
                var t = CommonBase.Extensions.GetVideoCoverImage(VideoRecording).ConfigureAwait(false);
                cover = t.GetAwaiter().GetResult();
            }
            return cover;
        }
    }
}