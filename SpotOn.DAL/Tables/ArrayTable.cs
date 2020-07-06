using System;
using SQLite;

namespace SpotOn.DAL.Models
{
    /// <summary>
    /// A Generic Table to store table-formatted values from an array 
    /// </summary>
    [Table("ArrayTable")]
    public class ArrayTable
    {
        // -----------------------------------------------------------------------------------------------------------
        // NOTE: For detail on how this works, 
        // see https://docs.google.com/spreadsheets/d/1HXlVD8RC9rZko7r2kXzZYmPtL4Smgzp0nYtL5BInf5M/edit#gid=0
        // -----------------------------------------------------------------------------------------------------------
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public string OrgId { get; set; }

        public string Key { get; set; }
        public string Value { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }

    }
}