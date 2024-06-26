using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using EcoSim;
using UnityEngine;
using static EcoSim.EcoEngine;
using System.IO;
using System.Text.Json;
using System.Text;
using System;

/// <summary>
/// File controlling the logic of the event EditorWindow, handles connections 
/// between the simulation and the Unity UI tool, and triggering things on user interaction.
/// </summary>
public class Events : EditorWindow
{
    /// <summary>
    /// Unity visual element consisting of a dropdown tool that contains the names of all current simulations.
    /// </summary>
    private DropdownField _simulationSelectionDropDown;

    /// <summary>
    /// Unity visual element consisting of a dropdown tool that contains the names of all commodities in the currently selected simulation.
    /// </summary>
    private DropdownField _commodityDropDownField;

    /// <summary>
    /// Unity visual element that can be changed to set the amount that inflation will change for the selected commodity.
    /// </summary>
    private IntegerField _inflationShiftIntegerField;

    /// <summary>
    /// Unity visual element that can be changed to set the amount that supply will change for the selected commodity.
    /// </summary>
    private IntegerField _supplyShiftIntegerField;

    /// <summary>
    /// Unity visual element that can be changed to set the amount that demand will change for the selected commodity.
    /// </summary>
    private IntegerField _demandShiftIntegerField;

    /// <summary>
    /// Unity visual element that can be changed to set the amount that the need level will change for the selected commodity.
    /// </summary>
    private IntegerField _needShiftIntegerField;

    /// <summary>
    /// Unity visual element that can be changed to set the amount that inflation will change for the selected commodity.
    /// </summary>
    private IntegerField _movingAvgDurationShiftIntegerField;

    /// <summary>
    /// Unity visual element used to display the status, and any potential issues, with the current event pieces settings.
    /// </summary>
    private Label _eventPieceStatusLabel;

    /// <summary>
    /// Unity visual element that when interacted with will add the event piece to enable it to be run, but does not save it to file.
    /// </summary>
    private UnityEngine.UIElements.Button _addEventPieceButton;

    /// <summary>
    /// Unity visual element that shows all event pieces that are currently ready to run.
    /// </summary>
    private ScrollView _eventPieceScrollView;

    /// <summary>
    /// Unity visual element consisting of a dropdown tool that contains a reference to all event pieces to allow deletion.
    /// </summary>
    private DropdownField _eventPieceDeletionDropDownField;

    /// <summary>
    /// Unity visual element that when interacted with will delete the selected event piece but does not save this change to file.
    /// </summary>
    private UnityEngine.UIElements.Button _deleteEventPieceButton;

    /// <summary>
    /// Unity visual element that when interacted with will run the event in the selected simulation.
    /// </summary>
    private UnityEngine.UIElements.Button _runEventButton;

    /// <summary>
    /// Unity visual element that when interacted with will save the event to file.
    /// </summary>
    private UnityEngine.UIElements.Button _saveEventButton;

    /// <summary>
    /// Unity visual element used to display if the current event has been saved to file.
    /// </summary>
    private Label _saveStateLabel;

    /// <summary>
    /// Unity visual element that when interacted with will clear all event pieces from the event.
    /// </summary>
    private UnityEngine.UIElements.Button _clearEventButton;

    /// <summary>
    /// Back end representation of the event managed by the Event tool.
    /// </summary>
    private List<EventPiece> event1 = new List<EventPiece>();

    /// <summary>
    /// File path of the commodity configuration json file.
    /// </summary>
    private string _commodityConfigFile;

    /// <summary>
    /// Hash set of all the commodities in the current simulation.
    /// </summary>
    private HashSet<Commodity> _commodities;

    /// <summary>
    /// File path of the event configuration json file.
    /// </summary>
    private string _eventConfigFile;

    /// <summary>
    /// Hash set of event pieces used to create a visual representation of the pieces for the tool
    /// </summary>
    private HashSet<EventPiece> _eventPieces;

    /// <summary>
    /// Shows the current Event config tool if one exists, or triggers the creation of one otherwise.
    /// Also alerts the quick start guide that the event configuration file has been opened.
    /// </summary>
    [MenuItem("EcoSim/Events configuration", false, 113)]
    public static void ShowWindow() 
    {
        EditorWindow.GetWindow(typeof(Events));
        QuickStartGuide.NotifyOfOpen(QuickStartGuide.UI_Type.EventConfig);
    }

    /// <summary>
    /// Creates a new event configuration tool, and fills in the values that it can from file. Also sets up the back end logic.
    /// </summary>
    void CreateGUI()
    {
        // Set up the window
        EditorWindow editorWindow = GetWindow(typeof(Events));
        
        var root = editorWindow.rootVisualElement;
        VisualTreeAsset eventsAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/EcoSim/Editor/Configuration-Tools/Events/Events.uxml");
        VisualElement eventsUI = eventsAsset.Instantiate();
        root.Add(eventsUI);

        // Collect the UI elements
        _simulationSelectionDropDown = root.Q<DropdownField>("simulationSelectionDropDown");
        _commodityDropDownField = root.Q<DropdownField>("commodityDropDownField");
        _inflationShiftIntegerField = root.Q<IntegerField>("inflationShiftIntegerField");
        _supplyShiftIntegerField = root.Q<IntegerField>("supplyShiftIntegerField");
        _demandShiftIntegerField = root.Q<IntegerField>("demandShiftIntegerField");
        _needShiftIntegerField = root.Q<IntegerField>("needShiftIntegerField");
        _movingAvgDurationShiftIntegerField = root.Q<IntegerField>("movingAvgDurationShiftIntegerField");
        _eventPieceStatusLabel = root.Q<Label>("eventPieceStatusLabel");
        _addEventPieceButton = root.Q<UnityEngine.UIElements.Button>("addEventPieceButton");
        _eventPieceScrollView = root.Q<ScrollView>("eventPieceScrollView");
        _eventPieceDeletionDropDownField = root.Q<DropdownField>("eventPieceDeletionDropDownField");
        _deleteEventPieceButton = root.Q<UnityEngine.UIElements.Button>("deleteEventPieceButton");
        _runEventButton = root.Q<UnityEngine.UIElements.Button>("runEventButton");
        _saveEventButton = root.Q<UnityEngine.UIElements.Button>("saveEventButton");
        _saveStateLabel = root.Q<Label>("saveStateLabel");
        _clearEventButton = root.Q<UnityEngine.UIElements.Button>("clearEventButton");

        _commodityDropDownField.AddManipulator(new PlayModeToolTip(root));
        _inflationShiftIntegerField.AddManipulator(new PlayModeToolTip(root));
        _supplyShiftIntegerField.AddManipulator(new PlayModeToolTip(root));
        _demandShiftIntegerField.AddManipulator(new PlayModeToolTip(root));
        _needShiftIntegerField.AddManipulator(new PlayModeToolTip(root));
        _movingAvgDurationShiftIntegerField.AddManipulator(new PlayModeToolTip(root));
        _eventPieceDeletionDropDownField.AddManipulator(new PlayModeToolTip(root));

        //Callbacks
        _simulationSelectionDropDown.RegisterCallback<MouseOverEvent>(fillSimulations);
        _simulationSelectionDropDown.RegisterValueChangedCallback(PopulateFields);
        _addEventPieceButton.RegisterCallback<ClickEvent>(AddEventPiece);
        _runEventButton.RegisterCallback<ClickEvent>(RunEvent);
        _clearEventButton.RegisterCallback<ClickEvent>(ClearEvent);
        _saveEventButton.RegisterCallback<ClickEvent>(SaveEvent);
        _commodityDropDownField.RegisterCallback<MouseOverEvent>(fillCommodities);
        _eventPieceDeletionDropDownField.RegisterCallback<MouseOverEvent>(fillEventPieces);
        _deleteEventPieceButton.RegisterCallback<ClickEvent>(DeleteEventPiece);

        _needShiftIntegerField.RegisterValueChangedCallback(ValidateNeedsLevel);
        _movingAvgDurationShiftIntegerField.RegisterValueChangedCallback(ValidateMvgAvgLevel);
        _commodityDropDownField.RegisterValueChangedCallback(EnableAddEventPieceButton);
        _eventPieceDeletionDropDownField.RegisterValueChangedCallback(EnableDeleteEventPieceButton);

        _commodityDropDownField.SetEnabled(false);
        _inflationShiftIntegerField.SetEnabled(false);
        _supplyShiftIntegerField.SetEnabled(false);
        _demandShiftIntegerField.SetEnabled(false);
        _needShiftIntegerField.SetEnabled(false);
        _movingAvgDurationShiftIntegerField.SetEnabled(false);
        _eventPieceDeletionDropDownField.SetEnabled(false);
        _runEventButton.SetEnabled(false);
        _saveEventButton.SetEnabled(false);
        _clearEventButton.SetEnabled(false);

        _addEventPieceButton.SetEnabled(false);
        _deleteEventPieceButton.SetEnabled(false);
        
        hasUnsavedChanges = false;     
    }

    /// <summary>
    /// Loads the selected simulations commodity config file, and fills all fields
    /// </summary>
    /// <param name="evt">The click event that triggered this method</param>
    private void PopulateFields(ChangeEvent<string> evt)
    {
        if (_commodities != null) _commodities.Clear();
        if (_eventPieces != null) _eventPieces.Clear();
        _commodityConfigFile = Application.dataPath + "/EcoSim/Runtime/Configuration/" + _simulationSelectionDropDown.value + "/commodityConfig.json";
        _eventConfigFile = Application.dataPath + "/EcoSim/Runtime/Configuration/" + _simulationSelectionDropDown.value + "/eventConfig.json";
        LoadEvent();

        _eventPieceScrollView.Clear();
        foreach (EventPiece e in _eventPieces)
        {
            StringBuilder sb = new();
            sb.Append($"{e.commodity}:\n\tInflation Change: {e.inflationShift}\n\tSupply Change: {e.supplyShift}\n\tDemand Change: {e.demandShift}");
            sb.Append($"\n\tNeed Level Change: {e.needShift}\n\tMoving average duration Change: {e.movingAvgDurationShift}");
            string eventPieceString = sb.ToString();

            _eventPieceScrollView.Add(new Label(eventPieceString));
            event1.Add(e);
        }
        _commodityDropDownField.SetEnabled(true);
        _inflationShiftIntegerField.SetEnabled(true);
        _supplyShiftIntegerField.SetEnabled(true);
        _demandShiftIntegerField.SetEnabled(true);
        _needShiftIntegerField.SetEnabled(true);
        _movingAvgDurationShiftIntegerField.SetEnabled(true);
        _eventPieceDeletionDropDownField.SetEnabled(true);
        _runEventButton.SetEnabled(true);
        _saveEventButton.SetEnabled(true);
        _clearEventButton.SetEnabled(true);
    }

    /// <summary>
    /// Adds and event piece both to the display, and to the backend logic
    /// </summary>
    /// <param name="evt">The click event that triggered this method</param>
    private void AddEventPiece(ClickEvent evt)
    {
        if (_inflationShiftIntegerField.value == 0 && _supplyShiftIntegerField.value == 0 && _demandShiftIntegerField.value == 0 && _needShiftIntegerField.value == 0 && _movingAvgDurationShiftIntegerField.value == 0)
        {
            _eventPieceStatusLabel.text = "This event would have no effect, so it was not added";
        }
        else
        {
            EventPiece piece = new EventPiece(_commodityDropDownField.value, _inflationShiftIntegerField.value, _supplyShiftIntegerField.value, _demandShiftIntegerField.value, _needShiftIntegerField.value, _movingAvgDurationShiftIntegerField.value);
            event1.Add(piece);
            _eventPieces.Add(piece);
            StringBuilder sb = new();
            sb.Append($"{piece.commodity}:\n\tInflation Change: {piece.inflationShift}\n\tSupply Change: {piece.supplyShift}\n\tDemand Change: {piece.demandShift}");
            sb.Append($"\n\tNeed Level Change: {piece.needShift}\n\tMoving average duration Change: {piece.movingAvgDurationShift}");
            string eventPieceString = sb.ToString();

            _eventPieceScrollView.Add(new Label(eventPieceString));
            _saveStateLabel.text = "**Changes NOT Saved**";
            _commodityDropDownField.index = -1;
            _inflationShiftIntegerField.value = 0;
            _supplyShiftIntegerField.value = 0;
            _demandShiftIntegerField.value = 0;
            _needShiftIntegerField.value = 0;
            _movingAvgDurationShiftIntegerField.value = 0;
            _eventPieceStatusLabel.text = "Event Piece OK";
            hasUnsavedChanges = true;
        }
    }

    /// <summary>
    /// Deletes the event piece at the selected index, this change will affect future running events, 
    /// but will not be saved until the save button is clicked.
    /// </summary>
    /// <param name="evt">The click event that triggered this method</param>
    private void DeleteEventPiece(ClickEvent evt)
    {
        int indexToDelete = Int32.Parse(_eventPieceDeletionDropDownField.value) - 1;
        EventPiece eventRemoved = event1[indexToDelete];
        _eventPieces.Remove(eventRemoved);
        event1.RemoveAt(indexToDelete);
        _eventPieceScrollView.RemoveAt(indexToDelete);
        _saveStateLabel.text = "**Changes NOT Saved**";
        hasUnsavedChanges = true;
    }

    /// <summary>
    /// Runs the event in the selected simulation as soon as possible
    /// </summary>
    /// <param name="evt">The click event that triggered this method</param>
    private void RunEvent(ClickEvent evt)
    {
        var ecoSimMan = GameObject.FindAnyObjectByType<EcosimManager>();
        if (ecoSimMan != null)
        {
            ecoSimMan.simulations[_simulationSelectionDropDown.value].events.Enqueue(event1);
        }
    }

    /// <summary>
    /// Clear all events from the model and from the display
    /// </summary>
    /// <param name="evt">The click event that triggered this method</param>
    private void ClearEvent(ClickEvent evt)
    {
        _eventPieceScrollView.Clear();
        event1.Clear();
        _eventPieces.Clear();
        _saveStateLabel.text = "**Changes NOT Saved**";
        hasUnsavedChanges = true;
    }

    /// <summary>
    /// Fills the commodity drop down field with all of the commodities in the simulation.
    /// If the simulation is running, it will not fill it with the commodities in the config file,
    /// but instead with those in the currently running simulation.
    /// </summary>
    /// <param name="evt">The mouse over event that triggered this method</param>
    private void fillCommodities(MouseOverEvent evt)
    {
        if (_simulationSelectionDropDown.value != null && _simulationSelectionDropDown.value != "")
        {
            var ecoSimMan = GameObject.FindAnyObjectByType<EcosimManager>();
            if (Application.isPlaying && ecoSimMan.simulations[_simulationSelectionDropDown.value].API.GetRoundCount() > 0)
            {
                _commodityDropDownField.choices.Clear();
                foreach (string commodity in ecoSimMan.simulations[_simulationSelectionDropDown.value].API.GetCommodityNames())
                {
                    _commodityDropDownField.choices.Add(commodity);
                }
            }
            else
            {
                string json = File.ReadAllText(_commodityConfigFile);
                _commodities = JsonSerializer.Deserialize<HashSet<Commodity>>(json);
                _commodityDropDownField.choices.Clear();
                foreach (Commodity commodity in _commodities)
                {
                    _commodityDropDownField.choices.Add(commodity.name);
                }
            }
        }
    }

    /// <summary>
    /// Fills the event piece to delete drop down menu with all possible indexes.
    /// </summary>
    /// <param name="evt">The mouse over event that triggered this method</param>
    private void fillEventPieces(MouseOverEvent evt)
    {
        if (_eventPieceScrollView.childCount > 0)
        {
            _eventPieceDeletionDropDownField.choices.Clear();
            for (int i = 0; i < _eventPieceScrollView.childCount; i++)
            {
                _eventPieceDeletionDropDownField.choices.Add((i + 1).ToString());
            }
        }
        else 
        {
            _eventPieceDeletionDropDownField.choices.Clear();
            _eventPieceDeletionDropDownField.choices.Add("No Event Pieces To Delete");
        }
    }

    /// <summary>
    ///  Loads the currently saved event on window creation.
    /// </summary>
    private void LoadEvent()
    {
        _eventPieceScrollView.Clear();
        if (!File.Exists(_eventConfigFile))
        {
            _eventPieces = new HashSet<EventPiece>();
            return;
        }
        string json = File.ReadAllText(_eventConfigFile);
        _eventPieces = JsonSerializer.Deserialize<HashSet<EventPiece>>(json);
    }

    /// <summary>
    /// Commits all changes to the json file so they can be reloaded later.
    /// </summary>
    /// <param name="evt">The click event that triggered this method</param>
    private void SaveEvent(ClickEvent evt)
    {
        string json = JsonSerializer.Serialize(_eventPieces, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_eventConfigFile, json);
        _saveStateLabel.text = "Changes Saved";
        hasUnsavedChanges = false;
    }

    /// <summary>
    /// Adds a yellow border around the selected element to indicate a potential issue.
    /// </summary>
    /// <param name="element">The unity visual element to give a yellow border to.</param>
    private void ShowAlert(VisualElement element)
    {
        element.style.borderBottomColor = Color.yellow;
        element.style.borderTopColor = Color.yellow;
        element.style.borderLeftColor = Color.yellow;
        element.style.borderRightColor = Color.yellow;
    }

    /// <summary>
    /// Removes the yellow border around the selected element to indicate there is no longer a potential issue.
    /// </summary>
    /// <param name="element">The unity visual element to give a clear border to.</param>
    private void ClearError(VisualElement element)
    {
        element.style.borderBottomColor = Color.clear;
        element.style.borderTopColor = Color.clear;
        element.style.borderLeftColor = Color.clear;
        element.style.borderRightColor = Color.clear;
    }

    /// <summary>
    /// Function used to check if the set value of need level shift may have an issue later.
    /// </summary>
    /// <param name="evt">The value changed event that triggered this method</param>
    private void ValidateNeedsLevel(ChangeEvent<int> evt)
    {

        if (evt.newValue < 0)
        {
            Debug.LogWarning($"If need level becomes negative simulation output may be illogical.");
            ShowAlert(_needShiftIntegerField);
            _eventPieceStatusLabel.text = "Negative values in highlighted fields may result in illogical output";
            return;
            
        }
        if (_movingAvgDurationShiftIntegerField.style.borderBottomColor == Color.clear) 
        {
            _eventPieceStatusLabel.text = "Event Piece OK";
        }
        
        ClearError(_needShiftIntegerField);
    }

    /// <summary>
    /// Function used to check if the set value of moving average duration change may have an issue later.
    /// </summary>
    /// <param name="evt">The value changed event that triggered this method</param>
    private void ValidateMvgAvgLevel(ChangeEvent<int> evt)
    {

        if (evt.newValue < 0)
        {
            Debug.LogWarning($"If moving average duration becomes negative simulation output may be illogical.");
            ShowAlert(_movingAvgDurationShiftIntegerField);
            _eventPieceStatusLabel.text = "Negative values in highlighted fields may result in illogical output";
            return;

        }
        if (_needShiftIntegerField.style.borderBottomColor == Color.clear)
        {
            _eventPieceStatusLabel.text = "Event Piece OK";
        }

        ClearError(_movingAvgDurationShiftIntegerField);
    }

    /// <summary>
    /// Enables or disables the add event piece button depending on whether or not a commodity has been selected.
    /// </summary>
    /// <param name="evt">The value changed event that triggered this method</param>
    private void EnableAddEventPieceButton(ChangeEvent<string> evt)
    {
        if (evt.newValue == null) _addEventPieceButton.SetEnabled(false);
        else _addEventPieceButton.SetEnabled(true);
    }

    /// <summary>
    /// Enables or disables the delete event piece button depending on whether or not an index has been selected.
    /// </summary>
    /// <param name="evt">The value changed event that triggered this method</param>
    private void EnableDeleteEventPieceButton(ChangeEvent<string> evt)
    {
        if (evt.newValue == null) _deleteEventPieceButton.SetEnabled(false);
        else _deleteEventPieceButton.SetEnabled(true);
    }

    /// <summary>
    /// Called to save the changes when the user chooses to save changes on window closed.
    /// </summary>
    public override void SaveChanges()
    {
        string json = JsonSerializer.Serialize(_eventPieces, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_eventConfigFile, json);
        _saveStateLabel.text = "Changes Saved";

        base.SaveChanges();
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
    /// On deletion of the event configuration window, alerts the quick start guide.
    /// </summary>
    private void OnDestroy()
    {
        QuickStartGuide.NotifyOfClose(QuickStartGuide.UI_Type.EventConfig);
    }


}