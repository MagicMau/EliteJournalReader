namespace EliteJournalReader.Events
{
    //The "Passengers" event contains:

    //"Manifest": array of passenger records, each containing:
    //o MissionID (int)
    //o Type (string)
    //o VIP (bool)
    //o Wanted (bool)
    //o Count (int) 
    public class PassengersEvent : JournalEvent<PassengersEvent.PassengersEventArgs>
    {
        public PassengersEvent() : base("Passengers") { }

        public class PassengersEventArgs : JournalEventArgs
        {
            public Passenger[] Manifest { get; set; }
        }
    }
}
