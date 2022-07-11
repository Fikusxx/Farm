using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;


// sits in each extra tilemap with a specific gridBoolProperty below

[ExecuteAlways]
public class TileMapGridProperties : MonoBehaviour
{

    private Tilemap tilemap;
    [SerializeField] private SO_GridProperties gridProperties = null;
    [SerializeField] private GridBoolProperty gridBoolProperty = GridBoolProperty.diggable;


    private void OnEnable()
    {
        // Only populate in the editor
        if (!Application.IsPlaying(gameObject))
        {
            tilemap = GetComponent<Tilemap>(); // get Tilemap component

            if (gridProperties != null)
            {
                gridProperties.gridPropertyList.Clear();
            }
        }
    }

    private void OnDisable()
    {
        // Only populate in the editor
        if (!Application.IsPlaying(gameObject))
        {
            UpdateGridProperties();

            if (gridProperties != null)
            {
                // This is required to ensure that the updated gridProperties GO gets saved when the game is saved - otherwise they are not saved.
                EditorUtility.SetDirty(gridProperties);
            }
        }
    }


    private void UpdateGridProperties()
    {
        // Compress tilemap bounds. Kinda delete all emptiness between cells we painted
        tilemap.CompressBounds();

        // Only populate in the editor
        if (!Application.IsPlaying(gameObject))
        {
            if (gridProperties != null)
            {
                Vector3Int startCell = tilemap.cellBounds.min; // gets basically 0,0 
                Vector3Int endCell = tilemap.cellBounds.max; // gets something like 30,30

                for (int x = startCell.x; x < endCell.x; x++)
                {
                    for (int y = startCell.y; y < endCell.y; y++)
                    {
                        TileBase tile = tilemap.GetTile(new Vector3Int(x, y, 0));

                        // certain cells after CompressBounds() will be null (we didnt paint there)
                        // therefore there will be "default" GridProperty object with gridBoolValue set to "false", so we cant do anything with it
                        if (tile != null) 
                        {
                            gridProperties.gridPropertyList.Add(new GridProperty(new GridCoordinate(x, y), gridBoolProperty, true));
                        }
                    }
                }
            }
        }
    }

    // Just warning to disable property tile map
    private void Update()
    {
        if (!Application.IsPlaying(gameObject))
        {
            Debug.Log("DISABLE PROPERTY TILEMAPS");
        }
    }
}
