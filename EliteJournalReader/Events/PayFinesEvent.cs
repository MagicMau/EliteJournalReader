namespace EliteJournalReader.Events
{
    //When Written: when paying fines
    //Parameters:
    //�	Amount
    //�	BrokerPercentage (present if paid via a Broker)
    //�	AllFines: bool
    //�	Faction: (if paying off an individual faction's fines)
    //�	ShipID

    public class PayFinesEvent : JournalEvent<PayFinesEvent.PayFinesEventArgs>
    {
        public PayFinesEvent() : base("PayFines") { }

        public class PayFinesEventArgs : JournalEventArgs
        {
            public int Amount { get; set; }
            public double? BrokerPercentage { get; set; }
            public bool AllFines { get; set; }
            public string Faction { get; set; }
            public string Faction_Localised { get; set; }
            public long ShipID { get; set; }

        }
    }
}
