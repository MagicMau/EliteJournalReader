using Newtonsoft.Json.Linq;
using System;

namespace EliteJournalReader
{
    public class JournalEventArgs : EventArgs
    {
        public JToken OriginalEvent { get; set; }

        public DateTime Timestamp { get; set; }

        public JournalEventArgs()
        {
        }

        public virtual void PostProcess(JObject evt, JournalWatcher journalWatcher) { }

        public virtual JournalEventArgs Clone() => (JournalEventArgs)MemberwiseClone();
    }
}