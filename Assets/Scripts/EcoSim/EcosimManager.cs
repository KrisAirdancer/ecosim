using System.Collections.Generic;
using UnityEngine;
using EcoSim;
using System;
using System.IO;
using static EcoSim.EcoEngine;

/// <summary>
/// Used to control all of the EcoEngines running with the Unity Game. Sets up each simulation when the game starts, then runs new rounds as needed,
/// and provides information about them to the visualizer and game.
/// </summary>
public class EcosimManager : MonoBehaviour
{
    /// <summary>
    /// All of the simulations currently being managed.
    /// </summary>
    public Dictionary<string, EcoEngine> simulations = new Dictionary<string, EcoEngine>();

    /// <summary>
    /// Whether or not the simulations should currently be running.
    /// </summary>
    [SerializeField] private bool runSimulation = false;

    /// <summary>
    /// 
    /// </summary>
    [SerializeField] private bool selfUpdate = false;

    /// <summary>
    /// Value for if the simulations have run, and therefore if they need to be initialized.
    /// </summary>
    private bool hasRun = false;

    /// <summary>
    /// Occurs every time the simulations complete a round, and sends data about the simulation to the visualizer.
    /// </summary>
    public event Action<RowData, string> RowChanged;

    /// <summary>
    /// The DataAPI's connected to each engine
    /// </summary>
    public Dictionary<string, EcoEngine.DataAPI> DataAPIs = new Dictionary<string, DataAPI>();

    /// <summary>
    /// On startup of the EcosimManager, gets all of the commodities from all of the simulations, and builds a HashSet of their names
    /// </summary>
    private void Awake()
    {
        string[] dirs = Directory.GetDirectories(Application.dataPath + "/EcoSim/Runtime/Configuration/");
        HashSet<string> CommodityNames = new HashSet<string>();
        //The directories are how we are splitting up the simulations, each sim has its own folder
        foreach (string sim in dirs)
        {
            string[] simSplit = sim.Split('/');
            EcoEngine e = new EcoEngine(simSplit[simSplit.Length - 1]);
            simulations.Add(simSplit[simSplit.Length - 1], e);
            DataAPIs.Add(simSplit[simSplit.Length - 1], e.API);
            foreach (string commodityName in e.API.GetCommodityNames())
            {
                CommodityNames.Add(commodityName);
            }
        }
    }

    /// <summary>
    /// Given a single eventPiece, and a simulation, will run that event in that simulation at the next
    /// possible opportunity.
    /// </summary>
    /// <param name="piece">Event to run</param>
    /// <param name="simulation">Simulation to run the event in</param>
    public void RunEventPiece(EventPiece piece, string simulation)
    {
        simulations[simulation].events.Enqueue(new List<EventPiece> { piece });
    }

    /// <summary>
    /// Unity calls update automatically, and we use this to trigger checks for if we should update the simulation.
    /// </summary>
    private void Update()
    {
        if(selfUpdate)
        {
            if(!hasRun)
            {
                hasRun = true;
            }
            DoUpdate();
        }
    }


    /// <summary>
    /// If the simulations are supposed to be running, which is toggled via the space bar by default, tries to run a round for each simulation, and provide information
    /// about that round to the visualizer.
    /// </summary>
    public void DoUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            runSimulation = !runSimulation;
        }
        if(runSimulation)
        {
            foreach (KeyValuePair<string, EcoEngine> entry in simulations)
            {
                bool hasStep = entry.Value.Step();
                if (hasStep)
                {
                    List<RowData> rows = entry.Value.GetEconomicState();
                    foreach (RowData data in rows)
                    {
                        if (data != null)
                        {
                            RowChanged?.Invoke(data, entry.Key);
                        }
                    }
                }
            }
        }
        

    }

}
