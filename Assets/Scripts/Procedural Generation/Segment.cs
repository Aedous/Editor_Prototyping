using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


[ExecuteInEditMode]
public class Segment : MonoBehaviour
{
    //This script is a segment that holds the starting point of it's rectangle, and end point.
    //The segment works together with the ProceduralGeneration_Core to organize and build blocks.

    #region Structures
    [Serializable]
    public struct RoomPosition
    {
        public Vector2 startPoint;
        public Vector2 endPoint;
    }
    #endregion

    #region inspector variables
    public RoomPosition segment_position;
    public int segment_index; //Used to for organizing the positioning of segments according to z axis
    public Vector2 startPoint, roomStartPoint; //References to our start point and the room we create start point
    public Vector2 endPoint, roomEndPoint; //Reference to our end point and the room we create end point
    #endregion

    #region public variables
    public ProceduralGeneration_Core procedural_reference { get; set; } //Reference to the procedural core script
    public Vector3 boxguide { get; set; }
    public Vector3 paddingguide { get; set; }
    public GameObject LevelPart { get; set; } //Holds a reference to the part we are going to build
    public Vector3 LevelPartPosition { get; set; } //Holds a reference to the game object
    public ObjectHandler LevelPartScript { get; set; } //Holds a reference to the script
    public bool levelPartCreated { get; set; } //Reference for checking if the segment has been built
	public bool hasRoom; //Reference for only showing the segments which have rooms
    public int width, height; //Height and width of the segment
    #endregion

    #region private variables
    
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

    #endregion

    #region Class Methods
    public Transform GetChildSegment(int index)
    {
        //Get the child game object and cut it open
        Transform childobject = transform.GetChild(index);
        return childobject;
    }

    public void CreateBlockGuideRect()
    {
        //Create a guide to show where the blocks will be created, to save processing time.

        //Check if our grid script is also valid, so we can collect the tile object to use to build
        GridHandler grid_reference = procedural_reference.grid_script;

        if (grid_reference) //We have a reference to our grid object
        {
            //Aslong as we have a script to our procedural core we can find out how much space we have to work
            if (width > 1)
            {
                //We can atleast build along the width
                if (height > 1)
                {
                    //we have space to work with the height
                }
                else
                {
                    //Our height and width is one block, so we can create a block at that position
                    boxguide = CreateBlockGuide(0, 0);
                }
            }
            else if (height > 1) //lets check our height to see if we can build any wide rooms
            {
                if (width > 1)
                {
                    //we have space to work with the width
                }
                else
                {
                    //Our height and width is one block, so we can create a block at that position
                    boxguide = CreateBlockGuide(0, 0);
                    
                }
            }
            else
            {
                //Our height and width is one block, so we can create a block at that position
                boxguide = CreateBlockGuide(0, 0);
            }
        }
    }

    public void CreateRoom()
    {
        //We will select which part fits in that area and create it
        /*levelPartCreated = false;
        LevelPart = (GameObject)procedural_reference.levelparts[0];
        LevelPartScript = LevelPart.GetComponent<ObjectHandler>();

        //This script creates a room in the space it has
        if (procedural_reference != null)
        {

            int hutwidth = LevelPartScript.Width;
            int hutdepth = LevelPartScript.Depth;
            

            //Check if our grid script is also valid, so we can collect the tile object to use to build
            GridHandler grid_reference = procedural_reference.grid_script;

            //if (grid_reference) //We have a reference to our grid object
            {
                //Aslong as we have a script to our procedural core we can find out how much space we have to work
                if (width >= LevelPartScript.Width)
                {
                    //We can atleast build along the width
                    if (height >= LevelPartScript.Depth)
                    {
                        //we have space to work with the height
                        //CreateBlock(hut, 0, 0);
                        //Record segment
                        procedural_reference.segmentswithrooms.Add(this);
                    }
                    else
                    {
                        //Our height and width is one block, so we can create a block at that position
                        //if (grid_reference.tile)
                        {
                            //Debug.Log("Creating tile not hut");
                            //CreateBlock(grid_reference.tile, 0, 0);
                        }
                    }
                }
                else if (height >= LevelPartScript.Height) //lets check our height to see if we can build any wide rooms
                {
                    if (width >= LevelPartScript.Width)
                    {
                        //we have space to work with the width
                        procedural_reference.segmentswithrooms.Add(this);
                    }
                    else
                    {
                        //Our height and width is one block, so we can create a block at that position
                        //if (grid_reference.tile)
                        {
                           // Debug.Log("Creating tile not hut");
                            //CreateBlock(grid_reference.tile, 0, 0);
                        }
                    }
                }
                else
                {
                    //Our height and width is one block, so we can create a block at that position
                   // if (grid_reference.tile)
                    {
                        //Debug.Log("Creating tile not hut");
                        //CreateBlock(grid_reference.tile, 0, 0);
                    }
                }
            }
            /*else
            {
                Debug.Log("No grid reference");
            }6
        }*/

    }

    public Vector3 CreateBlockGuide(int row, int col)
    {
        //This creates an array of points for the handles to use to draw its guide

        //we use the row and col to figure out where to build the block, we do this by adding the row or col to the startpoint
        //and build around that
        Vector2 blockpoints = CalculateBlockPoint(row, col);
		
        //Once we have our position, we can create a block at that position by adding it to the transform position
		Vector3[] blockposition = procedural_reference.CalculatePoints(startPoint, endPoint, 0, procedural_reference.creationDirection);
        //Vector3 blockposition = CalculatePosition(blockpoints, procedural_reference.blocksize, procedural_reference.blocksize, procedural_reference.creationDirection); //Create above
		
		//Vector3 middle = new Vector3( blockposition[0].x + (width / 2) , 0 , blockposition[0].y + (height / 2));
        //Once we have the positon we can then pass it back for the handles to use to draw
		//Debug.Log ("Creating room guide at " + blockposition[0].ToString());
		
		//Work out the middle from the 4 points passed from the CalculatePoints method
		//0 - start point Bottom Left
		//1 - Top Left
		//2 - Top Right
		//3 - Bottom Right
		//Middle X is Top Left / 2 
		//Middle Y is Bottom Right / 2
		float shiftx = (width * procedural_reference.blocksize) / 2;
		float shifty = (height * procedural_reference.blocksize) / 2;
		Vector3 middle = new Vector3(blockposition[0].x + shiftx, blockposition[0].y, blockposition[0].z + shifty);
		//hasRoom = true;
		return middle;
    }

    

    public GameObject CreateBlock(GameObject block,int row, int col)
    {
        //Create a block at the position passed
        //we use the row and col to figure out where to build the block, we do this by adding the row or col to the startpoint
        //and build around that
        Vector2 blockpoints = CalculateBlockPoint(row, col);

        //Once we have our position, we can create a block at that position by adding it to the transform position
        Vector3 blockposition = CalculatePosition(blockpoints, procedural_reference.blocksize, procedural_reference.blocksize, procedural_reference.creationDirection); //Create above

        //Create the block
        GameObject blockgameobject = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(block);

        //Set the properties for this created block
        blockgameobject.transform.position = CalculatePadding(blockposition, procedural_reference.creationDirection);
        blockgameobject.transform.parent = procedural_reference.levelPartsManagerTransform;
        

        //Rotate the object randomly in 90 to 180 degrees
        int dice = UnityEngine.Random.Range(1, 5);
        ObjectHandler script = blockgameobject.GetComponent<ObjectHandler>();
        Vector3 pivotpoint = script.center;
        switch(dice)
        {
            case 1:
                {
                    blockgameobject.transform.RotateAround(pivotpoint,Vector3.up, 90f);
                }
                break; 
            case 2:
                {
                    blockgameobject.transform.RotateAround(pivotpoint, Vector3.up, -90f);
                }
                break;
            case 3:
                {
                    blockgameobject.transform.RotateAround(pivotpoint, Vector3.up, 180f);
                }
                break;
            case 4:
                {
                    blockgameobject.transform.RotateAround(pivotpoint, Vector3.up, -180f);
                }
                break;
            default:
                {
                }
                break;
        }

        LevelPartPosition = blockgameobject.transform.position;
        return blockgameobject;
    }

    public Vector3 CalculatePadding(Vector3 position, ProceduralGeneration_Core.CreationDirection direction)
    {
        //This function will apply a padding to the object to center it
        //Calculate the
        Vector3 paddingposition = Vector3.zero;
        int widthoffset = width / 2;
        int heightoffset = height / 2;
        int blockwidthoffset = LevelPartScript.Width / 2;
        int blockheightoffset = LevelPartScript.Depth / 2;

        //Calculate the width and height center
        int widthcenter = (int)(widthoffset - blockwidthoffset);
        int heightcenter = (int)(heightoffset - blockheightoffset);

        //Then apply to the blocks position depending on the direction
        switch (direction)
        {
            case ProceduralGeneration_Core.CreationDirection.FLAT:
                {
                    //Apply the height padding to the z axis
                    paddingposition.x = (float)widthcenter;
                    paddingposition.z = (float)heightcenter;
                }
                break;
            case ProceduralGeneration_Core.CreationDirection.UPRIGHT:
                {
                    //Apply the height padding to the y axis
                    paddingposition.x = (float)widthcenter;
                    paddingposition.y = (float)heightcenter;
                }
                break;
        }

        //To have the correct sizing multiply by the block size
        paddingguide = paddingposition;

        Debug.Log("Padding Guide : " + paddingguide);
        paddingposition *= procedural_reference.blocksize;
        //Alter position
        position += paddingposition;

        return position;

    }

    public Vector3 CalculatePosition(Vector2 blockposition,int blocksize, int depth, ProceduralGeneration_Core.CreationDirection direction = ProceduralGeneration_Core.CreationDirection.UPRIGHT) //Depth is the z axis, to calculate the depth of the object
    {
        //This takes the row and col position, and returns the actual position to create the block according
        //to the gameobject
        Vector3 blockGameObjectPosition = new Vector3();

        //The way to work out the actual position, is to add it to our local position
        Vector3 parentposition = procedural_reference.transform.localPosition; //get the position of the procedural core

        //Calculate the position depending on the direction we are creating
        switch(direction)
        {
            case ProceduralGeneration_Core.CreationDirection.UPRIGHT:
                {
                    parentposition = new Vector3(parentposition.x * blocksize, parentposition.y * blocksize, parentposition.z * blocksize);

                    //Set start point and end point according to our position
                    blockGameObjectPosition = new Vector3(blockposition.x + parentposition.x, blockposition.y + parentposition.y, parentposition.z + depth);
                }
                break;
            case ProceduralGeneration_Core.CreationDirection.FLAT:
                {
                    parentposition = new Vector3(parentposition.x * blocksize, parentposition.y * blocksize, parentposition.z * blocksize);

                    //Set start point and end point according to our position
                    //blockGameObjectPosition = new Vector3(blockposition.x + parentposition.x, parentposition.y + depth, blockposition.y + parentposition.z);
					blockGameObjectPosition = new Vector3(blockposition.x + parentposition.x, 0f, blockposition.y + parentposition.z);
                }
                break;

        }
        return blockGameObjectPosition;
    }

    public Vector2 CalculateBlockPoint(int row, int col)
    {
		//Currently not referencing the row and col *investigate into this!!*
        //New position is calculate by taking the row and col and adding it to the startpoint row and col.
        Vector2 blockposition = new Vector2();

        //Multiply by the blocksize, to get the correct positioning
        blockposition.x = Mathf.Abs(row + startPoint.x) * procedural_reference.blocksize;
        blockposition.y = Mathf.Abs(col + startPoint.y) * procedural_reference.blocksize;

        return blockposition;
    }

    public void CalculateWidth(int startpointx,int endpointx)
    {
        //Calculate the difference between the two points which will return the width
        width = Mathf.Abs(startpointx - endpointx);
    }

    public void CalculateHeight(int startpointy, int endpointy)
    {
        //Calculate the difference between the two points which will return the height
        height = Mathf.Abs(startpointy - endpointy);
    }
	
	public void PositionSegmentTransform()
	{
		//This positions the segment transform at the 0,0 of it's segment guide
		Vector3[] blockposition = procedural_reference.CalculatePoints(startPoint, endPoint, 0, procedural_reference.creationDirection);
		
		transform.position = blockposition[0];
		
	}

    public void CloneSegment(ref Segment segment)
    {
        //This function will set the variables for a particular segment to match it's
        //own
        //Set the start point and end point
        segment.procedural_reference = this.procedural_reference;
        segment.segment_position.startPoint = startPoint;
        segment.segment_position.endPoint = endPoint;
        segment.startPoint = startPoint;
        segment.endPoint = endPoint;

        //Work out width and height and set the segment variables
        segment.CalculateWidth((int)startPoint.x, (int)endPoint.x);
        segment.CalculateHeight((int)startPoint.y, (int)endPoint.y);
        segment.segment_index = segment_index;
        segment.levelPartCreated = false;
        segment.PositionSegmentTransform();
    }
    #endregion
}
