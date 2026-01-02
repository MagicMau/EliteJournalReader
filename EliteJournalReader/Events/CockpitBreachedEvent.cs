namespace EliteJournalReader.Events
{
    //When written: when cockpit canopy is breached
    //Parameters: none
    public class CockpitBreachedEvent : JournalEvent<CockpitBreachedEvent.CockpitBreachedEventArgs>
    {
        public CockpitBreachedEvent() : base("CockpitBreached") { }

        public class CockpitBreachedEventArgs : JournalEventArgs
        {
        }
    }
}
