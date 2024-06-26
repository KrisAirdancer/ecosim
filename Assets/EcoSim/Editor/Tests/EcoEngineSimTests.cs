using System.Collections;
using System.Collections.Generic;
using System.IO;
using EcoSim;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Does some testing for abnormalities in the simulation currently being tested. Any changes made to the simulation settings
/// for testing purposes will be undone at the end of each test.
/// </summary>
public class EcoEngineSimTests
{
    //Change this to change which simulation is being tested
    /// <summary>
    /// The name of the simulation to test
    /// </summary>
    string simToTest = "sim1";

    /// <summary>
    /// Tests to see if any commodity in the simulation has a negative price in 100 rounds.
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    public IEnumerator EcoEngineSimTestsNoCommodityNegativePriceIn100Rounds()
    {
        string engineFilePath = Application.dataPath + "/EcoSim/Runtime/Configuration/engineConfig.json";
        string engineFile = "";
        bool engineNeedsReset = false;
        if (File.Exists(engineFilePath))
        {
            engineFile = File.ReadAllText(engineFilePath);
            engineNeedsReset = true;
        }
        try
        {
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
            List<string> commodities = e.API.GetCommodityNames();
            for (int i = 0; i < 100; i++)
            {
                e.Step();
                foreach (string commodity in commodities)
                {
                    Assert.IsTrue(e.API.GetCurrentPrice(commodity) > 0);
                }
            }
        }
        finally
        {
            
            if (engineNeedsReset) File.WriteAllText(engineFilePath, engineFile);
            else File.Delete(engineFilePath);
        }
        yield return null;
    }

    /// <summary>
    /// Tests to see if any commodity in the simulation has a negative price in 1000 rounds.
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    public IEnumerator EcoEngineSimTestsNoCommodityNegativePriceIn1000Rounds()
    {
        string engineFilePath = Application.dataPath + "/EcoSim/Runtime/Configuration/engineConfig.json";
        string engineFile = "";
        bool engineNeedsReset = false;
        if (File.Exists(engineFilePath))
        {
            engineFile = File.ReadAllText(engineFilePath);
            engineNeedsReset = true;
        }
        try
        {
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
            List<string> commodities = e.API.GetCommodityNames();
            for (int i = 0; i < 1000; i++)
            {
                e.Step();
                foreach (string commodity in commodities)
                {
                    Assert.IsTrue(e.API.GetCurrentPrice(commodity) > 0);
                }
            }
        }
        finally
        {

            if (engineNeedsReset) File.WriteAllText(engineFilePath, engineFile);
            else File.Delete(engineFilePath);
        }
        yield return null;
    }

    /// <summary>
    /// Tests to see if any commodity in the simulation has a negative price in 10000 rounds.
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    public IEnumerator EcoEngineSimTestsNoCommodityNegativePriceIn10000Rounds()
    {
        string engineFilePath = Application.dataPath + "/EcoSim/Runtime/Configuration/engineConfig.json";
        string engineFile = "";
        bool engineNeedsReset = false;
        if (File.Exists(engineFilePath))
        {
            engineFile = File.ReadAllText(engineFilePath);
            engineNeedsReset = true;
        }
        try
        {
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
            List<string> commodities = e.API.GetCommodityNames();

            for (int i = 0; i < 10000; i++)
            {
                e.Step();
                foreach (string commodity in commodities)
                {
                    Assert.IsTrue(e.API.GetCurrentPrice(commodity) > 0);
                }
            }
        }
        finally
        {

            if (engineNeedsReset) File.WriteAllText(engineFilePath, engineFile);
            else File.Delete(engineFilePath);
        }
        yield return null;
    }


    /// <summary>
    /// Tests to see if any commodity in the simulation has a negative supply in 100 rounds.
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    public IEnumerator EcoEngineSimTestsNoCommodityNegativeSupplyIn100Rounds()
    {
        string engineFilePath = Application.dataPath + "/EcoSim/Runtime/Configuration/engineConfig.json";
        string engineFile = "";
        bool engineNeedsReset = false;
        if (File.Exists(engineFilePath))
        {
            engineFile = File.ReadAllText(engineFilePath);
            engineNeedsReset = true;
        }
        try
        {
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
            List<string> commodities = e.API.GetCommodityNames();

            for (int i = 0; i < 100; i++)
            {
                e.Step();
                foreach (string commodity in commodities)
                {
                    Assert.IsTrue(e.API.GetSupply(commodity) > 0);
                }
            }
        }
        finally
        {

            if (engineNeedsReset) File.WriteAllText(engineFilePath, engineFile);
            else File.Delete(engineFilePath);
        }
        yield return null;
    }

    /// <summary>
    /// Tests to see if any commodity in the simulation has a negative supply in 1000 rounds.
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    public IEnumerator EcoEngineSimTestsNoCommodityNegativeSupplyIn1000Rounds()
    {
        string engineFilePath = Application.dataPath + "/EcoSim/Runtime/Configuration/engineConfig.json";
        string engineFile = "";
        bool engineNeedsReset = false;
        if (File.Exists(engineFilePath))
        {
            engineFile = File.ReadAllText(engineFilePath);
            engineNeedsReset = true;
        }
        try
        {
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
            List<string> commodities = e.API.GetCommodityNames();
            for (int i = 0; i < 1000; i++)
            {
                e.Step();
                foreach (string commodity in commodities)
                {
                    Assert.IsTrue(e.API.GetSupply(commodity) > 0);
                }
            }
        }
        finally
        {

            if (engineNeedsReset) File.WriteAllText(engineFilePath, engineFile);
            else File.Delete(engineFilePath);
        }
        yield return null;
    }

    /// <summary>
    /// Tests to see if any commodity in the simulation has a negative supply in 10000 rounds.
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    public IEnumerator EcoEngineSimTestsNoCommodityNegativeSupplyIn10000Rounds()
    {
        string engineFilePath = Application.dataPath + "/EcoSim/Runtime/Configuration/engineConfig.json";
        string engineFile = "";
        bool engineNeedsReset = false;
        if (File.Exists(engineFilePath))
        {
            engineFile = File.ReadAllText(engineFilePath);
            engineNeedsReset = true;
        }
        try
        {
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
            List<string> commodities = e.API.GetCommodityNames();
            for (int i = 0; i < 10000; i++)
            {
                e.Step();
                foreach (string commodity in commodities)
                {
                    Assert.IsTrue(e.API.GetSupply(commodity) > 0);
                }
            }
        }
        finally
        {

            if (engineNeedsReset) File.WriteAllText(engineFilePath, engineFile);
            else File.Delete(engineFilePath);
        }
        yield return null;
    }

    /// <summary>
    /// Tests to see if any commodity in the simulation has a negative demand in 100 rounds.
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    public IEnumerator EcoEngineSimTestsNoCommodityNegativeDemandIn100Rounds()
    {
        string engineFilePath = Application.dataPath + "/EcoSim/Runtime/Configuration/engineConfig.json";
        string engineFile = "";
        bool engineNeedsReset = false;
        if (File.Exists(engineFilePath))
        {
            engineFile = File.ReadAllText(engineFilePath);
            engineNeedsReset = true;
        }
        try
        {
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
            List<string> commodities = e.API.GetCommodityNames();

            for (int i = 0; i < 100; i++)
            {
                e.Step();
                foreach (string commodity in commodities)
                {
                    Assert.IsTrue(e.API.GetDemand(commodity) > 0);
                }
            }
        }
        finally
        {

            if (engineNeedsReset) File.WriteAllText(engineFilePath, engineFile);
            else File.Delete(engineFilePath);
        }
        yield return null;
    }


    /// <summary>
    /// Tests to see if any commodity in the simulation has a negative demand in 1000 rounds.
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    public IEnumerator EcoEngineSimTestsNoCommodityNegativeDemandIn1000Rounds()
    {
        string engineFilePath = Application.dataPath + "/EcoSim/Runtime/Configuration/engineConfig.json";
        string engineFile = "";
        bool engineNeedsReset = false;
        if (File.Exists(engineFilePath))
        {
            engineFile = File.ReadAllText(engineFilePath);
            engineNeedsReset = true;
        }
        try
        {
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
            List<string> commodities = e.API.GetCommodityNames();
            for (int i = 0; i < 1000; i++)
            {
                e.Step();
                foreach (string commodity in commodities)
                {
                    Assert.IsTrue(e.API.GetDemand(commodity) > 0);
                }
            }
        }
        finally
        {

            if (engineNeedsReset) File.WriteAllText(engineFilePath, engineFile);
            else File.Delete(engineFilePath);
        }
        yield return null;
    }

    /// <summary>
    /// Tests to see if any commodity in the simulation has a negative demand in 10000 rounds.
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    public IEnumerator EcoEngineSimTestsNoCommodityNegativeDemandIn10000Rounds()
    {
        string engineFilePath = Application.dataPath + "/EcoSim/Runtime/Configuration/engineConfig.json";
        string engineFile = "";
        bool engineNeedsReset = false;
        if (File.Exists(engineFilePath))
        {
            engineFile = File.ReadAllText(engineFilePath);
            engineNeedsReset = true;
        }
        try
        {
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
            List<string> commodities = e.API.GetCommodityNames();
            for (int i = 0; i < 10000; i++)
            {
                e.Step();
                foreach (string commodity in commodities)
                {
                    Assert.IsTrue(e.API.GetDemand(commodity) > 0);
                }
            }
        }
        finally
        {

            if (engineNeedsReset) File.WriteAllText(engineFilePath, engineFile);
            else File.Delete(engineFilePath);
        }
        yield return null;
    }
}
