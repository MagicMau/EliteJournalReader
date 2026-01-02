namespace EliteJournalReader.Events
{
    //When written: when looking at the cockpit RHS modules info panel, if data has changed
    //This also writes a ModulesInfo.json file alongside the journal, listing the modules in the same order as displayed
    //Parameters: None

    public class ModulesInfoEvent : JournalEvent<ModulesInfoEvent.ModulesInfoEventArgs>
    {
        public ModulesInfoEvent() : base("ModulesInfo") { }

        public class ModulesInfoEventArgs : JournalEventArgs
        {
        }
    }
}
