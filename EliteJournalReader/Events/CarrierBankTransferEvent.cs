namespace EliteJournalReader.Events
{
    //When written: If you should ever reset your game
    //Parameters:
    //�	Name: commander name
    public class CarrierBankTransferEvent : JournalEvent<CarrierBankTransferEvent.CarrierBankTransferEventArgs>
    {
        public CarrierBankTransferEvent() : base("CarrierBankTransfer") { }

        public class CarrierBankTransferEventArgs : JournalEventArgs
        {
            public long CarrierID { get; set; }
            public long Deposit { get; set; }
            public long PlayerBalance { get; set; }
            public long CarrierBalance { get; set; }
        }
    }
}
