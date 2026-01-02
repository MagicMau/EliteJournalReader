namespace EliteJournalReader.Events
{
    //When written: when switching control between the main ship and a fighter
    //Parameters:
    //�	To: ( Mothership/Fighter)
    public class VehicleSwitchEvent : JournalEvent<VehicleSwitchEvent.VehicleSwitchEventArgs>
    {
        public VehicleSwitchEvent() : base("VehicleSwitch") { }

        public class VehicleSwitchEventArgs : JournalEventArgs
        {
            public Vehicle To { get; set; }
        }
    }

    public enum Vehicle
    {
        Unknown,
        Mothership,
        Fighter
    }
}
