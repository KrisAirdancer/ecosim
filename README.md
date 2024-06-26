EcoDocs
=======

Welcome to the EcoSim docs!

If you're reading this after May 2024, the core EcoSim files can be found at `Assets > EcoSim`. The "Editor" files define the Unity Toolkit Extensions for EcoSim and the "Runtime" files define the simulation engine itself.

Quickstart Guide
----------------

A webpage version of these docs can be found [here](https://krisairdancer.github.io/ecosim-docs/).

EcoSim's quickstart guide is built into the package itself. Once you've installed EcoSim in Unity, you will have access to the guide via the "EcoSim" menu.

Installation
------------

1.  Download the EcoSim package.
2.  Create a new Unity project or open an existing one.
3.  Import the EcoSim package into Unity by either,
    *   dragging the package into the "Assets" folder in your Unity project,
    *   or by right clicking on the "Assets" folder, selecting "Import Package" then "Custom Package".
4.  Create a new game object to be used as the EcoSimManager object.

*   The EcoSimManager object allows your game to communicate with EcoSim.

6.  Name the new game object "EcoSimManager".
7.  Use the "Inspector" tool to add the "EcosimManager" script to the new game object.
8.  Tick the "Run Simulation" and "Self Update" options in the script's inspector menu.

*   This will ensure that the simulation runs when your game is started and can be configured differently per your needs.
*   Self Update: Tells the simulation script to call the `doUpdate()` method every time the Unity engine updates the game engine that the script is attached to, instead of you having to call it manually.
*   Run Simulation: Toggles the simulation on and off. The simulation can be started/stopped by either not sending updates to it or by unchecking this option.

10.  Run your game in Unity.
11.  In Unity, navigate to "EcoSim" > "Visualizer" to open the EcoSim simulation Visualizer tool.
12.  Select one of the default simulations from the "Simulation" dropdown.
13.  If EcoSim has been installed correctly, simulation data should be displayed in the Visualizer.
14.  The simulation must now be connected to your game via implementation of the "EcoSimBridge" class.

*   For instructions on how to do this, see the "Implementing EcoSimBridge" section of these docs.

16.  Finally, depending on the other packages that you've installed, you may have to update the assembly definition for the scripts in your project to ensure that they are all accessible by Unity.
17.  If all of the above steps have been completed, installation and setup is done and you can now configure the simulation to your needs.

Implementing EcoSim Bridge
--------------------------

EcoSimBridge is a static class that defines the connection between your game's ScriptableObjects and EcoSim.

Specifically, EcoSim needs to know how which of your in-game objects correspond to which commodities in the economic simulation. EcoSimBridge is where you will define that link.

Because Unity provides developers with wide flexibility in defining their ScriptableObjects, you must tell EcoSim how you've defined them. As such, there are several methods in the EcoSimBridge class that will require implementations that instruct EcoSimBridge how to work with your ScriptableObjects.

Before we being, note that EcoSimBridge is implemented as a static class, but that this is only done for convenience. If you would like to change this, you may do so.

The following steps outline how to implement the EcoSimBridge Class.

\*Note that example implementations are provided for the methods that you will need to implement.

1.  Implement `CreateNewCommodity()` method.

*   This method takes in a Unity ScriptableObject and creates a commodity in the simulation based on its properties.
*   Additionally, later in this guide, you will need to select a property to be used as a unique identifier for each commodity. So it is a good idea to include something like an ID or name from your ScriptableObjects in this step.

3.  Implement the `RegisterScriptableObjectAndCommodity` method.

*   This method connects Unity ScriptableObjects to a unique identifier used to link them to commodities in the economic simulation.

5.  Implement the `RegisterScriptableObjectWithConfig()` method.

*   This method is used to match each of the ScriptableObjects in your game with a commodity in the simulation when your game first starts up.

7.  Implement the `LoadScriptableObjectsFromDisk()` method.

*   By default, this method loads data from the disk into a generic Unity object, and will need to be updated if you need a more specific type to be returned.
*   Otherwise, you can leave this method as is.

9.  Implement the `GetItemPrice()` method.

*   This method returns the price associated with a given Unity ScriptableObject.
*   Since this method needs knowledge of your specific ScriptableObjects, you will need to implement this method with logic that accesses EcoSim's DataAPI to get the price of the ScriptableObject.
*   The methods you've implemented up to this point as well as the ones described below will be useful when implementing this method.

### Provided EcoSimBridge Methods

The following methods are part of the EcoSimBridge class but do not need to be implemented by you. Instead, they are needed by EcoSimBridge and can also serve as useful functionality for your use when implementing your portion of EcoSimBridge.

*   `ExportScriptableObjectsToCommodities()`

*   This method takes in a path to the directory where your ScriptableObjects are stored and creates a new commodity in the simulation that is associated with each one.

*   `SetCurrentEcoEngine()`

*   This method sets which economic simulation EcoSimBridge will use when responding to subsequent requests (method calls) and can be called anywhere in your game code that it is needed.
*   Call this method when you need to access commodities in one simulation or the other.
*   If you aren't using multiple economic simulations in your game, you can ignore this method.

*   `AddCommodityWithDefaults()`

*   This method creates a new commodity and tries to add it to the simulation.
*   It will fail, return false, if a commodity with the same unique identifier is already present in the economic simulation.

*   `NotifyOfChange()`

*   Notifies the economic simulation that an economic event has occurred.

*   `NotifyOfTrade()`

*   This method is a convenience function for the `NotifyOfChange()` method.

Importing Unity Scriptable Objects into EcoSim
----------------------------------------------

Once EcoSim has been installed in a Unity project and EcoSimBridge implemented, you can import all of your existing ScriptableObjects into EcoSim's economic simulation (convert them to EcoSim commodities) using the "Engine Configuration" tool found in the "EcoSim" menu in Unity.

Simply provide a path to your ScriptableObjects and click "Import Scriptable Objects".

If you run into errors when using this feature, it is likely that EcoSimBridge has been improperly implemented.

Events System
-------------

Each EcoEngine simulation has support for events that can be used for either testing the simulation, or to interact in various ways with the simulation during gameplay.

*   For example, a developer could set up events to have player interaction with vendors influence the simulation, or the developer could set up an event to trigger a massive change in the economic simulation in response to a in game story event.

To use events to affect the simulation during gameplay, one will first need to create an `Ecosim.EventPiece`. An event piece consists of a commodity to affect and the attributes on that commodity to change, such as inflation, supply, demand, need level, and moving average duration.

Once created, the EventPiece can be triggered by calling `RunEventPiece` in the `EcosimManager` class with the new `EventPiece` and the name of the simulation you want to trigger the EventPiece in.

Events can also be created and run directly through EcoSim's Unity tools.

To use events for testing via the Unit tools,

1.  Open the "Events" Unity UI tool.
2.  Select the simulation you want to test events on.
3.  Select the commodity they want to affect.

*   To trigger multiple events, create and add multiple event pieces.

5.  Set the values for as desired.
6.  Start the simulation in Unity.
7.  Run the event by clicking the "Run Event" button.
8.  The simulation will trigger the event as soon as possible.

*   The simulation can be paused and the Run Event button will still work, but the event will not trigger until the simulation resumes.

Exporting Simulation Data
-------------------------

EcoSim gives you the ability to export data from simulation instances for deeper analysis. In the engine settings configuration, tick the "Export data?" box and the export settings/filters will appear. By default all data for all commodities for every round will be exported. The settings and filters allow for more narrow exporting.

The available filters (applied to all chosen commodities):

*   `All Data`: This filter tells the simulation to export all Price, Supply, Demand, and Average Price History.
*   `Price`: Export the price of each commodity for the given round.
*   `Supply`: Export the supply of each commodity for the given round.
*   `Demand`: Export the demand of each commodity for the given round.
*   `Average Price History`: Exports the average price of each commodity, as well as the range of rounds that are influencing this average.
*   `Starting Round`: This is the first round that data begins being exported.

*   `0` by default, meaning the very first round of simulation.

*   `Ending Round`: This is the final round that data will be exported for.

*   `-1` by default. `-1` indicates that data will be continually exported until the simulation is terminated.
*   For example, if `Starting Round = 10` and `Ending Round = -1`, then data will be exported from round 10 to the end of execution.

You can select which commodities data is exported. By default, all commodities in the selected simulation will be exported. Using the dropdown, you can select which commodities to add or remove. Any commodities that will be tracked are displayed in a list underneath the "Remove Commodity" button.

Data is exported into a `.csv` file, allowing you to import the data into Excel, an R project, or any environment that will accept the `.csv` type for further and deeper analysis. These files will be found in the `Assets/EcoSim/Editor/Data` directory with a name format of `<simulation_name>-<date/time_simulation_started>.csv` for easier management.

Demo Game
---------

Note that a demo game was built by Jared Hogan as part of this capstone project for testing and demonstration purposes.

It is not included as part of the EcoSim package, but is available in the "DemoGame" repository in our team's GitLab.
