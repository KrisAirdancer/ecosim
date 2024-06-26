using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// A custom Unity visual element used to graph the change over time of the details of the commodities in a simulation.
/// </summary>
public class GraphElement : VisualElement
{
    /// <summary>
    /// 
    /// </summary>
    public Queue<int> testData = new Queue<int>();

    /// <summary>
    /// Stores the information about the currently selected commodity.
    /// </summary>
    private ItemData itemData = null;

    /// <summary>
    /// The title for the graph displaying the commodity data.
    /// </summary>
    public string title { get; set; }

    /// <summary>
    /// How many rounds back in the simulation should be graphed.
    /// </summary>
    public int roundsToShow { get; set; }

    /// <summary>
    /// Control line thickness
    /// </summary>
    public int lineThickness { get; set; }

    /// <summary>
    /// Controls padding around the graph but not the labels, IE space saved for labels
    /// </summary>
    public int padding { get; set; }

    /// <summary>
    /// Size of the labels for each axis of the graph.
    /// </summary>
    public int labelFontSize { get; set; }

    /// <summary>
    /// Value used to decide if the supply of the commodity should be drawn on the graph.
    /// </summary>
    public bool drawSupply = true;

    /// <summary>
    /// Value used to decide if the demand of the commodity should be drawn on the graph.
    /// </summary>
    public bool drawDemand = true;

    /// <summary>
    /// Value used to decide if the historic mean price of the commodity should be drawn on the graph.
    /// </summary>
    public bool drawHMP = true;

    /// <summary>
    /// The color of the line that shows the supply of the commodity.
    /// </summary>
    public Color supplyColor = new Color(.39f, 143 / 255f, 1f);

    /// <summary>
    /// The color of the line that shows the demand of the commodity.
    /// </summary>
    public Color demandColor = new Color(220 / 255f, 38 / 255f, 127 / 255f);

    /// <summary>
    /// The color of the line that shows the historic mean price of the commodity.
    /// </summary>
    public Color hmpColor = new Color(1f, 176 / 255, 0f);


    /// <summary>
    /// Required class for UI Builder
    /// </summary>
    // Need to show up in UI builder
    public new class UxmlFactory : UxmlFactory<GraphElement, UxmlTraits> { }

    /// <summary>
    /// Used to add editable values to items in Ui builder
    /// </summary>
    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        UxmlStringAttributeDescription m_title =
            new UxmlStringAttributeDescription { name = "Graph Title", defaultValue = "New Graph" };
        UxmlIntAttributeDescription m_rounds =
            new UxmlIntAttributeDescription { name = "Rounds Displayed", defaultValue = 20 };


        public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
        {
            get { yield break; }
        }

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
            var ate = ve as GraphElement;

            ate.title = m_title.GetValueFromBag(bag, cc);
            ate.roundsToShow = m_rounds.GetValueFromBag(bag, cc);
        }
    }

    /// <summary>
    /// Constructor subscribes our custom method to 
    /// </summary>
    public GraphElement()
    {
        generateVisualContent += OnGenerateVisualContent;
        lineThickness = 5;
        padding = 32;
        labelFontSize = 13;
        testData.Enqueue(5);
        testData.Enqueue(10);
        testData.Enqueue(15);
        testData.Enqueue(20);
        testData.Enqueue(25);
    }

    /// <summary>
    /// Takes the information about a commodity, and sets it to be what is graphed.
    /// </summary>
    /// <param name="itemData">The commodity information to graph.</param>
    public void SetRowData(ItemData itemData)
    {
        this.itemData = itemData;
    }

    /// <summary>
    /// Method to draws graph when called based on various controls
    /// </summary>
    /// <param name="context"></param>
    void OnGenerateVisualContent(MeshGenerationContext context)
    {
        Painter2D pc = context.painter2D;
        float width = contentRect.width;
        float height = contentRect.height;

        pc.lineJoin = LineJoin.Round;

        pc.strokeColor = Color.white;
        pc.lineWidth = lineThickness/5;
        if (itemData is not null)
        {
            DrawGrid(pc);
            pc.lineWidth = lineThickness * 0.6f;
            if (drawSupply)
            {
                DrawLineGraph(pc, context, itemData.SupplyHistory, width, height, supplyColor);
            }
            if (drawDemand)
            {
                DrawLineGraph(pc, context, itemData.DemandHistory, width, height, demandColor, true);
            }
            if (drawHMP)
            {
                DrawLineGraph(pc, context, itemData.HMPHistory, width, height, hmpColor, true);
            }
        }
        else
        {
            context.DrawText($"Select an item to view graph", new Vector2(10.0f, 10.0f), 12.0f, new Color(180, 180, 180));
        }
    }

    /// <summary>
    /// Draws the actual lines on the graph, can label each point when drawn
    /// </summary>
    /// <param name="pc"></param>
    /// <param name="context"></param>
    /// <param name="queue"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="graphColor"></param>
    /// <param name="drawOnRight"></param>
    /// <param name="labelPoints"></param>
    //Look into adding left padding
    void DrawLineGraph(Painter2D pc, MeshGenerationContext context, Queue<int> queue, float width, float height, Color graphColor, bool drawOnRight = false, bool labelPoints = false)
    {
        int count = queue.Count;

        if(count == 0)
        {
            return;
        }

        int min = int.MinValue; 
        int max = int.MaxValue;
        int[] data = queue.ToArray();
        min = Mathf.RoundToInt(data.Min() * 0.9f);
        max = Mathf.RoundToInt(data.Max() * 1.1f);
        pc.strokeColor = graphColor;
        pc.BeginPath();

        for (int i =count-1;i>=0;i--)
        {
            float pointHeight = height - (((float)(data[i] - min) / (float)(max - min)) * height);
            float pointWidth = width - (count-i) * (width/roundsToShow);
            pc.LineTo(new Vector2(pointWidth,pointHeight));
            if(labelPoints)
            {
                context.DrawText($"({data[i]})", new Vector2(pointWidth+3.0f,pointHeight),12, graphColor);
            }
        }

        pc.Stroke();
        DrawLabels(pc, context, min, max, 8, graphColor, drawOnRight);
    }

    /// <summary>
    /// Adds the labels to the graph.
    /// </summary>
    /// <param name="pc"></param>
    /// <param name="context"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="numLabels"></param>
    /// <param name="labelColor"></param>
    /// <param name="drawOnRight"></param>
    void DrawLabels(Painter2D pc, MeshGenerationContext context, int min, int max, int numLabels, Color labelColor,  bool drawOnRight)
    {
        float height = contentRect.height;
        int stepSize = Mathf.RoundToInt(height / numLabels);
        int range = max - min;
        int stepAmount = range / numLabels;
        float labelHeight = height;
        float labelPosWidth = 0.0f;

        if (drawOnRight)
        {
            labelPosWidth = contentRect.width-25f;
        }
        
        for(int i =0;i<numLabels;i++)
        {
            labelHeight = height - i * stepSize - stepSize;
            Vector2 position = new Vector2(labelPosWidth,labelHeight);
            context.DrawText($"{min+stepAmount*i}", position, labelFontSize, labelColor);
        }
    }

    /// <summary>
    /// Draws the background grid for the graph.
    /// </summary>
    /// <param name="pc"></param>
    void DrawGrid(Painter2D pc)
    {
        float width = contentRect.width;
        float height = contentRect.height;
        int virtLines = Mathf.FloorToInt(width / roundsToShow);
        int horzLine = 10;
        float virtStep = (width - 2* padding) / virtLines;
        float horzStep = height / horzLine;

        pc.lineWidth = lineThickness/10f;
        pc.BeginPath();
        
        for (int x = 0; x < virtLines; x++)
        {
            pc.MoveTo(new Vector2(padding + x*virtStep, 0));
            pc.LineTo(new Vector2(padding + x * virtStep, height));
        }
        
        for (int y = 0; y < horzLine; y++)
        {
            pc.MoveTo(new Vector2(padding, y*horzStep));
            pc.LineTo(new Vector2(width-padding, y*horzStep));
        }
        
        pc.MoveTo(new Vector2(width-padding, 0));
        pc.LineTo(new Vector2(width - padding,height));
        pc.LineTo(new Vector2(padding, height));
        pc.Stroke();
    }
}
