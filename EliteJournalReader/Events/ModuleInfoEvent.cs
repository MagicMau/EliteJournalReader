namespace EliteJournalReader.Events
{
    //When written: when docking an SRV with the ship
    //Parameters: none
    public class ModuleInfoEvent : JournalEvent<ModuleInfoEvent.ModuleInfoEventArgs>
    {
        public ModuleInfoEvent() : base("ModuleInfo") { }

        public class ModuleInfoEventArgs : JournalEventArgs
        {
        }
    }
}
