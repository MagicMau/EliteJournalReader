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
        internal FakeJournalWatcher(bool isLive=true)
        {
            this.IsLive = isLive;
        }

        internal FakeJournalWatcher(string path,bool isLive=true) : base(path)
        {
            this.IsLive = isLive;
        }

        public override Task StartWatching()
        {
            // assume IsLive was set on construction
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
