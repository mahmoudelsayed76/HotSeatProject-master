using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using SQLite;
// using Java.Sql;

namespace SpotOn.DAL.Models
{

    public class LegendItem
    {
        public DateTime PostedDate { get; set; }

        public int Id { get; set; }
        public string VideoPath { get; set; }
        public int VideoCoverImgId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int VotesFor { get; set; }
        public int VotesAgainst { get; set; }
        public double Score { get; set; }
        public string Comments { get; set; }

    }
}