using System;
using System.IO;

using SQLite;
using SpotOn.DAL.Models;
using Cointeco.GoogleHelper;
using System.Collections.Generic;
using Cointeco;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace SpotOn.DAL
{
    public class LessonAddedEventArgs : EventArgs
    {
        public int Id { get; set; } = -1;
        public string Message { get; set; }
    }

    public class LessonORM : ORMBase<Lesson>
    {

        public LessonORM() : base("Lesson") { }

        public int Upsert(
            int id, string orgId, DateTime createdDate, string author, string title, string description,
            string transcript, string localRecordingPath = null, string videoCoverPath = null,
            string localVideoPath = null, string alphasort = null, string course = null, 
            string notes = null, string attachment = null)
        {
            int retVal = -1;
            CommonBase.Logger.Information("Lesson.Save: Saving lesson {l} by {a} on {d}", title, author, createdDate);

            try
            {

                var newLesson = new Lesson()
                {
                    Author = author,
                    VideoCoverPath = videoCoverPath,
                    CreatedDate = createdDate,
                    Description = description,
                    LocalRecordingPath = localRecordingPath,
                    LocalVideoPath = localVideoPath,
                    Title = title,
                    AlphaSort = alphasort ?? title,
                    Course = course,
                    Transcript = transcript,
                    Notes = notes,
                    Attachment = attachment
                };
                if (id <= 0)
                {
                    db.Insert(newLesson);
                }
                else
                {
                    newLesson.Id = id;
                    db.Update(newLesson);
                }
                retVal = newLesson.Id;
            }
            catch (Exception e)
            {
                CommonBase.Logger.Error($"Lesson.Save Exception: {e.Message}");
            }
            return retVal;
        }


        /// <summary>
        ///  also delete all the lesson artefacts 
        /// </summary>
        /// <returns></returns>
        public override bool DeleteAll()
        {
            var allLessons = GetAll();
            foreach (var l in allLessons)
                Delete(l.Id);
            return true;
        }

        public override bool Delete(int id)
        {
            try
            {
                // need to read it before it is deleted...
                var lesson = this.Get(id);

                // delete all assignments 
                bool retVal = UnassignAll(id);
                if (!retVal)
                    CommonBase.Logger.Error("LessonORM.Delete() Error: Unable to unassign lesson {id} from all users", id);

                // compound &
                retVal &= base.Delete(id);

                if (retVal) // also delete the local media / video file(s)
                {
                    if (!string.IsNullOrEmpty(lesson?.LocalRecordingPath))
                        File.Delete(lesson?.LocalRecordingPath);
                    if (!string.IsNullOrEmpty(lesson?.LocalVideoPath))
                        File.Delete(lesson?.LocalVideoPath);
                    if (!string.IsNullOrEmpty(lesson?.Attachment))
                        File.Delete(lesson?.Attachment);
                }
                else
                    CommonBase.Logger.Error("LessonORM.Delete() Error: Unable to delete lesson {id} ", id);

                return retVal;
            }
            catch (Exception e)
            {
                CommonBase.Logger.Error($"LessonORM.Delete Exception: {e.Message}");
            }
            return false;
        }

        /// <summary>
        /// Assign the lesson to the user 
        /// </summary>
        /// <returns></returns>
        public bool Assign(int id, int assigneeId, int reviewerId, int dueDays = 7)
        {
            // find out if there is an assignment for this lesson for the user 
            if (IsAssigned(id, assigneeId))
            {
                CommonBase.Logger.Warning("LessonORM.Assign() Error: Lesson {id} already assigned to {u}", id, assigneeId);
                return false;
            }
            var dd = DateTime.UtcNow + new TimeSpan(dueDays, 0, 0, 0);

            // insert new entry (upsert returns a value >= 0)
            return (0 <= ORM.Assignment.AssignTo(
                id, // lesson id 
                assigneeId,
                reviewerId,
                DateTime.UtcNow, // assigned date
                dd));
        }

        /// <summary>
        /// see if the lesson is already assigned to the user 
        /// </summary>
        /// <param name="lessonid"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public bool IsAssigned(int id, int assigneeId)
        {
            return ORM.Assignment.GetAssignments(id, assigneeId)?.Count > 0;
        }

        public  Lesson GetFirst(string lessonTitle)
        {
            var escapedLT = lessonTitle.Replace("'", "''");
            string query = $"Select * from {TableName} where Title = '{escapedLT}' LIMIT 1";

            var lessons = db.Query<Lesson>(query);
            if (lessons?.Count != 0)
                return lessons[0];
            else
                return null;
        }


        /// <summary>
        /// unassign this lesson from the user 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Unassign(int id, int userid)
        {
            // get all assignments for this lesson for the user 
            // (there should really only be one)
            var assignments = ORM.Assignment.GetAssignments(id, userid);
            bool retval = true;
            if (assignments?.Count > 0)
            {
                // delete them all 
                foreach (var ass in assignments)
                {
                    if (!ORM.Assignment.Delete(ass.Id))
                    {
                        CommonBase.Logger.Warning("LessonORM.Unassign() : Unable to unassign lesson {lid} from user {u}. Assigment Id {asid} could not be deleted.", id, userid, ass.Id);
                        retval = false;
                    }
                }
            }

            return retval;
        }


        /// <summary>
        /// unassign this lesson from ALL users 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool UnassignAll(int id)
        {

            // get all assignments for this lesson  
            var assignments = ORM.Assignment.GetAssignments(id);
            bool retval = true;
            if (assignments?.Count > 0)
            {
                // delete them all 
                foreach (var assgnmt in assignments)
                {
                    if (!ORM.Assignment.Delete(assgnmt.Id))
                    {
                        var assignedUser = assgnmt.GetAssigneeName();
                        CommonBase.Logger.Warning("LessonORM.UnassignAll() Error: Unable to unassign lesson {lid} from user {u}. Assigment Id {asid} could not be deleted.", id, assignedUser, assgnmt.Id);
                        retval = false;
                    }
                }
            }

            return retval;
        }

        public event EventHandler<LessonAddedEventArgs> OnSampleAdded;

        public void LoadLessonsFromSheets(string SheetId, string SheetName, bool forceReload = false)
        {
            CommonBase.Logger.Information("LessonORM.LoadSamplesFromSheets() : Loading Samples from {sn} ({si})", SheetName, SheetId);
            string[,] arr = CommonBase.GoogleSheetsInstance.ReadEntries(SheetId, SheetName, "A:L");
            string[] expectedColumns = new string[] { "Name", "Description", "Transcript", "AudioURI", "AudioType", "VideoURI", "VideoType", "Course", "Enabled", "AlphaSort", "Notes", "Attachment" };
            List<Task> downloads = new List<Task>();
            int numLessonsAdded = 0;
            try
            {
                if (!CommonBase.ValidateColumns("Lessons", expectedColumns, arr))
                {
                    OnSampleAdded?.Invoke(this, new LessonAddedEventArgs()
                    {
                        Id = -1,
                        Message = $"Unable to load Lesson Samples from sheet {SheetName}  - it does not have the correct columns."
                    });
                }
                else
                {
                    for (int r = 1; r < arr.GetLength(0); r++)
                    {
                        string lessonTitle = arr[r, 0];
                        // SPOT-67 : Added a few columns
                        string enabled = arr[r, 8];
                        bool isEnabled = (enabled.Trim().ToLower() == "y" || enabled.Trim().ToLower() == "1" || enabled.Trim().ToLower() == "t");

                        if (string.IsNullOrEmpty(lessonTitle)) // do not import blank lines
                            continue;

                        var xstgLesson = this.GetFirst(lessonTitle);

                        // SPOT-67 : effect of checking (isEnabled==false) is to delete 
                        // existing lessons that are disabled.
                        if (xstgLesson != null && (forceReload == true || isEnabled == false))
                        {
                            // this forces the lesson to be reloaded, even if it does exist 
                            // NOTE: This will delete all assignments related to the lesson , if any 
                            this.Delete(xstgLesson.Id);
                            xstgLesson = null;
                        }

                        // only add sample that does not exist 
                        if (xstgLesson == null)
                        {
                            if (!isEnabled)
                            {
                                CommonBase.Logger.Warning("Lesson {t} is NOT enabled - skipping.", lessonTitle);
                                continue;
                            }

                            string lessonDescription = arr[r, 1];
                            string lessonTranscript = arr[r, 2];
                            string audioURI = arr[r, 3];
                            string audioType = arr[r, 4];
                            string videoURI = arr[r, 5];
                            string videoType = arr[r, 6];
                            string course = arr[r, 7];
                            string alphasort = arr[r, 9];
                            string notes = arr[r, 10];
                            string attachmentURI = arr[r, 11];

                            string audioPath = CommonBase.GetSafeFileName(lessonTitle, audioType, CommonBase.UserTempPath);
                            string videoPath = CommonBase.GetSafeFileName(lessonTitle, videoType, CommonBase.UserTempPath);

                            // 2020-06-02 : Refactor downloading of audio and video 
                            audioPath = DownloadItem(lessonTitle, audioType, "Audio", audioURI, downloads);
                            videoPath = DownloadItem(lessonTitle, videoType, "Video", videoURI, downloads);

                            // if there is no video, and no audio, then convert the transcript to a speech WAV
                            if (videoPath is null)
                            {
                                if (audioPath is null)
                                {
                                    // transcribe it 
                                    CommonBase.Logger.Information("Transcribing Text for  Lesson {l} ", audioURI, lessonTitle);
                                    audioPath = CommonBase.Extensions.SynthesizeTextToAudioFile(lessonTranscript); 
                                }
                            }

                            // wait for downloads to finish 
                            Task.WaitAll(downloads.ToArray());

                            // get video cover 
                            string videoCover = null;
                            if (videoPath != null )
                            {
                                var t = CommonBase.Extensions.GetVideoCoverImage(videoPath).ConfigureAwait(false);
                                videoCover = t.GetAwaiter().GetResult();
                            }

                            // erase downloads
                            downloads.Clear();

                            // save to db
                            var lessonId = Upsert(-1, 
                                Organization.PUBLIC_ORG_ID,
                                DateTime.UtcNow,
                                User.SYSTEM_USER,
                                lessonTitle,
                                lessonDescription,
                                lessonTranscript,
                                audioPath, 
                                videoCover,
                                videoPath,
                                alphasort,
                                course,
                                notes,
                                attachmentURI); // SPOT-69 - also save attachment

                            // count
                            numLessonsAdded++;

                            // notify caller 
                            OnSampleAdded?.Invoke(this, new LessonAddedEventArgs()
                            {
                                Id = lessonId,
                                Message = $"Added [{lessonTitle}]:'{lessonDescription}' ({lessonId})."
                            });
                        }
                        // else skip ... 
                    }

                    // report completion 
                    if (numLessonsAdded == 0)
                        OnSampleAdded?.Invoke(this, new LessonAddedEventArgs() { Message = "No new Sample Lessons to add." });
                    else
                        OnSampleAdded?.Invoke(this, new LessonAddedEventArgs() { Message = $"Added {numLessonsAdded} new Sample Lessons." });
                }
            }
            catch (Exception ex)
            {
                CommonBase.Logger.Error("LessonORM.LoadSamplesFromSheets() Exception {e}", ex.Message);
            }

        }

        private string DownloadItem(string basefilename, string extension, string description, string uri, List<Task> downloads)
        {
            string returnPath = null;
            if (!String.IsNullOrEmpty(uri))
            {
                returnPath = Path.Combine(CommonBase.UserTempPath, Regex.Replace(basefilename, @"\W", "_")) + $".{extension}";
                uri = CommonBase.AutoMungeGDriveURL(uri);
                // load it from Cloud 
                CommonBase.Logger.Information("DownloadItem(): Downloading item {d}: {f} => {p} ", description, uri, returnPath);
                downloads.Add(Task.Run(async () =>
                {
                    await CommonBase.DownloadFileAsync(uri, returnPath).ConfigureAwait(false);
                    CommonBase.Logger.Information("...Downloaded to {f} ", returnPath);
                }));
            }
            return returnPath;
        }

        public bool Update(Lesson lesson)
        {
            int numRows = db.Update(lesson);
            return  (numRows == 1);
        }

        public bool Update(
            int id, string orgId, DateTime createdDate, string author, string title, string description,
            string transcript, string audioPath, string videoPath, string notes, string videoCover)
        {
            bool retVal;
            CommonBase.Logger.Information("Lesson.Update(): Updating lesson {l} by {a} on {d}", title, author, createdDate);

            try
            {
                var lesson = Get(id);
                if (lesson != null)
                {
                    lesson.OrgId = orgId;
                    lesson.CreatedDate = createdDate;
                    lesson.Author = author;
                    lesson.Title = title;
                    lesson.Description = description;
                    lesson.Transcript = transcript;
                    lesson.LocalRecordingPath = audioPath;
                    lesson.LocalVideoPath = videoPath;
                    lesson.VideoCoverPath = videoCover;
                    lesson.Notes = notes;

                    // NOTE: these are not touched because there is no UI for them:
                    // lesson.Attachment;
                    // lesson.AlphaSort;
                    // lesson.Course

                    retVal  = Update(lesson);

                }
                else
                    retVal = false;
            }
            catch (Exception e)
            {
                retVal = false;
                CommonBase.Logger.Error($"Lesson.Update() Exception: {e.Message}");
            }
            return retVal;
        }

    }
}