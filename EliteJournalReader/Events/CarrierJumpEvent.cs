using System.Linq;
using Newtonsoft.Json;

namespace EliteJournalReader.Events
{
    // {
    //  "timestamp": "2025-03-19T19:03:12Z",
    //  "event": "CarrierJump",
    //  "Docked": true,
    //  "StationName": "V1B-WQB",
    //  "StationType": "FleetCarrier",
    //  "MarketID": 3705689344,
    //  "StationFaction": { "Name": "FleetCarrier" },
    //  "StationGovernment": "$government_Carrier;",
    //  "StationGovernment_Localised": "Private Ownership",
    //  "StationServices": [
    //    "dock",
    //    "autodock",
    //    "blackmarket",
    //    "commodities",
    //    "contacts",
    //    "exploration",
    //    "outfitting",
    //    "crewlounge",
    //    "rearm",
    //    "refuel",
    //    "repair",
    //    "shipyard",
    //    "engineer",
    //    "flightcontroller",
    //    "stationoperations",
    //    "stationMenu",
    //    "carriermanagement",
    //    "carrierfuel",
    //    "livery",
    //    "voucherredemption",
    //    "socialspace",
    //    "bartender",
    //    "vistagenomics",
    //    "pioneersupplies"
    //  ],
    //  "StationEconomy": "$economy_Carrier;",
    //  "StationEconomy_Localised": "Private Enterprise",
    //  "StationEconomies": [
    //    {
    //      "Name": "$economy_Carrier;",
    //      "Name_Localised": "Private Enterprise",
    //      "Proportion": 1.0
    //    }
    //  ],
    //  "Taxi": false,
    //  "Multicrew": false,
    //  "StarSystem": "HR 3635",
    //  "SystemAddress": 1694121347427,
    //  "StarPos": [64.0625, 75.34375, -79.59375],
    //  "SystemAllegiance": "Alliance",
    //  "SystemEconomy": "$economy_Agri;",
    //  "SystemEconomy_Localised": "Agriculture",
    //  "SystemSecondEconomy": "$economy_Refinery;",
    //  "SystemSecondEconomy_Localised": "Refinery",
    //  "SystemGovernment": "$government_Democracy;",
    //  "SystemGovernment_Localised": "Democracy",
    //  "SystemSecurity": "$SYSTEM_SECURITY_high;",
    //  "SystemSecurity_Localised": "High Security",
    //  "Population": 7910623478,
    //  "Body": "HR 3635 A",
    //  "BodyID": 1,
    //  "BodyType": "Star",
    //  "ControllingPower": "Edmund Mahon",
    //  "Powers": ["Edmund Mahon"],
    //  "PowerplayState": "Fortified",
    //  "PowerplayStateControlProgress": 0.052366,
    //  "PowerplayStateReinforcement": 1396,
    //  "PowerplayStateUndermining": 758,
    //  "Factions": [
    //    {
    //      "Name": "HR 3635 Democrats",
    //      "FactionState": "None",
    //      "Government": "Democracy",
    //      "Influence": 0.018831,
    //      "Allegiance": "Federation",
    //      "Happiness": "$Faction_HappinessBand2;",
    //      "Happiness_Localised": "Happy",
    //      "MyReputation": 0.0
    //    },
    //    {
    //    "Name": "HR 3635 Universal & Co",
    //      "FactionState": "None",
    //      "Government": "Corporate",
    //      "Influence": 0.015857,
    //      "Allegiance": "Independent",
    //      "Happiness": "$Faction_HappinessBand2;",
    //      "Happiness_Localised": "Happy",
    //      "MyReputation": 0.0
    //    },
    //    {
    //    "Name": "K'uanele PLC",
    //      "FactionState": "None",
    //      "Government": "Corporate",
    //      "Influence": 0.029732,
    //      "Allegiance": "Federation",
    //      "Happiness": "$Faction_HappinessBand2;",
    //      "Happiness_Localised": "Happy",
    //      "MyReputation": 0.0
    //    },
    //    {
    //    "Name": "League of HR 3635",
    //      "FactionState": "None",
    //      "Government": "Dictatorship",
    //      "Influence": 0.030723,
    //      "Allegiance": "Independent",
    //      "Happiness": "$Faction_HappinessBand2;",
    //      "Happiness_Localised": "Happy",
    //      "MyReputation": 0.0
    //    },
    //    {
    //    "Name": "HR 3635 Industries",
    //      "FactionState": "None",
    //      "Government": "Corporate",
    //      "Influence": 0.191278,
    //      "Allegiance": "Independent",
    //      "Happiness": "$Faction_HappinessBand2;",
    //      "Happiness_Localised": "Happy",
    //      "MyReputation": 0.0
    //    },
    //    {
    //    "Name": "Dragons of HR 3635",
    //      "FactionState": "None",
    //      "Government": "Anarchy",
    //      "Influence": 0.014866,
    //      "Allegiance": "Independent",
    //      "Happiness": "$Faction_HappinessBand2;",
    //      "Happiness_Localised": "Happy",
    //      "MyReputation": 0.0
    //    },
    //    {
    //    "Name": "Flat Galaxy Society",
    //      "FactionState": "None",
    //      "Government": "Democracy",
    //      "Influence": 0.698712,
    //      "Allegiance": "Alliance",
    //      "Happiness": "$Faction_HappinessBand2;",
    //      "Happiness_Localised": "Happy",
    //      "MyReputation": 100.0,
    //      "PendingStates": [{ "State": "Expansion", "Trend": 0 }]
    //    }
    //  ],
    //  "SystemFaction": { "Name": "Flat Galaxy Society" }
    //}

    public class CarrierJumpEvent : JournalEvent<CarrierJumpEvent.CarrierJumpEventArgs>
    {
        public CarrierJumpEvent() : base("CarrierJump") { }

        public class CarrierJumpEventArgs : JournalEventArgs
        {

            public string StarSystem { get; set; }
            public long SystemAddress { get; set; }

            [JsonConverter(typeof(SystemPositionConverter))]
            public SystemPosition StarPos { get; set; }

            public string Body { get; set; }
            public int BodyID { get; set; }

            [JsonConverter(typeof(ExtendedStringEnumConverter<BodyType>))]
            public BodyType BodyType { get; set; }

            public bool Docked { get; set; }
            public double? Latitude { get; set; }
            public double? Longitude { get; set; }

            public string StationName { get; set; }
            public string StationType { get; set; }
            public long MarketID { get; set; }
            public Faction StationFaction { get; set; }
            public string StationGovernment { get; set; }
            public string StationAllegiance { get; set; }
            public string[] StationServices { get; set; }
            public string StationEconomy { get; set; }
            public string StationEconomy_Localised { get; set; }
            public Economy[] StationEconomies { get; set; }

            public Faction SystemFaction { get; set; }
            public string SystemAllegiance { get; set; }
            public string SystemEconomy { get; set; }
            public string SystemEconomy_Localised { get; set; }
            public string SystemSecondEconomy { get; set; }
            public string SystemSecondEconomy_Localised { get; set; }
            public string SystemGovernment { get; set; }
            public string SystemGovernment_Localised { get; set; }
            public string SystemSecurity { get; set; }
            public string SystemSecurity_Localised { get; set; }


            public bool Wanted { get; set; }
            public long? Population { get; set; }
            public string ControllingPower { get; set; }
            public string[] Powers { get; set; }

            [JsonConverter(typeof(ExtendedStringEnumConverter<PowerplayState>))]
            public PowerplayState PowerplayState { get; set; }
            public double PowerplayStateControlProgress { get; set; }
            public int PowerplayStateReinforcement { get; set; }
            public int PowerplayStateUndermining { get; set; }
            public PowerplayConflictProgress[] PowerplayConflictProgress { get; set; }

            public Faction[] Factions { get; set; }
            public Conflict[] Conflicts { get; set; }
            public bool Taxi { get; set; }
            public bool Multicrew { get; set; }
            

            public override JournalEventArgs Clone()
            {
                var clone = (CarrierJumpEventArgs)base.Clone();
                clone.StationFaction = StationFaction?.Clone();
                clone.SystemFaction = SystemFaction?.Clone();
                clone.StationEconomies = StationEconomies?.Select(e => e.Clone()).ToArray();
                clone.Factions = Factions?.Select(f => f.Clone()).ToArray();
                clone.Conflicts = Conflicts?.Select(c => c.Clone()).ToArray();
                return clone;
            }
        }
    }
}
