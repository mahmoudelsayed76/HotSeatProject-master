using Cointeco;
using System;
using System.Collections.Generic;
using SpotOn.DAL.Models;

namespace SpotOn.DAL
{

    public class ArrayTableORM : ORMBase<ArrayTable>
    {
        private const string COLUMN = "Column";
        private const string VALUE = "Value";
        private const string TABLE = "Table";

        public ArrayTableORM() : base("ArrayTable") { }

        /// <summary>
        /// addes or inserts the value into the table, returning the Id
        /// </summary>
        public int Upsert(int id, string orgId, string key, string value, int row = -1, int column = -1)
        {
            var arrTbl = new ArrayTable()
            {
                Id = id,
                Key = key,
                Value = value,
                Row = row,
                Column = column
            };

            if (Get(arrTbl.Id) is null)
                db.Insert(arrTbl);
            else
                db.Update(arrTbl);

            return arrTbl.Id;
        }

        /// <summary>
        /// deletes all entries for "table"
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns>Number Of Rows Deleted</returns>
        public int Delete(string tableName)
        {
            int rowsDeleted = 0;
            string query = $"Select * from {TableName} where (key = 'Table' AND Value ='{tableName}') OR (Key like '{tableName}.%')";
            var rowList = db.Query<ArrayTable>(query);
            foreach (var item in rowList)
            {
                Delete(item.Id);
                rowsDeleted++;
            }
            return rowsDeleted;
        }


        public List<string> GetTableNames(string startingWith = null)
        {
            string query = $"Select * from {TableName} where key = '{TABLE}' ";
            if (!string.IsNullOrEmpty(startingWith))
            {
                query += $" AND Value LIKE '{startingWith}%' ";
            }
            var tables = db.Query<ArrayTable>(query);
            var list = new List<string>(tables.Count);

            // copy table names found 
            foreach (var t in tables)
                list.Add(t.Value);

            return list;
        }

        /// <summary>
        /// read values for table  with name 'tableName' into an array 
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public string[,] Get(string tableName, string orgId)
        {
            string query = $"Select * from {TableName} where (key = '{TABLE}' AND Value ='{tableName}') ";
            var tableDefn = db.Query<ArrayTable>(query);
            if (tableDefn.Count == 0)
            {
                CommonBase.Logger.Warning("ArrayTableORM.Get() : Table {t} not found.", tableName);
                return null;
            }

            if (tableDefn.Count > 1)
            {
                CommonBase.Logger.Warning("ArrayTableORM.Get() : Table {t} is corrupted", tableName);
                return null;
            }

            // Get the columns 
            query = $"Select * from {TableName} where (Key = '{tableName}.{COLUMN}') ORDER BY COLUMN ASC";
            var columnDefn = db.Query<ArrayTable>(query);
            int numColumns = columnDefn.Count;

            // get the cell data
            query = $"Select * from {TableName} where (Key = '{tableName}.{VALUE}') ORDER BY ROW ASC, COLUMN ASC";
            var cells = db.Query<ArrayTable>(query);
            int numRows = cells.Count / numColumns;

            // we have numRows+1 (0th row has the column-headers)
            string[,] returnArray = new string[numRows + 1, numColumns];

            // copy headers 
            for (int c = 0; c < columnDefn.Count; c++)
                returnArray[0, c] = columnDefn[c].Value;

            // copy values (offset the row by 1 because of the header column) 
            foreach (var cell in cells)
                returnArray[cell.Row + 1, cell.Column] = cell.Value;

            return returnArray;
        }

        /// <summary>
        /// read values for table  with name 'tableName' into an array 
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public string[,] GetAll()
        {
            string query = $"Select * from {TableName} ORDER BY KEY ASC, ROW ASC, COLUMN ASC";
            var everything = db.Query<ArrayTable>(query);

            string[,] returnArray = new string[everything.Count, 4];

            int row = 0; 
            foreach (var cell in everything)
            {
                returnArray[row, 0] = cell.Key;
                returnArray[row, 1] = cell.Value;
                returnArray[row, 2] = cell.Row.ToString();
                returnArray[row, 3] = cell.Column.ToString();
                row++;
            }

            return returnArray;
        }
        /// <summary>
        /// replace the data for "tablename" with the attached 
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="tableArray"></param>
        public bool Upsert(string tableName, string[,] tableArray)
        {
            // blow existing table away (if any) 
            var numRows = Delete(tableName);
            if (numRows > 0)
                CommonBase.Logger.Information("ArrayTableORM.Upsert() : Deleted {n} rows for Table {t} ", numRows, tableName);

            // -------------------------
            // Insert Table Definition 
            // -------------------------
            Upsert(-1, Organization.PUBLIC_ORG_ID, "Table", tableName);

            // -------------------------
            // Insert Table headings 
            // -------------------------
            for (int c = 0; c < tableArray.GetLength(1); c++)
            {
                var columName = tableArray[0, c].Trim(); // row 0 must contain the column headings 
                if (!string.IsNullOrEmpty(columName))
                    Upsert(-1, Organization.PUBLIC_ORG_ID, $"{tableName}.{COLUMN}", columName, -1, c);
                else
                    throw new Exception($"Column {c} is not defined.");
            }
            // -------------------------
            // Insert Table Values
            // -------------------------
            for (int r = 1; r < tableArray.GetLength(0); r++)
            {
                for (int c = 0; c < tableArray.GetLength(1); c++)
                {
                    var cellValue = tableArray[r, c];
                    Upsert(-1, Organization.PUBLIC_ORG_ID, $"{tableName}.{VALUE}", cellValue, r-1, c);
                }
            }

            return true;
        }
    }
}