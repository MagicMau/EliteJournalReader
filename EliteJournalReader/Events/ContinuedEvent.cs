using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: if the journal file grows to 500k lines, we write this event, close the file, and start a new one
    //Parameters:
    //�	Message: next part number
    public class ContinuedEvent : JournalEvent<ContinuedEvent.ContinuedEventArgs>
    {
        public ContinuedEvent() : base("Continued")
        {

        }

        internal override JournalEventArgs FireEvent(JournalWatcher journalWatcher, JObject evt)
        {
            var args = base.FireEvent(journalWatcher, evt);

            // a continued event signals that a new file is coming, so
            // let's start polling for it
            journalWatcher?.StartPollingForNewJournal();

            return args;
        }

        public class ContinuedEventArgs : JournalEventArgs
        {
            public int Part { get; set; }
        }
    }
}
