
namespace EcoSim
{
    /// <summary>
    /// An Event piece is a single change that occurs during an event, like increasing supply or decreasing demand of a commodity.
    /// The values that can be affected are inflation, supply, demand, need level, and moving average duration. All can be 
    /// increased or decreased
    /// </summary>
    public class EventPiece
    {
        /// <summary>
        /// The name of the commodity to affect
        /// </summary>
        public string commodity { get; }

        /// <summary>
        /// The change to the inflation of the commodity, positive values will increase inflation, negative will decrease it.
        /// </summary>
        public int inflationShift { get; }

        /// <summary>
        /// The change to the supply of the commodity, positive values will increase supply, negative will decrease it.
        /// </summary>
        public int supplyShift { get; }

        /// <summary>
        /// The change to the demand of the commodity, positive values will increase demand, negative will decrease it.
        /// </summary>
        public int demandShift { get; }

        /// <summary>
        /// The change to the need level of the commodity, positive values will increase the need level, negative will decrease it.
        /// </summary>
        public int needShift { get; }

        /// <summary>
        /// The change to the moving average duration of the commodity, positive values will increase the moving average duration, negative will decrease it.
        /// </summary>
        public int movingAvgDurationShift { get; }

        /// <summary>
        /// Creates a new event piece, an event piece can affect all of the parameters - inflation, supply, demand, need level, moving average duration - or just some, if you do not want to affect a parameter, set it to zero.
        /// </summary>
        /// <param name="commodity">The name of the commodity to affect</param>
        /// <param name="inflationShift">The change to inflation</param>
        /// <param name="supplyShift">The change to supply</param>
        /// <param name="demandShift">The change to demand</param>
        /// <param name="needShift">The change to need level</param>
        /// <param name="movingAvgDurationShift">The change to moving average duration</param>
        public EventPiece (string commodity, int inflationShift, int supplyShift, int demandShift, int needShift, int movingAvgDurationShift)
        {
            this.commodity = commodity;
            this.inflationShift = inflationShift;
            this.supplyShift = supplyShift;
            this.demandShift = demandShift;
            this.needShift = needShift;
            this.movingAvgDurationShift = movingAvgDurationShift;
        }
    }
}
