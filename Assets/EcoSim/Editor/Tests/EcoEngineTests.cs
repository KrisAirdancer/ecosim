using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using EcoSim;
using System.Linq;
using static EcoSim.EcoEngine;
using System.IO;

/// <summary>
/// Tests for EcoEngine that are independent of the current simulation settings.
/// </summary>
public class EcoEngineTests
{
    /// <summary>
    /// The name of the simulation we are testing. If there exists a simulation with a same name, its data will be saved and reset after testing.
    /// </summary>
    string simToTest = "sim1";

    /// <summary>
    /// Test the CalcPriceVelocityAtParity method to ensure it works as expected under all situations.
    /// </summary>
    [Test]
    public void EcoEngineTestCalcPriceVelocityAtParity()
    {
        EcoEngine e = new EcoEngine(simToTest);
        Commodity c = new Commodity();
        c.price = 10;
        c.movingAvgDuration = 50;
        c.InitializeMovingAverage();
        c.price = 30;
        Assert.IsTrue(e.CalcPriceVelocityAtParity(c) < 0);
        c.price = 5;
        Assert.IsTrue(e.CalcPriceVelocityAtParity(c) > 0);
        c.price = 10;
        c.inflationRate = 0;
        Assert.IsTrue(e.CalcPriceVelocityAtParity(c) == 0);
        Assert.IsTrue(c.GetMovingAvg() == 10);

        c.inflationRate = 100;
        e.CalcPriceVelocityAtParity(c);
        Assert.IsTrue(c.GetMovingAvg() > 10);
    }

    /// <summary>
    /// Test the CalcDemandVelocity method to ensure it works as expected under all situations.
    /// </summary>
    [Test]
    public void EcoEngineTestCalcDemandVelocity()
    {
        EcoEngine e = new EcoEngine(simToTest);
        Commodity c = new Commodity();
        c.demand = 100;
        c.needLevel = 90;
        Assert.IsTrue(e.CalcDemandVelocity(c) < 0);
        c.needLevel = 200;
        Assert.IsTrue(e.CalcDemandVelocity(c) > 0);
        c.needLevel = 100;
        Assert.IsTrue(e.CalcDemandVelocity(c) == 0);
    }

    /// <summary>
    /// Test the CalcPriceAndSupplyVelocity method to ensure it works as expected under all situations.
    /// </summary>
    [Test]
    public void EcoEngineTestCalcPriceAndSupplyVelocity()
    {
        EcoEngine e = new EcoEngine(simToTest);
        Commodity c = new Commodity();
        c.supply = 100;
        c.demand = 200;
        (double, double) a = e.CalcPriceAndSupplyVelocity(c);
        Assert.IsTrue(a.Item1 > 0 && a.Item2 > 0);
        c.demand = 50;
        a = e.CalcPriceAndSupplyVelocity(c);
        Assert.IsTrue(a.Item1 < 0 && a.Item2 < 0);
        c.price = 10;
        c.movingAvgDuration = 50;
        c.InitializeMovingAverage();
        c.demand = 100;
        a = e.CalcPriceAndSupplyVelocity(c);
        Assert.IsTrue(a.Item1 == 0 && a.Item2 == 0);
    }

    /// <summary>
    /// Tests that the simulation will work for basic things.
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    public IEnumerator EcoEngineTestBasicSimFunction()
    {
        string commoditiesFilePath = Application.dataPath + "/EcoSim/Runtime/Configuration/" + simToTest + "/commodityConfig.json";
        string engineFilePath = Application.dataPath + "/EcoSim/Runtime/Configuration/" + simToTest + "engineConfig.json";
        string commoditiesFile = "";
        string engineFile = "";
        bool commodityNeedsReset = false;
        bool engineNeedsReset = false;
        if (File.Exists(commoditiesFilePath))
        {
            commoditiesFile = File.ReadAllText(commoditiesFilePath);
            commodityNeedsReset = true;
        }
        if (File.Exists(engineFilePath))
        {
            engineFile = File.ReadAllText(engineFilePath);
            engineNeedsReset = true;
        }
        try
        {
            string testCommodities =
            @"[
                {
                    ""name"": ""Commodity_A"",
                    ""price"": 100,
                    ""inflationRate"": 5,
                    ""supply"": 10000,
                    ""demand"": 1000,
                    ""needLevel"": 100,
                    ""movingAvgDuration"": 50

                }
            ]";
            File.WriteAllText(commoditiesFilePath, testCommodities);
            string testEngine =
            @"{
                ""timeStep"": ""0"",
                ""currencyName"": ""US Dollar"",
                ""currencyUnit"": ""dollar"",
                ""currencySymbol"": ""$"",
                ""export"": ""0"",
                ""exportPrice"": ""0"",
                ""exportSupply"": ""0"",
                ""exportDemand"": ""0"",
                ""exportAvgHistory"": ""0"",
                ""exportMinRound"": ""0"",
                ""exportMaxRound"": ""0"",
                ""exportAllCommodities"": ""0""
            }";
            File.WriteAllText(engineFilePath, testEngine);

            EcoEngine e = new EcoEngine(simToTest);
            e.Step();
            EventPiece piece = new EventPiece("Commodity_A", 0, 50, -20, 0, 0);
            List<EventPiece> event1 = new List<EventPiece>();
            event1.Add(piece);
            e.events.Enqueue(event1);
            double initialSup = e.API.GetSupply("Commodity_A");
            double initialDem = e.API.GetDemand("Commodity_A");
            double initialPrice = e.API.GetInitialPrice("Commodity_A");
            for (int i = 0; i < 10; i++)
            {
                e.Step();
            }
            List<double> sup = new List<double>();
            List<double> dem = new List<double>();
            List<double> price = new List<double>();
            for (int i = 0; i < 3; i++)
            {
                double endSup = e.API.GetSupply("Commodity_A");
                double endDem = e.API.GetDemand("Commodity_A");
                double endPrice = e.API.GetCurrentPrice("Commodity_A");
                sup.Add(endSup);
                dem.Add(endDem);
                price.Add(endPrice);
            }
            Assert.AreEqual(sup.Distinct().Count(), 1);
            Assert.AreEqual(dem.Distinct().Count(), 1);
            Assert.AreEqual(price.Distinct().Count(), 1);
        }
        finally
        {
            if (commodityNeedsReset) File.WriteAllText(commoditiesFilePath, commoditiesFile);
            else File.Delete(commoditiesFilePath);
            if (engineNeedsReset) File.WriteAllText(engineFilePath, engineFile);
            else File.Delete(engineFilePath);
        }
        yield return null;
    }
}
