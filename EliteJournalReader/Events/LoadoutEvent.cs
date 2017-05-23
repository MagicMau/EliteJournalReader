using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: at startup, when loading from main menu
    //Parameters:
    //•	Modules: array of installed items, each with:
    //  o Slot: slot name
    //  o Item: module name
    //  o On: bool, indicates on or off
    //  o Priority: power priority
    //  o AmmoInClip: (if relevant)
    //  o AmmoInHopper: (if relevant)
    //  o EngineerBlueprint: blueprint name(if engineered)
    //  o EngineerLevel: blueprint level(if engineered)
    //(For a passenger cabin, AmmoInClip holds the number of places in the cabin)
    public class LoadoutEvent : JournalEvent<LoadoutEvent.LoadoutEventArgs>
    {
        public LoadoutEvent() : base("Loadout") { }

        public class LoadoutEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Ship = evt.Value<string>("Ship");
                ShipId = evt.Value<int>("ShipID");
                ShipName = evt.Value<string>("ShipName");
                ShipIdent = evt.Value<string>("ShipIdent");
                Modules = evt["Modules"]?.ToObject<List<Module>>();
            }

            public string Ship { get; set; }
            public int ShipId { get; set; }
            public string ShipName { get; set; }
            public string ShipIdent { get; set; }
            public List<Module> Modules { get; set; }
        }

        public class Module
        {
            public string Slot { get; set; }
            public string Item { get; set; }
            public bool On { get; set; }
            public int Priority { get; set; }
            public int? AmmoInClip { get; set; }
            public int? AmmoInHopper { get; set; }
            public string EngineerBlueprint { get; set; }
            public int? EngineerLevel { get; set; }
        }
    }
}
