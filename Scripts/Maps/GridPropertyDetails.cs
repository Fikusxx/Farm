using UnityEngine;


[System.Serializable]
public class GridPropertyDetails
{

    public int gridX;
    public int gridY;

    // Populated out of SO_GridProperties
    public bool isDiggable = false;
    public bool canDropItem = false;
    public bool canPlaceFurniture = false;
    public bool isPath = false;
    public bool isNPCObstacle = false;

    // Populated during gameplay
    public int daysSinceDug = -1;
    public int daysSinceWatered = -1;
    public int seedItemCode = -1;
    public int drowthDays = -1;
    public int daysSinceLastHarvest = -1;

    public GridPropertyDetails() { }

}
