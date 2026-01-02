namespace EliteJournalReader.Events
{
    //When written: when taking damage due to overheating
    //Parameters:none
    public class HeatDamageEvent : JournalEvent<HeatDamageEvent.HeatDamageEventArgs>
    {
        public HeatDamageEvent() : base("HeatDamage") { }

        public class HeatDamageEventArgs : JournalEventArgs
        {
        }
    }
}
