using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//This class handles the layers and the objects that they carry.
[ExecuteInEditMode]
public class LayerHandler : MonoBehaviour
{
    #region inspector variables
    public int currentIndex;
    public bool showDetails = false;
    public bool drawguide = false; //Draws a square by square guide of how big a square is

    //Editor Variables
    public int numberofrows, numberofcolumns, blocksize; //draws the squares with their block size
    public Color guidecolor;
    #endregion

    #region public variables
    public GameObject childObject{get; set;} //reference for getting the child object. (called on enable )
    public GameObject parentObject { get; set; } //Reference for getting parent object.
    public Vector2 hoverIndex { get; set; } //Reference for the transform we have our mouse over
    #endregion

    #region private variables
    
    #endregion

    #region Editor Methods
    #endregion

    #region Unity Methods
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Parent object ---- //
        if (parentObject == null)
        {
            if (transform.parent.gameObject != null)
            {
                parentObject = transform.parent.gameObject;
            }
            else
            {
                Debug.Log("No parent object for transform");
            }
        }
    }

    void OnEnable()
    {   
        if (parentObject == null)
        {
            //We have to manually search for the gameobject
            Debug.Log("Do not have reference for parent object variable");
        }

        //Update the settings for the layer
        UpdateGridSettings();

        //Create the game objects, and update if necessary
        CreateAndUpdateRows();
    }

    void OnAwake()
    {

    }

    void OnDisable()
    {
        //When the game object layer is disabled
    }
    #endregion

    #region Class Methods
    public void SyncName(string name)
    {
       //Change name
        transform.name = name;
       
    }

    public GameObject GetColumnObject(Vector2 hoverindex)
    {
        //Create a block in the position
        GameObject column, row;

        row = GetRowObject((int)hoverindex.x);
        column = row.transform.GetChild((int)hoverindex.y).gameObject;

        return column;
    }

    public GameObject GetColumnObject(int rowindex, int colindex)
    {
        GameObject column, row;
        row = GetRowObject(rowindex);
        column = row.transform.GetChild(colindex).gameObject;

        return column;
    }

    public GameObject GetRowObject(int index)
    {
        GameObject row;

        row = transform.GetChild(index).gameObject;

        return row;
    }

    public void AddGameObjectToLayer(GameObject _object, int index = -1)
    {
        //Add the game object as a child of this layer
        _object.transform.parent = transform;
        _object.layer = LayerMask.NameToLayer("Default");
        
    }

    public void MergeGameObjectList(List<GameObject> merger)
    {
        //Add the list of game objects as children in this layer
    }

    public void RemoveObjectInLayer(GameObject _object)
    {
        //Remove the specific game object
        if (transform.FindChild(_object.name))
        {
            //Delete Object as well
            DestroyImmediate(_object);
        }
    }

    public void RemoveAllObjectsInLayer()
    {
        if (transform.childCount > 0)
        {
            //Remove and destroy all children
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;
                DestroyImmediate(child);
            }
        }
    }

    public void RemoveLayer()
    {
        DestroyImmediate(gameObject); //Destroy self
    }

    public void SetLayerIndex(int index)
    {
        currentIndex = index; //Current layer
    }

    public List<GameObject> GetAListOfChildren()
    {
        int count = transform.childCount;
        List<GameObject> childlist = new List<GameObject>();

        for (int i = 0; i < count; i++)
        {
            //Get the child as a game object and put it in the list
            GameObject child = transform.GetChild(i).gameObject;
            childlist.Add(child);
        }

        return childlist;
    }

    public void DisableAllChildren()
    {
        //Disable all children
        List<GameObject> children = GetAListOfChildren();

        for (int i = 0; i < children.Count; i++)
        {
            GameObject child = children[i];

            child.active = false;
            
        }
    }

    public void EnableAllChildren()
    {
        //Enable all children
        List<GameObject> children = GetAListOfChildren();

        for (int i = 0; i < children.Count; i++)
        {
            GameObject child = children[i];

            child.active = true;
            
        }

    }

    public void FixPosition(float damp)
    {
		Vector3 newposition = Vector3.zero; //Alter the new position
		
		if(parentObject) //Check our parent object exists
		{
			GridHandler grid = parentObject.GetComponent<GridHandler>();
			
			if(grid) //Check our gridhandler script also exists
			{
				//Work out the directions
				switch(grid.creationDirection)
				{
					
					case GridHandler.CreationDirection.UP:
					{
						//Move the layer upwards
						newposition = new Vector3(0f, (float)(currentIndex * damp), 0f);
					}
					break;
					case GridHandler.CreationDirection.DOWN:
					{
						//Move the layer downwards
						newposition = new Vector3(0f, (float)(currentIndex * damp) * -1, 0f);
					}
					break;
					case GridHandler.CreationDirection.FORWARD:
					{
						//Move the layer forwards
						newposition = new Vector3(0f, 0f, (float)(currentIndex * damp));
					}
					break;
					case GridHandler.CreationDirection.BACKWARD:
					{
						//Move the layer backwards
						newposition = new Vector3(0f, 0f, (float)(currentIndex * damp) * -1);
					}
					break;
					case GridHandler.CreationDirection.LEFT:
					{
						//Multiply the damp by the number of layers to get the width (damp is usually the block size)
						//Move the layer left
						newposition = new Vector3((float)(currentIndex * (damp * grid.maxlayers) * -1),0f,0f);
					}
					break;
					case GridHandler.CreationDirection.RIGHT:
					{
						//Multiply the damp by the number of layers to get the width (damp is usually the block size)
						//Move the layer right
						newposition = new Vector3((float)(currentIndex * (damp * grid.maxlayers)),0f,0f);
					}
					break;
				}
			}
		}
         
		//newposition = new Vector3(0f, 0f,(float)(currentIndex * damp));
        transform.localPosition = newposition;
    }

    public void CreateAndUpdateRows(bool addcollisions = true)
    {
        //First we check to see if we have any children, if we don't we create our rows
        //If we do have children, then we make sure we have the right amount and create accordingly
        int numberofchildren = transform.childCount;

        if (numberofchildren <= 0)
        {
            //If we have no children, create the rows
            CreateRows(numberofrows, transform.position, addcollisions);
        }
        else
        {
            //Check our row amount, and create accordingly
            if (numberofchildren < numberofrows)
            {
                //If we have some missing rows, cycle through and check if a child exists
                //at that location
                for (int i = numberofchildren; i < numberofrows; i++)
                {
                    int index = i;
                    //We start at the amount of children we have and just create the rest
                    CreateRow(index, transform.position, addcollisions);
                }
            }
            else if (numberofchildren > numberofrows)
            {
                //We have some extra row's we need to delete
                //for (int i = numberofrows; i < numberofchildren; i++)
                //{
                    //Grab the children and delete
                    GameObject rowobject = transform.GetChild(numberofrows).gameObject;
                    DestroyImmediate(rowobject);
                //}
            }
        }

        //Update all the rows that we have and make sure they have the right coordinates
        UpdateRows();
        
    }

    public void UpdateGridSettings()
    {
        //Parent object ---- //
        parentObject = transform.parent.gameObject;

        //This script updates the block size, rowamount, rowheight from parent 'Grid Handler'
        if (parentObject) //If the parent object exists, grab the variables from there
        {
            GridHandler handler = parentObject.GetComponent<GridHandler>();

            //Check if the script exists
            if (handler)
            {
                //Get the variables and store them here
                blocksize = handler.blocksize;
                numberofrows = handler.maximumrows;
                numberofcolumns = handler.maximumcolumns;
            }
            else
            {
                Debug.LogError("Can't find script 'GridHandler', check parent object.");
            }
        }
        else
        {
            Debug.LogError("Missing Parent 'Grid' Object.");
        }
    }

    public void CreateRow(int index, Vector3 rowposition, bool addcollisions = true)
    {
        Debug.Log("Creating row at index : " + index);
        int collisionsize = blocksize * numberofcolumns;
		int move = blocksize * index;
		Vector3 alteredposition = Vector3.zero;

        //Create a gameobject call it row_i
        GameObject rowobject = new GameObject("row_" + index);

        //The row has a box collision the size of the all the blocks
        if (addcollisions)
        {
            BoxCollider collider = rowobject.AddComponent<BoxCollider>();
            collider.size = new Vector3(collisionsize, blocksize, blocksize);

            //Center box collider
            int xmove = (collisionsize - blocksize) / 2;
            Vector3 newposition = new Vector3(xmove, 0, 0);
            collider.center = newposition;
        }

        //Set the parent 
        rowobject.transform.parent = transform;
        rowobject.layer = transform.gameObject.layer;

        //Alter position
		if(parentObject) //Aslong as we have the parent object, we build the rows according to the layer
		{
			GridHandler grid = parentObject.GetComponent<GridHandler>();
			
			if(grid)
			{
				switch(grid.creationDirection)
				{
					
					case GridHandler.CreationDirection.UP:
					{
						//Move the row upwards
						alteredposition = new Vector3(rowposition.x, rowposition.y, rowposition.z + move);
					}
					break;
					case GridHandler.CreationDirection.DOWN:
					{
						//Move the row downwards
						alteredposition = new Vector3(rowposition.x, rowposition.y, rowposition.z - move);
					}
					break;
					case GridHandler.CreationDirection.FORWARD:
					{
						//Move the row forwards
						alteredposition = new Vector3(rowposition.x, rowposition.y + move, rowposition.z);
					}
					break;
					case GridHandler.CreationDirection.BACKWARD:
					{
						//Move the row backwards
						alteredposition = new Vector3(rowposition.x, rowposition.y - move, rowposition.z);
					}
					break;
					case GridHandler.CreationDirection.LEFT:
					{
						move *= grid.maxlayers; //multiply the move offset by the length of the row
						//Move the row left
						alteredposition = new Vector3(rowposition.x - move, rowposition.y, rowposition.z);
					}
					break;
					case GridHandler.CreationDirection.RIGHT:
					{
						move *= grid.maxlayers; //multiply the move offset by the length of the row
						//Move the row right
						alteredposition = new Vector3(rowposition.x + move, rowposition.y, rowposition.z);
					}
					break;
				}
			}
			
		}
		
        //int ymove = blocksize * index;
        //Vector3 alteredposition = new Vector3(rowposition.x, rowposition.y + ymove, rowposition.z);
		Debug.Log ("Position: " + alteredposition.ToString());
        //Work out the position to put the object
        rowobject.transform.position = alteredposition;

        //--Set the tag---------------------------------------------//
        rowobject.tag = "row";

        //After creating a row need to create the corresponding columns.
        for (int blockindex = 0; blockindex < numberofcolumns; blockindex++)
        {
            //Create the column game objects which will either hold the block tile, or be the collision for mouse interaction
            CreateColumn(blockindex, rowobject, addcollisions);
        }

    }

    public void CreateRows(int rowamount, Vector3 rowposition, bool addcollisions = true)
    {
        Debug.Log("Creating " + rowamount + " rows.");

        //Create a row to handle the collection of blocks - which is another gameobject
        for (int i = 0; i < rowamount; i++)
        {
            CreateRow(i, rowposition, addcollisions);
        }
    }

    public void CreateColumn(int index,GameObject rowobject, bool addcollisions = true)
    {
        //Create the column game objects which will either hold the block tile, or be the collision for mouse interaction
        GameObject colobject = new GameObject("col_" + index);

        //Set the position of the object
        colobject.transform.parent = rowobject.transform;
        colobject.layer = rowobject.layer;

        //Alter the position along the x axis
        int col_xmove = blocksize * index; //how far to shift the position to the left
        Vector3 col_newposition = new Vector3(rowobject.transform.position.x + col_xmove, rowobject.transform.position.y, rowobject.transform.position.z);

        colobject.transform.position = col_newposition; //Set our position along the x axis according to which block we are creating

        //--Add a collider set size and center ---------------------------//
        if (addcollisions)
        {
            BoxCollider col_collider = colobject.AddComponent<BoxCollider>();
            col_collider.size = new Vector3(blocksize, blocksize, blocksize);

            //----------------------------------------------------------------//

            //--Set the center -------------------------------------------------//
            int center_x = blocksize / 2;
            Vector3 centerposition = new Vector3(0, 0, 0);
            col_collider.center = centerposition;

        }
        //----------------------------------------------------------------//

        //--Set the tag---------------------------------------------//
        colobject.tag = "column";

        //----------------------------------------------------------//
    }

    public void UpdateRows()
    {
        //Temporary script to update the rows in the layer which also takes control of deleting the extra blocks
        /*
         * This script goes through all the children and makes sure they are in the right position and the 
         * the right collision size
         * */
        for (int i = 0; i < transform.childCount; i++)
        {
            //We also check if we have changed the amount of rows we want
            int collisionsize = blocksize * numberofcolumns;
            GameObject rowobject = transform.GetChild(i).gameObject;

            //Update the name of the object
            rowobject.name = "row_" + i;

            int ymove = blocksize * i;
            Vector3 rowposition = transform.position;
			
			int move = blocksize * i;
			Vector3 alteredposition = Vector3.zero;
			
	        //Alter position
			if(parentObject) //Aslong as we have the parent object, we build the rows according to the layer
			{
				GridHandler grid = parentObject.GetComponent<GridHandler>();
				
				if(grid)
				{
					switch(grid.creationDirection)
					{
						
						case GridHandler.CreationDirection.UP:
						{
							//Move the row upwards
							alteredposition = new Vector3(rowposition.x, rowposition.y, rowposition.z + move);
						}
						break;
						case GridHandler.CreationDirection.DOWN:
						{
							//Move the row downwards
							alteredposition = new Vector3(rowposition.x, rowposition.y, rowposition.z - move);
						}
						break;
						case GridHandler.CreationDirection.FORWARD:
						{
							//Move the row forwards
							alteredposition = new Vector3(rowposition.x, rowposition.y + move, rowposition.z);
						}
						break;
						case GridHandler.CreationDirection.BACKWARD:
						{
							//Move the row backwards
							alteredposition = new Vector3(rowposition.x, rowposition.y - move, rowposition.z);
						}
						break;
						case GridHandler.CreationDirection.LEFT:
						{
							move *= grid.maxlayers; //multiply the move offset by the length of the row
							//Move the row left
							alteredposition = new Vector3(rowposition.x - move, rowposition.y, rowposition.z);
						}
						break;
						case GridHandler.CreationDirection.RIGHT:
						{
							move *= grid.maxlayers; //multiply the move offset by the length of the row
							//Move the row right
							alteredposition = new Vector3(rowposition.x + move, rowposition.y, rowposition.z);
						}
						break;
					}
				}
				
			}
			
            //Vector3 alteredposition = new Vector3(rowposition.x, rowposition.y + ymove, rowposition.z);

            //Work out the position to put the object
            rowobject.transform.position = alteredposition;


            Vector3 size = new Vector3(collisionsize, blocksize, blocksize);

            BoxCollider collider = rowobject.GetComponent<BoxCollider>();
            if (collider)
            {
                collider.size = size;

                int xmove = (collisionsize - blocksize) / 2;
                Vector3 newposition = new Vector3(xmove, 0, 0);
                collider.center = newposition;
            }

            //Update the columns aswell
            UpdateColumns(rowobject.transform);
            
        }
    }

    public void UpdateColumns(Transform rowobject)
    {
        //Temporary script which updates the columns in the rows - Make sure we have the correct amount
        /*
         * First we check to see if the number of children we have is greater than the amount of columns
         * we need and delete accordingly.
         * Filter through the columns for every row, and make sure we are in the right position, and also
         * have the correct collision size.
         * 
         * */
        int childrencount = rowobject.childCount;

        if (childrencount > numberofcolumns)
        {
            //Delete the extra columns
            for (int i = numberofcolumns; i < childrencount; i++)
            {
                GameObject col_object = rowobject.GetChild(numberofcolumns).gameObject;

                //Destroy the object
                DestroyImmediate(col_object);
            }
        }
        else
        {
            for (int i = 0; i < numberofcolumns; i++)
            {
                //We made need to create new columns depending on if our number of columns
                //is greater than the children we have

                if (i < childrencount)
                {
                    //Let's filter through and update them accordingly
                    GameObject colobject = rowobject.GetChild(i).gameObject;

                    //Name of the column
                    colobject.name = "col_" + i;

                    //Update the position, and also the collision size,center 
                    //Set the position of the object
                    colobject.transform.parent = rowobject.transform;

                    //Alter the position along the x axis
                    int col_xmove = blocksize * i; //how far to shift the position to the left
                    Vector3 col_newposition = new Vector3(rowobject.transform.position.x + col_xmove, rowobject.transform.position.y, rowobject.transform.position.z);

                    colobject.transform.position = col_newposition; //Set our position along the x axis according to which block we are creating

                    //Add a collider set size and center
                    BoxCollider col_collider = colobject.GetComponent<BoxCollider>();
                    if (col_collider)
                    {
                        col_collider.size = new Vector3(blocksize, blocksize, blocksize);

                        //Set the center
                        int center_x = blocksize / 2;
                        Vector3 centerposition = new Vector3(0, 0, 0);
                        col_collider.center = centerposition;
                    }
                }
                else
                {
                    //We need to create new columns
                    CreateColumn(i, rowobject.gameObject);
                }

            }
        }
    }
	
	public Vector3 ReturnPositionAccordingToDirection(Vector3 rowposition, int index)
	{
		//Depending on which way the grid is being built, we return the correct positioning
		//according to the direction we built the grid in.
		Vector3 alteredposition = Vector3.zero;
		int move = blocksize * index;
		
		if(parentObject)
		{
			GridHandler grid = parentObject.GetComponent<GridHandler>();
			
			if(grid)
			{
				//Work out the directions
				switch(grid.creationDirection)
				{
					
					case GridHandler.CreationDirection.UP:
					{
						//Move the row upwards
						alteredposition = new Vector3(rowposition.x, rowposition.y, rowposition.z + move);
					}
					break;
					case GridHandler.CreationDirection.DOWN:
					{
						//Move the row downwards
						alteredposition = new Vector3(rowposition.x, rowposition.y, rowposition.z - move);
					}
					break;
					case GridHandler.CreationDirection.FORWARD:
					{
						//Move the row forwards
						alteredposition = new Vector3(rowposition.x, rowposition.y + move, rowposition.z);
					}
					break;
					case GridHandler.CreationDirection.BACKWARD:
					{
						//Move the row backwards
						alteredposition = new Vector3(rowposition.x, rowposition.y - move, rowposition.z);
					}
					break;
					case GridHandler.CreationDirection.LEFT:
					{
						move *= grid.maxlayers; //multiply the move offset by the length of the row
						//Move the row left
						alteredposition = new Vector3(rowposition.x - move, rowposition.y, rowposition.z);
					}
					break;
					case GridHandler.CreationDirection.RIGHT:
					{
						move *= grid.maxlayers; //multiply the move offset by the length of the row
						//Move the row right
						alteredposition = new Vector3(rowposition.x + move, rowposition.y, rowposition.z);
					}
					break;
				}
			}
		}
		
		return alteredposition;
	}

    public int FindChildIndex(string name)
    {
        int index = -1; //No child with that name found

        //Cycle through the children and once we found the one we are looking for
        //return the index
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            if (child.name == name)
            {
                index = i;
                return index;
            }
        }

        return index;
    }

    public int FindChildIndexOfRow(string name, Transform rowobject)
    {
        int index = -1;

        for (int i = 0; i < rowobject.childCount; i++)
        {
            //Find the column we are looking for and return the count
            Transform child = rowobject.GetChild(i);

            if (child.name == name)
            {
                index = i;
                return index;
            }
        }

        return index;
    }
    #endregion
}
