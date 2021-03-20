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
            JournalWatcher jw = new JournalWatcher(".");

            // create inline eventHandler for CargoEvent, which remembers last eventArgs passed
            jw.GetEvent<CargoEvent>().AddHandler((s, e) => { args = e; });

            // insert a journal cargoevent with inventory
            jw.ParseText(
                @"{ ""timestamp"":""2020-06-29T21:09:59Z"", ""event"":""Cargo"", ""Vessel"":""Ship"", ""Count"":27, ""Inventory"":[ { ""Name"":""personalweapons"", ""Name_Localised"":""Personal Weapons"", ""Count"":10, ""Stolen"":0 }, { ""Name"":""battleweapons"", ""Name_Localised"":""Battle Weapons"", ""Count"":7, ""Stolen"":0 }, { ""Name"":""drones"", ""Name_Localised"":""Limpet"", ""Count"":10, ""Stolen"":0 } ] }");

            // check that the cargo from inventory was seen
            Assert.IsNotNull(args);
            Assert.IsNotNull(args.Inventory.FirstOrDefault(x =>
                string.Compare(x.Name, "personalweapons", StringComparison.CurrentCultureIgnoreCase) == 0));

        }

        [TestMethod]
        public void Test1_WithOutInventory()
        {
            CargoEvent.CargoEventArgs args = null;

            // create journal watcher - checking local directory
            JournalWatcher jw = new JournalWatcher(".");

            // create inline eventHandler for CargoEvent, which remembers last eventArgs passed
            jw.GetEvent<CargoEvent>().AddHandler((s, e) => { args = e; });

            // insert a journal cargoevent with NO inventory, forcing the watcher to load the cargo.json file
            jw.ParseText(
                @"{ ""timestamp"":""2020-06-29T21:09:59Z"", ""event"":""Cargo"", ""Vessel"":""Ship"", ""Count"":27 }");

            // check args contain cargo from cargo.json file.

            Assert.IsNotNull(args);
            Assert.IsNotNull(args.Inventory.FirstOrDefault(x =>
                string.Compare(x.Name, "progenitorcells", StringComparison.CurrentCultureIgnoreCase) == 0));
        }

        


    }
}