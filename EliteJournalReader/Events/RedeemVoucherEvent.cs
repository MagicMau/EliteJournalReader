namespace EliteJournalReader.Events
{
    //When Written: when claiming payment for combat bounties and bonds
    //Parameters:
    //�	Type
    //�	Amount
    public class RedeemVoucherEvent : JournalEvent<RedeemVoucherEvent.RedeemVoucherEventArgs>
    {
        public RedeemVoucherEvent() : base("RedeemVoucher") { }

        public class RedeemVoucherEventArgs : JournalEventArgs
        {
            public class FactionAmount
            {
                public string Faction { get; set; }
                public int Amount { get; set; }
            }

            public string Type { get; set; }
            public int Amount { get; set; }
            public string Faction { get; set; }
            public double? BrokerPercentage { get; set; }
            public FactionAmount[] Factions { get; set; }
        }
    }
}
