using System.Collections.Generic;

namespace SpotOn.DAL.Models
{
    public class CourseProgress
    {
        public string Name { get; set; }
        public int CopmletedPct { get; set; }
        public int InProgressPct { get; set; }
        public bool HeaderRow { get; set; }

        static List<CourseProgress> DemoItems;
        public static List<CourseProgress> GetDemoItems()
        {
            if (DemoItems == null)
            {
                DemoItems = new List<CourseProgress>(6);
                DemoItems.Add(new CourseProgress()
                {
                    HeaderRow = true
                });

                int id = 1;
                DemoItems.Add(new CourseProgress()
                {
                    Name = $"C.{id++} Intro",
                    CopmletedPct = 65,
                    InProgressPct = 30
                });

                DemoItems.Add(new CourseProgress()
                {
                    Name = $"C.{id++} Wrong Contact",
                    CopmletedPct = 88,
                    InProgressPct = 7
                });

                DemoItems.Add(new CourseProgress()
                {
                    Name = $"C.{id++} Quick Pitch",
                    CopmletedPct = 45,
                    InProgressPct = 43
                });
                DemoItems.Add(new CourseProgress()
                {
                    Name = $"C.{id++} Qualify",
                    CopmletedPct = 45,
                    InProgressPct = 30
                });
                DemoItems.Add(new CourseProgress()
                {
                    Name = $"C.{id++} Send Info",
                    CopmletedPct = 55,
                    InProgressPct = 20
                });
                DemoItems.Add(new CourseProgress()
                {
                    Name = $"C.{id++} Timing",
                    CopmletedPct = 72,
                    InProgressPct = 28
                });
                DemoItems.Add(new CourseProgress()
                {
                    Name = $"C.{id++} Expense",
                    CopmletedPct = 30,
                    InProgressPct = 70
                });
                DemoItems.Add(new CourseProgress()
                {
                    Name = $"C.{id++} Benefits",
                    CopmletedPct = 80,
                    InProgressPct = 15
                });
                DemoItems.Add(new CourseProgress()
                {
                    Name = $"C.{id++} Close",
                    CopmletedPct = 0,
                    InProgressPct = 50
                });
            }
            return DemoItems;
        }
    }
}