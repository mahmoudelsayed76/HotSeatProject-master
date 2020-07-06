using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using SQLite;

namespace SpotOn.DAL.Models
{

    [Table("Organization")]
    public class Organization
    {
        public const string PUBLIC_ORG_ID = "ANON";

        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }

        public string Description { get; set; }

        public string Address { get; set; }

        public string URL { get; set; }

        public string Contact { get; set; }

    }

}