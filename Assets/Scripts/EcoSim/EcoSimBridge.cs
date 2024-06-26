using EcoSim;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using UnityEditor;
using UnityEngine;
using static EcoSim.EcoEngine;


/// <summary>
/// Unimplemented static Class that connects the simulation system to the game.
/// Usage is the first set the currentEngine with SetCurrentEcoEngine
/// Next, Register connection between ScriptableObjects and EcoSim commodities with the RegisterScriptableObjectAndCommodity method
/// Finally, Call GetItemPrice when a price is needed. 
/// It is recommended to pause the simulation when the user is interacting with items else the price may change between when the UI get the price and the player interacts with the item
/// </summary>
public class EcoSimBridge
{
    /// <summary>
    /// Dictionary of Unity items to EcoEngine Commodities
    /// </summary>
    private static Dictionary<UnityEngine.Object, string> scriptableObjectToCommodity = new Dictionary<UnityEngine.Object, string>();

    //Example location of config file 
    //private static readonly string configFile = Application.dataPath + "/EcoSim/Runtime/Configuration/commodityConfig.json";

    /// <summary>
    /// The engine that the game is using to get prices for the items.
    /// </summary>
    private static EcoEngine currentEngine = null;

    /// <summary>
    /// Returns the current item price for a scriptable object 
    /// </summary>
    /// <param name="obj">ScriptableObject to get the price of</param>
    /// <returns>Current price of an object</returns>
    /// <exception cref="NotImplementedException"> Throws if this has not been implemented yet</exception>
    public static int GetItemPrice(UnityEngine.Object obj)
    {
        throw new NotImplementedException();
        /* Example Implementation
        
        public static int GetItemPrice(ItemDefinition item)
        {
            int price = item.SellPrice;
            if (itemsToCommodity.ContainsKey(item.ID))
            {
                if (GameplayManager.Instance.ecosimManager.DataAPI.GetCommodityNames().Contains(itemsToCommodity[item.ID]))
                {
                    price = GameplayManager.Instance.ecosimManager.DataAPI.GetCurrentPrice(itemsToCommodity[item.ID]);
                }
                else
                {
                    Debug.Log($"Item not found in collection {item.name}");
                }
            }
            else
            {
                Debug.Log($"Tried finding {item.name}'s ID in EcoSimBridge and failed");
            }
        
            Debug.Log($"Return Price for {item.name} was {price}");
            return price;

        }
        */
    }

    /// <summary>
    /// Send an event to the currentEngine about a given scriptable object
    /// </summary>
    /// <param name="obj">What scriptable object should be affected</param>
    /// <param name="supplyChange"> How the supply should change</param>
    /// <param name="demandChange"> How the demand should change</param>
    /// <param name="inflationChange">How the inflationRate should change</param>
    /// <param name="needChange"> How the needs should change</param>
    /// <param name="movingAvgChange">How the moving attribute should change for this commodity</param>
    /// <exception cref="NotImplementedException"> Throws if this has not been implemented yet</exception>
    public static void NotifyOfChange(UnityEngine.Object obj, int supplyChange = 0, int demandChange = 0, int inflationChange = 0, int needChange = 0, int movingAvgChange = 0)
    {
        throw new NotImplementedException();
        /* Example Implementation
        if (itemsToCommodity.ContainsKey(item.ID))
        {
            EcoSim.EventPiece piece = new EcoSim.EventPiece(itemsToCommodity[item.ID],inflationChange,supplyChange,demandChange,needChange,movingAvgChange);
            currentEngine.RunEventPiece(piece);
        }
         */

    }
    /// <summary>
    /// Helped method, just called NotifyOfTrade with fewer parameters
    /// </summary>
    /// <param name="obj">Scriptable Object that corresponds to the change</param>
    /// <param name="supplyChange">Change in supply</param>
    /// <param name="demandChange">Change in demand</param>
    public static void NotifyOfTrade(UnityEngine.Object obj, int supplyChange = 0, int demandChange = 0)
    {
        NotifyOfChange(obj, supplyChange, demandChange);
    }


    /// <summary>
    /// Set which EcoEngine to use to get the price from
    /// </summary>
    /// <param name="ecoEngine">New EcoEngine to use</param>
    public static void SetCurrentEcoEngine(EcoEngine ecoEngine)
    {
        currentEngine = ecoEngine;
    }

    /// <summary>
    /// Creates a EcoEngine usable commodity based off of a Unity object
    /// </summary>
    /// <param name="obj">The Unity object to turn into an EcoEngine commodity</param>
    /// <param name="commodityName">The name of this new commodity</param>
    /// <exception cref="NotImplementedException">Throws if this has not been implemented yet</exception>
    public static void RegisterScriptableObjectAndCommodity(UnityEngine.Object obj, string commodityName)
    {
        throw new NotImplementedException();
        /* Example Implementation
         if (!itemsToCommodity.ContainsKey(itemDef.ID))
        {
            itemsToCommodity.Add(itemDef.ID, commodityName);
        }
         */
    }

    /// <summary>
    /// Creates a EcoEngine usable commodity based off of a Unity object
    /// </summary>
    /// <param name="obj">The Unity object to turn into an EcoEngine commodity</param>
    /// <returns>The new EcoEngine Commodity</returns>
    /// <exception cref="NotImplementedException">Throws if this has not been implemented yet</exception>
    public static EcoEngine.Commodity CreateNewCommodity(UnityEngine.Object obj) 
    {
        throw new NotImplementedException();
        /* Example Implementation
        if(obj is testObject)
        {
            EcoEngine.Commodity c = new EcoEngine.Commodity();
            testObject testObject = (testObject)obj;
            c.name = testObject.Name;
            c.price = 50;
            c.initialPrice = c.price;
            c.supply = 1000;
            c.demand = 1000;
            c.needLevel = 1000;
            c.movingAvgDuration = 50;
            c.inflationRate = 5;
            c.InitializeMovingAverage();
            return c;

        }
        else
        {
            return null;
        }
        */

    }

    /// <summary>
    /// Export current commodities and all int the path directory to disk config
    /// </summary>
    /// <param name="pathToScriptableObjects"> Path to scriptable objects starting from Assets/ </param>
    /// <param name="dataAPI">Optional: DataAPI to pull list of current commodities</param>
    public static void ExportScriptableObjectsToCommodities(string pathToScriptableObjects, string configFilePath)
    {


        string currentJson = File.ReadAllText(configFilePath);
        HashSet<EcoEngine.Commodity> commodities = JsonSerializer.Deserialize<HashSet<Commodity>>(currentJson);

        List<UnityEngine.Object> scriptableObjectList = LoadScriptableObjectsFromDisk(pathToScriptableObjects);
        foreach(UnityEngine.Object obj in scriptableObjectList)
        {
            EcoEngine.Commodity commodity = CreateNewCommodity(obj);
            if(commodity == null)
            {
                continue;
            }
            if (commodities.Any(value => value.name == commodity.name))
            {
                continue;

            }
            else
            {
                commodities.Add(commodity);
            }
        }

        string json = JsonSerializer.Serialize(commodities, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(configFilePath, json);
        AssetDatabase.Refresh();

    }

    /// <summary>
    /// Add a new Commodity to a given Simulation. Must call ExportScriptableObjectsToCommodities to save hot loaded commodities 
    /// </summary>
    /// <param name="obj">Scriptable object to add as commodity</param>
    /// <param name="engine">EcoEngine to add the new commodity to</param>
    /// <returns>Newly created commodity</returns>
    public static EcoEngine.Commodity AddCommodityWithDefaults(UnityEngine.Object obj, EcoEngine engine)
    {
        bool success = false;
        EcoEngine.Commodity c = CreateNewCommodity(obj);

        Debug.Log($"Trying to add {c.name}");
        if(engine == null)
        {
            return c;
        }
        if (engine.TryHotAddCommodity(c))
        {
            EcoSimBridge.RegisterScriptableObjectAndCommodity(obj, c.name);
            success = true;
        }
        if (success)
        {
            return c;
        }
        return null;
    }

    /// <summary>
    /// Helper class to load type of scriptable object from disk to C# type
    /// </summary>
    /// <typeparam name="T">What type to try loading</typeparam>
        public static List<UnityEngine.Object> LoadScriptableObjectsFromDisk(string path)
        {
            List<UnityEngine.Object> result = new List<UnityEngine.Object>();
            string fullPath = Path.Combine("./", path);
            string[] files = Directory.GetFiles(fullPath);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);

                if (file.EndsWith(".meta"))
                {
                    continue;
                }

                UnityEngine.Object loadedObject = AssetDatabase.LoadAssetAtPath(path + name, typeof(ScriptableObject));
                if (loadedObject != null && loadedObject is UnityEngine.Object)
                {
                    result.Add(loadedObject);
                }
            }
            return result;
        }

    /// <summary>
    /// Helper method to register all Scriptable Objects on disk with all commodities in the Config
    /// </summary>
    /// <param name="path">Assets relative path to the folder holding the scriptable objects</param>
    public static void RegisterScriptableObjectWithConfig(string path)
    {
        /*
        List<UnityEngine.Object> scriptableObjects = LoadScriptableObjectsFromDisk(path);
        HashSet<EcoEngine.Commodity> commodities = currentEngine.API.GetCommodities();
        foreach(UnityEngine.Object obj in scriptableObjects)
        {
            //Match obj with correct Commodity
            try
            {
                EcoEngine.Commodity match = commodities.First(c => c.name == obj.name);
                RegisterScriptableObjectAndCommodity(obj, match.name);

            }catch(Exception e)
            {

            }
        }
        */
        throw new NotImplementedException();

    }



}
