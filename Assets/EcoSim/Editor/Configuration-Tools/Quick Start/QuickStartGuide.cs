using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// UI tool used to provide some basic info to users of Ecosim
/// </summary>
public class QuickStartGuide : EditorWindow
{
    /* ID of each UI tool 
     * 0 Quick Start
     * 1 Engine Config
     * 2 Commodity
     * 3 Event
     * 4 Visualizer
     */

    /// <summary>
    /// Enum used to more easily get the type of various windows
    /// </summary>
    public enum UI_Type
    {
        QuickStart,
        EngineConfig,
        CommodityConfig,
        EventConfig,
        Visualizer
    }

    /// <summary>
    /// Visual tree that all the visual elements of this tool are based off of.
    /// </summary>
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    /// <summary>
    /// Label used to provide information about the various tools, and instructions as to how to use them.
    /// </summary>
    private Label _labelDescription;

    /// <summary>
    /// Unity visual element that when interacted with begins the tutorial.
    /// </summary>
    private Button _tutorialButton;

    /// <summary>
    /// Unity visual element that when interacted with moves to the previous part of the tutorial.
    /// </summary>
    private Button _prevButton;

    /// <summary>
    /// Unity visual element that when interacted with moves to the next part of the tutorial.
    /// </summary>
    private Button _nextButton;

    /// <summary>
    /// Value used to help keep track as to which part of the tutorial is currently displayed.
    /// </summary>
    private int _previousState = 0;

    /// <summary>
    /// The total amount of parts in the tutorial.
    /// </summary>
    private int _maxState = 9;

    /// <summary>
    /// The starting page of the tutorial, and where it will go on reset.
    /// </summary>
    private int _minState = 0;

    /// <summary>
    /// An array containing all of the details and instructions that we provide as to how to use the various tools.
    /// </summary>
    private string[] _descriptions = {
        "<b>How the Simulation Works</b>" +
        "\n\n" +
        "The EcoSim algorithm updates the price, supply, and demand of each of the items " +
        "in the simulation based on the supply and demand, measured in units, of those items. " +
        "Supply and demand are inversely related, price and supply are directly related, and " +
        "price and demand are inversely related." +
        "\n\n" +
        "For example, if supply goes up, demand will go down, and price will go down. If supply " +
        "and demand are equal, price will not change." +
        "\n\n" +
        "The simulation runs in cycles, wherein the price, supply, and demand of each item will " +
        "be updated based on the changes that have occurred since the last round. " +
        "As such, the cycle rate can be used to set how fast prices change in response to events." +
        "\n\n" +
        "Click the \"Start Tutorial\" button to begin" +
        "\n\n\n\n" +
        "The official EcoSim documentation can be found here: " +
        "<a href=\"https://krisairdancer.github.io/ecosim-docs/\">https://krisairdancer.github.io/ecosim-docs/</a>",


        "<b>Configuring the Simulation Engine</b>" +
        "\n\n" +
        "Please open the Engine Configuration tool in the Unity menu." +
        "\n\n" +
        "EcoSim > Engine Configuration",


        "<b>Configuring the Simulation Engine</b>" +
        "\n\n" +
        "This tool controls the general settings of the simulation." +
        "\n\n" +
        "Multiple simulations can be managed in the top section of the tool." +
        "\n\n" +
        "Tick Rate and other settings per-simulation can be managed in the lower sections " +
        "(Simulation Settings and Currency Settings)." +
        "\n\n" +
        "Importing scriptable objects (your in-game items) can be done in the lowest section by " +
        "providing a path to the folder containing your scriptable objects and then  clicking the " +
        "Import button." +
        "\n\n" +
        "Please close the Engine Configuration tool and click \"Next\" when you are ready to proceed with the tutorial.",


        "<b>Configuring Commodities (in-game items)</b>" +
        "\n\n" +
        "Please open the Commodity Configuration tool in the Unity Menu." +
        "\n\n" +
        "EcoSim > Commodity Configuration",


        "<b>Configuring Commodities (in-game items)</b>" +
        "\n\n" +
        "This tool is used to create, modify, and delete commodities(items) within the economy. " +
        "\n\n" +
        "To configure a commodity, select the simulation in which the commodity resides from the " +
        "\"Simulation\" dropdown, then select the commodity from the \"Commodity\" dropdown. " +
        "Or, if you want to add a new commodity, select \"New Commodity\" in the \"Commodity\" dropdown." +
        "\n\n" +
        "Note that adding a new commodity to the simulation will NOT add a new scriptable object to your game." +
        "\n\n" +
        "From here, you can configure the commodity as needed. You can hover over the input field labels to " +
        "see tooltips with explanations of what each one is." +
        "\n\n" +
        "When you've finished configuring a commodity, click \"Add new commodity\", or \"Delete commodity\", " +
        "and then \"Save commodities\" to save your changes." +
        "\n\n" +
        "The lowest section of the tool displays a list of all commodities currently in the selected simulation." +
        "\n\n" +
        "Please close the Commodity Configuration tool and click \"Next\" when you are ready to proceed with the tutorial.",


        "<b>Configuring Events</b>" +
        "\n\n" +
        "Please open the Events Configuration tool in the Unity Menu." +
        "\n\n" +
        "EcoSim > Events Configuration",


        "<b>Configuring Events</b>" +
        "\n\n" +
        "Events represent notable economic activity in the simulation and your game. " +
        "For example, if the player goes rouge and burns down several acres of wheat " +
        "fields, the supply of wheat would decrease. We can model, and simulate, this " +
        "with an event." +
        "\n\n" +
        "Firstly, be aware that events modify the simulation in real-time without " +
        " the need for a restart." +
        "\n\n" +
        "To start, select the simulation you want the event to affect from the dropdown. " +
        "Then, select the commodity the event will effect. In our example above, we " +
        "would select wheat." +
        "\n\n" +
        "Next, adjust the parameters of the event piece. In our example, we would set demand " +
        "to decrease by, say 100 units." +
        "\n\n" +
        "An event piece represents a change to a single commodity in a larger event. " +
        "A single event can have one or more event pieces." +
        "\n\n" +
        "Once you're satisfied with the current event piece, click \"Add Event Piece\" " +
        "and the event piece will be added to the selected simulation." +
        "\n\n" +
        "The list in the middle of the configuration window displays all of the event " +
        "pieces in the event." +
        "\n\n" +
        "Finally, we can save and run the event, or delete it and create a new one." +
        "\n\n" +
        "Please close the Events Configuration tool and click \"Next\" when you are ready to proceed with the tutorial.",


        "<b>Monitoring a Running Simulation</b>" +
        "\n\n" +
        "Please open the Visualizer tool in the Unity Menu." +
        "\n\n" +
        "EcoSim > Visualization Menu",


        "<b>Monitoring a Running Simulation</b>" +
        "\n\n" +
        "When the simulation is running, this tool shows each commodity (items and services) in the simulation." +
        "Selecting a commodity from the list will display a graph of the supply, demand, and price of that commodity " +
        "that updates in real-time as the simulation runs. The table values update in real-time as well." +
        "\n\n" +
        "The lines on the graph can be toggled on and off and the colors of the lines changed as desired." +
        "\n\n" +
        "Please close the Visualization Menu tool and click \"Next\" when you are ready to proceed with the tutorial.",


        "<b>Linking EcoSim to Unity</b>" +
        "\n\n" +
        "The last item to cover is the connection between EcoSim and the Unity game engine. " +
        "EcoSim Bridge is the connection between your game and EcoSim and requires knowledge of how to serialize " +
        "your in-game objects so that they can be \"imported\" into and tracked in the economic simulation." +
        "\n\n" +
        "The EcoSimBridge file comes included with the EcoSim package and is located at Scripts > EcoSim > EcoSimBridge.cs" +
        "\n\n" +
        "Inside the file you will find commends explaining how to implement the necessary methods as well as " +
        "example implementations of those methods for your reference." +
        "\n\n" +
        "The logic in EcoSim Bridge can be expanded as needed via the EcoEngine.DataAPI class for each instance of EcoEngine." +
        "\n\n" +
        "This concludes this tutorial. If you need more information, we recommend that you take a look at the " +
        "official documentation at " +
        "<a href=\"https://krisairdancer.github.io/ecosim-docs/\">https://krisairdancer.github.io/ecosim-docs/</a>"
    };

    /// <summary>
    /// The current part of the tutorial displayed
    /// </summary>
    private static int state = 0;

    /// <summary>
    /// Whether or not the tutorial is currently running, which also determines if the quick start guide should respond to user interaction in other tools.
    /// </summary>
    private static bool running_tutorial = false;

    /// <summary>
    /// Displays the quick start guide if one already exists, if not triggers the creation of a new one.
    /// </summary>
    [MenuItem("EcoSim/Quick Start Guide", false, 100)]
    public static void ShowExample()
    {
        QuickStartGuide wnd = GetWindow<QuickStartGuide>();
        wnd.titleContent = new GUIContent("QuickStartGuide");
        wnd.maxSize = new Vector2(800, 600);
        wnd.minSize = wnd.maxSize;
    }

    /// <summary>
    /// Creates a new quick start guide and sets up the back end logic.
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

        _labelDescription = root.Q<Label>("TextDescription");
        _labelDescription.text = _descriptions[0];
        _tutorialButton = root.Q<Button>("TutorialButton");
        _prevButton = root.Q<Button>("PrevButton");
        _nextButton = root.Q<Button>("NextButton");
        _tutorialButton.RegisterCallback<ClickEvent>(TutorialButtonClicked);
        _prevButton.RegisterCallback<ClickEvent>(PreviousButtonClicked);
        _nextButton.RegisterCallback<ClickEvent>(NextButtonClicked);

        _prevButton.SetEnabled(false);
        _nextButton.SetEnabled(false);

    }

    /// <summary>
    /// On destruction/close of the quick start guide resets the tutorial.
    /// </summary>
    private void OnDestroy()
    {
        running_tutorial = false;
        state = 0;
    }

    /// <summary>
    /// Progress the tutorial to the next part.
    /// </summary>
    /// <param name="evt">The click event that triggered this method.</param>
    private void NextButtonClicked(ClickEvent evt)
    {
        if(running_tutorial)
        {
            state++;
            if(state > _maxState)
            {
                state=_maxState;
            }
            if (state == _maxState)
            {
                _nextButton.SetEnabled(false );
            }
        }
    }

    /// <summary>
    /// Changes the tutorial to the previous part.
    /// </summary>
    /// <param name="evt">The click event that triggered this method.</param>
    private void PreviousButtonClicked(ClickEvent evt)
    {
        if(running_tutorial)
        {
            state--;
            if(state < _minState)
            {
                state=_minState;
            }
            if(state == _minState)
            {
                _prevButton.SetEnabled(false );
            }
        }
    }

    /// <summary>
    /// Starts the tutorial, and enables the tutorial transition buttons.
    /// </summary>
    /// <param name="evt">The click event that triggered this method.</param>
    private void TutorialButtonClicked(ClickEvent evt)
    {
        if (running_tutorial)
        {
            state = 0;
            running_tutorial = false;
            _tutorialButton.text = "Start Tutorial";
            _prevButton.SetEnabled(false);
            _nextButton.SetEnabled(false);
        }
        else
        {
            state = 1;
            running_tutorial = true;
            _tutorialButton.text = "End Tutorial";
            _prevButton.SetEnabled(true);
            _nextButton.SetEnabled(true);
        }
    }

    /// <summary>
    /// Every frame checks if the back end of this tool has changed, and if so, updates the tutorial
    /// </summary>
    private void Update()
    {
        if(state != _previousState)
        {
            //State has changes since last update
            _labelDescription.text = _descriptions[state];

            _previousState = state;

            if(state == _minState && _prevButton.enabledSelf)
            {
                _prevButton.SetEnabled(false);

            }
            if(state == _maxState && _nextButton.enabledSelf)
            {
                _nextButton.SetEnabled(false);
            }
            if(state < _maxState && !_nextButton.enabledSelf)
            {
                _nextButton.SetEnabled(true);
            }
            if (state > _minState && !_prevButton.enabledSelf)
            {
                _prevButton.SetEnabled(true);
            }

        }

    }

    /*
     * This tutorial will be based on a state machine 
     * State 0 tutorial is not running
     * State 1 wait for open engine config UI
     * State 2 wait for close of engine config
     * State 3 wait for open commodity UI
     * State 4 wait for close of commodity UI
     * State 5 wait for open event UI
     * State 6 wait for close of event UI
     * State 7 wait for open of visualizer
     * State 8 wait for game to start or for stop button to be pressed
     */

    /* ID of each UI tool 
     * 0 Quick Start
     * 1 Engine Config
     * 2 Commodity
     * 3 Event
     * 4 Visualizer
     */

    /// <summary>
    /// On opening a different tool, notifies the quick start guide so it can respond and change which part of the tutorial is displayed.
    /// </summary>
    /// <param name="id">The ID of the tool window that called this method</param>
    public static void NotifyOfOpen(UI_Type id)
    {
        if (running_tutorial)
        {
            switch (id)
            {
                case 0:
                    break;
                case UI_Type.EngineConfig:
                    //This is the Engine Config Tool
                    if (state == 1)
                    {
                        state++;
                    }
                    break;
                case UI_Type.CommodityConfig:
                    //This this is Commodity Tool
                    if (state == 3)
                    {
                        state++;
                    }
                    break;
                case UI_Type.EventConfig:
                    //This is the Event tool
                    if (state == 5)
                    {
                        state++;
                    }
                    break;
                case UI_Type.Visualizer:
                    //This is a visualizer
                    if (state == 7)
                    {
                        state++;
                    }
                    break;
                default:
                    break;
            }
        }

    }

    /// <summary>
    /// On closing a different tool, notifies the quick start guide so it can respond and change which part of the tutorial is displayed.
    /// </summary>
    /// <param name="id">The ID of the tool window that called this method</param>
    public static void NotifyOfClose(UI_Type id)
    {
        if (running_tutorial)
        {
            switch (id)
            {
                case 0:
                    break;
                case UI_Type.EngineConfig:
                    //This is the Engine Config Tool
                    if (state == 2)
                    {
                        state++;
                    }
                    break;
                case UI_Type.CommodityConfig:
                    //This this is Commodity Tool
                    if (state == 4)
                    {
                        state++;
                    }
                    break;
                case UI_Type.EventConfig:
                    //This is the Event tool
                    if (state == 6)
                    {
                        state++;
                    }
                    break;
                case UI_Type.Visualizer:
                    //This is a visualizer
                    if (state == 8)
                    {
                        state++;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
