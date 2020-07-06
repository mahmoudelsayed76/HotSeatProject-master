using System;
using System.Collections.Generic;
using Cointeco;
using SpotOn.DAL.Models;

namespace SpotOn.Base
{
    /// <summary>
    ///  These are common to the web and mobile apps 
    /// </summary>
    public class SpotBase : CommonBase
    {
        #region Constants 
        public const long DEFAULT_SPEECH_RECOG_TIMEOUT_MS = 3000;

        #endregion

        #region Static Properties 
        #endregion

        public static User CurrentUser { get; set; }
        public static long SpeechRecognitionTimeoutMS { get; set; } = DEFAULT_SPEECH_RECOG_TIMEOUT_MS;



        /// <summary>
        ///  gets all the values of an enum 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<EnumData> ItemizeEnum<T>() where T: Enum
        {
            List<EnumData> returnList = new List<EnumData>();
            var enums = Enum.GetValues(typeof(T));
            foreach (var e in enums)
            {
                returnList.Add(new EnumData() { Value = (int)e, Name = Enum.GetName(typeof(T), e) }); 
            }
            return returnList; 
        }
    }

    public class EnumData
    {
        public int Value { get; set; }
        public string Name { get; set; }
    }
}
