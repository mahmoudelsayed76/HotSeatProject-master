using System;
using Cointeco;
using System.IO;

using SQLite;
using SpotOn.DAL.Models;
using SpotOn.DAL.Metrics;

namespace SpotOn.DAL
{

    public class AssessmentORM : ORMBase<Assessment>
    {

        public AssessmentORM() : base("Assessment") { }


        /// <summary>
        /// Save a new entry 
        /// </summary>
        /// <param name="speaker">Name of the speaker </param>
        /// <param name="pace">pace in words per minute</param>
        /// <param name="accuracy">accuracy (clarity) in %</param>
        /// <param name="date">date of assessment</param>
        /// <param name="diffHtml">textual diff (optional)</param>
        /// <param name="transcript">transcript (optional)</param>
        /// <param name="subjMetrics">subject metrics (tbd)</param>
        /// <param name="saveToCloud">whether to save to cloud (optional)</param>
        /// <returns>
        /// Id of newly inserted item, or -1 if it failed 
        /// </returns>
        //public int Save(string speaker, double pace, double accuracy,
        //    DateTime date, string diffHtml = null, string transcript = null,
        //    string subjMetrics = null)
        //{
        //    return Upsert(new Assessment()
        //    {
        //        AccuracyPct = accuracy,
        //        Date = date,
        //        DiffHTML = diffHtml,
        //        PaceWPM = pace,
        //        Speaker = AppBase.TruncateTo(speaker, 50),
        //        SubjectiveMetric = AppBase.TruncateTo(subjMetrics, 1000),
        //    });
        //}

        /// <summary>
        /// returns id of item 
        /// </summary>
        /// <param name="assessment"></param>
        /// <returns></returns>
        public int Upsert(Assessment assessment)
        {
            int retVal = -1;
            CommonBase.Logger.Information($"Assessment.Save: Saving entry from speaker {assessment.Speaker} on {assessment.Date}");

            try
            {
                if (Get(assessment.Id) is null)
                    db.Insert(assessment);
                else
                    db.Update(assessment);
                retVal = assessment.Id;
            }
            catch (Exception e)
            {
                CommonBase.Logger.Error($"Assessment.Save Exception: {e.Message}");
            }
            return retVal;
        }

        public string[,] GetAsArray(int id)
        {
            try
            {
                var A = this.Get(id);
                return new string[,] {
                {
                    A.Date.ToString(),
                    A.Speaker,
                    A.PaceWPM.ToString("0.00"),
                    A.AccuracyPct.ToString("0.00"),
                    A.DiffHTML??"",
                    A.Transcript??"",
                    A.SubjectiveMetric??""
                }};
            }
            catch (Exception e)
            {
                CommonBase.Logger.Error($"Assessment.GetAsArray() Exception: {e.Message}");
            }
            return null;
        }

        /// <summary>
        /// Assess performance given an attempt .
        /// NOTE: Updates A.OverallScore
        /// </summary>
        /// <param name="A"></param>
        /// <param name="currentUser">pass in AppBase.CurrentUser</param>
        public static Assessment CalculateFrom(Attempt A, User currentUser, bool useDemoCalculations)
        {
            var asgmt = ORM.Assignment.Get(A.AssignmentId);
            var user = ORM.User.Get(asgmt.AssigneeUserId);
            double runningRating = 0.0;
            double maxRating = 0.0;

            var assessment = new Assessment()
            {
                Speaker = user?.FriendlyName ?? currentUser.FriendlyName,
                Date = A.Date,
            };

            // ------------------------
            // the metric ladder 
            // ------------------------
            var pm = new PitchMetric(A);
            assessment.PitchAvgHz = pm.Value;
            assessment.PitchMinHz = pm.ValueMin;
            assessment.PitchMaxHz = pm.ValueMax;
            assessment.PitchComment = pm.Commentary;
            runningRating += pm.Rating;
            maxRating += 100.0;

            var vm = new VolumeMetric(A, useDemoCalculations);
            assessment.VolumeLevel = vm.Value;
            assessment.VolumeComment = vm.Commentary;
            runningRating += vm.Rating;
            maxRating += 100.0;

            var pace = new PaceMetric(A.MediaDurationMS, A.ActualTranscript, useDemoCalculations);
            assessment.PaceWPM = pace.Value;
            assessment.PaceComment = pace.Commentary;
            runningRating += pace.Rating;
            maxRating += 100.0;

            var acc = new AccuracyMetric(A.ActualTranscript, A.ExpectedTranscript);
            assessment.AccuracyPct = acc.Value;
            assessment.AccuracyComment = acc.Commentary;
            assessment.DiffHTML = acc.DiffHTML;
            runningRating += acc.Rating;
            maxRating += 100.0;

            var cm = new ClarityMetric(assessment.PaceWPM, assessment.AccuracyPct, useDemoCalculations);
            assessment.ClarityLikert = cm.Value;
            assessment.ClarityComment = cm.Commentary;
            runningRating += cm.Rating;
            maxRating += 100.0;

            var cfm = new ConfidenceMetric(assessment.PitchAvgHz, assessment.PitchMinHz, assessment.PitchMaxHz, useDemoCalculations);
            assessment.ConfidenceLikert = cfm.Value;
            assessment.ConfidenceComment = cfm.Commentary;
            runningRating += cfm.Rating;
            maxRating += 100.0;

            var crm = new CredibilityMetric(assessment.PaceWPM, useDemoCalculations);
            assessment.CredibilityLikert = crm.Value;
            assessment.CredibilityComment = crm.Commentary;
            runningRating += crm.Rating;
            maxRating += 100.0;

            //var fm = new FluencyMetric(assessment.PaceWPM, assessment.AccuracyPct);
            //assessment.Fluency = fm.Value;
            //assessment.FluencyComment = fm.Commentary;
            //runningRating += fm.Rating;
            //maxRating += 100.0;

            //var im = new InflectionMetric(A.MediaLocation);
            //assessment.InflectionSD = im.Value;
            //assessment.InflectionComment = im.Commentary;
            //runningRating += im.Rating;
            //maxRating += 100.0;

            // place holder for subject metrics 
            // assessment.SubjectiveMetric = AndroidSpeechHelper.GetTone(assessment.PaceWPM, assessment.AccuracyPct);
            assessment.SubjectiveMetric = "Tone is often subjective (commented)";

            // Update overall score 
            assessment.Score = 100 * (runningRating / maxRating);

            return assessment;

        }
    }
}