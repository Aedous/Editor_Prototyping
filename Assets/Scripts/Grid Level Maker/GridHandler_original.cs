using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/*
 * Version 1.0:
 *
 * Creates a grid and handles creation of blocks
 * Block's can only be drawn flat on z axis
 * 
 * Working Version.
 * 
 * */

/*
[ExecuteInEditMode]
public class GridHandler : MonoBehaviour
{
    //Create a class that handles the blocks.
    [Serializable]
    public class Block
    {
        //This class handles the blocks created.
        public GameObject block; //The block prefab it holds

        //Collision variables for the block
        public bool hasRigidBody, hasCollision, hasTrigger, isKinematic;

        //Layer index, and list position
        public int layerIndex, positioninlist;

        //World and Local Position
        public Vector3 WorldPosition, LocalPosition;

        //Methods
        public Vector3 GetPosition(string type)
        {
            switch (type)
            {
                case "local":
                    LocalPosition = block.transform.localPosition;
                    return LocalPosition;

                case "world":
                    WorldPosition = block.transform.position;
                    return WorldPosition;
                default:
                    return Vector3.zero; //Return 0,0,0  for defualt position
            }
        }

    }

    #region inspector variables


    //Create grid height and width
    public float gridwidth = 20.0f;
    public float gridheight = 20.0f;

    //Create individual block height and width
    public float width = 32.0f;
    public float height = 32.0f;
    public Color color = Color.white;

    //Create variables for holding the list of blocks
    public GameObject tile;
    public int maxlayers = 2; //How many layers the tile has (moves in the z direction)
    public float layerspacing = 20f;

    #endregion

    #region public variables
    public int CurrentLayer { get; set; } //Start at 0.. so at bottom layer.
    public bool ShowGrid { get; set; } //Draw Grid ?
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
    }

    #region Editor Methods
    void OnDrawGizmos()
    {
        if (ShowGrid)
        {
            //Apply gizmo color
            Gizmos.color = color;

            //Get Transform position
            Vector3 position = transform.position;

            //Draw the WHOLE Grid with Onion Skinning
            //Draw this layers grid,
            DrawGrid(position, width, height, gridwidth, gridheight, CurrentLayer, Color.white);

            //If we are on a layer that has a behind
            int nextlayer = CurrentLayer + 1;
            int previouslayer = CurrentLayer - 1;

            //Aslong as the layer infront exists, draw it
            if (nextlayer <= maxlayers)
            {
                //Draw the next grid
                DrawGrid(position, width, height, gridwidth, gridheight, nextlayer, Color.grey);
            }

            if (previouslayer >= 0)
            {
                //Draw the previous grid
                DrawGrid(position, width, height, gridwidth, gridheight, previouslayer, Color.grey);
            }
        }
    }
    #endregion

    #endregion

    #region Class Methods
    public void DrawGrid(Vector3 gridposition, float blockwidth, float blockheight, float gridw, float gridh, int layer, Color gridcolor)
    {
        float depth;
        depth = layer * layerspacing;

        //Apply gizmo color
        Gizmos.color = gridcolor;

        for (float y = gridposition.y - gridh; y <= gridposition.y + gridh; y += blockheight)
        {
            Gizmos.DrawLine(new Vector3(-gridh * blockheight, Mathf.Floor(y / blockheight) * blockheight,  gridposition.z + depth),
                            new Vector3(gridh * blockheight, Mathf.Floor(y / blockheight) * blockheight, gridposition.z + depth));
        }

        //Horizontal Lines
        for (float x = gridposition.x - gridw; x <= gridposition.x + gridw; x += blockwidth)
        {
            Gizmos.DrawLine(new Vector3(Mathf.Floor(x / blockwidth) * blockwidth, -gridw * blockwidth, gridposition.z + depth),
                            new Vector3(Mathf.Floor(x / blockwidth) * blockwidth, gridw * blockwidth, gridposition.z + depth));
        }
    }

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

    
    public void CreateLayersAndHandlers()
    {
        //Create all the layers we need, especially the missing ones
        Debug.Log("Function called");

        //Get the amount of children and compare it to the max layers we are supposed to have
        int childcount = transform.childCount;

        //This function is called only when there are no children
        if (childcount == 0)
        {
            //Need to create the layers
            for (int i = 0; i < maxlayers; i++)
            {
                //Create the layers we need
                CreateLayer(i);  
            }
        }
    }

    public void UpdateLayer(int index)
    {
        //Change and update names, or make more configurations
        LayerHandler script = transform.GetChild(index).GetComponent<LayerHandler>();

        //Sync the name
        script.SyncName("Layer " + index);
        script.SetLayerIndex(index);

        //Also fix positioning as well
        OrganizeLayers();
    }

    public void UpdateLayers()
    {
       //This makes sure all the layers are up to date, (mostly handled by the CreateLayersAndHandlers Method
        int childcount = transform.childCount;

        //Before we update layers, we need to make sure everything is correct with the layers and the amount of children
        if (childcount < maxlayers)
        {
            //We have missing objects and need to recreate
            if (childcount == 0)
            {
                //If there are no children we need to just ReCreate Everything!
                CreateLayersAndHandlers();
            }
            else
            {
                //If we atleast have one object in there, we need to filter through, update and create the rest
                for (int i = 0; i < maxlayers; i++)
                {
                    //Check to see if the index is nout of bounds
                    if (i < childcount)
                    {
                        //If i is less than the amount of children, then we are in bounds and can check the object
                        if (transform.GetChild(i).gameObject) //If we have a game object here
                        {
                            //Check to make sure it's in the correct position in the list
                            LayerHandler script = transform.GetChild(i).GetComponent<LayerHandler>();
                            script.SyncName("Layer " + i);
                        }
                        else
                        {
                            //If a game object of layer does not exist, create it again
                            CreateLayer(i);
                        }
                    }
                    else
                    {
                        //Otherwise if we are out of bounds, we need to create the layer
                        CreateLayer(i);
                    }

                }
            }
            
        }
        else if (childcount > maxlayers)
        {
            //We have an extra object with less layers ? (probably would never happen)

        }
        else
        {
            for (int i = 0; i < childcount; i++)
            {
                //Filter through and update every layer
                UpdateLayer(i);
            }
        }
    }

    public GameObject CreateLayer(int index, string name = "Layer")
    {
        Debug.Log("Building Layer : " + index);
        GameObject layer = new GameObject(name + " " + index);

        
        layer.transform.parent = transform;
        layer.transform.position = Vector3.zero;
        layer.transform.localPosition = Vector3.zero;

        //Set the layer position to be that of the depth
        //Get Depth
        float depth;
        depth = index * layerspacing;

        layer.transform.position = new Vector3(layer.transform.position.x, layer.transform.position.y, layer.transform.position.z + depth);

        //After creating the layer attach a script component to it
        layer.AddComponent<LayerHandler>();
        LayerHandler script = layer.GetComponent<LayerHandler>();
        script.SetLayerIndex(index);

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

    public void SyncLayers()
    {
        //Make sure all the layer names are synced
        for (int i = 0; i < transform.childCount; i++)
        {
            string name = "Layer " + i;
            GameObject child = transform.GetChild(i).gameObject;

            //Child exists as a game object
            if (child != null)
            {
                //Sync the names
                LayerHandler script = child.GetComponent<LayerHandler>();
                script.SyncName(name);
                script.SetLayerIndex(i);
            }
        }

        //Organize the layers as well
        OrganizeLayers();

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

    public void CreateObjectForLayer(GameObject _object = null)
    {
        
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

    public float CalculateDepth()
    {
        //This calculates the depth of the grid by taking t
        float depth = 0f;



        return depth;
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
            LayerHandler script = transform.GetChild(i).GetComponent<LayerHandler>();
            script.FixPosition(layerspacing);
        }

    }

    #endregion
}
 
 */
