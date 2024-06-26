using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using UnityEngine;

namespace EcoSim
{
    /// <summary>
    /// The primary part of the simulation, on creation starts tracking all of the commodities in the corresponding JSON file,
    /// then updates the values of those commodities every round, with the round duration being set by the EngineConfig.json file.
    /// It will have a connected DataAPI that can be used to get the values of each commodity managed by the simulation.
    /// </summary>
    public partial class EcoEngine
    {
        /// <summary>
        /// The corresponding DataAPI used to provide information about the simulation to the game and visualizer.
        /// </summary>
        public DataAPI API;

        /// <summary>
        /// Used to add a bit of variety and range to the price of each commodity.
        /// </summary>
        private Vector2 priceRangePercent = new Vector2(0.9f, 1.1f);

        /// <summary>
        /// How many rounds the simulation has been running for.
        /// </summary>
        private int roundCount = 0;
        /// <summary>
        /// The fields parsed from EngineConfig.json, the most important being the duration of each round - timeStep
        /// </summary>
        private Dictionary<string, string> configurations;

        /// <summary>
        /// A hash set of all commodities tracked and updated by the simulation
        /// </summary>
        private HashSet<Commodity> commodities;

        /// <summary>
        /// A queue of any events that are to be run as soon as possible
        /// </summary>
        public Queue<List<EventPiece>> events = new Queue<List<EventPiece>>();

        /// <summary>
        /// The location of the commodityConfig.json file that this EcoEngine will read from
        /// </summary>
        private string commoditiesFilePath;

        /// <summary>
        /// The location of the EngineConfig.json file that this EcoEngine will read from
        /// </summary>
        private string configFilePath;
        private string exportCommoditiesFile;

        /// <summary>
        /// Used to make sure the simulation updates based off of real time, not based off of frames.
        /// </summary>
        private static Stopwatch watch = new Stopwatch();

        /// <summary>
        /// The time that still needs to pass before this round of the simulation can run
        /// </summary>
        private double remainingTickTime = 0;

        /// <summary>
        /// Value to reset remainingTickTime to once a round runs.
        /// </summary>
        private double tickTime;

        /// <summary>
        /// Adds a new commodity to be tracked and updated by the simulation and the DataAPI, if it does not already exist
        /// </summary>
        /// <param name="commodity">The new commodity to be tracked</param>
        /// <returns>True if the commodity was successfully added to the simulation and DataAPI, false otherwise.</returns>

        /**
         * Data export values
         * Default is to not export data. 
         * If we do export data, do so between MinRound and MaxRound of trading.
         * 
         * MaxRound = -1 indicates all rounds from MinRound to termination of the simulation.
         */
        private string simulationName;
        private string exportFilePath;
        private bool exportData = false;
        private bool exportPrice = false;
        private bool exportSupply = false;
        private bool exportDemand = false;
        private bool exportAvgHistory = false;
        private int exportMinRound = 0;
        private int exportMaxRound = 0;
        private HashSet<string> exportList;

        public bool TryHotAddCommodity(Commodity commodity)
        {
            Utils.Log($"Trying to add Commodity: {commodity.name}");
            if (commodities.Any(value => value.name == commodity.name))
            {
                return false;

            }
            else
            {
                commodities.Add(commodity);
                API.TrackNewCommodity(commodity);
                return true;
            }
        }


        /// <summary>
        /// If enough time has passed so that the simulation is supposed to have done a round, runs a round of the engine, otherwise just updates how much time needs
        /// to pass before a round should occur.
        /// </summary>
        /// <returns>True if a simulation round occurred, false otherwise.</returns>
        public bool Step()
        {
            bool result = false;
            if (remainingTickTime >= 0)
            {
                remainingTickTime -= (Time.deltaTime * 1000);
            }
            else
            {
                // Export data
                if (exportData && exportList != null)
                {
                    if (exportMinRound <= roundCount && (exportMaxRound == -1 ^ exportMaxRound >= roundCount))
                    {
                        ExportData();
                    }
                }
                watch.Restart();

                remainingTickTime = tickTime;
                EngineStep();
                result = true;

                watch.Stop();
                remainingTickTime -= watch.ElapsedMilliseconds;
            }
            return result;
        }

        /// <summary>
        /// Runs one cycle of the engine, which entails updating the price, supply, demand, and each of their velocities, and running any events.
        /// Also prints some information about what occurred this round.
        /// </summary>
        public void EngineStep()
        {
            tickTime = Int32.Parse(this.configurations["timeStep"]);

            // +++++ UPDATE PRICES +++++

            //Supply velocity is how the supply is changing, positive means an increase in supply, negative means decrease
            //Based off of the velocity, we change the prices, supply, and demand
            foreach (Commodity commodity in this.commodities)
            {
                // Update velocity metrics.
                (double, double) priceAndDemandVelocities = CalcPriceAndSupplyVelocity(commodity);
                commodity.priceVelocity = priceAndDemandVelocities.Item1;
                commodity.supplyVelocity = priceAndDemandVelocities.Item2;
                commodity.demandVelocity = CalcDemandVelocity(commodity);

                // Update commodity data.
                double endSellPrice = commodity.price + (commodity.priceVelocity * 100.0);
                endSellPrice *= UnityEngine.Random.Range(priceRangePercent.x, priceRangePercent.y);
                commodity.SetPrice((int)Math.Round(endSellPrice));
                commodity.supply += (int)(commodity.supplyVelocity * 100.0);
                commodity.demand += (int)(commodity.demandVelocity * 100.0);

                commodity.UpdateMovingAvg(commodity.price);
            }

            Dictionary<string, Commodity> commodityReference = new Dictionary<string, Commodity>();
            foreach (Commodity commodity in this.commodities)
            {
                commodityReference.Add(commodity.name, commodity);
            }

            //Run through each event and do the change, if any
            for (int i = 0; i < this.events.Count; i++) 
            {
                List<EventPiece> individualEvent = this.events.Dequeue();
                foreach (EventPiece eventPiece in individualEvent)
                {
                    commodityReference[eventPiece.commodity].inflationRate += eventPiece.inflationShift;
                    commodityReference[eventPiece.commodity].supply += eventPiece.supplyShift;
                    commodityReference[eventPiece.commodity].demand += eventPiece.demandShift;
                    commodityReference[eventPiece.commodity].needLevel += eventPiece.needShift;
                    commodityReference[eventPiece.commodity].movingAvgDuration += eventPiece.movingAvgDurationShift;

                }
            }

            roundCount++;
            PrintSimRoundData();

        }

        /// <summary>
        /// Initialized the simulation, which includes reading in the commodities to track and update and reading in the engine settings, 
        /// </summary>
        /// <param name="sim">The name of the simulation that this EcoEngine is tracking. The simulation will be a directory name that contains the config files</param>
        /// <exception cref="Exception"></exception>
        public EcoEngine(string sim)
        {
            commoditiesFilePath = Application.dataPath + "/EcoSim/Runtime/Configuration/" + sim + "/commodityConfig.json";
            configFilePath = Application.dataPath + "/EcoSim/Runtime/Configuration/engineConfig.json";
            exportCommoditiesFile = Application.dataPath + "/EcoSim/Runtime/Configuration/" + sim + "/commodityExportList.json";
            simulationName = sim;


            Utils.Log("\r\n#######                #####           \r\n#        ####   ####  #     # # #    # \r\n#       #    # #    # #       # ##  ## \r\n#####   #      #    #  #####  # # ## # \r\n#       #      #    #       # # #    # \r\n#       #    # #    # #     # # #    # \r\n#######  ####   ####   #####  # #    # \r\n");
            Utils.Log("Welcome to EcoSim!\n");
            Utils.Log("v1.0.0\n");

            Utils.Log("Initializing simulation model...");
            try
            {
                string configJSON = File.ReadAllText(this.configFilePath);
                this.configurations = JsonSerializer.Deserialize<Dictionary<string, string>>(configJSON);

                string commoditiesJSON = File.ReadAllText(this.commoditiesFilePath);
                this.commodities = JsonSerializer.Deserialize<HashSet<Commodity>>(commoditiesJSON);
                if(File.Exists(exportCommoditiesFile))
                {
                    string exportCommoditiesJSON = File.ReadAllText(this.exportCommoditiesFile);
                    this.exportList = JsonSerializer.Deserialize<HashSet<string>>(exportCommoditiesJSON);

                }

                // Finish initializing commodity objects.
                foreach (Commodity commodity in this.commodities)
                {
                    commodity.InitializeMovingAverage();
                    commodity.initialPrice = commodity.price;
                    commodity.SetHistoricalMeanPrice(commodity.price);
                }

                // Initialize DataAPI
                this.API = new DataAPI(this);

                tickTime = Int32.Parse(this.configurations["timeStep"]);
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to initialize simulation model {e}");
            }

            exportData = configurations["export"].Equals("1");
            exportPrice = configurations["exportPrice"].Equals("1");
            exportSupply = configurations["exportSupply"].Equals("1");
            exportDemand = configurations["exportDemand"].Equals("1");
            exportAvgHistory = configurations["exportAvgHistory"].Equals("1");
            exportMinRound = Int32.Parse(configurations["exportMinRound"]);
            exportMaxRound = Int32.Parse(configurations["exportMaxRound"]);

            // Since in setup, should only be set once, so the date/time value will remain the same throughout the run
            exportFilePath = Application.dataPath + "/EcoSim/Editor/Data/" + sim + "-" + DateTime.Now.ToString("MM-dd-yyyy- HH-mm-ss tt") + ".csv";

            Utils.Log("Simulation model initialized successfully!");
            Utils.Log("The simulation is running...\n");        
        }

        /// <summary>
        /// Determining what the price velocity should be when supply equals demand, if the current price is greater than the moving average price, 
        /// it should be negative, if the price is less than the moving average price, the velocity should be positive, if the price
        /// is equal to the moving average price, the velocity should be zero.
        /// </summary>
        /// <param name="commodity">The commodity which is having its price velocity updated.</param>
        /// <returns>The new price velocity for this commodity</returns>
        public double CalcPriceVelocityAtParity(Commodity commodity)
        {
            double priceVelocity = commodity.priceVelocity;

            if (commodity.price > commodity.GetMovingAvg())
            { // We need a negative priceVelocity.
                priceVelocity = MakeVelocityNegativeAndScale(priceVelocity, 1.5, -0.5);
                priceVelocity = Math.Min(priceVelocity, -0.01);
            }
            else if (commodity.price < commodity.GetMovingAvg())
            { // We need a positive price velocity.
                priceVelocity = MakeVelocityPositiveAndScale(priceVelocity, 1.5, -0.5);
                priceVelocity = Math.Max(priceVelocity, 0.01);
            }
            else
            {
                priceVelocity = 0;

                if (commodity.inflationRate != 0)
                {
                    double inflationAdjustment = 1 + (commodity.inflationRate / 100);
                    int inflatedPrice = (int)(commodity.price * inflationAdjustment);
                    commodity.UpdateMovingAvg(inflatedPrice);
                }
            }

            return priceVelocity;

        }

        /// <summary>
        /// Calculates the demand velocity based off of the difference of the demand from the need level of the commodity. If demand is greater than the need level, the velocity will be negative,
        /// if demand is less than the need level, velocity will be positive, otherwise it will be zero.
        /// </summary>
        /// <param name="commodity">The commodity which is having its demand velocity updated.</param>
        /// <returns>The new demand velocity for this commodity/returns>
        public double CalcDemandVelocity(Commodity commodity)
        {
            double demandVelocity = commodity.demandVelocity;

            if (commodity.demand > commodity.needLevel)
            {
                demandVelocity = MakeVelocityNegativeAndScale(demandVelocity, 1.5, -0.5);
                demandVelocity = Math.Min(demandVelocity, -0.01);
            }
            else if (commodity.demand < commodity.needLevel)
            {
                demandVelocity = MakeVelocityPositiveAndScale(demandVelocity, 1.5, -0.5);
                demandVelocity = Math.Max(demandVelocity, 0.01);
            }
            else
            {
                demandVelocity = 0;
            }

            return demandVelocity;
        }

        /// <summary>
        /// Calculates the price and supply velocities based off of the difference of the supply from the demand of the commodity. If supply is greater than the demand, the supply velocity will be negative
        /// and the price velocity will also be negative because there is more supply of the commodity than demand for it. If supply is less than the demand, the supply velocity will be positive
        /// and the price velocity will also be positive because there is less supply of the commodity than demand for it. Otherwise the supply velocity will be zero, but the price velocity will be calculated by CalcPriceVelocityAtParity
        /// </summary>
        /// <param name="commodity">The commodity which is having its demand velocity updated.</param>
        /// <returns>The new supply and price velocities for this commodity</returns>
        public (double, double) CalcPriceAndSupplyVelocity(Commodity commodity)
        {
            double priceVelocity = commodity.priceVelocity;
            double supplyVelocity = commodity.supplyVelocity;

            if (commodity.supply < commodity.demand) // Demand is greater than supply.
            {   // Price should increase b/c demand is greater than supply.
                priceVelocity = MakeVelocityPositiveAndScale(priceVelocity, 0.15, -0.15);
                supplyVelocity = MakeVelocityPositiveAndScale(supplyVelocity, 1.5, -0.5);

                priceVelocity = Math.Max(priceVelocity, 0.01);
                supplyVelocity = Math.Max(supplyVelocity, 0.01);
            }
            else if (commodity.supply > commodity.demand) // Demand is less than supply.
            {   // Price should decrease b/c demand is less than supply.
                priceVelocity = MakeVelocityNegativeAndScale(priceVelocity, 0.15, -0.15);
                supplyVelocity = MakeVelocityNegativeAndScale(supplyVelocity, 1.5, -0.5);

                priceVelocity = Math.Min(priceVelocity, -0.01);
                supplyVelocity = Math.Min(supplyVelocity, -0.01);
            }
            else // Demand equals supply.
            {
                priceVelocity = CalcPriceVelocityAtParity(commodity);
                // Setting velocities to zero prevents price creep.
                supplyVelocity = 0;
            }

            return (priceVelocity, supplyVelocity);
        }

        /// <summary>
        /// Given a price, supply, or demand velocity, ensures it will be negative, and scales it. If the velocity is already negative
        /// it will be multiplied by the retentionDelta, otherwise it will be multiplied by the inversionDelta
        /// </summary>
        /// <param name="velocity">The current velocity of the commodity</param>
        /// <param name="retentionDelta">The value to scale the velocity by if the velocity is already negative, the scaling value should be a positive value</param>
        /// <param name="inversionDelta">The value to scale the velocity by if the velocity is not negative, the scaling value should be a negative value</param>
        /// <returns>The scaled velocity</returns>
        public double MakeVelocityNegativeAndScale(double velocity, double retentionDelta, double inversionDelta)
        {
            if (velocity < 0)
            {
                velocity *= retentionDelta;
            }
            else
            {
                velocity *= inversionDelta;
            }

            return velocity;
        }

        /// <summary>
        /// Given a price, supply, or demand velocity, ensures it will be positive, and scales it. If the velocity is already positive
        /// it will be multiplied by the retentionDelta, otherwise it will be multiplied by the inversionDelta
        /// </summary>
        /// <param name="velocity">The current velocity of the commodity</param>
        /// <param name="retentionDelta">The value to scale the velocity by if the velocity is already positive, the scaling value should be a positive value</param>
        /// <param name="inversionDelta">The value to scale the velocity by if the velocity is not positive, the scaling value should be a negative value</param>
        /// <returns></returns>
        public double MakeVelocityPositiveAndScale(double velocity, double retentionDelta, double inversionDelta)
        {
            if (velocity > 0)
            {
                velocity *= retentionDelta;
            }
            else
            {
                velocity *= inversionDelta;
            }

            return velocity;
        }

        /***** PRINT FUNCTIONS *****/

        /// <summary>
        /// Logs information about the price, supply, and demand
        /// </summary>
        private void PrintSimRoundData()
        {
            Utils.Log($"+++++ ROUND {roundCount} +++++");
            Utils.Log();

            foreach (Commodity commodity in this.commodities)
            {
                Utils.Log($"+++++ {commodity.name} +++++");
                Utils.Log($"Price:           {commodity.price}");
                Utils.Log($"Price (Max/Min): {commodity.GetHistoricalMaxPrice()} / {commodity.GetHistoricalMinPrice()}");
                Utils.Log($"Moving Avg:      {commodity.GetMovingAvg()}");
                Utils.Log($"Price Velocity:  {commodity.priceVelocity}");
                Utils.Log($"Supply Velocity: {commodity.supplyVelocity}");
                Utils.Log($"Demand Velocity: {commodity.demandVelocity}");
                Utils.Log($"Supply: {commodity.supply}");
                Utils.Log($"Demand: {commodity.demand}");

                Utils.Log(); // For spacing
            }
        }

        /// <summary>
        /// Collects all the information displayed in the visualizer as a single List of RowData
        /// </summary>
        /// <returns>A list of RowData that contains information about all the commodities in the simulation.</returns>
        public List<RowData> GetEconomicState()
        {
            List<RowData> results = new List<RowData>();
            foreach (Commodity commod in this.commodities)
            {
                string name = commod.name;
                int lower = commod.GetHistoricalMinPrice();
                int upper = commod.GetHistoricalMaxPrice();
                int hmp = commod.GetHistoricalMeanPrice();
                int supply = (int)commod.supply;
                int demand = (int)commod.demand;
                RowData data = new RowData(name, commod.price, hmp, lower, upper, supply, demand);

                results.Add(data);
            }

            return results;
        }

        /// <summary>
        /// Checks if we are exporting data, and exports the data as applicable
        /// 
        /// If exporting all data, the csv should look like:
        /// Simulation, currentRound, Commodity, PriceThisRound, SupplyThisRound, DemandThisRound, HistoricalAverage, EarliestRoundIntoHistAvg, NumberOfRoundsFeedingIntoHistAvg\n
        /// </summary>
        private void ExportData()
        {
            foreach (Commodity c in commodities)
            {
                // If the commodity is not in the export list, ignore it
                if (!exportList.Contains(c.name))
                {
                    continue;
                }


                StringBuilder lineItem = new StringBuilder();
                lineItem.Append(simulationName + "," + roundCount.ToString() + "," + c.name);

                if (exportPrice)
                {
                    lineItem.Append("," + c.price);
                }

                if (exportSupply)
                {
                    lineItem.Append("," + c.supply);
                }

                if (exportDemand)
                {
                    lineItem.Append("," + c.demand);
                }

                if (exportAvgHistory)
                {
                    int minHistoricRound = roundCount - c.movingAvgDuration;
                    minHistoricRound = (minHistoricRound < 0) ? 0 : minHistoricRound;

                    lineItem.Append("," + c.GetHistoricalMeanPrice().ToString());
                    lineItem.Append("," + minHistoricRound);
                    lineItem.Append("," + (roundCount - minHistoricRound).ToString());
                }

                lineItem.Append(Environment.NewLine);

                File.AppendAllText(exportFilePath, lineItem.ToString());
            }
        }

    }
}

/// <summary>
/// Used to log various information like a print statement
/// </summary>
public class Utils : MonoBehaviour
{
    /// <summary>
    /// Logs the message, and adds a newline
    /// </summary>
    /// <param name="message">The message to log</param>
    public static void Log(string message = "")
    { 
        UnityEngine.Debug.Log(message + "\n");
    }
}

