namespace EliteJournalReader.Events
{
    //When written: at startup, when loading from main menu, or when switching ships, 
    //or after changing the ship in Outfitting, or when docking SRV back in mothership
    //Parameters:
    //•	Ship: current ship type
    //•	ShipID: ship id number(indicates which of your ships you are in)
    //•	ShipName: user-defined ship name
    //•	ShipIdent: user-defined ship ID string
    //•	HullValue – may not always be present
    //•	ModulesValue – may not always be present
    //•	HullHealth
    //•	UnladenMass – Mass of Hull and Modules, excludes fuel and cargo
    //•	FuelCapacity: { Main: , Reserve: }
    //•	CargoCapacity
    //•	MaxJumpRange: (based on zero cargo, and just enough fuel for 1 jump)
    //•	Rebuy
    //•	Hot: (if wanted at startup – may not always be present)

    //•	Modules: array of installed items, each with:
    //  o Slot: slot name
    //  o Item: module name
    //  o On: bool, indicates on or off
    //  o Priority: power priority
    //  o Health
    //  o Value
    //  o AmmoInClip: (if relevant)
    //  o AmmoInHopper: (if relevant)
    //  o Engineering: (if engineered)
    //	    - EngineerID
    //	    - Engineer: name
    //	    - BlueprintID
    //	    - BlueprintName: blueprint name
    //	    - Level
    //	    - Quality
    //	    - ExperimentalEffect: (name, if applied)
    //	    - Modifications: Json array of objects
    //          •	Label – (see §13.11 below)
    //          •	Value – may not always be present
    //          •	OriginalValue
    //          •	LessIsGood: bool

    // (For a passenger cabin, AmmoInClip holds the number of places in the cabin)

    public class SuitLoadoutEvent : JournalEvent<SuitLoadoutEvent.SuitLoadoutEventArgs>
    {
        public SuitLoadoutEvent() : base("SuitLoadout") { }

        public class SuitLoadoutEventArgs : JournalEventArgs
        {
            public long SuitID { get; set; }
            public string SuitName { get; set; }
            public string SuitName_Localised { get; set; }
            public object[] SuitMods { get; set; }
            public long LoadoutID { get; set; }
            public string LoadoutName { get; set; }
            public SuitModule[] Modules { get; set; }
        }
    }

    public class SuitModule
    {
        public string SlotName { get; set; }
        public long SuitModuleID { get; set; }
        public string ModuleName { get; set; }
        public string ModuleName_Localised { get; set; }
        public int Class { get; set; }
        public object[] WeaponMods { get; set; }
    }
}
