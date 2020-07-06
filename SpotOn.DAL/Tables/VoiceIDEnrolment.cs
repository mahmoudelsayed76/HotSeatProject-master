using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using SQLite;

namespace SpotOn.DAL.Models
{

    [Table("VoiceIDEnrolment")]
    public class VoiceIDEnrolment
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public string OrgId { get; set; }

        [MaxLength(100)]
        public string UserFriendlyName{ get; set; }

        public string ProfileId { get; set; }

    }


}