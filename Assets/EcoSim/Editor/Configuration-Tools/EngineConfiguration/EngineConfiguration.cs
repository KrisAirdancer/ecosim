using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static CommodityConfiguration;
using static EcoSim.EcoEngine;

/// <summary>
/// Custom UI tool for configuring the engine settings of all the simulations. Can create or delete simulations, and set the
/// round time for all simulations. Can also import Unity objects to Ecosim commodities
/// </summary>
public class EngineConfiguration : EditorWindow
{

    // Metadata
    /// <summary>
    /// File path of the engine configuration json file.
    /// </summary>
    private static readonly string _configFile = Application.dataPath + "/EcoSim/Runtime/Configuration/engineConfig.json";
    
    /// <summary>
    /// Location of the uxml file that the visual components of this tool are based on.
    /// </summary>
    private static readonly string _uxml = "Assets/EcoSim/Editor/Configuration-Tools/EngineConfiguration/EngineConfiguration.uxml";

    /// <summary>
    /// Dictionary representation of all the engine settings that the tool can affect.
    /// </summary>
    private static Dictionary<string, string> _config;
    private static string _commodityFile;
    private static string _exportListFile;
    private static HashSet<string> _exportList = new HashSet<string>();
    private static HashSet<Commodity> _commodities = new HashSet<Commodity>();
    private static readonly Color warningColor = Color.red;

    // Elements
    /// <summary>
    /// Unity visual element consisting of a tool to enter in the name for a new simulation.
    /// </summary>
    private static TextField _newSimulationNameTextField;

    /// <summary>
    /// Unity visual element consisting of a tool that when clicked creates a new simulation if possible.
    /// </summary>
    private static Button _createSimulationButton;

    /// <summary>
    /// Unity visual element consisting of a label that provides information as to what a valid simulation name is.
    /// </summary>
    private static Label _createSimInfoLabel;

    /// <summary>
    /// Unity visual element consisting of a dropdown tool that contains the names of all current simulations.
    /// </summary>
    private static DropdownField _simulationSelectionDropDown;

    /// <summary>
    /// Unity visual element consisting of a tool that when clicked deletes the selected simulation.
    /// </summary>
    private static Button _deleteSimulationButton;

    /// <summary>
    /// Label used to indicate the save status of the engine settings
    /// </summary>
    private static Label _simulationSettingsLabel;

    /// <summary>
    /// Unity visual element consisting of a tool that can be changed to affect the time between rounds of the simulations.
    /// </summary>
    private static SliderInt _tickRateSlider;


    /// <summary>
    /// Label used to indicate the save status of the currency settings
    /// </summary>
    private static Label _currencySettingsLabel;

    /// <summary>
    /// Unity visual element consisting of a tool to enter in the name for the currency used by the simulation.
    /// </summary>
    private static TextField _currencyNameTextField;

    /// <summary>
    /// Unity visual element consisting of a tool to enter in the name for the unit of the currency used by the simulation.
    /// </summary>
    private static TextField _currencyUnitTextField;

    /// <summary>
    /// Unity visual element consisting of a tool to enter in the name for the symbol of the currency used by the simulation.
    /// </summary>
    private static TextField _currencySymbolTextField;

    /// <summary>
    /// Unity visual element consisting of a tool that when clicked saves all of the changed settings to file.
    /// </summary>
    private static Button _saveSettingsButton;

    /// <summary>
    /// Unity visual element consisting of a tool that when clicked tries to convert the objects in the selected directory to Ecosim commodities.
    /// </summary>
    private static Button _exportButton;

    /// <summary>
    /// Unity visual element consisting of a tool to enter in the path to the objects that should be made into Ecosim commodities.
    /// </summary>
    private static TextField _pathToSOTextField;

    /// <summary>
    /// Unity visual element that informs the user as to the status of the export
    /// </summary>
    private static Label _exportOutputLabel;
    private static EditorWindow e;
    private static Toggle _exportDataToggle;

    // Data Export elements
    private static Toggle _exportAllDataToggle;
    private static Toggle _exportPriceToggle;
    private static Toggle _exportSupplyToggle;
    private static Toggle _exportDemandToggle;
    private static Toggle _exportAverageHistoryToggle;
    private static IntegerField _exportMinRoundIntField;
    private static IntegerField _exportMaxRoundIntField;
    private static Toggle _exportAllCommoditiesToggle;
    private static DropdownField _commoditiesDropDown;
    private static Button _addExportCommodityButton;
    private static Button _removeExportCommodityButton;
    private static Button _removeAllExportCommoditiesButton;
    private static ScrollView _exportCommoditiesScrollView;


    /// <summary>
    /// Shows the current engine configuration tool if one exists, if not creates a new one.
    /// Also alerts the quick start guide that engine configuration tool has been opened.
    /// </summary>
    [MenuItem("EcoSim/Engine configuration", false, 111)]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(EngineConfiguration));

        QuickStartGuide.NotifyOfOpen(QuickStartGuide.UI_Type.EngineConfig);
    }

    /// <summary>
    /// Creates a new engine configuration tool, and fills in the values that it can from file. Also sets up the back end logic.
    /// </summary>
    public void CreateGUI()
    {
        // Set up the window
        EditorWindow editorWindow = GetWindow(typeof(EngineConfiguration));
        var root = editorWindow.rootVisualElement;
        VisualTreeAsset engineConfigAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(_uxml);
        VisualElement engineConfigUI = engineConfigAsset.Instantiate();
        root.Add(engineConfigUI);

        // Collecting the UI elements
        _newSimulationNameTextField = root.Q<TextField>("newSimulationNameTextField");
        _createSimulationButton = root.Q<Button>("createSimulationButton");
        _createSimInfoLabel = root.Q<Label>("createSimInfoLabel");
        _simulationSelectionDropDown = root.Q<DropdownField>("simulationSelectionDropDown");
        _deleteSimulationButton = root.Q<Button>("deleteSimulationButton");

        _simulationSettingsLabel = root.Q<Label>("simulationSettingsLabel");
        _tickRateSlider = root.Q<SliderInt>("timeStep");

        _currencySettingsLabel = root.Q<Label>("currencySettingsLabel");
        _currencyNameTextField = root.Q<TextField>("currencyName");
        _currencyUnitTextField = root.Q<TextField>("currencyUnit");
        _currencySymbolTextField = root.Q<TextField>("currencySymbol");

        _exportDataToggle = root.Q<Toggle>("exportDataToggle");

        _saveSettingsButton = root.Q<Button>("saveEngineConfig");

        //Set up the setup button controls
        _pathToSOTextField = root.Q<TextField>("exportField");
        _exportButton = root.Q<Button>("exportButton");
        _exportOutputLabel = root.Q<Label>("exportResult");
        _exportButton.text = "Import Scriptable Objects";
        _pathToSOTextField.value = "Assets/";
        _exportOutputLabel.style.color = Color.white;
        _exportButton.RegisterCallback<ClickEvent>(ExportButtonClickHandler);


        // Data export elements
        _exportAllDataToggle = root.Q<Toggle>("exportAllDataToggle");
        _exportPriceToggle = root.Q<Toggle>("exportPriceToggle");
        _exportSupplyToggle = root.Q<Toggle>("exportSupplyToggle");
        _exportDemandToggle = root.Q<Toggle>("exportDemandToggle");
        _exportAverageHistoryToggle = root.Q<Toggle>("exportAverageHistoryToggle");
        _exportMinRoundIntField = root.Q<IntegerField>("exportMinRoundIntField");
        _exportMaxRoundIntField = root.Q<IntegerField>("exportMaxRoundIntField");

        _exportAllCommoditiesToggle = root.Q<Toggle>("exportAllCommoditiesToggle");
        _commoditiesDropDown = root.Q<DropdownField>("exportCommodityDropDown");
        _addExportCommodityButton = root.Q<Button>("addExportCommodityButton");
        _removeExportCommodityButton = root.Q<Button>("removeExportCommodityButton");
        _removeAllExportCommoditiesButton = root.Q<Button>("removeAllExportCommoditiesButton");
        _exportCommoditiesScrollView = root.Q<ScrollView>("exportCommoditiesScrollView");

        //Setting up playmode tooltips
        _tickRateSlider.AddManipulator(new PlayModeToolTip(root));
        _currencyNameTextField.AddManipulator(new PlayModeToolTip(root));
        _currencyUnitTextField.AddManipulator(new PlayModeToolTip(root));
        _currencySymbolTextField.AddManipulator(new PlayModeToolTip(root));
        _saveSettingsButton.AddManipulator(new PlayModeToolTip(root));
        _simulationSelectionDropDown.AddManipulator(new PlayModeToolTip(root));
        _newSimulationNameTextField.AddManipulator(new PlayModeToolTip(root));

        // Connecting events to the elements
        _saveSettingsButton.RegisterCallback<ClickEvent>(SaveConfig);
        _tickRateSlider.RegisterCallback<ChangeEvent<int>>(TickRateChanged);
        _currencyNameTextField.RegisterValueChangedCallback(MoneyNameChanged);
        _currencyUnitTextField.RegisterValueChangedCallback(MoneyUnitChanged);
        _currencySymbolTextField.RegisterValueChangedCallback(MoneySymbolChanged);

        // Data Export callbacks
        _exportDataToggle.RegisterValueChangedCallback(ExportDataFiltersDisplay);
        _exportAllDataToggle.RegisterValueChangedCallback(ExportAllData);
        _exportPriceToggle.RegisterValueChangedCallback(ToggleAllDataFilter);
        _exportSupplyToggle.RegisterValueChangedCallback(ToggleAllDataFilter);
        _exportDemandToggle.RegisterValueChangedCallback(ToggleAllDataFilter);
        _exportAverageHistoryToggle.RegisterValueChangedCallback(ToggleAllDataFilter);
        _exportMinRoundIntField.RegisterValueChangedCallback(ValidateExportMinRound);
        _exportMaxRoundIntField.RegisterValueChangedCallback(ValidateExportMaxRound);

        _exportAllCommoditiesToggle.RegisterValueChangedCallback(ExportAllCommoditiesFilter);
        _addExportCommodityButton.RegisterCallback<ClickEvent>(AddCommodityExport);
        _removeExportCommodityButton.RegisterCallback<ClickEvent>(RemoveCommodityExport);
        _removeAllExportCommoditiesButton.RegisterCallback<ClickEvent>(RemoveAllCommoditiesExport);
        _commoditiesDropDown.RegisterValueChangedCallback(CommodityValueChanged);

        // Simulation callbacks
        _createSimulationButton.RegisterCallback<ClickEvent>(CreateSim);
        _deleteSimulationButton.RegisterCallback<ClickEvent>(DeleteSim);
        _simulationSelectionDropDown.RegisterCallback<MouseOverEvent>(FillSimulations);
        _simulationSelectionDropDown.RegisterValueChangedCallback(PopulateFields);


        // Disable until a simulation is selected
        _deleteSimulationButton.SetEnabled(false);
        _exportDataToggle.SetEnabled(false);
        hasUnsavedChanges = false;
    }


    /// <summary>
    /// Removes all commodities from the export list
    /// </summary>
    /// <param name="evt"></param>
    private void RemoveAllCommoditiesExport(ClickEvent evt)
    {
        _exportAllCommoditiesToggle.value = false;
        _exportList.Clear();
        DrawExportList();
    }


    /// <summary>
    /// Update the add and remove buttons accordingly if the commodity exists in the exportList or not
    /// </summary>
    /// <param name="evt"></param>
    private void CommodityValueChanged(ChangeEvent<string> evt)
    {
        if (_exportList.Contains(evt.newValue))
        {
            _addExportCommodityButton.SetEnabled(false);
            _removeExportCommodityButton.SetEnabled(true);
        }
        else
        {
            _addExportCommodityButton.SetEnabled(true);
            _removeExportCommodityButton.SetEnabled(false);
        }
    }


    // Redraws the scroll view to display the contents of what commodities will be exported
    private static void DrawExportList()
    {
        _exportCommoditiesScrollView.Clear();
        foreach (string s in _exportList)
        {
            _exportCommoditiesScrollView.Add(new Label($"{s}"));
        }
        if (_exportDataToggle.value == true) _exportCommoditiesScrollView.visible = true;
        else _exportCommoditiesScrollView.visible = false;
    }

    /// <summary>
    /// Remove the selected commodity from the export list
    /// </summary>
    /// <param name="evt"></param>
    private void RemoveCommodityExport(ClickEvent evt)
    {
        if (!_exportList.Contains(_commoditiesDropDown.value))
        {
            Debug.LogError($"{_commoditiesDropDown.value} cannot be removed since it's not in the export list");
            return;
        }
        else
        {
            _exportList.Remove(_commoditiesDropDown.value);
            DrawExportList();
            _addExportCommodityButton.SetEnabled(true);
            _removeExportCommodityButton.SetEnabled(false);
            if (_exportAllCommoditiesToggle.value)
            {
                _exportAllCommoditiesToggle.value = false;
            }
        }
    }


    /// <summary>
    /// Add the selected commodity to the export list
    /// </summary>
    /// <param name="evt"></param>
    private void AddCommodityExport(ClickEvent evt)
    {
        if (_exportList.Contains(_commoditiesDropDown.value))
        {
            Debug.LogError($"{_commoditiesDropDown.value} is already in the export list");
            return;
        }
        else
        {
            _exportList.Add(_commoditiesDropDown.value);
            DrawExportList();
            _addExportCommodityButton.SetEnabled(false);
            _removeExportCommodityButton.SetEnabled(true);
        }
    }


    /// <summary>
    /// Alter the state of the commodities section of data export filters when "All Commodities" is flipped on or off
    /// </summary>
    /// <param name="evt"></param>
    private void ExportAllCommoditiesFilter(ChangeEvent<bool> evt)
    {
        if (evt == null)
        {
            Debug.LogError("Export all commodities toggle value is null");
            ShowError(_exportAllCommoditiesToggle);
            return;
        }

        ClearError(_exportAllCommoditiesToggle);
        if (evt.newValue)
        {
            _exportList.Clear();
            foreach (Commodity c in _commodities)
            {
                _exportList.Add(c.name);
            }

            _addExportCommodityButton.SetEnabled(false);
            _removeExportCommodityButton.SetEnabled(true);
            DrawExportList();
        }
    }


    /// <summary>
    /// Validates if the First round for data exporting is a valid round number
    /// </summary>
    /// <param name="evt"></param>
    private static void ValidateExportMinRound(ChangeEvent<int> evt)
    {
        if (evt == null)
        {
            Debug.LogError("Minimum round for exporting cannot be null");
            ShowError(_exportMinRoundIntField);
            _saveSettingsButton.SetEnabled(false);
            return;
        }

        if (evt.newValue < 0)
        {
            Debug.LogError("Minimum round cannot be negative");
            ShowError(_exportMinRoundIntField);
            _saveSettingsButton.SetEnabled(false);
            return;
        }

        if (evt.newValue > _exportMaxRoundIntField.value && _exportMaxRoundIntField.value != -1)
        {
            Debug.LogError("Minimum round cannot be less than the maximum round");
            ShowError(_exportMinRoundIntField);
            _saveSettingsButton.SetEnabled(false);
            return;
        }


        ClearError(_exportMinRoundIntField);
        _saveSettingsButton.SetEnabled(true);
    }


    /// <summary>
    /// Validates the maximum round for data exporting
    /// </summary>
    /// <param name="evt"></param>
    private static void ValidateExportMaxRound(ChangeEvent<int> evt)
    {
        if (evt == null)
        {
            Debug.LogError("Maximum round for exporting cannot be null");
            ShowError(_exportMaxRoundIntField);
            _saveSettingsButton.SetEnabled(false);
            return;
        }

        if (evt.newValue < -1)
        {
            Debug.LogError("Maximum round for exporting cannot be negative (-1 only if all rounds until simulation termination are to be exported)");
            ShowError(_exportMaxRoundIntField);
            _saveSettingsButton.SetEnabled(false);
            return;
        }

        if (evt.newValue != -1 && evt.newValue < _exportMinRoundIntField.value)
        {
            Debug.LogError("Maximum round cannot be less than minimum round");
            ShowError(_exportMaxRoundIntField);
            _saveSettingsButton.SetEnabled(false);
            return;
        }

        ClearError(_exportMaxRoundIntField);
        _saveSettingsButton.SetEnabled(true);
    }


    /// <summary>
    /// If all data toggle is true, and a filter toggle is flipped to true, set all data to false
    /// </summary>
    /// <param name="evt"></param>
    private static void ToggleAllDataFilter(ChangeEvent<bool> evt)
    {
        if (evt.newValue == true)
        {
            if (_exportAllDataToggle.value == true)
            {
                _exportAllDataToggle.value = false;
            }
        }

    }

    /// <summary>
    /// Toggles whether we just want all the data exported or just some of the data
    /// </summary>
    /// <param name="evt"></param>
    private static void ExportAllData(ChangeEvent<bool> evt)
    {
        if (evt.newValue == true)
        { // Since we want all data, turn the other filters off (except for the round min/max values
            _exportPriceToggle.value = false;
            _exportSupplyToggle.value = false;
            _exportDemandToggle.value = false;
            _exportAverageHistoryToggle.value = false;
        }

        // If value == false, we don't care to do anything here, the user decides what fields they want exported
    }


    /// <summary>
    /// Loads the selected engine config file, and fills all fields
    /// </summary>
    private void PopulateFields(ChangeEvent<string> evt)
    {
        if (_config != null) _config.Clear();
        _commodityFile = Application.dataPath + "/EcoSim/Runtime/Configuration/" + _simulationSelectionDropDown.value + "/commodityConfig.json";
        _exportListFile = Application.dataPath + "/EcoSim/Runtime/Configuration/" + _simulationSelectionDropDown.value + "/commodityExportList.json";
        LoadEngineConfig();


        // Populating the fields with the values read from the file
        _tickRateSlider.UnregisterCallback<ChangeEvent<int>>(TickRateChanged);
        _currencyNameTextField.UnregisterCallback<ChangeEvent<string>>(MoneyNameChanged);
        _currencyUnitTextField.UnregisterCallback<ChangeEvent<string>>(MoneyUnitChanged);
        _currencySymbolTextField.UnregisterCallback<ChangeEvent<string>>(MoneySymbolChanged);

        _tickRateSlider.value = Int32.Parse(_config["timeStep"]);
        _currencyNameTextField.value = _config["currencyName"];
        _currencyUnitTextField.value = _config["currencyUnit"];
        _currencySymbolTextField.value = _config["currencySymbol"];

        _exportDataToggle.value = _config["export"].Equals("0") ? false : true;
        _exportPriceToggle.value = _config["exportPrice"].Equals("0") ? false : true;
        _exportSupplyToggle.value = _config["exportSupply"].Equals("0") ? false : true;
        _exportDemandToggle.value = _config["exportDemand"].Equals("0") ? false : true;
        _exportAverageHistoryToggle.value = _config["exportAvgHistory"].Equals("0") ? false : true;
        _exportMinRoundIntField.value = Int32.Parse(_config["exportMinRound"]);
        _exportMaxRoundIntField.value = Int32.Parse(_config["exportMaxRound"]);
        _exportAllCommoditiesToggle.value = _config["exportAllCommodities"].Equals("0") ? false : true;

        // Handle the commodity list for the simulation
        _commodities.Clear();
        if (File.Exists(_commodityFile))
        {
            string commodityList = File.ReadAllText(_commodityFile);
            _commodities = JsonSerializer.Deserialize<HashSet<Commodity>>(commodityList);
            foreach (Commodity c in _commodities)
            {
                _commoditiesDropDown.choices.Add(c.name);
            }
        }
        else
        {
            Debug.LogError($"Commodity list does not exist for {_simulationSelectionDropDown.value} yet");
        }

        // Handle the export list for the simulation
        _exportList.Clear();
        if (!File.Exists(_exportListFile))
        {
            File.Create(_exportListFile).Dispose();
            if (_exportAllCommoditiesToggle.value)
            {
                foreach(Commodity c in _commodities)
                {
                    _exportList.Add(c.name);
                }
            }

            Debug.Log("Export list created");
            AssetDatabase.Refresh();
        }

        string exportListJson = File.ReadAllText(_exportListFile);
        if (!String.IsNullOrEmpty(exportListJson))
        {
            _exportList = JsonSerializer.Deserialize<HashSet<string>>(exportListJson);
        }

        if (_exportPriceToggle.value && _exportSupplyToggle.value && _exportDemandToggle.value && _exportAverageHistoryToggle.value)
        {
            _exportAllDataToggle.value = true;
        }
        else
        {
            _exportAllDataToggle.value = false;
        }

        DrawExportList();
        _tickRateSlider.SetEnabled(true);
        _currencyNameTextField.SetEnabled(true);
        _currencyUnitTextField.SetEnabled(true);
        _currencySymbolTextField.SetEnabled(true);
        _saveSettingsButton.SetEnabled(true);
        _deleteSimulationButton.SetEnabled(true);
        _exportDataToggle.SetEnabled(true);


        ResetUI();
        Thread backgroundThread = new Thread(new ThreadStart(ReRegister));
        backgroundThread.Start();

    }

    private void ReRegister()
    {
        _tickRateSlider.RegisterCallback<ChangeEvent<int>>(TickRateChanged);
        _currencyNameTextField.RegisterValueChangedCallback(MoneyNameChanged);
        _currencyUnitTextField.RegisterValueChangedCallback(MoneyUnitChanged);
        _currencySymbolTextField.RegisterValueChangedCallback(MoneySymbolChanged);
    }


    /// <summary>
    /// Check for the existence of 'engineConfig.json' and loads it. Creates the file with template data if not found. 0
    /// </summary>
    private static void LoadEngineConfig()
    {
        if (!File.Exists(_configFile))
        {
            Debug.Log("Engine config does not exist. Creating...");

            string template =
@"{
    ""timeStep"": ""100"",
    ""currencyName"": ""US Dollar"",
    ""currencyUnit"": ""dollar"",
    ""currencySymbol"": ""$"",
    ""export"": ""0"",
    ""exportPrice"": ""1"",
    ""exportSupply"": ""1"",
    ""exportDemand"": ""1"",
    ""exportAvgHistory"": ""1"",
    ""exportMinRound"": ""0"",
    ""exportMaxRound"": ""-1"",
    ""exportAllCommodities"": ""1""
}";

            File.Create(_configFile).Dispose();
            File.WriteAllText(_configFile, template);

            AssetDatabase.Refresh();
            Debug.Log("\'engineConfig.json\' has been created.");
        }
        else
        {
            Debug.Log("'engineConfig.json' was found.");
        }

        string json = File.ReadAllText(_configFile);
        _config = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

        if (!File.Exists(_commodityFile))
        {
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

            File.WriteAllText(_commodityFile, template);
        }

        string commodityJson = File.ReadAllText(_commodityFile);
        _commodities = JsonSerializer.Deserialize<HashSet<Commodity>>(commodityJson);
    }


    /// <summary>
    /// Saves the engine configuration settings, as seen on the inspector, to 'engineConfig.json' 
    /// </summary>
    /// <param name="evt">The click event that triggered this method</param>
    private void SaveConfig(ClickEvent evt)
    {
        Debug.Log("Settings saved!");

        ResetUI();

        _config["timeStep"] = _tickRateSlider.value.ToString();
        _config["currencyName"] = _currencyNameTextField.value;
        _config["currencyUnit"] = _currencyUnitTextField.value;
        _config["currencySymbol"] = _currencySymbolTextField.value;


        if (_exportDataToggle.value)
        {
            if (_exportAllDataToggle.value)
            {
                _config["export"] = "1";
                _config["exportPrice"] = "1";
                _config["exportSupply"] = "1";
                _config["exportDemand"] = "1";
                _config["exportAvgHistory"] = "1";
            }
            else
            {
                _config["export"] = "1";
                _config["exportPrice"] = _exportPriceToggle.value ? "1" : "0";
                _config["exportSupply"] = _exportSupplyToggle.value ? "1" : "0";
                _config["exportDemand"] = _exportDemandToggle.value ? "1" : "0";
                _config["exportAvgHistory"] = _exportAverageHistoryToggle.value ? "1" : "0";
            }

            _config["exportMinRound"] = _exportMinRoundIntField.value.ToString();
            _config["exportMaxRound"] = _exportMaxRoundIntField.value.ToString();
            _config["exportAllCommodities"] = _exportAllCommoditiesToggle.value ? "1" : "0";

        }
        else
        {
            _config["export"] = "0";
            _config["exportPrice"] = "0";
            _config["exportSupply"] = "0";
            _config["exportDemand"] = "0";
            _config["exportAvgHistory"] = "0";
            _config["exportMinRound"] = "0";
            _config["exportMaxRound"] = "0";
            _config["exportAllCommodities"] = "0";
        }

        string json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_configFile, json);

        string exportListJson = JsonSerializer.Serialize(_exportList, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_exportListFile, exportListJson);

        AssetDatabase.Refresh();
        hasUnsavedChanges = false;
    }

    /// <summary>
    /// Handle when the export button is clicked.
    /// Calls EcoSimBridge.ExportScriptableObjectsToCommodities
    /// Calls CommodityConfiguration.Refresh
    /// </summary>
    /// <param name="evt">The click event that triggered this method</param>
    private static void ExportButtonClickHandler(ClickEvent evt)
    {
        if (Application.isPlaying)
        {
            _exportOutputLabel.style.color = Color.red;
            _exportOutputLabel.text = "Error can not export if the game is running";
            return;
        }
        if(_simulationSelectionDropDown.value == null)
        {
            _exportOutputLabel.style.color = Color.red;
            _exportOutputLabel.text = "Error please select a Simulation to import the commodities into";
            return;
        }
        try
        {
            string path = _pathToSOTextField.value;
            if (!path.EndsWith("/"))
            {
                path += "/";
            }
            string configPath = Application.dataPath + "/EcoSim/Runtime/Configuration/"+ _simulationSelectionDropDown.value +"/commodityConfig.json";
            Debug.Log($"{configPath}");
            EcoSimBridge.ExportScriptableObjectsToCommodities(path, configPath);
            _exportOutputLabel.style.color = Color.white;
            _exportOutputLabel.text = "Export Successful";
            CommodityConfiguration.Refresh();

        }catch(NotImplementedException)
        {
            _exportOutputLabel.style.color= Color.red;
            _exportOutputLabel.text="Error Implement EcoSim Bridge Class";
        }
    }


    /// <summary>
    /// Updates the UI to alert the user that the tickRate has changed to a new, unsaved value
    /// </summary>
    /// <param name="evt">The value changed event that triggered this method</param>
    private void TickRateChanged(ChangeEvent<int> evt) 
    {
        _simulationSettingsLabel.text = "Simulation Settings*";
        hasUnsavedChanges = true;
    }


    /// <summary>
    /// Alert the user that the name of money has been changed but is unsaved
    /// </summary>
    /// <param name="evt">The value changed event that triggered this method</param>
    private void MoneyNameChanged(ChangeEvent<string> evt)
    {
        _currencySettingsLabel.text = "Currency Settings*";
        hasUnsavedChanges = true;
    }


    /// <summary>
    /// Alert the user that the unit of currency has been changed but is unsaved
    /// </summary>
    /// <param name="evt">The value changed event that triggered this method</param>
    private void MoneyUnitChanged(ChangeEvent<string> evt)
    {
        _currencySettingsLabel.text = "Currency Settings*";
        hasUnsavedChanges = true;
    }


    /// <summary>
    /// Alert the user that the symbol of currency has been changed but is unsaved
    /// </summary>
    /// <param name="evt">The value changed event that triggered this method</param>
    private void MoneySymbolChanged(ChangeEvent<string> evt)
    {
        _currencySettingsLabel.text = "Currency Settings*";
        hasUnsavedChanges = true;
    }


    /// <summary>
    /// Reset the UI to default look to reflect the state has been saved
    /// </summary>
    private static void ResetUI()
    {
        // Change settings title to reflect no changes
        _simulationSettingsLabel.text = "Simulation Settings";

        // Change currency settings title
        _currencySettingsLabel.text = "Currency Settings";
    }

    /// <summary>
    /// Adds the new simulation, if it does not already exist
    /// </summary>
    private static void CreateSim(ClickEvent evt)
    {
        List<string> sims = new List<string>();
        string[] dirs = Directory.GetDirectories(Application.dataPath + "/EcoSim/Runtime/Configuration/");
        foreach (string sim in dirs)
        {
            string[] simSplit = sim.Split('/');
            sims.Add(simSplit[simSplit.Length - 1]);
        }
        if (_newSimulationNameTextField.value != "" && !sims.Contains(_newSimulationNameTextField.value))
        {
            Directory.CreateDirectory(Application.dataPath + "/EcoSim/Runtime/Configuration/" + _newSimulationNameTextField.value);
            _newSimulationNameTextField.style.borderBottomColor = Color.clear;
            _newSimulationNameTextField.style.borderTopColor = Color.clear;
            _newSimulationNameTextField.style.borderLeftColor = Color.clear;
            _newSimulationNameTextField.style.borderRightColor = Color.clear;
            _createSimInfoLabel.text = "Simulation Created";
            AssetDatabase.Refresh();
        }
        else
        {
            if (_newSimulationNameTextField.value == "") _createSimInfoLabel.text = "*Simulation name cannot be null*";
            else _createSimInfoLabel.text = "*Simulation name has to be unique*";
            _newSimulationNameTextField.style.borderBottomColor = Color.red;
            _newSimulationNameTextField.style.borderTopColor = Color.red;
            _newSimulationNameTextField.style.borderLeftColor = Color.red;
            _newSimulationNameTextField.style.borderRightColor = Color.red;
        }
    }

    /// <summary>
    /// Deletes the simulation selected by the simulation selection dropdown
    /// </summary>
    private static void DeleteSim(ClickEvent evt)
    {
        Directory.Delete(Application.dataPath + "/EcoSim/Runtime/Configuration/" + _simulationSelectionDropDown.value, true);
        File.Delete(Application.dataPath + "/EcoSim/Runtime/Configuration/" + _simulationSelectionDropDown.value + ".meta");
        AssetDatabase.Refresh();

        _deleteSimulationButton.SetEnabled(false);
        _exportDataToggle.SetEnabled(false);
    }

    /// <summary>
    /// Fills the simulations selection dropdown with all current simulations
    /// </summary>
    private static void FillSimulations(MouseOverEvent evt)
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
    /// On engine configuration window close/deletion, alerts the quick start guide tool
    /// </summary>
    public void OnDestroy()
    {
        QuickStartGuide.NotifyOfClose(QuickStartGuide.UI_Type.EngineConfig);
    }


    // Flip the fiters on/off visible/invisible as the toggle flips
    private static void ExportDataFiltersDisplay(ChangeEvent<bool> evt)
    {
        if (evt.newValue == true)
        { // Enable and display export filters 
            _exportAllDataToggle.visible = true;
            _exportAllDataToggle.SetEnabled(true);
            _exportAllDataToggle.value = true;

            _exportPriceToggle.visible = true;
            _exportPriceToggle.SetEnabled(true);
            _exportPriceToggle.value = false;

            _exportSupplyToggle.visible = true;
            _exportSupplyToggle.SetEnabled(true);
            _exportSupplyToggle.value = false;

            _exportDemandToggle.visible = true;
            _exportDemandToggle.SetEnabled(true);
            _exportDemandToggle.value = false;

            _exportAverageHistoryToggle.visible = true;
            _exportAverageHistoryToggle.SetEnabled(true);
            _exportAverageHistoryToggle.value = false;

            _exportMinRoundIntField.visible = true;
            _exportMinRoundIntField.SetEnabled(true);
            _exportMinRoundIntField.value = 0;

            _exportMaxRoundIntField.visible = true;
            _exportMaxRoundIntField.SetEnabled(true);
            _exportMaxRoundIntField.value = -1;

            _exportAllCommoditiesToggle.visible = true;
            _exportAllCommoditiesToggle.SetEnabled(true);
            _exportAllCommoditiesToggle.value = true;

            _commoditiesDropDown.visible = true;
            _commoditiesDropDown.SetEnabled(true);
            _commoditiesDropDown.index = 0;

            _addExportCommodityButton.visible = true;
            _addExportCommodityButton.SetEnabled(true);

            _removeExportCommodityButton.visible = true;
            _removeExportCommodityButton.SetEnabled(true);

            _removeAllExportCommoditiesButton.visible = true;
            _removeAllExportCommoditiesButton.SetEnabled(true);

            _exportCommoditiesScrollView.visible = true;

        }
        else
        { // Disable and hide export filters
            _exportAllDataToggle.visible = false;
            _exportAllDataToggle.SetEnabled(false);
            _exportAllDataToggle.value = true;

            _exportPriceToggle.visible = false;
            _exportPriceToggle.SetEnabled(false);
            _exportPriceToggle.value = false;

            _exportSupplyToggle.visible = false;
            _exportSupplyToggle.SetEnabled(false);
            _exportSupplyToggle.value = false;

            _exportDemandToggle.visible = false;
            _exportDemandToggle.SetEnabled(false);
            _exportDemandToggle.value = false;

            _exportAverageHistoryToggle.visible = false;
            _exportAverageHistoryToggle.SetEnabled(false);
            _exportAverageHistoryToggle.value = false;

            _exportMinRoundIntField.visible = false;
            _exportMinRoundIntField.SetEnabled(false);
            _exportMinRoundIntField.value = 0;

            _exportMaxRoundIntField.visible = false;
            _exportMaxRoundIntField.SetEnabled(false);
            _exportMaxRoundIntField.value = -1;

            _exportAllCommoditiesToggle.visible = false;
            _exportAllCommoditiesToggle.SetEnabled(false);
            _exportAllCommoditiesToggle.value = false;

            _commoditiesDropDown.visible = false;
            _commoditiesDropDown.SetEnabled(false);
            _commoditiesDropDown.index = 0;

            _addExportCommodityButton.visible = false;
            _addExportCommodityButton.SetEnabled(false);

            _removeExportCommodityButton.visible = false;
            _removeExportCommodityButton.SetEnabled(false);

            _removeAllExportCommoditiesButton.visible = false;
            _removeAllExportCommoditiesButton.SetEnabled(false);

            _exportCommoditiesScrollView.visible = false;
        }
    }

    /// <summary>
    /// Displays a red boarder around the given element to indicate an error
    /// </summary>
    /// <param name="element"></param>
    private static void ShowError(VisualElement element)
    {
        element.style.borderBottomColor = warningColor;
        element.style.borderTopColor = warningColor;
        element.style.borderLeftColor = warningColor;
        element.style.borderRightColor = warningColor;
    }

    private static void ClearError(VisualElement element)
    {
        element.style.borderBottomColor = Color.clear;
        element.style.borderTopColor = Color.clear;
        element.style.borderLeftColor = Color.clear;
        element.style.borderRightColor = Color.clear;
    }


    /// <summary>
    /// Enables the delete simulation button
    /// </summary>
    /// <param name="evt">The value changed event that triggered this method</param>
    private static void enableDeleteButton(ChangeEvent<string> evt)
    {
        _deleteSimulationButton.SetEnabled(true);
    }

    /// <summary>
    /// Called when saving changes on window close
    /// </summary>
    public override void SaveChanges()
    {
        SaveConfig(new ClickEvent());

        string json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_configFile, json);

        base.SaveChanges();
    }
}