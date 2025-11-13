using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteJournalReader.Tests
{
    internal class FakeJournalWatcher : JournalWatcher
    {
        internal FakeJournalWatcher()
        {
            string journalPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Saved Games", "Frontier Developments", "Elite Dangerous");
            if (!Directory.Exists(journalPath))
            {
                journalPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "TestJournalEvents");
                Directory.CreateDirectory(journalPath);
            }
            Path = journalPath;
        }

        public override Task StartWatching()
        {
            IsLive = true;
            return Task.CompletedTask;
        }

        public override void StopWatching()
        {
            base.StopWatching();
        }

        public void FireFakeEvent(string json)
        {
            ParseAndProcess(json); // this will fire the event
        }

        public EliteJournalReader.JournalEventArgs FireFakeEventAndReturn(string json)
        {
            var obj = JObject.Parse(json);
            var eventType = obj.Value<string>("event");
            return FireEvent(eventType, obj);
        }
    }
}
