using System.Collections.Generic;
using System;
using System.Linq;

namespace EcoSim
{
    partial class EcoEngine
    {
        /// <summary>
        /// Commodities are used by EcoEngine to store information about the various details of items that exist inside of the game.
        /// If the commodities are imported by using EcoSimBridge, it will take all existing items from the provided directory, and make some default values for them, and save them to a commodities file, 
        /// otherwise the developer will have to manually make each commodity, and link them to their game. The variables of a commodity are used and modified by EcoEngine to set the supply, price, demand, and other fields for each commodity.
        /// </summary>
        public class Commodity
        {
            /// <summary>
            /// The reference name for this commodity
            /// </summary>
            public string name { get; set; }

            /// <summary>
            /// The current price of this commodity.
            /// </summary>
            public int price { get; set; }

            /// <summary>
            /// The price that the commodity started the simulation with.
            /// </summary>
            public int initialPrice;

            /// <summary>
            /// The highest price this commodity has reached during this run of the simulation.
            /// </summary>
            private int historicalMaxPrice;

            /// <summary>
            /// The lowest price this commodity has reached during this run of the simulation.
            /// </summary>
            private int historicalMinPrice;

            /// <summary>
            /// The average price of this commodity during this run of the simulation.
            /// </summary>
            private int historicalMeanPrice;

            /// <summary>
            /// The rate at which the price of the commodity will increase over time expressed in arbitrary units.
            /// </summary>
            public double inflationRate { get; set; }

            /// <summary>
            /// Units currently available on the market. Note: This is different from circulation.
            /// </summary>
            public double supply { get; set; }

            /// <summary>
            /// Units wanted (asked) by the market.
            /// </summary>
            public double demand { get; set; }
            /// <summary>
            /// The baseline level of demand for the commodity. Demand will always try to return back to this value.
            /// </summary>
            public double needLevel { get; set; }

            /// <summary>
            /// The size of the window used to determine the movingAvg
            /// </summary>
            public int movingAvgDuration { get; set; }

            /// <summary>
            /// The movingAvg is used to get the average price over a recent interval, instead of the average price over all time.
            /// The window starts with the most recent round, and goes back movingAvgDuration rounds.
            /// </summary>
            private Queue<int> movingAvg = new Queue<int>();

            /// <summary>
            /// The rate at which price changes. Zero indicates it will stay the same
            /// </summary>
            public double priceVelocity = 0;

            /// <summary>
            ///  The rate at which supply changes. Zero indicates it will stay the same
            /// </summary>
            public double supplyVelocity = 0;

            /// <summary>
            /// The rate at which demand changes. Zero indicates it will stay the same
            /// </summary>
            public double demandVelocity = 0; 

            /// <summary>
            /// Called at the start of a simulation to fill the window that movingAverage is based off with the starting price. As the simulation runs,
            /// these values will be replaced one at a time with the new price, shifting the moving Average to the newer price range.
            /// </summary>
            public void InitializeMovingAverage()
            {
                this.historicalMaxPrice = this.price;
                this.historicalMinPrice = this.price;

                while (movingAvg.Count < this.movingAvgDuration)
                {
                    this.movingAvg.Enqueue(price);
                }
            }

            /// <summary>
            /// Sets this.price equal to its new value - price - and uses price to update the historicalMeanPrice, and if price is greater than this.historicalMaxPrice, or 
            /// less than this.historicalMinPrice, updates those values to be price.
            /// </summary>
            /// <param name="price">The new price for this commodity</param>
            public void SetPrice(int price)
            {
                this.price = price;
                this.historicalMeanPrice = (historicalMeanPrice + price) / 2;

                if (price > this.historicalMaxPrice) { this.historicalMaxPrice = price; }
                if (price < this.historicalMinPrice) { this.historicalMinPrice = price; }
            }

            /// <summary>
            /// Averages the values in movingAvg, then returns that value rounded up.
            /// </summary>
            /// <returns>The moving average of this commodity, rounded up to the nearest int.</returns>
            public int GetMovingAvg()
            {
                return (int)Math.Ceiling(this.movingAvg.Average());
            }

            /// <summary>
            /// If the moving average window is not full, just adds "value" to the window, otherwise it "value", and removes the oldest value.
            /// </summary>
            /// <param name="value">The price for this commodity that should be added to the moving average window</param>
            public void UpdateMovingAvg(int value)
            {
                if (this.movingAvg.Count < this.movingAvgDuration)
                {
                    this.movingAvg.Enqueue(value);
                }
                else
                {
                    this.movingAvg.Enqueue(value);
                    this.movingAvg.Dequeue();
                }
            }

            /// <summary>
            /// Returns the value of this commodities historical max price.
            /// </summary>
            /// <returns>The integer value of this commodities historical max price</returns>
            public int GetHistoricalMaxPrice()
            {
                return this.historicalMaxPrice;
            }

            /// <summary>
            /// Returns the value of this commodities historical min price.
            /// </summary>
            /// <returns>The integer value of this commodities historical min price</returns>
            public int GetHistoricalMinPrice()
            {
                return this.historicalMinPrice;
            }

            /// <summary>
            /// Returns the value of this commodities historical average price.
            /// </summary>
            /// <returns>The integer value of this commodities historical average price</returns>
            public int GetHistoricalMeanPrice()
            {
                return this.historicalMeanPrice;
            }

            /// <summary>
            /// Manually sets the historicalMeanPrice to "price". 
            /// </summary>
            /// <param name="price">The new historical mean price for this commodity</param>
            public void SetHistoricalMeanPrice(int price)
            {
                this.historicalMeanPrice = price;
            }
        }
    }

}
