using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using SQLite;

namespace SpotOn.DAL.Models
{

    public class AutoIncrement 
    {
        public string id { get; set; }

        /// <summary>
        /// This is the highest id in the system. 
        /// </summary>
        public int MaxId { get; set; }

    }

}