using SQLite;
using System;

namespace SpotOn.DAL.Models
{

    public enum TranscriptType
    {
        Header = 1,
        Detail = 2
    }
    public enum TranscriptModule
    {
        Roleplay = 1,
        Sidekick = 2
    }
    /// <summary>
    /// This table is not normalized - it contains different types of rows as follows : 
    ///     Header Row - one per ConversationId. In this row, 
    ///                 Sequence = 0  
    ///                 SpeakerId = OwnerId, 
    ///                 Text = A description of the conversation
    ///                 DateTime = last modified date of the conversation
    ///     Detail Row - N per ConversationId. In this row, 
    ///                 Sequence = 1..N  
    ///                 SpeakerId = SpeakerId, 
    ///                 Text = what the speaker said
    ///                 DateTime = time of this snippet of conversation
    /// </summary>
    [Table("Transcript")]
    public class Transcript
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public string OrgId { get; set; }

        /// <summary>
        /// All entries for a single conversation have the same [ConversationId]
        /// </summary>
        public string ConversationId { get; set; }

        /// <summary>
        /// Entries within a single conversation have consecutive sequenceNumbers
        /// </summary>
        public int Sequence { get; set; }

        /// <summary>
        ///  ID of the speaker (in a conversation, there may be more than one speaker)
        /// </summary>
        public int SpeakerId { get; set; }

        /// <summary>
        /// actual text spoken 
        /// </summary>
        public string Text { get; set; }

        public string AudioRecordingPath { get; set; }

        public TranscriptType Type { get; set; }

        public TranscriptModule Module { get; set; }
        public DateTime DateTime { get; set; }

        /// <summary>
        ///  returns a new Transcript object based on the current one
        /// </summary>
        /// <returns></returns>
        public Transcript Next()
        {
            return new Transcript()
            {
                Id = -1,
                SpeakerId = this.SpeakerId,
                Sequence = this.Sequence + 1, // important 
                ConversationId = this.ConversationId,// important  
                Module = this.Module, 
                Text = "",
                Type = this.Type,
                DateTime = default,
                AudioRecordingPath = null
            };
        }

        public static Transcript New(
                int speakerId,
                string orgId,
                string text,
                int sequence = 0,
                TranscriptType type = TranscriptType.Detail,
                TranscriptModule module = TranscriptModule.Roleplay,
                DateTime dateTime = default,
                string conversationId = null,
                string recordingPath = null
            )
        {
            // give it a new GUID
            conversationId ??= Guid.NewGuid().ToString();
            return new Transcript()
            {
                Id = -1,
                OrgId = orgId,
                SpeakerId = speakerId,
                Text = text,
                Sequence = sequence,
                Module = module,
                Type = type,
                DateTime = dateTime == default ? DateTime.UtcNow : dateTime,
                ConversationId = conversationId,
                AudioRecordingPath = recordingPath
            };
        }

    }
}