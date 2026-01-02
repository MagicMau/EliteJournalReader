namespace EliteJournalReader.Events
{
    //    When written: when in Supercruise, and distance from planet drops to within the 'Orbital Cruise' zone
    //    Parameters:
    //�	StarSystem
    //�	SystemAddress
    //�	Body
    //�	BodyID
    public class ApproachBodyEvent : JournalEvent<ApproachBodyEvent.ApproachBodyEventArgs>
    {
        public ApproachBodyEvent() : base("ApproachBody") { }

        public class ApproachBodyEventArgs : JournalEventArgs
        {
            public string StarSystem { get; set; }
            public long SystemAddress { get; set; }
            public string Body { get; set; }
            public int BodyID { get; set; }
        }
    }
}
