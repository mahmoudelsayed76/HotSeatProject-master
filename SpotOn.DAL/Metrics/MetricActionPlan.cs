using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SQLite;
using Cointeco;

namespace SpotOn.DAL.Metrics
{
    public class MetricActionPlan
    {
        private static Dictionary<string, string> _actionPlans;
        public static void FromArray(string[,] array)
        {
            // take 1 less than the total because the first item is the header
            _actionPlans = new Dictionary<string, string>(array.GetLength(0) - 1);
            for (int r = 1; r < array.GetLength(0); r++)
            {
                string metric = array[r, 0];
                int score = -1;
                string plan = array[r, 2];
                if (int.TryParse(array[r, 1], out score) && !string.IsNullOrEmpty(metric) && !string.IsNullOrEmpty(plan) && score >= 0)
                    _actionPlans.Add(MetricKey(metric, score), plan);
            }
        }

        private static string MetricKey(string metric, int score)
        {
            return $"{metric}:{score}";
        }

        public static string GetActionPlan(string metric, int score)
        {
            // if (_actionPlans == null) Load();
            if (_actionPlans == null) throw new Exception("GetActionPlan() : Initialize Action Plans using FromArray()");
            string key = MetricKey(metric, score);
            if (true == _actionPlans?.ContainsKey(key))
                return _actionPlans[key];
            else
                return $"No Action Plan Defined For {metric}={score}";
        }

        public static string[,] LoadFromSheets()
        {
            return CommonBase.GoogleSheetsInstance.ReadEntries(
                CommonBase.AppSetting["GoogleSheets.SheetId"],
                CommonBase.AppSetting["GoogleSheets.MetricActionPlanSheetName"], 
                "A:C");
        }
    }

}