using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using SQLite;

namespace SpotOn.DAL.Models

{

    public enum LessonActivityResult
    {
        Added = 1,
        Updated,
        Deleted,
        Discarded
    }


    [Table("Lesson")]
    public class Lesson
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public string OrgId { get; set; }

        public DateTime CreatedDate { get; set; }

        [MaxLength(50)]
        public string Author { get; set; }

        [MaxLength(50)]
        public string Title { get; set; }

        [MaxLength(150)]
        public string Description { get; set; }

        public string Transcript { get; set; }

        public string LocalRecordingPath { get; set; }

        public string LocalVideoPath { get; set; }

        public string VideoCoverPath { get; set; }
        public string Course { get; set; }
        public string AlphaSort { get; set; }
        public string Notes { get; set; }
        public string Attachment { get; set; }


    }


}