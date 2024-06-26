using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

/// <summary>
/// The visual representation of the data associated with a commodity.
/// </summary>
public class VisualizerRowElement : VisualElement
{
    /// <summary>
    /// How far back in the simulation rounds to store data about this commodity. 
    /// </summary>
    private static int QueueDepth = 20;

    /// <summary>
    /// How visually tall this row of data will be.
    /// </summary>
    private static int rowHeight = 25;

    /// <summary>
    /// The name of the commodity that this row represents.
    /// </summary>
    private string commodityName;

    /// <summary>
    /// The historic mean price of the commodity.
    /// </summary>
    private int hmp;

    /// <summary>
    /// The historic min price of the commodity.
    /// </summary>
    private double histMin;

    /// <summary>
    /// The historic max price of the commodity.
    /// </summary>
    private double histMax;

    /// <summary>
    /// The current supply  of the commodity
    /// </summary>
    private int supply;

    /// <summary>
    /// The current demand of the commodity.
    /// </summary>
    private int demand;

    /// <summary>
    /// The history of supply over the last QueueDepth rounds
    /// </summary>
    public Queue<int> SupplyHistory = new Queue<int>();

    /// <summary>
    /// The history of demand over the last QueueDepth rounds
    /// </summary>
    public Queue<int> DemandHistory = new Queue<int>();

    /// <summary>
    /// The history of the historic mean price over the last QueueDepth rounds
    /// </summary>
    public Queue<int> HMPHistory = new Queue<int>();


    /// <summary>
    /// Changes or gets the name of the commodity.
    /// </summary>
    public string CommodityName { get => commodityName; set => setCommodityName(value); }

    /// <summary>
    /// Changes or gets the historic mean price of the commodity.
    /// </summary>
    public int HMP { get => hmp; set => setHMP(value); }

    /// <summary>
    /// Changes or gets the historic min price of the commodity.
    /// </summary>
    public double HistMin { get => histMin; set => setHMin(value); }

    /// <summary>
    /// Changes or gets the historic max price of the commodity.
    /// </summary>
    public double HistMax { get => histMax; set => setHMax(value); }

    /// <summary>
    /// Changes or gets the supply of the commodity.
    /// </summary>
    public int Supply { get => supply; set => setSupply(value); }

    /// <summary>
    /// Changes or gets the demand of the commodity.
    /// </summary>
    public int Demand { get => demand; set => setDemand(value); }


    /// <summary>
    /// Sets the visual name of the commodity this row represents.
    /// </summary>
    /// <param name="name">The name for the commodity this row represents.</param>
    private void setCommodityName(string name)
    {
        Label nameLabel = this.Q<Label>("nameLabel");
        if(nameLabel != null)
        {
            nameLabel.text = name;
            commodityName = name;
        }
    }

    /// <summary>
    /// Sets the visual historic mean price of the commodity this row represents.
    /// </summary>
    /// <param name="hmp">The historic mean price for the commodity this row represents.</param>
    private void setHMP(int hmp)
    {
        Label label = this.Q<Label>("hmpLabel");
        if (label != null)
        {
            label.text = $"{hmp}";
            this.hmp = hmp;
        }
        HMPHistory.Enqueue(hmp);
        if (HMPHistory.Count > QueueDepth)
        {
            HMPHistory.Dequeue();
        }
    }

    /// <summary>
    /// Sets the visual historic min price of the commodity this row represents.
    /// </summary>
    /// <param name="value">The historic min price for the commodity this row represents.</param>
    private void setHMin(double value)
    {
        Label label = this.Q<Label>("hMinLabel");
        if (label != null)
        {
            label.text = $"{value}";
            this.histMin = value;
        }
    }

    /// <summary>
    /// Sets the visual historic max price of the commodity this row represents.
    /// </summary>
    /// <param name="value">The historic max price for the commodity this row represents.</param>
    private void setHMax(double value)
    {
        Label label = this.Q<Label>("hMaxLabel");
        if (label != null)
        {
            label.text = $"{value}";
            this.histMax = value;
        }
    }

    /// <summary>
    /// Sets the visual supply of the commodity this row represents.
    /// </summary>
    /// <param name="value">The supply for the commodity this row represents.</param>
    private void setSupply(int value)
    {
        Label label = this.Q<Label>("supplyLabel");
        if (label != null)
        {
            label.text = $"{value}";
            this.supply = value;
        }
        SupplyHistory.Enqueue(value);
        if(SupplyHistory.Count > QueueDepth)
        {
            SupplyHistory.Dequeue();
        }
    }

    /// <summary>
    /// Sets the visual demand of the commodity this row represents.
    /// </summary>
    /// <param name="value">The demand for the commodity this row represents.</param>
    private void setDemand(int value)
    {
        Label label = this.Q<Label>("demandLabel");
        if (label != null)
        {
            label.text = $"{value}";
            this.demand = value;
        }
        DemandHistory.Enqueue(value);
        if (DemandHistory.Count > QueueDepth)
        {
            DemandHistory.Dequeue();
        }
    }

    /// <summary>
    /// From a provided RowData, sets the visual representation of the historic mean price, the historic min price, the historic max price,
    /// the supply, and the demand of the commodity this row represents.
    /// </summary>
    /// <param name="data"></param>
    public void UpdateRowData(RowData data)
    {
        setHMP(data.hmp);
        setHMin(data.avgLowerBound);
        setHMax(data.avgUpperBound);
        setSupply(data.supply);
        setDemand(data.demand);
    }

    /// <summary>
    /// Creates a new visual row representation of the data of a commodity.
    /// </summary>
    /// <param name="commodityName">The name of the commodity this row represents.</param>
    /// <param name="hmp">The historic mean price of the commodity this row represents.</param>
    /// <param name="minHistoricPrice">The historic min price of the commodity this row represents.</param>
    /// <param name="maxHistoricPrice">The historic max price of the commodity this row represents.</param>
    /// <param name="supply">The supply of the commodity this row represents.</param>
    /// <param name="demand">The demand of the commodity this row represents.</param>
    public VisualizerRowElement(string commodityName, int hmp, double minHistoricPrice, double maxHistoricPrice, int supply, int demand)
    {
        this.commodityName = commodityName;
        this.hmp = hmp;
        this.histMin = minHistoricPrice;
        this.histMax = maxHistoricPrice;
        this.supply = supply;
        this.demand = demand;

        this.style.backgroundColor = Color.black;

        VisualElement rowElement = new VisualElement
        {
            name = "rowElement",
            style = { flexDirection = FlexDirection.Row, height = rowHeight, justifyContent = Justify.SpaceBetween }

        };
        rowElement.AddToClassList("row");


        var itemNameLabel = new Label {
            name = "itemName",
            text = commodityName,
            style = { flexGrow = 1 }
        };
        
        rowElement.Add(itemNameLabel);

        var hmpLabel = new Label { name = "historicMeanPrice",
            text = $"{hmp}",
            style = { flexGrow = 1 }
        };
        
        rowElement.Add(hmpLabel);

        var minPriceLabel = new Label {
            name ="minHMP",
            text = $"{minHistoricPrice}",
            style = { flexGrow = 1 }
        };
        rowElement.Add(minPriceLabel);

        var maxPriceLabel = new Label {
            name = "maxHMP",
            text = $"{maxHistoricPrice}",
            style = { flexGrow = 1 }
        };
        rowElement.Add(maxPriceLabel);

        var supplyLabel = new Label {
            name = "supply",
            text = $"{supply}",
            style = { flexGrow = 1 }
        };
        rowElement.Add(supplyLabel);

        var demandLabel = new Label {
            name = "demand",
            text = $"{demand}" ,
            style = { flexGrow = 1 }
        };
        rowElement.Add(demandLabel);

        Add(rowElement);
    }
}
