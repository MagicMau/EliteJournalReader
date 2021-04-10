using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using EliteJournalReader.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EliteJournalReader.Tests
{
    [TestClass]

    public class MarketTests
    {

        [TestMethod]
        public void Test1_WithItems()
        {
            MarketEvent.MarketEventArgs args = null;
            // create journal watcher - checking local directory
            FakeJournalWatcher jw = new FakeJournalWatcher(".");

            // create inline eventHandler for CargoEvent, which remembers last eventArgs passed with price-data
            // check sender is jw because tests run in  parallel might receive each others events
            jw.GetEvent<MarketEvent>().AddHandler((s, e) => { if(s==jw && e.Items!=null) args = e; });

            // insert a journal marketdata event with price-data
            jw.ParseText(
                @"{ ""timestamp"":""2020-06-28T22:18:32Z"", ""event"":""Market"", ""MarketID"":3225246976, ""StationName"":""Shen Terminal"", ""StationType"":""Coriolis"", ""StarSystem"":""Guuguyni"" , ""Items"":[ 
            { ""id"":128049152, ""Name"":""local_name"", ""Name_Localised"":""Platinum"", ""Category"":""$MARKET_category_metals;"", ""Category_Localised"":""Metals"", ""BuyPrice"":0, ""SellPrice"":26134, ""MeanPrice"":58247, ""StockBracket"":0, ""DemandBracket"":3, ""Stock"":0, ""Demand"":31, ""Consumer"":true, ""Producer"":false, ""Rare"":false } ] }",false);

            // check that the cargo from inventory was seen
            Assert.IsNotNull(args);
            Assert.IsNotNull(args.Items);
            Assert.IsNotNull(args.Items.FirstOrDefault(x =>
                string.Compare(x.Name, "local_name", StringComparison.CurrentCultureIgnoreCase) == 0));

        }

        [TestMethod]
        public void Test1_WithOutItemsPreviousJournal()
        {
            MarketEvent.MarketEventArgs args = null;

            // create journal watcher - checking local directory
            FakeJournalWatcher jw = new FakeJournalWatcher(".",isLive:false);

            // create inline eventHandler for CargoEvent, which remembers last eventArgs passed with price-data
            // check sender is jw because tests run in  parallel might receive each others events
            jw.GetEvent<MarketEvent>().AddHandler((s, e) => { if(s==jw && e.Items!=null) args = e; });

            // insert a journal market-data event with NO price data, we are not live the market.json file will be ignored
            jw.ParseText(
                @"{ ""timestamp"":""2020-06-28T22:18:32Z"", ""event"":""Market"", ""MarketID"":3225246976, ""StationName"":""Shen Terminal"", ""StationType"":""Coriolis"", ""StarSystem"":""Guuguyni"" }");

            // check args are null 
            Assert.IsNull(args);
            
        }

        [TestMethod]
        public void Test1_WithOutItemsLive()
        {
            MarketEvent.MarketEventArgs args = null;

            // create journal watcher - checking local directory
            FakeJournalWatcher jw = new FakeJournalWatcher(".",isLive:true);

            // create inline eventHandler for CargoEvent, which remembers last eventArgs passed with price-data
            // check sender is jw because tests run in  parallel might receive each others events
            jw.GetEvent<MarketEvent>().AddHandler((s, e) => { if(s==jw && e.Items!=null) args = e; });

            // insert a journal market-data event with NO price information, forcing the watcher to load the market.json file
            jw.ParseText(
                @"{ ""timestamp"":""2020-06-28T22:18:32Z"", ""event"":""Market"", ""MarketID"":3225246976, ""StationName"":""Shen Terminal"", ""StationType"":""Coriolis"", ""StarSystem"":""Guuguyni"" }");

            // check args contain prices from market.json file.
            Assert.IsNotNull(args);
            Assert.IsNotNull(args.Items);
            Assert.IsNotNull(args.Items.FirstOrDefault(x =>
                string.Compare(x.Name, "$platinum_name;", StringComparison.CurrentCultureIgnoreCase) == 0));
        }




    }
}