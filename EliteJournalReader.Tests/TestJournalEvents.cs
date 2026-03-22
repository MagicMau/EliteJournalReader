using System;
using System.Linq;
using System.Reflection;
using EliteJournalReader.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EliteJournalReader.Tests
{
    [TestClass]
    [DoNotParallelize]
    [TestCategory("Journal Events")]
    public class TestJournalEvents
    {
        private FakeJournalWatcher watcher;

        [TestInitialize]
        public void Initialize()
        {
            watcher = new FakeJournalWatcher();
            watcher.StartWatching();
        }

        [TestCleanup]
        public void CleanUp()
        {
            watcher.StopWatching();
            watcher = null;
        }

        [TestMethod]
        public void Test_FileheaderEvent()
        {
            //{ "timestamp":"2017-12-30T13:36:28Z", "event":"Fileheader", "part":1, "language":"English\\UK", "gameversion":"2.4", "build":"r160439/r0 " }
            var returned = watcher.FireFakeEventAndReturn(@"{ ""timestamp"":""2017-12-30T13:36:28Z"", ""event"":""Fileheader"", ""part"":1, ""language"":""English\\UK"", ""gameversion"":""2.4"", ""build"":""r160439/r0 "" }");
            var fe = returned as FileheaderEvent.FileheaderEventArgs;
            Assert.IsNotNull(fe);
            Assert.AreEqual("English\\UK", fe.Language);
        }

        [TestMethod]
        public void Test_DiedEvent_Single()
        {
            //{ "timestamp":"2016-12-05T19:14:42Z", "event":"Died", "KillerName":"Wightman", "KillerShip":"diamondback", "KillerRank":"Expert" }
            var returnedSingle = watcher.FireFakeEventAndReturn(@"{ ""timestamp"":""2016-12-05T19:14:42Z"", ""event"":""Died"", ""KillerName"":""Wightman"", ""KillerShip"":""diamondback"", ""KillerRank"":""Expert"" }");
            var diedSingle = returnedSingle as DiedEvent.DiedEventArgs;
            Assert.IsNotNull(diedSingle);
            Assert.HasCount(1, diedSingle.Killers);
            Assert.AreEqual("Wightman", diedSingle.Killers[0].Name);
        }

        [TestMethod]
        public void Test_DiedEvent_Wing()
        {
            //{ "timestamp":"2016-12-14T21:55:24Z", "event":"Died", "Killers":[ { "Name":"Cmdr Ekol Tieja", "Ship":"federation_gunship", "Rank":"Elite" }, { "Name":"Cmdr Yellowboze", "Ship":"federation_gunship", "Rank":"Novice" } ] }
            var returnedWing = watcher.FireFakeEventAndReturn(@"{ ""timestamp"":""2016-12-14T21:55:24Z"", ""event"":""Died"", ""Killers"" :[ { ""Name"":""Cmdr Ekol Tieja"", ""Ship"":""federation_gunship"", ""Rank"":""Elite"" }, { ""Name"":""Cmdr Yellowboze"", ""Ship"":""federation_gunship"", ""Rank"":""Novice"" } ] }");
            var diedWing = returnedWing as DiedEvent.DiedEventArgs;
            Assert.IsNotNull(diedWing);
            Assert.HasCount(2, diedWing.Killers);
            Assert.AreEqual("Cmdr Ekol Tieja", diedWing.Killers[0].Name);
            Assert.AreEqual("Cmdr Yellowboze", diedWing.Killers[1].Name);
        }

        [TestMethod]
        public void Test_NavRoute()
        {
            var returnedRoute = watcher.FireFakeEventAndReturn(@"{ ""timestamp"":""2020-04-27T08:02:52Z"", ""event"":""NavRoute"", ""Route"" :[ 
                { ""StarSystem"":""i Bootis"", ""SystemAddress"":1281787693419, ""StarPos"" :[-22.37500,34.84375,4.00000], ""StarClass"":""G"" }, 
                { ""StarSystem"":""Acihaut"", ""SystemAddress"":11665802405289, ""StarPos"" :[-18.50000,25.28125,-4.00000], ""StarClass"":""M"" }, 
                { ""StarSystem"":""LHS 455"", ""SystemAddress"":3686969379179, ""StarPos"" :[-16.90625,10.21875,-3.43750], ""StarClass"":""DQ"" }, 
                { ""StarSystem"":""SPF-LF 1"", ""SystemAddress"":22661187052961, ""StarPos"" :[2.90625,6.31250,-9.56250], ""StarClass"":""M"" }, 
                { ""StarSystem"":""Luyten's Star"", ""SystemAddress"":7268024264097, ""StarPos"" :[6.56250,2.34375,-10.25000], ""StarClass"":""M"" }] }
                ");
            var navRoute = returnedRoute as NavRouteEvent.NavRouteEventArgs;
            Assert.IsNotNull(navRoute);
            Assert.HasCount(5, navRoute.Route);
            Assert.AreEqual("i Bootis", navRoute.Route[0].StarSystem);
            Assert.AreEqual("DQ", navRoute.Route[2].StarClass);
            Assert.AreEqual(-10.25m, navRoute.Route[4].StarPos.Z);
        }

        [TestMethod]
        public void Test_FSDJumpEvent()
        {
            var returned = watcher.FireFakeEventAndReturn(@"{ ""timestamp"":""2017-09-25T12:03:06Z"", ""event"":""FSDJump"", ""StarSystem"":""Eranin"", ""SystemAddress"":2832631632594, ""StarPos"":[-22.375,34.844,4.000], ""Body"":""Eranin"", ""BodyID"":0, ""BodyType"":""Star"", ""JumpDist"":12.495, ""FuelUsed"":0.4, ""FuelLevel"":15.6, ""SystemAllegiance"":""Independent"", ""SystemEconomy"":""$economy_Agri;"", ""SystemEconomy_Localised"":""Agriculture"", ""SystemGovernment"":""$government_Democracy;"", ""SystemGovernment_Localised"":""Democracy"", ""SystemSecurity"":""$SYSTEM_SECURITY_medium;"", ""SystemSecurity_Localised"":""Medium Security"", ""Population"":450000 }");
            var fsdJump = returned as FSDJumpEvent.FSDJumpEventArgs;
            Assert.IsNotNull(fsdJump);
            Assert.AreEqual("Eranin", fsdJump.StarSystem);
            Assert.AreEqual(12.495, fsdJump.JumpDist, 0.001);
            Assert.AreEqual(15.6, fsdJump.FuelLevel, 0.001);
        }

        [TestMethod]
        public void Test_LocationEvent()
        {
            var returned = watcher.FireFakeEventAndReturn(@"{ ""timestamp"":""2017-09-25T12:00:00Z"", ""event"":""Location"", ""StarSystem"":""Shinrarta Dezhra"", ""SystemAddress"":3932277478106, ""StarPos"":[55.719,17.594,27.156], ""Body"":""Shinrarta Dezhra A"", ""BodyID"":1, ""BodyType"":""Star"", ""Docked"":true, ""StationName"":""Jameson Memorial"", ""StationType"":""Orbis"", ""MarketID"":128666762, ""SystemAllegiance"":""Independent"", ""SystemEconomy"":""$economy_HighTech;"", ""SystemEconomy_Localised"":""High Tech"", ""SystemGovernment"":""$government_Democracy;"", ""SystemGovernment_Localised"":""Democracy"", ""SystemSecurity"":""$SYSTEM_SECURITY_high;"", ""SystemSecurity_Localised"":""High Security"", ""Population"":85206935 }");
            var location = returned as LocationEvent.LocationEventArgs;
            Assert.IsNotNull(location);
            Assert.AreEqual("Shinrarta Dezhra", location.StarSystem);
            Assert.IsTrue(location.Docked);
            Assert.AreEqual("Jameson Memorial", location.StationName);
        }

        [TestMethod]
        public void Test_DockedEvent()
        {
            var returned = watcher.FireFakeEventAndReturn(@"{ ""timestamp"":""2017-10-02T10:37:58Z"", ""event"":""Docked"", ""StationName"":""Jameson Memorial"", ""StationType"":""Orbis"", ""StarSystem"":""Shinrarta Dezhra"", ""SystemAddress"":3932277478106, ""MarketID"":128666762, ""StationFaction"":{ ""Name"":""The Pilots Federation"" }, ""StationGovernment"":""$government_Democracy;"", ""StationGovernment_Localised"":""Democracy"", ""StationAllegiance"":""PilotsFederation"", ""StationServices"":[""dock"",""autodock"",""commodities"",""contacts""], ""StationEconomy"":""$economy_HighTech;"", ""StationEconomy_Localised"":""High Tech"", ""DistFromStarLS"":325.1 }");
            var docked = returned as DockedEvent.DockedEventArgs;
            Assert.IsNotNull(docked);
            Assert.AreEqual("Jameson Memorial", docked.StationName);
            Assert.AreEqual("Shinrarta Dezhra", docked.StarSystem);
            Assert.AreEqual("Orbis", docked.StationType);
        }

        [TestMethod]
        public void Test_LoadGameEvent()
        {
            var returned = watcher.FireFakeEventAndReturn(@"{ ""timestamp"":""2017-10-02T10:37:00Z"", ""event"":""LoadGame"", ""Commander"":""HRC1"", ""FID"":""F1234567"", ""Horizons"":true, ""Odyssey"":false, ""Ship"":""CobraMkIII"", ""Ship_Localised"":""Cobra Mk. III"", ""ShipID"":1, ""ShipName"":""Ouroboros"", ""ShipIdent"":""HR-01C"", ""FuelLevel"":16.0, ""FuelCapacity"":16.0, ""GameMode"":""Solo"", ""Credits"":600303, ""Loan"":0 }");
            var loadGame = returned as LoadGameEvent.LoadGameEventArgs;
            Assert.IsNotNull(loadGame);
            Assert.AreEqual("HRC1", loadGame.Commander);
            Assert.AreEqual("CobraMkIII", loadGame.Ship);
            Assert.AreEqual(600303L, loadGame.Credits);
        }

        [TestMethod]
        public void Test_BountyEvent()
        {
            var returned = watcher.FireFakeEventAndReturn(@"{ ""timestamp"":""2017-10-02T10:45:00Z"", ""event"":""Bounty"", ""Rewards"":[ { ""Faction"":""Eranin People's Party"", ""Reward"":2415 } ], ""Target"":""Sidewinder"", ""TotalReward"":2415, ""VictimFaction"":""Eranin People's Party"" }");
            var bounty = returned as BountyEvent.BountyEventArgs;
            Assert.IsNotNull(bounty);
            Assert.AreEqual("Eranin People's Party", bounty.VictimFaction);
            Assert.AreEqual(2415, bounty.TotalReward);
            Assert.HasCount(1, bounty.Rewards);
        }

        [TestMethod]
        public void Test_CarrierJumpEvent()
        {
            var returned = watcher.FireFakeEventAndReturn(@"{ ""timestamp"":""2025-03-19T19:03:12Z"", ""event"":""CarrierJump"", ""Docked"":true, ""StationName"":""V1B-WQB"", ""StationType"":""FleetCarrier"", ""MarketID"":3705689344, ""StarSystem"":""HR 3635"", ""SystemAddress"":1694121347427, ""StarPos"":[64.0625,75.34375,-79.59375], ""SystemAllegiance"":""Alliance"", ""Taxi"":false, ""Multicrew"":false, ""Factions"":[{""Name"":""Flat Galaxy Society"",""FactionState"":""None"",""Government"":""Democracy"",""Influence"":0.698712,""Allegiance"":""Alliance"",""Happiness"":""$Faction_HappinessBand2;"",""Happiness_Localised"":""Happy"",""MyReputation"":100.0}] }");
            var carrierJump = returned as CarrierJumpEvent.CarrierJumpEventArgs;
            Assert.IsNotNull(carrierJump);
            Assert.AreEqual("HR 3635", carrierJump.StarSystem);
            Assert.IsTrue(carrierJump.Docked);
            Assert.AreEqual("V1B-WQB", carrierJump.StationName);
            Assert.HasCount(1, carrierJump.Factions);
            Assert.AreEqual("Flat Galaxy Society", carrierJump.Factions[0].Name);
        }

        [TestMethod]
        public void Test_EngineerProgressEvent_Single()
        {
            var returned = watcher.FireFakeEventAndReturn(@"{ ""timestamp"":""2024-01-01T00:00:00Z"", ""event"":""EngineerProgress"", ""Engineer"":""Felicity Farseer"", ""EngineerID"":300100, ""Progress"":""Unlocked"", ""Rank"":5 }");
            var ep = returned as EngineerProgressEvent.EngineerProgressEventArgs;
            Assert.IsNotNull(ep);
            Assert.AreEqual("Felicity Farseer", ep.Engineer);
            Assert.AreEqual(5, ep.Rank);
            Assert.AreEqual("Unlocked", ep.Progress);
        }

        [TestMethod]
        public void Test_EngineerProgressEvent_List()
        {
            var returned = watcher.FireFakeEventAndReturn(@"{ ""timestamp"":""2024-01-01T00:00:00Z"", ""event"":""EngineerProgress"", ""Engineers"":[{""Engineer"":""Felicity Farseer"",""EngineerID"":300100,""Progress"":""Unlocked"",""Rank"":5},{""Engineer"":""The Dweller"",""EngineerID"":300180,""Progress"":""Known""}] }");
            var ep = returned as EngineerProgressEvent.EngineerProgressEventArgs;
            Assert.IsNotNull(ep);
            Assert.HasCount(2, ep.Engineers);
            Assert.AreEqual("Felicity Farseer", ep.Engineers[0].Engineer);
            Assert.AreEqual("The Dweller", ep.Engineers[1].Engineer);
        }

        [TestMethod]
        public void Test_LoadoutEvent()
        {
            var returned = watcher.FireFakeEventAndReturn(@"{ ""timestamp"":""2024-01-01T00:00:00Z"", ""event"":""Loadout"", ""Ship"":""CobraMkIII"", ""ShipID"":1, ""ShipName"":""Cobra"", ""ShipIdent"":""CB-01"", ""HullValue"":0, ""ModulesValue"":0, ""HullHealth"":1.0, ""UnladenMass"":100.0, ""FuelCapacity"":{""Main"":16.0,""Reserve"":0.41}, ""CargoCapacity"":0, ""MaxJumpRange"":25.53, ""Rebuy"":0, ""Modules"":[{""Slot"":""Armour"",""Item"":""CobraMkIII_Armour_Grade1"",""On"":true,""Priority"":1,""Health"":1.0,""Value"":0}] }");
            var loadout = returned as LoadoutEvent.LoadoutEventArgs;
            Assert.IsNotNull(loadout);
            Assert.AreEqual("CobraMkIII", loadout.Ship);
            Assert.AreEqual(25.53, loadout.MaxJumpRange, 0.001);
            Assert.HasCount(1, loadout.Modules);
            Assert.AreEqual("Armour", loadout.Modules[0].Slot);
        }

        [TestMethod]
        public void Test_AllEvents_CanFireWithMinimalJson()
        {
            var journalEventType = typeof(JournalEvent);
            var eventTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract && !t.IsGenericType && journalEventType.IsAssignableFrom(t));

            foreach (var type in eventTypes)
            {
                var instance = (JournalEvent)Activator.CreateInstance(type);
                foreach (var eventName in instance.EventNames)
                {
                    var json = $@"{{ ""timestamp"": ""2024-01-01T00:00:00Z"", ""event"": ""{eventName}"" }}";
                    var result = watcher.FireFakeEventAndReturn(json);
                    Assert.IsNotNull(result, $"Event '{eventName}' returned null — may not be registered");
                    Assert.AreNotEqual(default(DateTime), result.Timestamp, $"Event '{eventName}' has default Timestamp");
                }
            }
        }
    }
}
