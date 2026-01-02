namespace EliteJournalReader.Events
{
    //When written: when contributing materials to a "research" community goal
    //Parameters:
    //�	Name: material name
    //�	Category
    //�	Count
    public class ScientificResearchEvent : JournalEvent<ScientificResearchEvent.ScientificResearchEventArgs>
    {
        public ScientificResearchEvent() : base("ScientificResearch") { }

        public class ScientificResearchEventArgs : JournalEventArgs
        {
            public long MarketID { get; set; }
            public string Name { get; set; }
            public string Category { get; set; }
            public int Count { get; set; }
        }
    }
}
