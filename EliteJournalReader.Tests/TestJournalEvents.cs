using System;
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
    }
}
