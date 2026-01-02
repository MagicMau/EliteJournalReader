namespace EliteJournalReader.Events
{
    //When written: player was HeatWarning by player or npc
    //Parameters: 
    //�	Submitted: true or false
    public class HeatWarningEvent : JournalEvent<HeatWarningEvent.HeatWarningEventArgs>
    {
        public HeatWarningEvent() : base("HeatWarning") { }

        public class HeatWarningEventArgs : JournalEventArgs
        {
        }
    }
}
