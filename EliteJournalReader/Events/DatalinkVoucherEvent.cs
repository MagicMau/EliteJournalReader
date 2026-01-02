namespace EliteJournalReader.Events
{
    //When written: when scanning a datalink generates a reward
    //Parameters:
    //�	Reward: value in credits
    //�	VictimFaction
    //�	PayeeFaction
    public class DatalinkVoucherEvent : JournalEvent<DatalinkVoucherEvent.DatalinkVoucherEventArgs>
    {
        public DatalinkVoucherEvent() : base("DatalinkVoucher") { }

        public class DatalinkVoucherEventArgs : JournalEventArgs
        {
            public int Reward { get; set; }
            public string VictimFaction { get; set; }
            public string PayeeFaction { get; set; }
        }
    }
}
