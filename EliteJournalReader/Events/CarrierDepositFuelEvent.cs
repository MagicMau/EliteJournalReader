namespace EliteJournalReader.Events
{
    //When written: If you should ever reset your game
    //Parameters:
    //�	Name: commander name
    public class CarrierDepositFuelEvent : JournalEvent<CarrierDepositFuelEvent.CarrierDepositFuelEventArgs>
    {
        public CarrierDepositFuelEvent() : base("CarrierDepositFuel") { }

        public class CarrierDepositFuelEventArgs : JournalEventArgs
        {
            public long CarrierID { get; set; }
            public long Amount { get; set; }
            public long Total { get; set; }
        }
    }
}
