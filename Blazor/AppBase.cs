using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.Messaging;
using SpotOn.Base;
using SpotOn.DAL;
using SpotOn.DAL.Models;
using System.IO;
using System.Linq;

namespace SpotOn.Web
{
    public class AppBase : SpotBase
    {
        public string StorageDirectory
        {
            get
            {
                return Path.Combine(Directory.GetCurrentDirectory(), "Storage");
            }
        }
        public string LogDirectory
        {
            get
            {
                return Path.Combine(StorageDirectory, "Logs");
            }
        }

        public string DatabaseDirectory
        {
            get
            {
                return Path.Combine(StorageDirectory, "Database");
            }
        }
        public AppBase()
        {
            // start file logging
            SpotBase.InitializeFileLogging(LogDirectory, SpotBase.ProgramName);
            Logger.Information("I live.");

            // initialize ORM
            ORMSettings.Initialize(DatabaseDirectory);
        }

        
    }

    public static class StringExtensions
    {

        public static string Append(this string s, string message)
        {
            var newString = s + "<br/>" + message;
            return newString;
        }
    }
}
