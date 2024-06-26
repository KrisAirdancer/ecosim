using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using System.IO;

/// <summary>
/// This tool can graph various details over time for all of the commodities in each simulation.
/// </summary>
public class Visualizer : EditorWindow
{
    /// <summary>
    /// Visual tree that all the visual elements of this tool are based off of.
    /// </summary>
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    /// <summary>
    /// The Ecosim Manager instance that will be providing information about the simulation  
    /// </summary>
    private static EcosimManager ecosimManager = null;

    /// <summary>
    /// This is used to show part of the commodities in the simulation and some information on each
    /// </summary>
    private VisualElement listView;

    /// <summary>
    /// This in tandem with listView show all of the commodities of a simulation, this part has the actual values.
    /// </summary>
    private MultiColumnListView columnListView;

    /// <summary>
    /// This is the visual element that does most of the actual graphing.
    /// </summary>
    private GraphElement graphElement;

    /// <summary>
    /// This control various setting for graphs, like which fields to graph.
    /// </summary>
    private VisualElement graphControlContainer;

    /// <summary>
    /// Determines whether or not the graph should currently be trying to update based off of simulation data.
    /// </summary>
    private bool DoGraphUpdate = false;
    
    /// <summary>
    /// The particular commodity to graph information about.
    /// </summary>
    private ItemData selectedData = null;

    /// <summary>
    /// Contains all of the commodities basic information.
    /// </summary>
    private static Dictionary<string, RowData> rows;

    /// <summary>
    /// Used to contain all of the actual information about the commodities, like their supply demand.
    /// </summary>
    private static Dictionary<string, List<ItemData>>rowsData = new Dictionary<string, List<ItemData>>();

    /// <summary>
    /// Indicates whether or not the application is running, and the simulation as well.
    /// </summary>
    private bool gettingRowUpdates = false;

    /// <summary>
    /// Indicates whether or not the application has been run.
    /// </summary>
    private bool hasAppStarted = false;

    /// <summary>
    /// Used to store the changeable value that is the color of the supply graph line.
    /// </summary>
    ColorField supplyColorField;

    /// <summary>
    /// Used to store the changeable value that is the color of the demand graph line.
    /// </summary>
    ColorField demandColorField;

    /// <summary>
    /// Used to store the changeable value that is the color of the historic mean price graph line.
    /// </summary>
    ColorField hmpColorField;

    /// <summary>
    /// Indicates if the chosen supply color has changed, but is not yet reflected in the tool.
    /// </summary>
    private bool supplyColorChanged = false;

    /// <summary>
    /// Indicates if the chosen demand color has changed, but is not yet reflected in the tool.
    /// </summary>
    private bool demandColorChanged = false;

    /// <summary>
    /// Indicates if the chosen historic mean price color has changed, but is not yet reflected in the tool.
    /// </summary>
    private bool hmpColorChanged = false;

    /// <summary>
    /// Unity visual element consisting of a dropdown tool that contains the names of all current simulations.
    /// </summary>
    private static DropdownField _simulationSelectionDropDown;

    /// <summary>
    /// The currently selected sim whose data is being displayed and graphed.
    /// </summary>
    private string selectedSim;

    /// <summary>
    /// Unity visual element that contains rows of each commodity and their details.
    /// </summary>
    private VisualElement _itemsList;

    /// <summary>
    /// Displays the Visualizer if one already exists, if not triggers the creation of a new one.
    /// </summary>
    [MenuItem("EcoSim/Visualizer")]
    public static void ShowWindow()
    {
        QuickStartGuide.NotifyOfOpen(QuickStartGuide.UI_Type.Visualizer);
        Visualizer wnd = GetWindow<Visualizer>();
        wnd.maxSize = new Vector2(600, 800);
        wnd.minSize = wnd.maxSize;
        wnd.titleContent = new GUIContent("Visualizer");
    }

    /// <summary>
    /// On enabling the window fills in the table of commodities.
    /// </summary>
    private void OnEnable()
    {
        if (listView != null)
        {
            foreach (RowData row in rows.Values)
            {
                VisualizerRowElement newRow = new VisualizerRowElement(row.name, row.hmp, row.avgLowerBound, row.avgUpperBound, row.supply, row.demand);
                newRow.name = $"{row.name}".Replace(" ", "_");
                listView.Add(newRow);
            }
        }
        RegisterGraphContainerCallbacks();
    }

    /// <summary>
    /// On disabling the window unregisters callbacks and stops taking in information from the simulation.
    /// </summary>
    private void OnDisable()
    {
        if (ecosimManager != null && Application.isPlaying)
        {
            ecosimManager.RowChanged -= SetRow;
            gettingRowUpdates = false;
        }

        supplyColorField.UnregisterValueChangedCallback(ColorChangedCallback);
        Toggle supplyToggle = graphControlContainer.Q<Toggle>("supplyToggle");
        supplyToggle.UnregisterValueChangedCallback(ToggleValueChanged);

        demandColorField.UnregisterValueChangedCallback(ColorChangedCallback);
        Toggle demandToggle = graphControlContainer.Q<Toggle>("demandToggle");
        demandToggle.UnregisterValueChangedCallback(ToggleValueChanged);

        hmpColorField.UnregisterValueChangedCallback(ColorChangedCallback);
        Toggle hmpToggle = graphControlContainer.Q<Toggle>("hmpToggle");
        hmpToggle.UnregisterValueChangedCallback(ToggleValueChanged);
    }

    /// <summary>
    /// On close of this window notifies the quick start guide
    /// </summary>
    private void OnDestroy()
    {
        QuickStartGuide.NotifyOfClose(QuickStartGuide.UI_Type.Visualizer);
    }

    /// <summary>
    /// Creates a new visualizer and sets up the back end logic. 
    /// </summary>
    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Remove TemplateContainer element to allow main container to fill entire window. Source: https://forum.unity.com/threads/visualelements-wont-grow-to-fill-space-for-some-reason.759725/#post-5101916
        VisualElement uxml = m_VisualTreeAsset.CloneTree();
        while (uxml.childCount > 0)
        {
            root.Add(uxml.ElementAt(0));
        }

        graphElement = rootVisualElement.Q<GraphElement>();

        

        _simulationSelectionDropDown = root.Q<DropdownField>("simulationSelectionDropDown");

        // Add graph group elements.
        graphControlContainer = root.Q<VisualElement>("graphControls");
        supplyColorField = graphControlContainer.Q<ColorField>("supplyColorPicker");
        demandColorField = graphControlContainer.Q<ColorField>("demandColorPicker");
        hmpColorField = graphControlContainer.Q<ColorField>("hmpColorPicker");
        RegisterGraphContainerCallbacks();

        
        _simulationSelectionDropDown.RegisterCallback<MouseOverEvent>(fillSimulations);
        _simulationSelectionDropDown.RegisterValueChangedCallback(PopulateFields);

        _itemsList = root.Q<VisualElement>("itemsList");
        _itemsList.Add(BuildDataListObject());
    }

    /// <summary>
    /// Every frame checks in the visualizer needs to be update to represent changes made, and applys changes if needed.
    /// </summary>
    void Update()
    {
        if (supplyColorChanged)
        {
            graphElement.supplyColor = supplyColorField.value;
            graphElement.MarkDirtyRepaint();
        }
        if (demandColorChanged)
        {
            graphElement.demandColor = demandColorField.value;
            graphElement.MarkDirtyRepaint();
        }
        if (hmpColorChanged)
        {
            graphElement.hmpColor = hmpColorField.value;
            graphElement.MarkDirtyRepaint();
        }

        if (DoGraphUpdate && graphElement != null && selectedData != null)
        {
            graphElement.SetRowData(selectedData);
            graphElement.MarkDirtyRepaint();
            DoGraphUpdate = false;
        }
        if (Application.isPlaying && ecosimManager == null)
        {
            var test = GameObject.FindAnyObjectByType<EcosimManager>();
            if (test != null)
            {
                ecosimManager = test;
            }
        }
        if (gettingRowUpdates == false && ecosimManager != null && Application.isPlaying)
        {
            ecosimManager.RowChanged += SetRow;
            gettingRowUpdates = true;
        }
        if (!hasAppStarted && Application.isPlaying)
        {
            hasAppStarted = true;
        }
        if (hasAppStarted && !Application.isPlaying)
        {
            VisualizerReset();
        }

    }

    /// <summary>
    /// Creates the structure of the table showing information about the commodities, and sets it up so that users can select
    /// commodities to see the graphed values.
    /// </summary>
    /// <returns></returns>
    public MultiColumnListView BuildDataListObject()
    {
        columnListView = new MultiColumnListView();
        if (selectedSim != "" && selectedSim != null)
        {
        // Construct column headers.
        columnListView.columns.Add(new Column { name = "itemName", title = "Item Name", minWidth = 80 });
        columnListView.columns.Add(new Column { name = "price", title = "Price", minWidth = 80 });
        columnListView.columns.Add(new Column { name = "hmp", title = "HMP", minWidth = 80 });
        columnListView.columns.Add(new Column { name = "minHMP", title = "Min HMP", minWidth = 80 });
        columnListView.columns.Add(new Column { name = "maxHMP", title = "Max HMP", minWidth = 80 });
        columnListView.columns.Add(new Column { name = "supply", title = "Supply", minWidth = 80 });
        columnListView.columns.Add(new Column { name = "demand", title = "Demand", minWidth = 80 });

        // Set the data source.
        if (!rowsData.ContainsKey(selectedSim)) rowsData.Add(selectedSim, new List<ItemData>());
        columnListView.itemsSource = rowsData[selectedSim];

        // Set the makeItem function for each column.
        columnListView.columns["itemName"].makeCell = () => null;
        columnListView.columns["price"].makeCell = () => new Label();
        columnListView.columns["hmp"].makeCell = () => new Label();
        columnListView.columns["minHMP"].makeCell = () => new Label();
        columnListView.columns["maxHMP"].makeCell = () => new Label();
        columnListView.columns["supply"].makeCell = () => new Label();
        columnListView.columns["demand"].makeCell = () => new Label();

        // Set the bindItem function for each column.

        columnListView.columns["itemName"].bindCell = (VisualElement element, int index) => (element as Label).text = rowsData[selectedSim][index].GetRowData().name;
        columnListView.columns["price"].bindCell = (VisualElement element, int index) => (element as Label).text = rowsData[selectedSim][index].GetRowData().price.ToString();
        columnListView.columns["hmp"].bindCell = (VisualElement element, int index) => (element as Label).text = rowsData[selectedSim][index].GetRowData().hmp.ToString();
        columnListView.columns["minHMP"].bindCell = (VisualElement element, int index) => (element as Label).text = rowsData[selectedSim][index].GetRowData().avgLowerBound.ToString();
        columnListView.columns["maxHMP"].bindCell = (VisualElement element, int index) => (element as Label).text = rowsData[selectedSim][index].GetRowData().avgUpperBound.ToString();
        columnListView.columns["supply"].bindCell = (VisualElement element, int index) => (element as Label).text = rowsData[selectedSim][index].GetRowData().supply.ToString();
        columnListView.columns["demand"].bindCell = (VisualElement element, int index) => (element as Label).text = rowsData[selectedSim][index].GetRowData().demand.ToString();

        // Add an event handler to handle row selection by the user.
        columnListView.RegisterCallback<MouseDownEvent>(HandleRowSelection);
        }
        return columnListView;
    }

    /// <summary>
    /// Handles the selection of a commodity to displays its graph information.
    /// </summary>
    /// <param name="e">The mouse down event that triggered this method</param>
    private void HandleRowSelection(MouseDownEvent e)
    {
        if (e.target is MultiColumnListView multiColumnListView)
        {
            int selectedIndex = multiColumnListView.selectedIndex;
            if (selectedIndex == -1)
                return;
            this.selectedData = rowsData[selectedSim][selectedIndex];

            graphElement.SetRowData(selectedData);
            graphElement.MarkDirtyRepaint();
        }
    }

    /// <summary>
    /// Sets up the callbacks for changing parts of the graph, like line colors, or which lines are shown.
    /// </summary>
    private void RegisterGraphContainerCallbacks()
    {
 
        if (graphControlContainer != null && graphElement != null)
        {
            supplyColorField.RegisterValueChangedCallback(ColorChangedCallback);
            Toggle supplyToggle = graphControlContainer.Q<Toggle>("supplyToggle");
            supplyToggle.RegisterValueChangedCallback(ToggleValueChanged);
            graphElement.drawSupply = supplyToggle.value;

            demandColorField.RegisterValueChangedCallback(ColorChangedCallback);
            Toggle demandToggle = graphControlContainer.Q<Toggle>("demandToggle");
            demandToggle.RegisterValueChangedCallback(ToggleValueChanged);
            graphElement.drawDemand = demandToggle.value;

            hmpColorField.RegisterValueChangedCallback(ColorChangedCallback);
            Toggle hmpToggle = graphControlContainer.Q<Toggle>("hmpToggle");
            hmpToggle.RegisterValueChangedCallback(ToggleValueChanged);
            graphElement.drawHMP = hmpToggle.value;
        }
    }

    /// <summary>
    /// Handles the triggering of a redraw of the graph when the user changes which lines should be drawn.
    /// </summary>
    /// <param name="evt">The change event that triggered this method</param>
    private void ToggleValueChanged(ChangeEvent<bool> evt)
    {
        VisualElement target = evt.target as VisualElement;
        if (target.name == "supplyToggle")
        {
            graphElement.drawSupply = evt.newValue;
            graphElement.MarkDirtyRepaint();
        }
        else if (target.name == "demandToggle")
        {
            graphElement.drawDemand = evt.newValue;
            graphElement.MarkDirtyRepaint();
        }
        else if (target.name == "hmpToggle")
        {
            graphElement.drawHMP = evt.newValue;
            graphElement.MarkDirtyRepaint();
        }
    }

    /// <summary>
    /// Handles the triggering of a redraw of the graph when the user changes the color of the drawn lines.
    /// </summary>
    /// <param name="evt">The change event that triggered this method</param>
    private void ColorChangedCallback(ChangeEvent<Color> evt)
    {
        VisualElement target = evt.target as VisualElement;
        if (target.name == "supplyColorPicker")
        {
            supplyColorChanged = true;
        } else if(target.name == "demandColorPicker")
        {
            demandColorChanged = true;
        } else if( target.name == "hmpColorPicker")
        {
            hmpColorChanged = true;
        }
    }

    /// <summary>
    /// Sets up the values in each row of the commodities table, and creates a new row if needed.
    /// </summary>
    /// <param name="row">The commodity information to fill this row.</param>
    /// <param name="simulation">The simulation to which this commodity belongs.</param>
    public void SetRow(RowData row, string simulation)
    {
        DoGraphUpdate = true;
        bool addNewRow = true;
        if (!rowsData.ContainsKey(simulation)) rowsData.Add(simulation, new List<ItemData>());
        foreach (ItemData rowData in rowsData[simulation].Where(value => value.GetRowData().name.Equals(row.name)))
        {
            addNewRow = false;
            rowData.Update(row);
            break;
        }

        if (addNewRow)
        { // Add a new row to the list of rows.
            if (columnListView is not null)
            {
                rowsData[simulation].Add(new ItemData(row));
            }
        }

        // Update the list view on the UI.
        columnListView.RefreshItems();
    }

    /// <summary>
    /// Fills the simulations selection dropdown with all current simulations
    /// </summary>
    /// <param name="evt">The mouse over event that triggered this method</param>
    private void fillSimulations(MouseOverEvent evt)
    {
        _simulationSelectionDropDown.choices.Clear();
        string[] dirs = Directory.GetDirectories(Application.dataPath + "/EcoSim/Runtime/Configuration/");
        foreach (string sim in dirs)
        {
            string[] simSplit = sim.Split('/');
            _simulationSelectionDropDown.choices.Add(simSplit[simSplit.Length - 1]);
        }

    }

    /// <summary>
    /// Loads the selected simulation data
    /// </summary>
    /// <param name="evt">The change event that triggered this method</param>
    private void PopulateFields(ChangeEvent<string> evt)
    {
        selectedSim = _simulationSelectionDropDown.value;
        _itemsList.Clear();
        _itemsList.Add(BuildDataListObject());
    }

    /// <summary>
    /// Reset the Window and clears the reference to EcoSim Manager
    /// </summary>
    private static void VisualizerReset()
    {
        ecosimManager = null;
        Visualizer window = GetWindow<Visualizer>();
        window.Close();
        ShowWindow();

    }
}