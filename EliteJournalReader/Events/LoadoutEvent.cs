using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: at startup, when loading from main menu, or when switching ships, or after changing the ship in Outfitting
    //Parameters:
    //•	Ship: current ship type
    //•	ShipID: ship id number(indicates which of your ships you are in)
    //•	ShipName: user-defined ship name
    //•	ShipIdent: user-defined ship ID string
    //•	HullValue – may not always be present
    //•	ModulesValue – may not always be present
    //•	HullHealth
    //•	Rebuy
    //•	Hot: (if wanted at startup – may not always be present)

    //•	Modules: array of installed items, each with:
    //o Slot: slot name
    //o Item: module name
    //o On: bool, indicates on or off
    //o Priority: power priority
    //o Health
    //o Value
    //o AmmoInClip: (if relevant)
    //o AmmoInHopper: (if relevant)
    //o Engineering: (if engineered)
    //	EngineerID
    //	Engineer: name
    //	BlueprintID
    //	BlueprintName: blueprint name
    //	Level
    //	Quality
    //	ExperimentalEffect: (name, if applied)
    //	Modifications: Json array of objects
    //•	Label – (see §13.11 below)
    //•	Value – may not always be present
    //•	OriginalValue
    //•	LessIsGood: bool

    // (For a passenger cabin, AmmoInClip holds the number of places in the cabin)

    public class LoadoutEvent : JournalEvent<LoadoutEvent.LoadoutEventArgs>
    {
        public LoadoutEvent() : base("Loadout") { }

        public class LoadoutEventArgs : JournalEventArgs
        {
            public string Ship { get; set; }
            public int ShipID { get; set; }
            public string ShipName { get; set; }
            public string ShipIdent { get; set; }
            public int HullValue { get; set; }
            public int ModulesValue { get; set; }
            public double HullHealth { get; set; }
            public int Rebuy { get; set; }
            public bool Hot { get; set; }
            public List<Module> Modules { get; set; }
        }
    }
}
