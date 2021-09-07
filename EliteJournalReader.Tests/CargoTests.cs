using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using EliteJournalReader.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EliteJournalReader.Tests
{
    [TestClass]

    public class CargoTests
    {

        [TestMethod]
        public void Test1_WithInventory()
        {
            CargoEvent.CargoEventArgs args = null;
            // create journal watcher - checking local directory
            FakeJournalWatcher jw = new FakeJournalWatcher(".");

            // create inline eventHandler for CargoEvent, which remembers last eventArgs passed
            jw.GetEvent<CargoEvent>().AddHandler((s, e) => {
                if (s==jw && e.Inventory != null) { args = e; }
            });

            // insert a journal cargoevent with inventory
            jw.ParseText(
                @"{ ""timestamp"":""2020-06-29T21:09:59Z"", ""event"":""Cargo"", ""Vessel"":""Ship"", ""Count"":27, ""Inventory"":[ { ""Name"":""personalweapons"", ""Name_Localised"":""Personal Weapons"", ""Count"":10, ""Stolen"":0 }, { ""Name"":""battleweapons"", ""Name_Localised"":""Battle Weapons"", ""Count"":7, ""Stolen"":0 }, { ""Name"":""drones"", ""Name_Localised"":""Limpet"", ""Count"":10, ""Stolen"":0 } ] }");

            // check that the cargo from inventory was seen
            Assert.IsNotNull(args);
            Assert.IsNotNull(args.Inventory.FirstOrDefault(x =>
                string.Compare(x.Name, "personalweapons", StringComparison.CurrentCultureIgnoreCase) == 0));

        }

        [TestMethod]
        public void Test1_WithOutInventoryPreviousJournal()
        {
            CargoEvent.CargoEventArgs args = null;

            // create journal watcher - checking local directory
            FakeJournalWatcher jw = new FakeJournalWatcher(".",isLive:false);

            // create inline eventHandler for CargoEvent, which remembers last eventArgs passed
            // check sender is jw because tests run in  parallel might receive each others events
            jw.GetEvent<CargoEvent>().AddHandler((s, e) => {
                if (s==jw && e.Inventory != null) { args = e; }
            });

            // insert a journal cargoevent with NO inventory, we are not live so cargo.json will not be read
            jw.ParseText(
                @"{ ""timestamp"":""2020-06-29T21:09:59Z"", ""event"":""Cargo"", ""Vessel"":""Ship"", ""Count"":27 }");

            // check args contain cargo from cargo.json file.

            Assert.IsNull(args);

        }

        
        [TestMethod]
        public void Test1_WithOutInventoryLive()
        {
            CargoEvent.CargoEventArgs args = null;

            // create journal watcher - checking local directory
            FakeJournalWatcher jw = new FakeJournalWatcher(".",isLive:true);
            
            // create inline eventHandler for CargoEvent, which remembers last eventArgs passed
            // check sender is jw because tests run in parallel might receive each others events
            jw.GetEvent<CargoEvent>().AddHandler((s, e) => {
                if (e.Inventory != null) { args = e; }
            });

            // insert a journal cargoevent with NO inventory, forcing the watcher to load the cargo.json file
            jw.ParseText(
                @"{ ""timestamp"":""2020-06-29T21:09:59Z"", ""event"":""Cargo"", ""Vessel"":""Ship"", ""Count"":27 }");

            // check args contain cargo from cargo.json file.

            Assert.IsNotNull(args);
            Assert.IsNotNull(args.Inventory);
            Assert.IsNotNull(args.Inventory.FirstOrDefault(x =>
                string.Compare(x.Name, "progenitorcells", StringComparison.CurrentCultureIgnoreCase) == 0));
        }


    }
}