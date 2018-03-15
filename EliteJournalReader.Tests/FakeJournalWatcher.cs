using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteJournalReader.Tests
{
    internal class FakeJournalWatcher : JournalWatcher
    {
        internal FakeJournalWatcher()
        {
        }

        public override Task StartWatching()
        {
            IsLive = true;
            return Task.CompletedTask;
        }

        public override void StopWatching()
        {
            // nothing to do here
        }

        public void FireFakeEvent(string json)
        {
            Parse(json); // this will fire the event
        }
    }
}
