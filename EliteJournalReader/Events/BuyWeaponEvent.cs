namespace EliteJournalReader.Events
{
    public class BuyWeaponEvent : JournalEvent<BuyWeaponEvent.BuyWeaponEventArgs>
    {
        public BuyWeaponEvent() : base("BuyWeapon") { }

        public class BuyWeaponEventArgs : JournalEventArgs
        {
            public string Name { get; set; }
            public string Name_Localised { get; set; }
            public int Price { get; set; }
            public long SuitModuleID { get; set; }
            public string Class { get; set; }
            public string[] WeaponMods { get; set; }
        }
    }
}