using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using SQLite;

namespace SpotOn.DAL.Models
{
    public interface IModel 
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

    }

}