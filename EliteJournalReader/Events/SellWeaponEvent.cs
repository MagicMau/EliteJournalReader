namespace EliteJournalReader.Events
{
    public class SellWeaponEvent : JournalEvent<SellWeaponEvent.SellWeaponEventArgs>
    {
        public SellWeaponEvent() : base("SellWeapon") { }

        public class SellWeaponEventArgs : JournalEventArgs
        {
            public string Name { get; set; }
            public int Price { get; set; }
            public long SuitModuleID { get; set; }
            public string Class { get; set; }
            public string[] WeaponMods { get; set; }
        }
    }
}