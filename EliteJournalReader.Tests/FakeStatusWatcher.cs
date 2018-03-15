using EliteJournalReader.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteJournalReader.Tests
{
    internal class FakeStatusWatcher : StatusWatcher
    {
        public override void StartWatching()
        {
            
        }

        public void StartWatching(string path)
        {
            Initialize(path);
            base.StartWatching();
        }

        public void SendFakeStatusUpdate(StatusFileEvent evt)
        {
            FireStatusUpdatedEvent(evt);
        }
    }
}
