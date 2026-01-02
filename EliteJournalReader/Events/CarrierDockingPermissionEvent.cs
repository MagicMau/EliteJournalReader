namespace EliteJournalReader.Events
{
    //When written: If you should ever reset your game
    //Parameters:
    //�	Name: commander name
    public class CarrierDockingPermissionEvent : JournalEvent<CarrierDockingPermissionEvent.CarrierDockingPermissionEventArgs>
    {
        public CarrierDockingPermissionEvent() : base("CarrierDockingPermission") { }

        public class CarrierDockingPermissionEventArgs : JournalEventArgs
        {
            public long CarrierID { get; set; }
            public string DockingAccess { get; set; }
            public bool AllowNotorious { get; set; }
        }
    }
}
