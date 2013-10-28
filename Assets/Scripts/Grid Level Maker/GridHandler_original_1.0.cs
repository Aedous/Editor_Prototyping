using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/*
 * Version 1.1:
 * 
 * Layers are more advanced and control the blocks better.
 * 
 * This script simply handles the interaction between user and tool
 * 
 * */

/*
[ExecuteInEditMode]
public class GridHandler : MonoBehaviour
{
    #region inspector variable
    //Version 1.1 - Variables
    public int blocksize, maximumrows, maximumcolumns;
    public Vector2 hoverIndex { get; set; }

    //Create individual block height and width
    public Color color = Color.white;
    public Color guidecolor = Color.red;
    public Color hovercolor = Color.blue;

    //Create variables for holding the list of blocks
    public GameObject tile;
    public int maxlayers = 2; //How many layers the tile has (moves in the z direction)
    public float layerspacing = 20f;

    

    #endregion

    #region public variables
    public int CurrentLayer { get; set; } //Start at 0.. so at bottom layer.
    public bool ShowGrid { get; set; } //Draw Grid ?
    public bool AutomaticUpdates { get; set; } //If this is turned on, the object updates itself everytime it's activated.
    public string NameOfLevel { get; set; } //Name of level
    public bool KeepFunctionality { get; set; } //Keep layer functionality
    public bool Clip { get; set; } //Clip out the empty objects
    public bool GridShowing { get; set; } //This is used for editor scripting to allow switching of the toggle
    #endregion

    #region private variables
    
    #endregion

    #region Unity Methods
    // Use this for initialization
    void Start()
    {
        //Set the layer
        CurrentLayer = 0;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnEnable()
    {
        //Create the layer manager
        //Make sure that the children are referenced in the layer list
        NameOfLevel = "level_name";
        KeepFunctionality = false;
        Clip = false;
        
    }

    #region Editor Methods
    
    #endregion

    #endregion

    #region Class Methods
    public bool MoveToLayer(int layer)
    {
        //First check if the layer is in bounds
        if(layer < maxlayers && layer > -1)
        {
            CurrentLayer = layer;
            return true;
        }

        //Out of bounds
        return false;
    }

    public void CreateGrid(int num_rows, int num_col, int block_size, int layers, int layer_spacing, bool addcollisions = true)
    {
        //The addcollision parameter is to control whether or not to build collision around the columns
        //Set rows and columns and block size, and create layer
        maximumrows = num_rows;
        maximumcolumns = num_col;
        blocksize = block_size;

        maxlayers = layers; //number of layers
        layerspacing = layer_spacing;

        //Create layers
        //CreateAndUpdateLayers();
        CreateLayer(0, "Layer", addcollisions);
    }

    public void CreateAndUpdateLayers()
    {
        //This script cycles through, and create's layer's depending on if there
        //are no children and updates if there are.
        int numberoflayers = transform.childCount;

        if (numberoflayers <= 0)
        {
            //Create the layers that we need
            for (int i = 0; i < maxlayers; i++)
            {
                //Create the layers
                CreateLayer(i);
            }
        }
        else
        {
            //If we have some children we need to update and create
            //new ones if needed.
            if (numberoflayers < maxlayers)
            {
                //If we have some missing rows, cycle through and check if a child exists
                //at that location
                for (int i = numberoflayers; i < maxlayers; i++)
                {
                    int index = i;
                    //We start at the amount of children we have and just create the rest
                    CreateLayer(index);
                }
            }
            else if (numberoflayers > maxlayers)
            {
                //We have some extra row's we need to delete
                for (int i = maxlayers; i < numberoflayers; i++)
                {
                    //Grab the children and delete
                    GameObject rowobject = transform.GetChild(maxlayers).gameObject;
                    DestroyImmediate(rowobject);
                }
            }
        }

        //Then we need to update the layers, and make sure they are all in the right position
        UpdateAndManageLayers();

    }

    public void UpdateAndManageLayers()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            UpdateLayer(i);
        }
    }

    public void UpdateLayer(int index)
    {
        //Change and update names, or make more configurations
        LayerHandler script = transform.GetChild(index).GetComponent<LayerHandler>();

        //Sync the name
        script.SyncName("Layer " + index);
        script.SetLayerIndex(index);
        script.UpdateGridSettings();
        script.CreateAndUpdateRows();

        //Also fix positioning as well
        OrganizeLayer(index);
    }

    public void UpdateLayers()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            //Update the layer ------------ //
            UpdateLayer(i);
        }
    }

    public GameObject CreateLayer(int index, string name = "Layer", bool addcollision = true)
    {
        Debug.Log("Building Layer : " + index);
        GameObject layer = new GameObject(name + " " + index);

        
        layer.transform.parent = transform;
        layer.transform.position = Vector3.zero;
        layer.transform.localPosition = Vector3.zero;
        layer.layer = LayerMask.NameToLayer("ignorerowcol");

        //Set the layer position to be that of the depth
        //Get Depth
        float depth;
        depth = index * layerspacing;

        layer.transform.position = new Vector3(layer.transform.position.x, layer.transform.position.y, layer.transform.position.z + depth);

        //After creating the layer attach a script component to it
        layer.AddComponent<LayerHandler>();
        LayerHandler script = layer.GetComponent<LayerHandler>();
        script.SetLayerIndex(index);
        script.UpdateGridSettings();
        script.CreateAndUpdateRows(addcollision);

        //Set the max layers to how many children we now have
        maxlayers = transform.childCount;

        return layer;
    }


    public void DeleteLayer(int index)
    {
        //Destroy the current layer we are looking at
        Debug.Log("Deleting Layer : " + index);

        //Delete the child object using the layer name
        GameObject childLayer = transform.GetChild(index).gameObject;
        
        if (childLayer != null)
        {
            //Send a message to the child layer to clear its own list
            LayerHandler script = childLayer.GetComponent<LayerHandler>();
            script.RemoveAllObjectsInLayer();
            DestroyImmediate(childLayer);
        }

        //After deleting the layer subtract the number of layers by one
        maxlayers--;

        if (CurrentLayer == maxlayers)
        {
            Debug.Log("Current Layer : " + CurrentLayer);
            int prev = CurrentLayer--;
            Debug.Log("Prev Layer : " + prev);
            if (prev > 0)
            {
                CurrentLayer = prev;
            }
            else
                CurrentLayer = 0;
            Debug.Log("Current Layer After : " + CurrentLayer);
        }
        
    }

    public void MoveLayers(int currentindex, int movingindex)
    {
        //Make a copy of the current object list, and make a copy of the moving index list
        LayerHandler currentlayerscript = transform.GetChild(currentindex).GetComponent<LayerHandler>();
        LayerHandler movelayerscript = transform.GetChild(movingindex).GetComponent<LayerHandler>();

        //Make a copy of the children from the layer passed in
        List<GameObject> currentobjectlist = new List<GameObject>(), moveobjectlist = new List<GameObject>();

        for (int i = 0; i < transform.GetChild(currentindex).childCount; i++)
        {
            //Set the children in the list
            //currentobjectlist.Add(layer.transform.GetChild(i).gameObject);
        }

        //After making copy of the children in the layer, make the second copy
        for (int i = 0; i < transform.GetChild(movingindex).childCount; i++)
        {
            //Make a second copy
            //moveobjectlist.Add(layerManager[movingindex].transform.GetChild(i).gameObject);
        }

        //Once we have a copy of the two, switch them around
        //Remove all objects in both positions we are moving to
        currentlayerscript.RemoveAllObjectsInLayer();
        movelayerscript.RemoveAllObjectsInLayer();

        //Update the layers to match everything up
        UpdateLayers();
        
    }

    public GameObject GetColumnToBuildBlock()
    {
        //This script takes the object we want to create and passes it down to the layer
        //which then takes care of handling the location of where to build the block

        if (CurrentLayer < transform.childCount)
        {
            GameObject layerobject = transform.GetChild(CurrentLayer).gameObject;
            LayerHandler layer = layerobject.GetComponent<LayerHandler>();

            //Make sure layer script exists
            if (layer)
            {
                //Also make sure our hover index is all in bound.
                if (hoverIndex.x != -1 && hoverIndex.y != -1)
                {
                    //Grab the column we are looking for
                    GameObject columntobuildblock = layer.GetColumnObject(hoverIndex);
                    return columntobuildblock;

                }
                else
                {
                    Debug.Log("Index out of bounds for building block");
                    return null;
                }
            }

            Debug.Log("Could not enter layer handler in 'GetColumnToBuildBlock'");
            return null; //Problem
        }

        Debug.Log("Current Layer Index is out of bounds");
        return null; 

    }

    public bool CheckIfChildGameObjectExists(int index)
    {
        GameObject child = transform.GetChild(index).gameObject;

        if (child)
        {
            return true;
        }

        return false;
    }

    public GameObject GetChildGameObject(int index)
    {
        //Check to see if the index is in bounds
        int childcount = transform.childCount;
        if (index < childcount)
        {
            return transform.GetChild(index).gameObject;
        }

        return null; //Index out of bounds
    }

    public void OrganizeLayers()
    {
        //Sometimes the positions of the layer's mess up, so we need to reorganize them before
        //we allow building.
        int childcount = transform.childCount;

        for (int i = 0; i < childcount; i++)
        {
            //Work out the depth of the object
            //Make sure all layers are in the right position
            OrganizeLayer(i);
        }
    }


    //Organize layer ------------------------------------------------------------//
    public void OrganizeLayer(int index)
    {
        //Sometimes the positions of the layer's mess up, so we need to reorganize them before
        LayerHandler script = transform.GetChild(index).GetComponent<LayerHandler>();
        script.FixPosition(layerspacing);
    }

    public void DisableLayers()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject layer = transform.GetChild(i).gameObject;

            layer.active = false;
            
        }
    }

    public void EnableLayers()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject layer = transform.GetChild(i).gameObject;

            layer.active = true;
            
        }
    }

    public void DisableAllColumns()
    {
        //Cycle through all the columns and disable them
        for (int i = 0; i < transform.childCount; i++)
        {
            //Get the layer
            GameObject child = GetChildGameObject(i);

            //Get the script
            LayerHandler script = child.transform.GetComponent<LayerHandler>();

            //Check it exists and disable all the children
            if (script)
            {
                script.DisableAllChildren();
            }
        }
    }

    public void EnableAllColumns()
    {
        //Cycle through all columns and enable them
        for (int i = 0; i < transform.childCount; i++)
        {
            //Get the layer
            GameObject child = GetChildGameObject(i);

            //Get the script
            LayerHandler script = child.transform.GetComponent<LayerHandler>();

            //Check it exists and enable all the children
            if (script)
            {
                script.EnableAllChildren();
            }
        }
    }

    
    #endregion
}
 
*/