using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static EcoSim.EcoEngine;

/// <summary>
/// Custom UI tool for configuring all of the commodities for all current simulations. Can add, edit and delete commodities,
/// and save them to file. 
/// </summary>
public class CommodityConfiguration : EditorWindow
{
    // Metadata
    /// <summary>
    /// File path of the commodityConfig.json file.
    /// </summary>
    private static string _configFile;

    /// <summary>
    /// File path of the .uxml file that this tool's visual components are based off of.
    /// </summary>
    private static readonly string _uxml = "Assets/EcoSim/Editor/Configuration-Tools/CommodityConfiguration/CommodityConfiguration.uxml";

    /// <summary>
    /// Dictionary of all the commodities in the current simulation. Will also contain any newly created commodities, but they will not
    /// be saved to the config file until the save button is pressed
    /// </summary>
    private static Dictionary<string, Commodity> _commodities;

    /// <summary>
    /// Border color used to indicate an invalid value for a field
    /// </summary>
    private static readonly Color warningColor = Color.red;

    /// <summary>
    /// Converter used to convert a json file to a dictionary of commodities, or vice versa.
    /// </summary>
    private CommodityConverter converter = new CommodityConverter();

    /* { New Name, Initial Price, Initial Supply, Initial Demand, Needs Level } */
    /// <summary>
    /// Array of errors that need to be fixed before the file can be saved. Fales indices indicate there is no error in the corresponding field.
    /// Indices correspond to New Name, Initial Price, Initial Supply, Initial Demand, Needs Level
    /// </summary>
    private static bool[] _errors = { false, false, false, false, false };

    // UI Elements
    /// <summary>
    /// Unity visual element consisting of a dropdown tool that contains references to each commodity in the simulation.
    /// </summary>
    private static DropdownField _commoditiesDropDown;

    /// <summary>
    /// Unity visual element consisting of a dropdown tool that contains references to each existing simulation.
    /// </summary>
    private static DropdownField _simulationSelectionDropDown;

    /// <summary>
    /// Unity visual element consisting of a tool to enter a name for a new commodity.
    /// </summary>
    private static TextField _newCommodityNameTextField;

    /// <summary>
    /// Unity visual element consisting of a tool to enter a starting price for a new commodity.
    /// </summary>
    private static IntegerField _newCommodityPriceIntField;

    /// <summary>
    /// Unity visual element consisting of a tool to enter a starting supply for a new commodity.
    /// </summary>
    private static DoubleField _newCommoditySupplyDoubleField;

    /// <summary>
    /// Unity visual element consisting of a tool to enter a starting demand for a new commodity.
    /// </summary>
    private static DoubleField _newCommodityDemandDoubleField;

    /// <summary>
    /// Unity visual element consisting of a tool to enter the need level for a new commodity.
    /// </summary>
    private static DoubleField _newCommodityNeedsDoubleField;

    /// <summary>
    /// Unity visual element consisting of a tool to enter the moving average window size for a new commodity.
    /// </summary>
    private static SliderInt _newCommodityRollingAvgIntSlider;

    /// <summary>
    /// Unity visual element consisting of a tool to enter the inflation rate for a new commodity.
    /// </summary>
    private static DoubleField _newCommodityInflationRateDoubleField;

    /// <summary>
    /// Unity visual element consisting of a tool that triggers creating a new commodity, based off of the values in the fields.
    /// </summary>
    private static Button _addNewCommodityButton;

    /// <summary>
    /// Unity visual element consisting of a tool that triggers the deletion of the commodity selected by the dropdown.
    /// </summary>
    private Button _deleteCommodityButton;

    /// <summary>
    /// Unity visual element consisting of a label that is changed to indicate if the commodities in the simulation have been saved to file.
    /// </summary>
    private static Label _commodityScrollViewLabel;

    /// <summary>
    /// Unity visual element consisting of a scrollable view that shows details about all commodities in the simulation
    /// </summary>
    private static ScrollView _commodityScrollView;

    /// <summary>
    /// Unity visual element consisting of a tool that triggers the saving of the commodities to file.
    /// </summary>
    private static Button _saveCommoditiesButton;

    private VisualElement _confirmationOverlay;
    
    /// <summary>
    /// Shows the commodity configuration tool if it exists, otherwise triggers the creation of a new one to display.
    /// If the quick start guide is open, will also inform it that this window was open.
    /// </summary>
    [MenuItem("EcoSim/Commodity Configuration", false, 112)]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(CommodityConfiguration));
        QuickStartGuide.NotifyOfOpen(QuickStartGuide.UI_Type.CommodityConfig);
    }

    /// <summary>
    /// Creates a new instance of the Commodity Configuration tool, and sets up the back end logic for the tool.
    /// </summary>
    public void CreateGUI()
    {
        // Instantiating the inspector
        var root = EditorWindow.GetWindow(typeof(CommodityConfiguration)).rootVisualElement;
        VisualElement commodityConfigUI = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(_uxml).Instantiate();
        root.Add(commodityConfigUI);

        // Collecting the elements together
        _commoditiesDropDown = root.Q<DropdownField>("commoditiesDropDown");
        _simulationSelectionDropDown = root.Q<DropdownField>("simulationSelectionDropDown");
        _newCommodityNameTextField = root.Q<TextField>("newCommodityNameTextField");
        _newCommodityPriceIntField = root.Q<IntegerField>("newCommodityPriceIntField");
        _newCommoditySupplyDoubleField = root.Q<DoubleField>("newCommoditySupplyDoubleField");
        _newCommodityDemandDoubleField = root.Q<DoubleField>("newCommodityDemandDoubleField");
        _newCommodityNeedsDoubleField = root.Q<DoubleField>("newCommodityNeedsDoubleField");
        _newCommodityRollingAvgIntSlider = root.Q<SliderInt>("newCommodityRollingAvgIntSlider");
        _newCommodityInflationRateDoubleField = root.Q<DoubleField>("newCommodityInflationRateDoubleField");
        _addNewCommodityButton = root.Q<Button>("addNewCommodityButton");
        _deleteCommodityButton = root.Q<Button>("deleteCommodityButton");   

        _commodityScrollViewLabel = root.Q<Label>("commodityScrollViewLabel");
        _commodityScrollView = root.Q<ScrollView>("commoditiesScrollView");
        _saveCommoditiesButton = root.Q<Button>("saveCommoditiesButton");

        // Registering the buttons with callback functions
        _simulationSelectionDropDown.RegisterCallback<MouseOverEvent>(fillSimulations);
        _simulationSelectionDropDown.RegisterValueChangedCallback(PopulateFields);

        _newCommodityNameTextField.RegisterValueChangedCallback(ValidateName);
        _newCommodityPriceIntField.RegisterValueChangedCallback(ValidateInitialPrice);
        _newCommoditySupplyDoubleField.RegisterValueChangedCallback(ValidateInitialSupply);
        _newCommodityDemandDoubleField.RegisterValueChangedCallback(ValidateInitialDemand);
        _newCommodityNeedsDoubleField.RegisterValueChangedCallback(ValidateNeedsLevel);
        
        _addNewCommodityButton.RegisterCallback<ClickEvent>(AddCommodity);
        _deleteCommodityButton.RegisterCallback<ClickEvent>(DeleteCommodity);
        _saveCommoditiesButton.RegisterCallback<ClickEvent>(SaveCommodities);

        _commoditiesDropDown.RegisterValueChangedCallback(UpdateCommodityFields);        
        
        //This will allow tooltips to exist in play mode
        _newCommodityNameTextField.AddManipulator(new PlayModeToolTip(root));
        _newCommodityPriceIntField.AddManipulator(new PlayModeToolTip(root));
        _newCommoditySupplyDoubleField.AddManipulator(new PlayModeToolTip(root));
        _newCommodityDemandDoubleField.AddManipulator(new PlayModeToolTip(root));
        _newCommodityNeedsDoubleField.AddManipulator(new PlayModeToolTip(root));
        _newCommodityRollingAvgIntSlider.AddManipulator(new PlayModeToolTip(root));
        _newCommodityInflationRateDoubleField.AddManipulator(new PlayModeToolTip(root));
        _commodityScrollViewLabel.AddManipulator(new PlayModeToolTip(root));

        // Disable until a simulation is selected
        _newCommodityNameTextField.SetEnabled(false);
        _newCommodityPriceIntField.SetEnabled(false);
        _newCommoditySupplyDoubleField.SetEnabled(false);
        _newCommodityDemandDoubleField.SetEnabled(false);
        _newCommodityNeedsDoubleField.SetEnabled(false);
        _newCommodityRollingAvgIntSlider.SetEnabled(false);
        _newCommodityInflationRateDoubleField.SetEnabled(false);
        _addNewCommodityButton.SetEnabled(false);
        _deleteCommodityButton.SetEnabled(false);
        _saveCommoditiesButton.SetEnabled(false);

        hasUnsavedChanges = false;
    }

    /// <summary>
    /// Loads the selected commodity config file, and fills all fields
    /// </summary>
    private void PopulateFields(ChangeEvent<string> evt)
    {
        // Add commodities into the scroll view and the DropdownField
        _commoditiesDropDown.choices.Clear();
        _commoditiesDropDown.choices.Add("New commodity");
        _commoditiesDropDown.index = 0;
        _deleteCommodityButton.SetEnabled(false);

        if (_commodities != null) _commodities.Clear();
        _configFile = Application.dataPath + "/EcoSim/Runtime/Configuration/" + _simulationSelectionDropDown.value + "/commodityConfig.json";
        LoadCommodities();
        _commodityScrollView.Clear();
        // Add commodities into the scroll view
        foreach (string key in _commodities.Keys)
        {
            if (key == "New commodity") continue;

            Commodity c = _commodities[key];
            StringBuilder sb = new();
            sb.Append($"{c.name}:\n\tPrice: {c.price}\n\tInitial supply: {c.supply}\n\tInitial demand: {c.demand}");
            sb.Append($"\n\tNeeds level: {c.needLevel}\n\tMoving average duration: {c.movingAvgDuration}\n\tInflation rate: {c.inflationRate}");
            string commodityString = sb.ToString();

            _commoditiesDropDown.choices.Add(c.name);
            _commodityScrollView.Add(new Label(commodityString));
        }
        _newCommodityNameTextField.SetEnabled(true);
        _newCommodityPriceIntField.SetEnabled(true);
        _newCommoditySupplyDoubleField.SetEnabled(true);
        _newCommodityDemandDoubleField.SetEnabled(true);
        _newCommodityNeedsDoubleField.SetEnabled(true);
        _newCommodityRollingAvgIntSlider.SetEnabled(true);
        _newCommodityInflationRateDoubleField.SetEnabled(true);
        _addNewCommodityButton.SetEnabled(true);
        _saveCommoditiesButton.SetEnabled(true);

        hasUnsavedChanges = false;
    }

    /// <summary>
    /// Causes the window to refresh and deletes the current commodities
    /// Closes the window and reopens it
    /// </summary>
    public static void Refresh()
    {
        _commodities = null;
        if (EditorWindow.HasOpenInstances<CommodityConfiguration>())
        {
            CommodityConfiguration window = EditorWindow.GetWindow<CommodityConfiguration>();
            window.Close();
            ShowWindow();

        }
        
    }


    /// <summary>
    /// Rewrites the contents of the scroll view
    /// </summary>
    private static void RewriteScrollView()
    {
        _commodityScrollView.Clear();

        foreach (string key in _commodities.Keys)
        {
            if (key == "New commodity") continue;

            Commodity c = _commodities[key];
            StringBuilder sb = new();
            sb.Append($"{c.name}:\n\tPrice: {c.price}\n\tInitial supply: {c.supply}\n\tInitial demand: {c.demand}");
            sb.Append($"\n\tNeeds level: {c.needLevel}\n\tMoving average duration: {c.movingAvgDuration}\n\tInflation rate: {c.inflationRate}");
            string commodityString = sb.ToString();

            _commodityScrollView.Add(new Label(commodityString));
        }
    }

    /// <summary>
    /// Deletes the commodity selected by the dropdown, if it exists. Does not change the file.
    /// </summary>
    /// <param name="evt">The click event that triggered this method</param>
    private void DeleteCommodity(ClickEvent evt)
    {
        if (_commoditiesDropDown.index == 0) return; // Can't delete a commodity that doesn't exist
        else
        {
            Debug.LogWarning($"Deleting {_commoditiesDropDown.value} from commodities...");
            _commodities.Remove(_commoditiesDropDown.value);
            _commoditiesDropDown.choices.Remove(_commoditiesDropDown.value);
            _commoditiesDropDown.RemoveFromClassList(_commoditiesDropDown.value);
            _commoditiesDropDown.index = 0;
            _deleteCommodityButton.SetEnabled(false);
            _commodityScrollViewLabel.text = "Commodities*";
        }

        RewriteScrollView();
        hasUnsavedChanges = true;
    }

    /// <summary>
    /// Fills in the commodity detail fields with the values of the newly selected commodity.
    /// </summary>
    /// <param name="evt">The change event that triggered this method</param>
    private void UpdateCommodityFields(ChangeEvent<string> evt)
    {
        string newValue = evt.newValue;

        if (newValue == "New commodity")
            _deleteCommodityButton.SetEnabled(false);
        else
            _deleteCommodityButton.SetEnabled(true);

        // Update the values according to the selected commodity
        _newCommodityNameTextField.value = _commodities[newValue].name;
        _newCommodityPriceIntField.value = _commodities[newValue].price;
        _newCommoditySupplyDoubleField.value = _commodities[newValue].supply;
        _newCommodityDemandDoubleField.value = _commodities[newValue].demand;
        _newCommodityNeedsDoubleField.value = _commodities[newValue].needLevel;
        _newCommodityRollingAvgIntSlider.value = _commodities[newValue].movingAvgDuration;
        _newCommodityInflationRateDoubleField.value = _commodities[newValue].inflationRate;

        RewriteScrollView();

    }
   


    /// <summary>
    /// Reads and loads the commoditiesConfig file into memory
    /// </summary>
    private void LoadCommodities()
    {
        CreateCommoditiesConfigFile();
        
        Commodity c = new Commodity();
        c.name = "Enter a name...";
        c.price = 0;
        c.supply = 0; 
        c.demand = 0;
        c.needLevel = 0; 
        c.movingAvgDuration = 50;
        c.inflationRate = 0;


        string json = File.ReadAllText(_configFile);

        var options = new JsonSerializerOptions();
        options.Converters.Add(converter);
        _commodities = JsonSerializer.Deserialize<Dictionary<string, Commodity>>(json, options);
        _commodities.Add("New commodity", c);
        AssetDatabase.Refresh();
    }


    /// <summary>
    /// Checks for the existence of the configFile, and creates it if it does not exist
    /// </summary>
    private void CreateCommoditiesConfigFile()
    {
        if (File.Exists(_configFile)) return;


        File.Create(_configFile).Close();

        // Fill the file with a basic template to show structure and usage
        string template =
@"[
    {
        ""name"": ""Bread"",
        ""price"": 12,
        ""supply"": 1000,
        ""demand"": 950,
        ""needLevel"": 900,
        ""movingAvgDuration"": 50,
        ""inflationRate"": 10

    },
    {
        ""name"": ""Sword"",
        ""price"": 147,
        ""supply"": 300,
        ""demand"": 400,
        ""needLevel"": 250,
        ""movingAvgDuration"": 50,
        ""inflationRate"": -10
    }
]";

        File.WriteAllText(_configFile, template);
        return;
    }


    /// <summary>
    /// Add the new commodity to the list of commodities
    /// </summary>
    /// <param name="evt">Click event that triggered this method</param>
    private void AddCommodity(ClickEvent evt)
    {
        
        if (_commoditiesDropDown.value == "New commodity")
        {

            if (_newCommodityNameTextField.value.Equals("Enter a name..."))
            {
                Debug.LogError("Commodity must be given a new name.");
                ShowError(_newCommodityNameTextField);
                return;
            }

            foreach (bool e in _errors)
            {
                if (e)
                {
                    Debug.LogError("Clear all errors before adding a new commodity.");
                    return;
                }
            }

            Commodity c = new Commodity();
            c.name = _newCommodityNameTextField.value;
            c.price = _newCommodityPriceIntField.value;
            c.supply = _newCommoditySupplyDoubleField.value;
            c.demand = _newCommodityDemandDoubleField.value;
            c.needLevel = _newCommodityNeedsDoubleField.value;
            c.movingAvgDuration = _newCommodityRollingAvgIntSlider.value;
            c.inflationRate = _newCommodityInflationRateDoubleField.value;

            if (_commodities.ContainsKey(c.name))
            {
                Debug.LogError($"{c.name} already exists");
                return;
            }

            StringBuilder sb = new();
            sb.Append($"{c.name}:\n\tPrice: {c.price}\n\tInitial supply: {c.supply}\n\tInitial demand: {c.demand}");
            sb.Append($"\n\tNeeds level: {c.needLevel}\n\tMoving average duration: {c.movingAvgDuration}\n\tInflation rate: {c.inflationRate}");
            string commodityString = sb.ToString();

            _commodityScrollViewLabel.text = "Commodities*";
            _commodityScrollView.Add(new Label(commodityString));

            _commodities.Add(c.name, c);
            _commoditiesDropDown.choices.Add(c.name);

            _newCommodityNameTextField.value = "Enter a name...";
            _newCommodityPriceIntField.value = 0;
            _newCommoditySupplyDoubleField.value = 0;
            _newCommodityDemandDoubleField.value = 0;
            _newCommodityNeedsDoubleField.value = 0;
            _newCommodityRollingAvgIntSlider.value = 50;
            _newCommodityInflationRateDoubleField.value = 0;
        }
        else
        {
            foreach (bool e in _errors)
            {
                if (e)
                {
                    Debug.LogError("Clear all errors before adding a new commodity.");
                    return;
                }
            }

            _commodities[_commoditiesDropDown.value].name = _newCommodityNameTextField.value;
            _commodities[_commoditiesDropDown.value].price = _newCommodityPriceIntField.value;
            _commodities[_commoditiesDropDown.value].supply = _newCommoditySupplyDoubleField.value;
            _commodities[_commoditiesDropDown.value].demand = _newCommodityDemandDoubleField.value;
            _commodities[_commoditiesDropDown.value].needLevel = _newCommodityNeedsDoubleField.value;
            _commodities[_commoditiesDropDown.value].movingAvgDuration = _newCommodityRollingAvgIntSlider.value;
            _commodities[_commoditiesDropDown.value].inflationRate = _newCommodityInflationRateDoubleField.value;
            _commoditiesDropDown.index = 0;

            RewriteScrollView();

        }

        hasUnsavedChanges = true;
    }


    /// <summary>
    /// Saves the commodities list into _configFile
    /// </summary>
    /// <param name="evt">The click event that triggered this method</param>
    private void SaveCommodities(ClickEvent evt)
    {
        _commodityScrollViewLabel.text = "Commodities";

        var options = new JsonSerializerOptions();
        options.Converters.Add(converter);
        options.WriteIndented = true;
        
        string json = JsonSerializer.Serialize(_commodities, options);
        File.WriteAllText(_configFile, json );
        AssetDatabase.Refresh();
        Debug.Log("Commodities saved!");

        _commoditiesDropDown.index = 0;
        hasUnsavedChanges = false;
    }


    /// <summary>
    /// Displays a red border around the given element to indicate an error
    /// </summary>
    /// <param name="element">The visual element to give a red border</param>
    private static void ShowError(VisualElement element)
    {
        element.style.borderBottomColor = warningColor;
        element.style.borderTopColor = warningColor;
        element.style.borderLeftColor = warningColor;
        element.style.borderRightColor = warningColor;
    }


    /// <summary>
    /// Resets the border of the provided visual element to be clear, instead of a warning color.
    /// </summary>
    /// <param name="element">The visual element to give a clear border</param>
    private static void ClearError(VisualElement element) 
    {
        element.style.borderBottomColor = Color.clear;
        element.style.borderTopColor = Color.clear;
        element.style.borderLeftColor = Color.clear;
        element.style.borderRightColor = Color.clear;
    }


    /// <summary>
    /// Validates the name given to the commodity. We don't check for "Enter a name..."
    /// here because that is template text to prompt the user to enter a new name. 
    /// 
    /// _errors[0]
    /// </summary>
    /// <param name="evt">The value change event that triggered this method</param>
    private static void ValidateName(ChangeEvent<string> evt)
    {
        if (evt == null)
        {
            Debug.LogError("Name was set to a null value");
            ShowError(_newCommodityNameTextField);
            _errors[0] = true;
            return;
        }

        _errors[0] = false;
        ClearError(_newCommodityNameTextField);
    }


    /// <summary>
    /// Validates the value given to Initial Price
    /// 
    /// _errors[1]
    /// </summary>
    /// <param name="evt">The value change event that triggered this method</param>
    private static void ValidateInitialPrice(ChangeEvent<int> evt)
    {
        if (evt == null)
        {
            Debug.LogError("Initial Price set to a null value");
            ShowError(_newCommodityPriceIntField);
            _errors[1] = true;
            return;
        }

        if (evt.newValue < 0)
        {
            Debug.LogError($"Initial Price cannot be negative: {evt.newValue}");
            ShowError(_newCommodityPriceIntField);
            _errors[1] = true;
            return;
        }

        _errors[1] = false;
        ClearError(_newCommodityPriceIntField);
    }


    /// <summary>
    /// Validates the value given to Initial Supply
    /// 
    /// _errors[2]
    /// </summary>
    /// <param name="evt">The value change event that triggered this method</param>
    private static void ValidateInitialSupply(ChangeEvent<double> evt)
    {
        if (evt == null )
        {
            Debug.LogError("Initial Supply set to a null value");
            ShowError(_newCommoditySupplyDoubleField);
            _errors[2] = true;
            return;
        }

        if (evt.newValue < 0)
        {
            Debug.LogError($"Initial Supply cannot be negative: {evt.newValue}");
            ShowError(_newCommoditySupplyDoubleField);
            _errors[2] = true;
            return;
        }

        _errors[2] = false;
        ClearError(_newCommoditySupplyDoubleField);
    }


    /// <summary>
    /// Validates the value given to Initial Demand
    /// 
    /// _errors[3]
    /// </summary>
    /// <param name="evt">The value change event that triggered this method</param>
    private static void ValidateInitialDemand(ChangeEvent<double> evt)
    {
        if (evt == null) 
        {
            Debug.LogError("Initial Demand set to a null value");
            ShowError(_newCommodityDemandDoubleField);
            _errors[3] = true;
            return;
        }

        if (evt.newValue < 0)
        {
            Debug.LogError($"Initial Demand cannot be negative: {evt.newValue}");
            ShowError(_newCommodityDemandDoubleField);
            _errors[3] = true;
            return;
        }

        _errors[3] = false;
        ClearError(_newCommodityDemandDoubleField);
    }


    /// <summary>
    /// Validates the value given to Needs Level
    /// 
    /// _errors[4]
    /// </summary>
    /// <param name="evt">The value change event that triggered this method</param>
    private static void ValidateNeedsLevel(ChangeEvent<double> evt)
    {
        if (evt == null)
        {
            Debug.LogError("Needs Level was set to a null value");
            ShowError(_newCommodityNeedsDoubleField);
            _errors[4] = true;
            return;
        }

        if (evt.newValue < 0)
        {
            Debug.LogError($"Needs Level cannot be negative: {evt.newValue}");
            ShowError(_newCommodityNeedsDoubleField);
            _errors[4] = true;
            return;
        }

        _errors[4] = false;
        ClearError(_newCommodityNeedsDoubleField);
    }

    /// <summary>
    /// Used to convert a json file of commodities into a dictionary of string keys to commodities, or vice versa.
    /// </summary>
    public class CommodityConverter : JsonConverter<Dictionary<string, Commodity>>
    {
        /// <summary>
        /// Reads in a json commodity file, and returns a dictionary representing all the commodities read.
        /// </summary>
        /// <param name="reader">json reader used to get values from the file</param>
        /// <param name="typeToConvert">end type of the converter</param>
        /// <param name="options">additional options desired</param>
        /// <returns></returns>
        public override Dictionary<string, Commodity> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Dictionary<string, Commodity> result = new Dictionary<string, Commodity>();
            Commodity c = new Commodity();

            while(reader.Read()) 
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                   string property = reader.GetString();
                    if (property == "name")
                    {
                        reader.Read();
                        c.name = reader.GetString();
                        continue;
                    }
                    else if (property == "price")
                    {
                        reader.Read();
                        c.price = reader.GetInt32();
                        continue;
                    }
                    else if (property == "inflationRate")
                    {
                        reader.Read();
                        c.inflationRate = reader.GetDouble();
                        continue;
                    }
                    else if (property == "supply")
                    {
                        reader.Read();
                        c.supply = reader.GetDouble();
                        continue;
                    }
                    else if (property == "demand")
                    {
                        reader.Read();
                        c.demand = reader.GetDouble();
                        continue;
                    }
                    else if (property == "needLevel")
                    {
                        reader.Read();
                        c.needLevel = reader.GetDouble();
                        continue;
                    }
                    else if (property == "movingAvgDuration")
                    {
                        reader.Read();
                        c.movingAvgDuration = reader.GetInt32();

                        result[c.name] = c;
                        c = new Commodity();

                        continue;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Used to write a json file from a dictionary of commodities.
        /// </summary>
        /// <param name="writer">json writer used to write the files from the dictionary</param>
        /// <param name="value">Dictionary of commodities to convert to a json file</param>
        /// <param name="options">additonal options to set for conversion</param>
        public override void Write(Utf8JsonWriter writer, Dictionary<string, Commodity> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            foreach (string k in value.Keys)
            {
                if (value[k].name == "Enter a name...") continue;

                writer.WriteStartObject();

                writer.WriteString("name", value[k].name);
                writer.WriteNumber("price", value[k].price);
                writer.WriteNumber("inflationRate", value[k].inflationRate); 
                writer.WriteNumber("supply", value[k].supply);
                writer.WriteNumber("demand", value[k].demand);
                writer.WriteNumber("needLevel", value[k].needLevel);
                writer.WriteNumber("movingAvgDuration", value[k].movingAvgDuration);

                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }
    }

    /// <summary>
    /// Fills the simulations selection dropdown with all current simulations
    /// </summary>
    private static void fillSimulations(MouseOverEvent evt)
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
    /// On close/deletion of the commodity configuration window, alerts the quick start guide
    /// </summary>
    private void OnDestroy()
    {
        QuickStartGuide.NotifyOfClose(QuickStartGuide.UI_Type.CommodityConfig);
    }

    /// <summary>
    /// Called on closing window with unsaved changes
    /// </summary>
    public override void SaveChanges()
    {
        _commodityScrollViewLabel.text = "Commodities";

        var options = new JsonSerializerOptions();
        options.Converters.Add(converter);
        options.WriteIndented = true;

        string json = JsonSerializer.Serialize(_commodities, options);
        File.WriteAllText(_configFile, json);
        AssetDatabase.Refresh();
        Debug.Log("Commodities saved!");

        _commoditiesDropDown.index = 0;
        base.SaveChanges();
    }
}
