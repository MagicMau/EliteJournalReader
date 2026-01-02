namespace EliteJournalReader.Events
{
    //When written: at startup, when loading from main menu
    //Parameters:
    //�	Inventory: array of cargo, with Name and Count for each
    public class CargoTransferEvent : JournalEvent<CargoEvent.CargoEventArgs>
    {
        public CargoTransferEvent() : base("CargoTransfer") { }

        public class CargoTransferEventArgs : JournalEventArgs
        {
            public CargoTransfer[] Transfers { get; set; }
        }
    }
}
