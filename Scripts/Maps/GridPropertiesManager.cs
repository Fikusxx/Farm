using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;


// All extra classes like: SaveLoadManager, GameObjectSave > its children are in SaveLoad System Scripts (Notion)

[RequireComponent(typeof(GenerateGUID))]
public class GridPropertiesManager : SingletonMonobehaviour<GridPropertiesManager>, ISavable
{

    private Grid grid;
    private Tilemap groundDecoration1;
    private Tilemap groundDecoration2;

    // key is the coordinate location
    private Dictionary<string, GridPropertyDetails> gridPropertyDictionary;

    // GridProperties for each scene. Those are created by TileMaps themselves.
    [SerializeField] private SO_GridProperties[] so_gridPropertiesArray = null;
    [SerializeField] private Tile[] dugGround = null;
    [SerializeField] private Tile[] wateredGround = null;

    private string iSavableUniqueID;
    public string ISavableUniqueID { get => iSavableUniqueID; set => iSavableUniqueID = value; }

    private GameObjectSave gameObjectSave;
    public GameObjectSave GameObjectSave { get => gameObjectSave; set => gameObjectSave = value; }


    protected override void Awake()
    {
        base.Awake();

        ISavableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameObjectSave = new GameObjectSave();
    }

    private void Start()
    {
        InitialiseGridProperties();
    }

    private void OnEnable()
    {
        ISavableRegister();
        EventHandler.AfterSceneLoadedEvent += AfterSceneLoaded;
        EventHandler.AdvanceGameDayEvent += AdvanceDay;
    }

    private void OnDisable()
    {
        ISavableDeregister();
        EventHandler.AfterSceneLoadedEvent -= AfterSceneLoaded;
        EventHandler.AdvanceGameDayEvent -= AdvanceDay;
    }


    /// <summary>
    /// Remove ground decorations
    /// </summary>
    private void ClearDisplayGroundDecorations()
    {
        groundDecoration1.ClearAllTiles();
        groundDecoration2.ClearAllTiles();
    }

    private void ClearDisplayGridPropertyDetails()
    {
        ClearDisplayGroundDecorations();
    }

    public void DisplayDugGround(GridPropertyDetails gridPropertyDetails)
    {
        // Dug
        if (gridPropertyDetails.daysSinceDug > -1)
        {
            ConnectDugGround(gridPropertyDetails);
        }
    }

    public void DisplayWateredGround(GridPropertyDetails gridPropertyDetails)
    {
        // Watered
        if (gridPropertyDetails.daysSinceWatered > -1)
        {
            ConnectWateredGround(gridPropertyDetails);
        }
    }

    private void ConnectDugGround(GridPropertyDetails gridPropertyDetails)
    {
        // Select tile based on surroinding dug tiles

        Tile dugTile0 = SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY);
        groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0), dugTile0);

        // Set 4 tiles if dug surrounding current tile - up, down, left, right now that this central tile has been dug
        GridPropertyDetails adjacentGridPropertyDetails;

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);

        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile1 = SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1, 0), dugTile1);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);

        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile2 = SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1, 0), dugTile2);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);

        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile3 = SetDugTile(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY, 0), dugTile3);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);

        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile4 = SetDugTile(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY, 0), dugTile4);
        }
    }


    private Tile SetDugTile(int xGrid, int yGrid)
    {
        // Get whether surrounding tiles (up, down, left and right) are dug or not

        bool upDug = IsGridSquareDug(xGrid, yGrid + 1);
        bool downDug = IsGridSquareDug(xGrid, yGrid - 1);
        bool leftDug = IsGridSquareDug(xGrid - 1, yGrid);
        bool rightDug = IsGridSquareDug(xGrid + 1, yGrid);

        #region Set appropriate tile based on whether surrounding tiles are dug or not

        if (!upDug && !downDug && !rightDug && !leftDug) return dugGround[0];
        else if (!upDug && downDug && rightDug && !leftDug) return dugGround[1];
        else if (!upDug && downDug && rightDug && leftDug) return dugGround[2];
        else if (!upDug && downDug && !rightDug && leftDug) return dugGround[3];
        else if (!upDug && downDug && !rightDug && !leftDug) return dugGround[4];
        else if (upDug && downDug && rightDug && !leftDug) return dugGround[5];
        else if (upDug && downDug && rightDug && leftDug) return dugGround[6];
        else if (upDug && downDug && !rightDug && leftDug) return dugGround[7];
        else if (upDug && downDug && !rightDug && !leftDug) return dugGround[8];
        else if (upDug && !downDug && rightDug && !leftDug) return dugGround[9];
        else if (upDug && !downDug && rightDug && leftDug) return dugGround[10];
        else if (upDug && !downDug && !rightDug && leftDug) return dugGround[11];
        else if (upDug && !downDug && !rightDug && !leftDug) return dugGround[12];
        else if (!upDug && !downDug && rightDug && !leftDug) return dugGround[13];
        else if (!upDug && !downDug && rightDug && leftDug) return dugGround[14];
        else if (!upDug && !downDug && !rightDug && leftDug) return dugGround[15];

        return null;
        #endregion Set appropriate tile based on whether surrounding tiles are dug or not
    }

    private bool IsGridSquareDug(int xGrid, int yGrid)
    {
        GridPropertyDetails gridPropertyDetails = GetGridPropertyDetails(xGrid, yGrid);

        if (gridPropertyDetails == null)
        {
            return false;
        }
        else if (gridPropertyDetails.daysSinceDug > -1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void ConnectWateredGround(GridPropertyDetails gridPropertyDetails)
    {
        // Select tile based on surrounding watered tiles

        Tile wateredTile0 = SetWateredTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY);
        groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0), wateredTile0);

        // Set 4 tiles if watered surrounding current tile - up, down, left, right now that this central tile has been watered

        GridPropertyDetails adjacentGridPropertyDetails;

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceWatered > -1)
        {
            Tile wateredTile1 = SetWateredTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1, 0), wateredTile1);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceWatered > -1)
        {
            Tile wateredTile2 = SetWateredTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1, 0), wateredTile2);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceWatered > -1)
        {
            Tile wateredTile3 = SetWateredTile(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY, 0), wateredTile3);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceWatered > -1)
        {
            Tile wateredTile4 = SetWateredTile(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY, 0), wateredTile4);
        }
    }

    private Tile SetWateredTile(int xGrid, int yGrid)
    {
        // Get whether surrounding tiles (up, down, left and right) are watrered or not

        bool upWatered = IsGridSquareWatered(xGrid, yGrid + 1);
        bool downWatered = IsGridSquareWatered(xGrid, yGrid - 1);
        bool leftWatered = IsGridSquareWatered(xGrid - 1, yGrid);
        bool rightWatered = IsGridSquareWatered(xGrid + 1, yGrid);

        #region Set appropriate tile based on whether surrounding tiles are watered or not

        if (!upWatered && !downWatered && !rightWatered && !leftWatered) return wateredGround[0];
        else if (!upWatered && downWatered && rightWatered && !leftWatered) return wateredGround[1];
        else if (!upWatered && downWatered && rightWatered && leftWatered) return wateredGround[2];
        else if (!upWatered && downWatered && !rightWatered && leftWatered) return wateredGround[3];
        else if (!upWatered && downWatered && !rightWatered && !leftWatered) return wateredGround[4];
        else if (upWatered && downWatered && rightWatered && !leftWatered) return wateredGround[5];
        else if (upWatered && downWatered && rightWatered && leftWatered) return wateredGround[6];
        else if (upWatered && downWatered && !rightWatered && leftWatered) return wateredGround[7];
        else if (upWatered && downWatered && !rightWatered && !leftWatered) return wateredGround[8];
        else if (upWatered && !downWatered && rightWatered && !leftWatered) return wateredGround[9];
        else if (upWatered && !downWatered && rightWatered && leftWatered) return wateredGround[10];
        else if (upWatered && !downWatered && !rightWatered && leftWatered) return wateredGround[11];
        else if (upWatered && !downWatered && !rightWatered && !leftWatered) return wateredGround[12];
        else if (!upWatered && !downWatered && rightWatered && !leftWatered) return wateredGround[13];
        else if (!upWatered && !downWatered && rightWatered && leftWatered) return wateredGround[14];
        else if (!upWatered && !downWatered && !rightWatered && leftWatered) return wateredGround[15];

        return null;
        #endregion Set appropriate tile based on whether surrounding tiles are watered or not
    }

    private bool IsGridSquareWatered(int xGrid, int yGrid)
    {
        GridPropertyDetails gridPropertyDetails = GetGridPropertyDetails(xGrid, yGrid);

        if (gridPropertyDetails == null)
        {
            return false;
        }
        else if (gridPropertyDetails.daysSinceWatered > -1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void DisplayGridPropertyDetails()
    {
        // Loop thru all grid items
        foreach (KeyValuePair<string, GridPropertyDetails> item in gridPropertyDictionary)
        {
            GridPropertyDetails gridPropertyDetails = item.Value;
            DisplayDugGround(gridPropertyDetails);

            DisplayWateredGround(gridPropertyDetails);
        }
    }


    /// <summary>
    ///  This initialises the grid property dict with the values from the SO_GridProperties assets and stores the values for each scene in
    /// </summary>
    private void InitialiseGridProperties()
    {
        // Loop thru all gridProperties in the array
        foreach (var so_gridProperties in so_gridPropertiesArray)
        {
            // Create dict of grid property details
            var gridPropertyDictionary = new Dictionary<string, GridPropertyDetails>();

            // Populate grid property dict - iterate thru all the grid properties in the so_gridProperties list
            foreach (var gridProperty in so_gridProperties.gridPropertyList)
            {
                GridPropertyDetails gridPropertyDetails;

                gridPropertyDetails = GetGridPropertyDetails(gridProperty.gridCoordinate.x, gridProperty.gridCoordinate.y, gridPropertyDictionary);

                if (gridPropertyDetails == null)
                {
                    gridPropertyDetails = new GridPropertyDetails();
                }

                switch (gridProperty.gridBoolProperty)
                {
                    case GridBoolProperty.diggable:
                        gridPropertyDetails.isDiggable = gridProperty.gridBoolValue;
                        break;
                    case GridBoolProperty.canDropItem:
                        gridPropertyDetails.canDropItem = gridProperty.gridBoolValue;
                        break;
                    case GridBoolProperty.canPlaceFurniture:
                        gridPropertyDetails.canPlaceFurniture = gridProperty.gridBoolValue;
                        break;
                    case GridBoolProperty.isPath:
                        gridPropertyDetails.isPath = gridProperty.gridBoolValue;
                        break;
                    case GridBoolProperty.isNPCObstacle:
                        gridPropertyDetails.isNPCObstacle = gridProperty.gridBoolValue;
                        break;
                    default:
                        break;
                }

                SetGridPropertyDetails(gridProperty.gridCoordinate.x, gridProperty.gridCoordinate.y, gridPropertyDetails, gridPropertyDictionary);
            }

            // Create scene save for this gameObject
            var sceneSave = new SceneSave();

            // Add grid property dict to scene save data
            sceneSave.gridPropertyDetailsDictionary = gridPropertyDictionary;

            // if starting scene set the gridPropertyDictionary member variable  to the current iteration
            if (so_gridProperties.sceneName.ToString() == SceneControllerManager.Instance.startingSceneName.ToString())
            {
                this.gridPropertyDictionary = gridPropertyDictionary;
            }

            // Add scene save to gameObject scene data
            GameObjectSave.sceneData.Add(so_gridProperties.sceneName.ToString(), sceneSave);
        }
    }


    /// <summary>
    /// Returns the gridPropertyDetails at the gridLocation for the supplied dictionary, or null if no properties exist at that location
    /// </summary>
    public GridPropertyDetails GetGridPropertyDetails(int gridX, int gridY, Dictionary<string, GridPropertyDetails> gridPropertyDictionary)
    {
        // Construct key from coordinate
        string key = "x" + gridX + "y" + gridY;

        GridPropertyDetails gridPropertyDetails;

        // Check if grid property details exist for coordinate and retrieve
        if (!gridPropertyDictionary.TryGetValue(key, out gridPropertyDetails))
        {
            // if not found
            return null;
        }
        else
        {
            return gridPropertyDetails;
        }
    }


    /// <summary>
    /// Get the grid property details for the tile at (gridX / gridY). 
    /// If no grid property details exist - null is returned and can assume that all grid property details values are null or false
    /// </summary>
    public GridPropertyDetails GetGridPropertyDetails(int gridX, int gridY)
    {
        return GetGridPropertyDetails(gridX, gridY, gridPropertyDictionary);
    }



    /// <summary>
    /// Set the grid property details to gridPropertyDetails for the ile at (gridX, gridY) for the grodPropertyDictionary
    /// </summary>
    public void SetGridPropertyDetails(int gridX, int gridY, GridPropertyDetails gridPropertyDetails, Dictionary<string, GridPropertyDetails> gridPropertyDictionary)
    {
        // Construct key from coordinate
        string key = "x" + gridX + "y" + gridY;

        gridPropertyDetails.gridX = gridX;
        gridPropertyDetails.gridY = gridY;

        // Set value
        gridPropertyDictionary[key] = gridPropertyDetails;
    }


    /// <summary>
    /// Set the grid property details to gridPropertyDetails for the tile at (gridX, gridY) for current scene
    /// </summary>
    public void SetGridPropertyDetails(int gridX, int gridY, GridPropertyDetails gridPropertyDetails)
    {
        SetGridPropertyDetails(gridX, gridY, gridPropertyDetails, gridPropertyDictionary);
    }

    public void ISavableDeregister()
    {
        SaveLoadManager.Instance.iSavableObjectList.Remove(this);
    }

    public void ISavableRegister()
    {
        SaveLoadManager.Instance.iSavableObjectList.Add(this);
    }

    public void ISavableRestoreScene(string sceneName)
    {
        // Get sceneSave for scene - it exists since we created it in initialise
        if (GameObjectSave.sceneData.TryGetValue(sceneName, out SceneSave sceneSave))
        {
            // get grid property details dict - it exists since we created it in initialise
            if (sceneSave.gridPropertyDetailsDictionary != null)
            {
                gridPropertyDictionary = sceneSave.gridPropertyDetailsDictionary;
            }

            // if grid properties exist
            if (gridPropertyDictionary.Count > 0)
            {
                // grid property details found for the current scene destroy existing ground decoration
                ClearDisplayGridPropertyDetails();

                // Instantiate grid prop details for current scene
                DisplayGridPropertyDetails();
            }
        }
    }

    public void ISavableStoreScene(string sceneName)
    {
        // Remove sceneSave for scene
        GameObjectSave.sceneData.Remove(sceneName);

        // Create sceneSave for scene
        var sceneSave = new SceneSave();

        // create & add dict grid property details dict
        sceneSave.gridPropertyDetailsDictionary = gridPropertyDictionary;

        // Add scene save to gameObject scene data
        GameObjectSave.sceneData.Add(sceneName, sceneSave);
    }


    private void AfterSceneLoaded()
    {
        // Get Grid. We have only 1 on a scene
        grid = FindObjectOfType<Grid>();


        // Get tilemaps
        groundDecoration1 = GameObject.FindGameObjectWithTag(Tags.GroundDecoration1).GetComponent<Tilemap>();
        groundDecoration2 = GameObject.FindGameObjectWithTag(Tags.GroundDecoration2).GetComponent<Tilemap>();
    }

    private void AdvanceDay(int gameYear, Season gameSeason, int gameDay, string gameDayOfWeek, int gameHour, int gameMinute, int gameSecond)
    {
        // Clear Display All Grid Property Details
        ClearDisplayGridPropertyDetails();

        // Loop thru all scenes - by looping thru all gridprops in the array
        foreach (SO_GridProperties so_GridProperties in so_gridPropertiesArray)
        {
            // Get gridPropertyDetails dict for scene
            if (GameObjectSave.sceneData.TryGetValue(so_GridProperties.sceneName.ToString(), out SceneSave sceneSave))
            {
                if (sceneSave.gridPropertyDetailsDictionary != null)
                {
                    for (int i = sceneSave.gridPropertyDetailsDictionary.Count - 1; i >= 0; i--)
                    {
                        KeyValuePair<string, GridPropertyDetails> item = sceneSave.gridPropertyDetailsDictionary.ElementAt(i);

                        GridPropertyDetails gridPropertyDetails = item.Value;

                        #region Update all grid properties to reflect the advance in the day

                        // if ground is watered, then clear water
                        if (gridPropertyDetails.daysSinceWatered > -1)
                        {
                            gridPropertyDetails.daysSinceWatered = -1;
                        }

                        // Set gridPropertyDetail
                        SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails, sceneSave.gridPropertyDetailsDictionary);

                        #endregion Update all grid properties to reflect the advance in the day
                    }
                }
            }
        }

        // Display gridPropertyDetails to reflect changed values
        DisplayGridPropertyDetails();
    }
}
