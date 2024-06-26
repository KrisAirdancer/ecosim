using System.Collections.Generic;

namespace EcoSim
{
    partial class EcoEngine
    {
        /// <summary>
        /// DataAPI is used to provide information about the simulation and its commodities to the game, and to the visualizer.
        /// </summary>
        public class DataAPI
        {
            /// <summary>
            /// The EcoEngine to which this DataAPI is attached
            /// </summary>
            private EcoEngine engine;
            /// <summary>
            /// All of the commodities tracked and updated by the simulation
            /// </summary>
            private Dictionary<string, Commodity> commodities = new Dictionary<string, Commodity>();

            /// <summary>
            /// Sets this.engine equal to "engine", and fills this.commodities with all the commodities in "engine."
            /// </summary>
            /// <param name="engine">The EcoEngine instance that this DataAPI should track.</param>
            public DataAPI(EcoEngine engine)
            {
                this.engine = engine;

                foreach (Commodity commodity in engine.commodities)
                {
                    this.commodities.Add(commodity.name, commodity);
                }
            }


            /// <summary>
            /// Update the engine connected to this DataAPI
            /// Reloads all commodities from this new engine
            /// </summary>
            /// <param name="engine">The new EcoEngine instance that this DataApi should track</param>
            public void UpdateCommoditiesTracked(EcoEngine engine)
            {
                this.engine = engine;
                foreach (Commodity commodity in engine.commodities)
                {
                    this.commodities.TryAdd(commodity.name, commodity);
                }
            }

            /// <summary>
            /// Try to track a new commodity
            /// </summary>
            /// <param name="commodity">The new commodity to track</param>
            /// <param name="engine">The engine that this DataAPI should track</param>
            public void TrackNewCommodity(Commodity commodity, EcoEngine engine = null)
            {
                this.commodities.TryAdd(commodity.name, commodity);
                if (engine != null)
                {
                    this.engine = engine;
                }
            }

            /// <summary>
            /// Returns a HashSet of the current Commodities.
            /// </summary>
            public HashSet<Commodity> GetCommodities()
            {
                return engine.commodities;
            }


            /// <summary>
            /// Returns the number of economic simulation rounds that have passed since simulation start.
            /// </summary>
            public int GetRoundCount()
            {
                return engine.roundCount;
            }

            /// <summary>
            /// Returns a List<string> of commodity names representing the this.commodities currently in the simulation.
            /// </summary>
            public List<string> GetCommodityNames()
            {
                List<string> commodityNames = new List<string>();
                foreach (KeyValuePair<string, Commodity> pair in this.commodities)
                {
                    Commodity commodity = pair.Value;

                    commodityNames.Add(commodity.name);
                }
                return commodityNames;
            }

            /// <summary>
            /// Returns the number of this.commodities currently in the simulation.
            /// </summary>
            public int GetNumCommodities()
            {
                return this.commodities.Count;
            }

            /// <summary>
            /// Returns the maximum price the specified commodity has ever traded at.
            /// </summary>
            public int GetHistoricalMaxPrice(string commodityName)
            {
                return this.commodities[commodityName].GetHistoricalMaxPrice();
            }

            /// <summary>
            /// Returns the minimum price the specified commodity has ever traded at.
            /// </summary>
            public int GetHistoricalMinPrice(string commodityName)
            {
                return this.commodities[commodityName].GetHistoricalMinPrice();
            }

            /// <summary>
            /// Returns the need level of the specified commodity.
            /// </summary>
            public double GetNeedLevel(string commodityName)
            {
                return this.commodities[commodityName].needLevel;
            }

            /// <summary>
            /// Returns the current demand velocity of the specified commodity.
            /// </summary>
            public double GetDemandVelocity(string commodityName)
            {
                return this.commodities[commodityName].demandVelocity;
            }

            /// <summary>
            /// Returns the current supply velocity of the specified commodity.
            /// </summary>
            public double GetSupplyVelocity(string commodityName)
            {
                return this.commodities[commodityName].supplyVelocity;
            }

            /// <summary>
            ///  Returns the current price velocity of the specified commodity.
            /// </summary>
            public double GetPriceVelocity(string commodityName)
            {
                return this.commodities[commodityName].priceVelocity;
            }

            /// <summary>
            /// Returns the historical mean price of the specified commodity.
            /// </summary>
            public double GetHistoricalMeanPrice(string commodityName)
            {
                return this.commodities[commodityName].GetHistoricalMeanPrice();
            }

            /// <summary>
            /// Returns the current demand for the given commodity.
            /// </summary>
            public double GetDemand(string commodityName)
            {
                return this.commodities[commodityName].demand;
            }

            /// <summary>
            /// Returns the current supply of a given commodity.
            /// </summary>
            public double GetSupply(string commodityName)
            {
                return this.commodities[commodityName].supply;
            }

            /// <summary>
            ///  Returns the current price of the given commodity.
            /// </summary>
            public int GetCurrentPrice(string commodityName)
            {
                return this.commodities[commodityName].price;
            }

            /// <summary>
            /// Returns the price the given commodity had when the simulation first started.
            /// This value is specified in the this.commodities.json file.
            /// </summary>
            public int GetInitialPrice(string commodityName)
            {
                return this.commodities[commodityName].initialPrice;
            }

            /// <summary>
            /// Returns the current moving average for a specified commodity.
            /// </summary>
            public int GetMovingAverage(string commodityName)
            {
                return this.commodities[commodityName].GetMovingAvg();
            }

            /// <summary>
            /// Returns the size of the moving average queue.
            /// </summary>
            public int GetMovingAverageDuration(string commodityName)
            {
                return this.commodities[commodityName].movingAvgDuration;
            }
        }
    }
}
