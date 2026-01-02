namespace EliteJournalReader.Events
{
    //When written: at startup, when loading from main menu
    //Parameters:
    //�	Inventory: array of cargo, with Name and Count for each
    public class CargoEvent : JournalEvent<CargoEvent.CargoEventArgs>
    {
        public CargoEvent() : base("Cargo") { }

        public class CargoEventArgs : JournalEventArgs
        {
            public Commodity[] Inventory { get; set; }
            public int Count { get; set; }
            public string Vessel { get; set; }
        }
    }
}
