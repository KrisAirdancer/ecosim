using System.Collections.Generic;

/// <summary>
/// Stores the values over time of a commodity from a simulation.
/// </summary>
public class ItemData
{
    /// <summary>
    /// How long back into the simulation to store data.
    /// </summary>
    public int QueueDepth = 20;

    /// <summary>
    /// The starting data of this commodity
    /// </summary>
    public RowData RowData;

    /// <summary>
    /// A list of the supply this commodity has had for the last multiple rounds of the simulation.
    /// </summary>
    public Queue<int> SupplyHistory = new Queue<int>();

    /// <summary>
    /// A list of the demand this commodity has had for the last multiple rounds of the simulation.
    /// </summary>
    public Queue<int> DemandHistory = new Queue<int>();

    /// <summary>
    /// A list of the historic mean price this commodity has had for the last multiple rounds of the simulation.
    /// </summary>
    public Queue<int> HMPHistory = new Queue<int>();

    /// <summary>
    /// Creates an initial storage of data on an item from the raw data, and makes a RowData from that.
    /// Also enqueues the data that will be displayed in the graph.
    /// </summary>
    /// <param name="name">Name of the commodity which is having information stored about it.</param>
    /// <param name="price">The first price value of the commodity that will be stored.</param>
    /// <param name="hmp">The first hmp value of the commodity that will be stored.</param>
    /// <param name="lower">The historic min price of the commodity</param>
    /// <param name="upper">The historic max price of the commodity</param>
    /// <param name="supply">The first supply value of the commodity that will be stored.</param>
    /// <param name="demand">The first demand value of the commodity that will be stored.</param>
    public ItemData(string name, int price, int hmp, double lower, double upper, int supply, int demand)
    {
        RowData = new RowData(name, price, hmp, lower, upper, supply, demand);

        SupplyHistory.Enqueue(this.RowData.supply);
        DemandHistory.Enqueue(this.RowData.demand);
        HMPHistory.Enqueue(this.RowData.hmp);
    }

    /// <summary>
    /// Creates an initial storage of data on an item from a RowData.
    /// Also enqueues the data that will be displayed in the graph.
    /// </summary>
    /// <param name="rowData">Row data about the various fields of a commodity.</param>
    public ItemData(RowData rowData)
    {
        this.RowData = rowData;

        SupplyHistory.Enqueue(this.RowData.supply);
        DemandHistory.Enqueue(this.RowData.demand);
        HMPHistory.Enqueue(this.RowData.hmp);
    }

    /// <summary>
    /// Provides the row data of the commodity associated with this ItemData
    /// </summary>
    /// <returns>This Itemdatas RowData.</returns>
    public RowData GetRowData()
    {
        return this.RowData;
    }

    /// <summary>
    /// Updates the row data, supply history, demand history, and historic mean price history of this ItemData.
    /// </summary>
    /// <param name="newRowData">The new row data to update this ItemData with.</param>
    public void Update(RowData newRowData)
    {
        this.RowData = newRowData;
        SupplyHistory.Enqueue(newRowData.supply);
        DemandHistory.Enqueue(newRowData.demand);
        HMPHistory.Enqueue(newRowData.hmp);

        // Ensure queue depth.
        if (SupplyHistory.Count > QueueDepth)
        {
            SupplyHistory.Dequeue();
        }
        if (DemandHistory.Count > QueueDepth)
        {
            DemandHistory.Dequeue();
        }
        if (HMPHistory.Count > QueueDepth)
        {
            HMPHistory.Dequeue();
        }
    }
}