
/// <summary>
/// A RowData contains the name of a commodity, its price, its historic mean price, its historic min price,
/// its historic max price, its supply, and its demand. The row data is provided to the visualizer to display the changes in commodities over time.
/// </summary>
public class RowData
{
    /// <summary>
    /// The name of the commodity
    /// </summary>
    public string name;

    /// <summary>
    /// The current price of the commodity
    /// </summary>
    public int price;

    /// <summary>
    /// The historic mean price of the commodity
    /// </summary>
    public int hmp;

    /// <summary>
    /// The historic min price of the commodity
    /// </summary>
    public double avgLowerBound;

    /// <summary>
    /// The historic max price of the commodity
    /// </summary>
    public double avgUpperBound;

    /// <summary>
    /// The current supply of the commodity
    /// </summary>
    public int supply;

    /// <summary>
    /// The current demand of the commodity
    /// </summary>
    public int demand;

    /// <summary>
    /// Constructs a new RowData, and sets it fields to the parameters
    /// </summary>
    /// <param name="name">The name of the commodity</param>
    /// <param name="price">The current price of the commodity</param>
    /// <param name="hmp">The historic mean price of the commodity</param>
    /// <param name="lower">The historic min price of the commodity</param>
    /// <param name="upper">The historic max price of the commodity</param>
    /// <param name="supply">The current supply of the commodity</param>
    /// <param name="demand">The current demand of the commodity</param>
    public RowData(string name, int price, int hmp, double lower, double upper, int supply, int demand)
    {
        this.name = name;
        this.price = price;
        this.hmp = hmp;
        this.avgLowerBound = lower;
        this.avgUpperBound = upper;
        this.supply = supply;
        this.demand = demand;

    }

}
