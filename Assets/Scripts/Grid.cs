using UnityEngine;
using System;
using System.Collections;

[ExecuteInEditMode]
public class Grid : MonoBehaviour
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

    [Serializable]
    //Create a manager for the list of blocks for each layer
    public class BlockList
    {
        //This block list holds a number of layers, with each layer holding a number of blocks
        public ArrayList layerlist;
        public int currentLayer; //Current layer we are looking at

        public void CreateList(int numberoflayers, int maxblocksperlayer)
        {
            //Create the list of block list with their own lists of blocks
            layerlist = new ArrayList(); //this is a list of numbers matching the layer we are looking for

            //Create the list that holds the blocks
            for (int i = 0; i < numberoflayers; i++)
            {
                //This array list holds the block class when you create them
                layerlist[i] = new ArrayList();
                ArrayList blocklist = (ArrayList)layerlist[i]; //Get the array list of blocks from the layer list

                //Set the max the list of blocks can hold
                blocklist.Capacity = maxblocksperlayer;
            }
        }

        public void AddToList(int layer, GameObject blocktoadd)
        {
            //As long as the layer is less than the length of the list
            //add the block to the list in that layer
            if (layer > layerlist.Count || layer < 0)
            {
                //Set the layer to 0 to stop any errors
                layer = 0;
            }

            //Add the block to that layer in the block list
            
        }

        public ArrayList GetListOfBlocks
        {
            //Get the list of blocks from our current layer
            get { return (ArrayList)layerlist[currentLayer]; }
            set { layerlist[currentLayer] = value; }
        }

        public void SetLayer(int layer)
        {
            //Set the layer we are looking at
            currentLayer = layer;
        }

        public void SetListOfBlocks(ArrayList blocklist)
        {
            //Set the block list for the current layer we are looking at

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

    #endregion

    #region public variables
    
    #endregion

    #region private variables
    private int currentLayer = 0; //Start at 0.. so at bottom layer.
    #endregion

    #region Unity Methods
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    #region Editor Methods
    void OnDrawGizmos()
    {
        //Get the camera position
        Vector3 cameraPosition = Camera.current.transform.position;
        
        //Apply gizmo color
        Gizmos.color = color;

        //Two loops to draw horizontal and vertical lines
        //Vertical Lines
        for (float y = cameraPosition.y - gridheight; y < cameraPosition.y + gridheight; y += height)
        {
            Gizmos.DrawLine(new Vector3(-1000000.0f, Mathf.Floor(y / height) * height, 0.0f),
                            new Vector3(1000000.0f, Mathf.Floor(y / height) * height, 0.0f));
        }

        //Horizontal Lines
        for (float x = cameraPosition.x - gridwidth; x < cameraPosition.x + gridwidth; x += width)
        {
            Gizmos.DrawLine(new Vector3(Mathf.Floor(x / width) * width, -1000000.0f, 0.0f),
                            new Vector3(Mathf.Floor(x / width) * width, 1000000.0f, 0.0f));
        }
    }
    #endregion

    #endregion

    #region Class Methods
    public void UpdateGrid()
    {
        //width = 
    }
    #endregion
}
