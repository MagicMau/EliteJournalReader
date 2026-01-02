namespace EliteJournalReader.Events
{
    //When Written: when requesting an engineer upgrade
    //Parameters:
    //�	Engineer: name of engineer
    //�	Blueprint: name of blueprint
    //�	Level: crafting level
    //�	Ingredients: JSON object with names and quantities of materials required
    public class EngineerCraftEvent : JournalEvent<EngineerCraftEvent.EngineerCraftEventArgs>
    {
        public EngineerCraftEvent() : base("EngineerCraft") { }

        public class EngineerCraftEventArgs : JournalEventArgs
        {
            public string Slot { get; set; }
            public string Module { get; set; }
            public string Engineer { get; set; }
            public long EngineerID { get; set; }
            public string BlueprintName { get; set; }
            public long BlueprintID { get; set; }
            public int Level { get; set; }
            public double Quality { get; set; }
            public string ApplyExperimentalEffect { get; set; }
            public Material[] Ingredients { get; set; }
            public EngineeringModifiers[] Modifiers { get; set; }
        }
    }
}
