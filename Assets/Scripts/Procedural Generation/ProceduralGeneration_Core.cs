using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProceduralGeneration_Core : MonoBehaviour
{
    //This script first creates a grid with one layer to build the floor.
    //once the layer has been created we split into into segments (gameobjects)
    //these hold the locations of the segments on the grid.
    #region Enum/Structures
    public enum CreationDirection
    {
        FLAT,
        UPRIGHT
    }
    #endregion

    #region inspector variables
    public int maximumrows, maximumcolumns, blocksize; //Used to create the 1 layer grid
    public int numberOfSegments; //number of segments we have
    public int numberOfCuts; //Number of cut's to make
    public int range; //The range to decide on how we cut the segment.
    #endregion

    #region public variables
    public bool showGrid { get; set; } //showing grid for editor
    public bool completedCreatingRooms { get; set; } //boolean to check when rooms have being recorded
    public CreationDirection creationDirection;
    //Grab the blocks in the folder level parts and create those that will fit in a space
    public List<UnityEngine.Object> levelparts { get; set; }
    public List<Segment> segmentswithrooms { get; set; } //Holds a list of all the segments that will have a room
    public GameObject segmentManagerObject { get; set; } //reference to our segment manager
    public GameObject levelPartsManager { get; set; } //Holds all the level parts
    public Transform levelPartsManagerTransform { get; set; } //The transform to our level part manager
    public GridHandler grid_script { get; set; }//A reference to the grid handler
    public bool addcollisions { get; set; }
    public int CutDirection { get; set; } //Our cutting direction
    public int DifferenceAllowance { get; set; }
    public Object[] blockcollection { get; set; } //Used to show what kind of blocks we can use
    #endregion

    #region private variables
    string segmentmanagername = "Segment Manager";
    string levelmanagername = "Level Part Manager";
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

    void OnEnable()
    {
         //Check if a level part manager actually exists
        if (transform.childCount > 0)
        {
            foreach (Transform child in transform)
            {
                //Check the names of the objects and update accordingly
                if (child.name == levelmanagername)
                {
                    levelPartsManager = child.gameObject;
                    levelPartsManagerTransform = child;
                }
                else if(child.name == segmentmanagername)
                {
                    segmentManagerObject = child.gameObject;
                }
            }
        }

        //We then check to make sure they are not still equal to null and create accordingly
        if (levelPartsManager == null)
        {
            levelPartsManager = new GameObject("Level Part Manager");
            levelPartsManager.transform.parent = transform;
            levelPartsManagerTransform = levelPartsManager.transform;
        }

        if (segmentManagerObject == null)
        {
            segmentManagerObject = new GameObject("Segment Manager");
            segmentManagerObject.transform.parent = transform;
        }

    }

    #endregion

    #region Class Methods
    public void StartCoroutineTest(float time)
    {
        StartCoroutine(waitAndPrint("Testing coroutine!!", time));
    }

    public int CheckDirectionToCutIsValid(int other_start, int other_end,ref int cutpoint,ref int direction)
    {
        //Check to see if our cutpoint is valid, if it is not valid, we check the other direction to cut
        //and return whether or not we can make a cut.
        if (cutpoint > 0)
        {
            CutDirection = direction;
            return direction;
        }
        else
        {
            //We need to then check if a new cutpoint is valid in the other direction
            //if is not we pass back false
            int newcutpoint = DecideCutPoint(other_start, other_end);

            //Check new cutpoint is valid
            if (newcutpoint > 0)
            {
                if (direction == 0)
                {
                    direction = 1; //Invert
                }
                else direction = 0; //Invert

                cutpoint = newcutpoint;

                CutDirection = direction;
                return direction;
            }
        }

        //Pass back invalid directions to stop cutting
        direction = -1;
        CutDirection = direction;

        return direction; //Cannot make a cut

    }
    
    public void Cut(int direction, int cutpoint, Vector2 startPoint, Vector2 endPoint, int layerposition, Transform parent, bool createRoom = false)
    {
        if (direction == 0)
        {
            for (int i = 0; i < 2; i++)
            {
                int index = i + 1;
                if (i == 0)
                {
                    //First cut
                    CreateSegment(startPoint, new Vector2(endPoint.x, cutpoint), "S" + index, layerposition, parent);
                }
                else if (i == 1)
                {
                    //Second cut
                    CreateSegment(new Vector2(startPoint.x, cutpoint + 1), new Vector2(endPoint.x, endPoint.y), "S" + index, layerposition, parent);
                }
            }
        }
        else if (direction == 1)
        {
            for (int i = 0; i < 2; i++)
            {
                int index = i + 1;
                if (i == 0)
                {
                    //First cut -----------------------------------------------------------------//
                    CreateSegment(startPoint, new Vector2(cutpoint, endPoint.y), "S" + index, layerposition, parent);
                }
                else if (i == 1)
                {
                    //Second cut --------------------------------------------------------------------------------------------------//
                    CreateSegment(new Vector2(cutpoint + 1, startPoint.y), new Vector2(endPoint.x, endPoint.y), "S" + index, layerposition, parent);
                }
            }
        }
    }

    public void MakeCut(Vector2 startPoint, Vector2 endPoint, int direction, int layerposition, Transform parent = null, bool createRoom = false)
    {
        //Before we make a cut around horizontally or vertically we decide we can actually make those cuts
        //if not we swap over to the next direction and we can not make another cut we skip cutting the segment
        //but make the another segment with the same structure as it's parent

        int cutpoint = -1;

        if (direction == 0)
        {
            //Cut vertically and then swap over to horizontally if direction is invalid
            cutpoint = DecideCutPoint((int)startPoint.y, (int)endPoint.y);

            //Check if the direction to cut is valid, and pass in other other start point and end point
            //to check if the other direction is valid
            CheckDirectionToCutIsValid((int)startPoint.x, (int)endPoint.x, ref cutpoint, ref direction);
            if (direction != -1)
            {
                //We make a cut, however there may be a chance that the direction has been inverted,
                //we cater towards that just in case
                Cut(direction, cutpoint, startPoint, endPoint, layerposition, parent, createRoom);
            }
            else
            {
                //Otherwise we make the same segments twice
                for (int i = 0; i < 2; i++)
                {
                    //Second cut --------------------------------------------------------------------------------------------------//
                    CreateSegment(startPoint, endPoint, "S" + i, layerposition, parent, createRoom);
                }
            }
        }
        else if (direction == 1)
        {
            //Cut horizontally and then make sure to check the a vertical cut
            cutpoint = DecideCutPoint((int)startPoint.x, (int)endPoint.x);

            //Check if the direction to cut is valid, and pass in other other start point and end point
            //to check if the other direction is valid
            CheckDirectionToCutIsValid((int)startPoint.y, (int)endPoint.y, ref cutpoint, ref direction);

            if (direction != -1)
            {
                //We make a cut, however there may be a chance that the direction has been inverted,
                //we cater towards that just in case
                Cut(direction, cutpoint, startPoint, endPoint, layerposition, parent);
            }
            else
            {
                //Otherwise we make the same segments twice
                for (int i = 0; i < 2; i++)
                {
                    //Second cut --------------------------------------------------------------------------------------------------//
                    CreateSegment(startPoint, endPoint, "S" + i, layerposition, parent, createRoom);
                }
            }
        }

        
    }

    public Vector3[] CalculatePoints(Vector2 startPoint, Vector2 endPoint, int shift = 0, CreationDirection direction = CreationDirection.UPRIGHT)
    {
        //Create points to create square mesh
        Vector3[] points = new Vector3[4];

        Vector3 parentposition = transform.localPosition; //get the position of this game object
        //Multiply our start point and end point by the block size to get an accurate size
        startPoint *= blocksize; 
        endPoint *= blocksize;

        //Set start point and end point according to our position
        switch(direction)
        {
            case CreationDirection.FLAT:
                {
                    //Going to be drawn flat so we shift it along the
                    startPoint = new Vector3(startPoint.x + parentposition.x, startPoint.y + parentposition.y, parentposition.z);
                    endPoint = new Vector3(endPoint.x + parentposition.x, endPoint.y + parentposition.y, parentposition.z);
                    //float padding = blocksize / 2; //Padding to position correctly
					float padding = 0;


                    /*points[0] = new Vector3(startPoint.x - padding, startPoint.y - padding, parentposition.z + shift);
                    points[1] = new Vector3(startPoint.x - padding, endPoint.y + padding, parentposition.z + shift);
                    points[2] = new Vector3(endPoint.x + padding, endPoint.y + padding, parentposition.z + shift);
                    points[3] = new Vector3(endPoint.x + padding, startPoint.y - padding, parentposition.z + shift);*/

                    points[0] = new Vector3(startPoint.x - padding, parentposition.y + shift, startPoint.y - padding);
                    points[1] = new Vector3(startPoint.x - padding, parentposition.y + shift, endPoint.y + padding);
                    points[2] = new Vector3(endPoint.x + padding,parentposition.y + shift, endPoint.y + padding);
                    points[3] = new Vector3(endPoint.x + padding, parentposition.y + shift, startPoint.y - padding);
                }
                break;
            case CreationDirection.UPRIGHT:
                {
                    startPoint = new Vector3(startPoint.x + parentposition.x, startPoint.y + parentposition.y, parentposition.z);
                    endPoint = new Vector3(endPoint.x + parentposition.x, endPoint.y + parentposition.y, parentposition.z);
                    //float padding = blocksize / 2; //Padding to position correctly
					float padding = 0;

                    points[0] = new Vector3(startPoint.x - padding, startPoint.y - padding, parentposition.z + shift);
                    points[1] = new Vector3(startPoint.x - padding, endPoint.y + padding, parentposition.z + shift);
                    points[2] = new Vector3(endPoint.x + padding, endPoint.y + padding, parentposition.z + shift);
                    points[3] = new Vector3(endPoint.x + padding, startPoint.y - padding, parentposition.z + shift);
                }
                break;
        }
        
        
        return points;
    }


    public void CreateGrid(int row_length, int col_length, int blocksize)
    {
        if (grid_script) //Aslong as we have a grid script
        {
            //Create the grid with the grid script
            grid_script.CreateGrid(row_length, col_length, blocksize, 1, blocksize, addcollisions);
        }
        else
        {
            //Need to create a gameobject with the grid script inside of it, and then get
            //a reference to it.
            CreateGridObject("GridManager");
        }
    }

    public void CreateGridObject(string name)
    {
        //Check to see if the grid manager exists
        if (transform.childCount > 0)
        {
            //Find the grid manager
            for(int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i); //Get child

                if (child)
                {
                    grid_script = transform.GetChild(i).GetComponent<GridHandler>();

                    if (grid_script)
                    {
                        grid_script.CreateGrid(maximumrows, maximumcolumns, blocksize, 0, blocksize, addcollisions);
                    }
                }
            }

            //If there is still no grid script we create one ourselves
            if (!grid_script)
            {
                GameObject grid_object = new GameObject(name);
                grid_script = grid_object.AddComponent<GridHandler>();
                grid_script.CreateGrid(maximumrows, maximumcolumns, blocksize, 0, blocksize, addcollisions);
            }
        }
        else
        {
            //Create the game object for the grid
            GameObject grid = new GameObject(name);

            grid.transform.parent = transform;
            grid.transform.position = Vector3.zero;
            grid.transform.localPosition = Vector3.zero;

            //After creating the layer attach a script component to it
            grid.AddComponent<GridHandler>();
            GridHandler script = grid.GetComponent<GridHandler>();

            grid_script = script;

            //Create Grid------------------------------------//
            script.CreateGrid(maximumrows, maximumcolumns, blocksize, 0, blocksize, addcollisions);
        }
    }

    public void UpdateGridReference()
    {
        //This makes sure we always have a reference available
        if (!grid_script)
        {
            if (transform.childCount > 0)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform child = transform.GetChild(i);

                    if (child.name == "GridManager")
                    {
                        grid_script = child.GetComponent<GridHandler>();
                    }
                }
            }
        }
    }

    public void CreateSegment(Vector2 startPoint, Vector2 endPoint, string name,int layerposition, Transform parent = null, bool createRoom = false)
    {
        GameObject segment = new GameObject(name);

        //Attach the script and reference it
        Segment script = segment.AddComponent<Segment>();

        //Once we have created the segment, add it to our segment Manager
        if (segmentManagerObject) //Aslong as it exists
        {
            //Delete the children if it already exists
            //Make the segment manager object the parent
            if (parent == null)
            {
                segment.transform.parent = segmentManagerObject.transform;
            }
            else
                segment.transform.parent = parent;
			
            //Set the start point and end point
            script.procedural_reference = this;
            script.segment_position.startPoint = startPoint;
            script.segment_position.endPoint = endPoint;
            script.startPoint = startPoint;
            script.endPoint = endPoint;

            //Work out width and height and set the segment variables
            script.CalculateWidth((int)startPoint.x, (int)endPoint.x);
            script.CalculateHeight((int)startPoint.y, (int)endPoint.y);
            script.segment_index = layerposition;
            script.levelPartCreated = false;
			script.PositionSegmentTransform();
        }
    }

    public Vector3 CreateRoomGuide(Segment script)
    {
        if (script)
        {
            return script.CreateBlockGuide(0, 0);
        }
        else return Vector3.zero;
    }

    public List<Vector3> CreateRoomGuides()
    {
        //Collects a set of room guides to filter through and draw
        List<Vector3> listofroomguides = new List<Vector3>();

        if (segmentManagerObject)
        {
            Transform segmentA = segmentManagerObject.transform.GetChild(0);
            Transform segmentB = segmentManagerObject.transform.GetChild(1);
            List<Transform> lastgenerationofchildren = ReturnListOfLastChildren(segmentA, segmentB);

            //Create the guides in the last generation of children
            int childcount = lastgenerationofchildren.Count;

            if (childcount > 0)
            {
                for (int i = 0; i < childcount; i++)
                {
                    Transform child = lastgenerationofchildren[i];

                    Segment script = child.GetComponent<Segment>();
                    if (script)
                    {
						if(script.width > 2 && script.height > 2)
                        //Add the guides that we create to pass back to the handles
                        listofroomguides.Add(script.CreateBlockGuide(0,0));
                    }
                }
            }

        }

        return listofroomguides;
    }

    public void CreateRoom(Segment script)
    {
        //Aslong as we have a script we create the block inside
        if (script)
        {
            script.CreateRoom(); // Doesn't do anything come back and fix this
        }
    }

    public void CreateRooms()
    {
        if (segmentswithrooms != null)
        {
            segmentswithrooms.Clear();
        }
        completedCreatingRooms = false;

        if (levelPartsManager == null)
        {
            //Create it
            levelPartsManager = new GameObject("Level Part Manager");
            levelPartsManager.transform.parent = transform;
            levelPartsManagerTransform = levelPartsManager.transform;
        }

        //This script will go into the last generation of children and build the rooms
        if (segmentManagerObject)
        {
            Transform segmentA = segmentManagerObject.transform.GetChild(0);
            Transform segmentB = segmentManagerObject.transform.GetChild(1);
            List<Transform> lastgenerationofchildren = ReturnListOfLastChildren(segmentA, segmentB);


            //Create the rooms in the last generation of children
            int childcount = lastgenerationofchildren.Count;

            if (childcount > 0)
            {
                for (int i = 0; i < childcount; i++)
                {
                    Transform child = lastgenerationofchildren[i];
                    Segment script = child.GetComponent<Segment>();

                    if (script.width > 2 && script.height > 2) //Aslong as there's space to make a room
                    {
                        //Check if the room already exists, if it does then remove it first
                        if (child.childCount > 0)
                        {
                            GameObject child_room = child.GetChild(0).gameObject;
                            if (child_room)
                                DestroyImmediate(child_room);
                        }

                        //Create a gameobject which will be the room, which has the room script
                        //attached to it
                        GameObject room = new GameObject("Room");
                        Room room_script = room.AddComponent<Room>();
                        //Attach this to the child segment
                        room.transform.parent = child.transform;

                        //Activate the room building for the child
                        //Doing a GIT HUB THING HERE 28/10/2013 16:36 - Done 17:28
                        if (room_script)
                        {
                            //We have a room script so run the create room process
                            room_script.InitRoom();
                            room_script.CreateRoom();
                        }


                        if (script)
                        {
                            script.hasRoom = true; //Each child will have a room attached to it if passed the width/height test
                        }
                    }

                    if (i == (childcount - 1))
                    {
                        //the last room was created
                        Debug.Log("Finished recording rooms");
                        completedCreatingRooms = true;
                    }
                }
            }

            
        }
        
    }

    public void ActivateRoomBuilding()
    {
        //After we have a list of rooms to create we can start building the map
        Debug.Log("Building Rooms");
        BuildRooms(10f, 0);
    }

    //This script creates the rooms after the rooms have all been recorded in the list
    public void BuildRooms(float waittime, int roomindex)
    {
        if (completedCreatingRooms)
        {
            if (segmentswithrooms.Count > 0)
            {
                //We have rooms to build
                if (roomindex < segmentswithrooms.Count)
                {
                    Debug.Log("Building room" + roomindex.ToString());
                    Segment segment = segmentswithrooms[roomindex];
                    GameObject block = null;
                    //Build the room
                    if (segment.LevelPart)
                    {
                        Debug.Log("Building level part in segment");
                        block = segment.CreateBlock(segment.LevelPart, 0, 0);

                    }
                    //else
                    {
                        //segment.CreateBlock((GameObject)levelparts[0], 0, 0);
                    }

                    if (block != null)
                    {
                        int next = roomindex + 1;
                        Debug.Log("Building next room");
                        roomindex++;
                        //Start this coroutine again
                        BuildRooms(waittime, roomindex);
                    }
                    else
                    {
                        int next = roomindex + 1;
                        StartCoroutine(waitAndPrint("Waiting seconds : " + waittime.ToString() +
                                                    "for room to finish building : " + next.ToString(),
                                                    waittime * 10
                                                    ));
                        segment.levelPartCreated = true;
                        //Try again
                        BuildRooms(waittime, roomindex);
                    }
                }
            }
        }
        else
        {
            Debug.Log("Rooms have not been completed, waiting 1 second and retrying.");
            Debug.Log("Retrying build room.");
            BuildRooms(waittime, roomindex);
        }
    }

    public IEnumerator waitAndPrint(string message, float time)
    {
        yield return new WaitForSeconds(time);
        Debug.Log(message);
    }

    public void CreateSegmentManager(int row_length, int col_length)
    {
        if (levelPartsManager == null)
        {
            //Create it
            levelPartsManager = new GameObject("Level Part Manager");
            levelPartsManager.transform.parent = transform;
            levelPartsManagerTransform = levelPartsManager.transform;
			Debug.Log("Resetting position of levelPartsManager");
			levelPartsManagerTransform.localPosition = Vector3.zero;
        }

        //First we find out if we already have a segment manager, and if we do, we delete it
        //and create a new one.
        //Cut the segment into half depending on which direction we are going

        if (segmentManagerObject == null)
        {
            segmentManagerObject = new GameObject(segmentmanagername);
        }
        else
        {
            //Clear the children
            while(segmentManagerObject.transform.childCount > 0)
            {
                DestroyImmediate(segmentManagerObject.transform.GetChild(0).gameObject);
            }
            //Destroy segment manager object
            DestroyImmediate(segmentManagerObject);

            //Create a new one
            segmentManagerObject = new GameObject(segmentmanagername);
        }

        segmentManagerObject.transform.parent = transform;
		Debug.Log("Resetting position of segmentManagerObject");
		segmentManagerObject.transform.localPosition = Vector3.zero;
		

        int decision = DecideHorizontalVertical(range);

        //Cut Vertically
        for (int i = 0; i < numberOfCuts; i++)
        {
            if (i == 0)
            {
                //Make our first cut (pass in proper indexes )
                MakeCut(new Vector2(0, 0), new Vector2(row_length - 1, col_length - 1), decision, i);
            }
            else
            {
                //Make a cut in a different manner because we are no longer on the second cut
                //On the second cut we go through the children in segment manager and make new cuts,
                //according to the size of the segment.
                if (segmentManagerObject)
                {
                    //We are going to go through the children and find out if theres segments to cut
                    int childcount = segmentManagerObject.transform.childCount;

                    if (childcount > 0)
                    {
                        //We have children to cut so we find out how many cuts we are going to make
                        Transform segment_1 = segmentManagerObject.transform.GetChild(0);
                        List<Transform> listofchildrentocut = FindSegmentsToCut(segment_1);

                        //We can going through the first segment to make cuts
                        if (listofchildrentocut.Count > 0)
                        {
                            //decision = DecideHorizontalVertical(range);

                            //Theres some transforms in there to cut
                            for (int seg = 0; seg < listofchildrentocut.Count; seg++)
                            {
                                //Get the transform segment
                                Transform segment_child = listofchildrentocut[seg];
                                Segment script = segment_child.GetComponent<Segment>();

                                decision = DecideHorizontalVertical(range);

                                //Make a cut inside the segment
                                MakeCut(script.startPoint, script.endPoint, decision, i, segment_child);
                            }
                        }
                        else
                        {
                            //If we are not returning any children then we need to make cuts in the current segment
                            Segment script = segment_1.GetComponent<Segment>();
                            decision = DecideHorizontalVertical(range);

                            MakeCut(script.startPoint, script.endPoint, decision, i, segment_1);
                        }

                        //Make Cut for second segment
                        Transform segment_2 = segmentManagerObject.transform.GetChild(1);
                        listofchildrentocut = FindSegmentsToCut(segment_2);

                        //We can going through the first segment to make cuts
                        if (listofchildrentocut.Count > 0)
                        {
                            //Theres some transforms in there to cut
                            for (int seg = 0; seg < listofchildrentocut.Count; seg++)
                            {
                                //Get the transform segment
                                Transform segment_child = listofchildrentocut[seg];
                                Segment script = segment_child.GetComponent<Segment>();

                                decision = DecideHorizontalVertical(range);
                                //Make a cut inside the segment
                                MakeCut(script.startPoint, script.endPoint, decision, i, segment_child);
                            }
                        }
                        else
                        {
                            //If we are not returning any children then we need to make cuts in the current segment
                            Segment script = segment_2.GetComponent<Segment>();

                            decision = DecideHorizontalVertical(range);
                            MakeCut(script.startPoint, script.endPoint, decision, i, segment_2);
                        }

                    }
                }

            }
        }
       
    }

    public List<Transform> FindSegmentsToCut(Transform segment) //We pass in the transform to check if it has children
    {
        //This script goes through each game object in the segment manager and finds out if it has children
        //if it doesn't we return the transform, if it does, we go through it find another transform to cut

        //Make a list of transforms to pass back
        List<Transform> listoftransforms = new List<Transform>();

        int childcount = segment.childCount;

        //We check the segment to check if it has any children, if it does we filter through the children
        //if it doesn't we cut the segment
        if (childcount == 0)
        {
            listoftransforms.Add(segment); //Add the segment we passed in and return it
            return listoftransforms; //We can cut the segment we passed in
        }
        else
        {
            //The segment we passed has some children, so we need to find out if we can make a cut in the children
            for (int i = 0; i < childcount; i++)
            {
                //For every child we check if we can cut, and if we can then we add it to the list and pass it back at the end
                Transform currentchild = segment.GetChild(i);

                if (currentchild.childCount == 0)
                {
                    listoftransforms.Add(currentchild);
                }
                else
                {
                    //If it has children we need to check to see if we can make a cut inside that child
                    List<Transform> updatedlistoftransforms = FindSegmentsToCut(currentchild); //Create a new list of updated transforms

                    listoftransforms.AddRange(updatedlistoftransforms);
                }
            }
            
        }

        //This should be a list of children we can cut
        return listoftransforms;
    }

    public int DecideCutPoint(int start, int end)
    {
        //Used to check where on the grid we are going to cut once we have decided which way
        //we are cutting.
        int randomnumber;

        //Before we make a cut, we need to figure out whether or not we can actually make a cut
        //in that direction by checking if the space between is atleast greater than 1, so we can
        //atleast cut in the middle, if not we then check if we can cut vertically, if not we skip
        //the cut

        int difference = Mathf.Abs(start - end);

        if (difference > DifferenceAllowance) //We can make a cut in this direction, because we atleast have half to work with
        {
            //Aslong as we can cut this segment in half, 
            //we then pick a value that is between start_point + 1 to make sure we always make a cut if we can
            //and end_point - 1 (can add padding for cuts) if we want padding
            randomnumber = Random.Range(start + DifferenceAllowance, end);

            return randomnumber;
        }
        else
        {
            //If the difference we have is less than or equal to 1, we cannot cut so we return -1
            return -1;
        }
    }

    public int DecideHorizontalVertical(int range)
    {
        int decision = -1; //variable to decide whether to cut vertically or horizontal 0 - vertically 1 - horizontally

        //Generate a random number between 0 - range if we are 50 or greater we cut horizontally (test threshold)
        int randomnumber = Random.Range(0, range + 1); //range never returns max so add 1 to range
        int middlerange = range / 2;

        if (randomnumber >= middlerange)
        {
            //Cut horizontally
            decision = 1;
        }
        else
        {
            //Cut vertically 
            decision = 0;
        }

        return decision;
    }
     
    public void DisableGridView()
    {
        if (grid_script) //Aslong as we have a reference to the grid handler
        {
            Debug.Log("Disable layers");
            grid_script.DisableAllColumns();
            grid_script.DisableLayers();
            grid_script.gameObject.SetActive(false);
        }
        else
        {
            //No reference to grid object
            Debug.Log("No reference to grid script");
        }
    }

    public void EnableGridView()
    {
        if (grid_script)
        {
            Debug.Log("Enable layers");
            grid_script.EnableLayers();
            grid_script.EnableAllColumns();
            grid_script.gameObject.SetActive(true);
        }
        else
        {
            //No reference to grid object
            Debug.Log("No reference to grid script");
        }
    }

    public List<Transform> ReturnListOfLastChildren(Transform segmentA, Transform segmentB)
    {
        //This script returns a list of the last children in both segments
        List<Transform> listofchildren = new List<Transform>();

        //Get the list of the last generation of children in segment A
        listofchildren.AddRange(ReturnListOfLastChildrenInSegment(segmentA));

        //Then we do the same to segment B
        listofchildren.AddRange(ReturnListOfLastChildrenInSegment(segmentB));

        return listofchildren;
    }

    public List<Transform> ReturnListOfLastChildrenInSegment(Transform segment)
    {
        //This script returns the list of the last children in a particular segment ( A or B )
        List<Transform> listofchildren = new List<Transform>();

        //We check if the segment has children, if it does, we check if it's children have children and repeat until
        //we find just children
        int childcount = segment.childCount;

        if (childcount > 0) 
        {
            //Because we are going to be adding gameobjects inside of a segment, we need to have a rule that if there
            //is one child in the segment then it is most likely the last child in the segment tree
            for (int i = 0; i < childcount; i++)
            {
                //Get the children and check if it has children
                Transform child = segment.GetChild(i);

                int children_count = child.childCount;

                //If the child at segment index has more than 1 child then we can assume it's
                //not the last generation of children, however if the children count is exactly 1
                //then we can assume it is the last generation of children in the segment tree
                if (children_count == 1 || children_count <= 0)
                {
                    listofchildren.Add(child);
                    continue;
                }

                //If we have more than 1 child we cycle again
                //We call this function again and add whatever whatever children we find to the list
                listofchildren.AddRange(ReturnListOfLastChildrenInSegment(child));
                

            }
        }


        return listofchildren;
    }

    public void ClearLevelParts()
    {
        if (levelPartsManager != null)
        {
            while (levelPartsManagerTransform.childCount > 0)
            {
                Transform child = levelPartsManagerTransform.GetChild(0);
                GameObject child_gameobject = child.gameObject;
                DestroyImmediate(child_gameobject);
            }
        }
    }
    #endregion
}
