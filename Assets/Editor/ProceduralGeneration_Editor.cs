using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

//This makes a custom editor for the inspector and 
//targets it to the Grid Script
//[CustomEditor(typeof(ProceduralGeneration_Core))]
public class ProceduralGeneration_Editor : EditorWindow
{
    #region inspector variables
    #endregion

    #region public variables
    public int filter { get; set; } //This is the filter we use to alter colours
    public int segment_spacing { get; set; } //The amount to space out the segments
    GameObject pgc_gameobject { get; set; } //chosen game object as procedural generator
    
    #endregion

    #region private variables
    private ProceduralGeneration_Core pgc;
    private bool eventoccured; //This is to detect if there is an event that happens on the screen
    private bool updateview; //This is to control the amount of times the screen redraws itself ( to increase frame rate )
    private bool drawGrid; //Option to draw grid
    private bool drawRoom; //Option to draw room
    private bool drawGridOutline; //Option to draw grid outline

    private static GUIContent
        directionContent = new GUIContent("Direction", "Pick the direction to build the grid");

    private static GUILayoutOption
        buttonWidth = GUILayout.MaxWidth(100f),
        buttonWidthSmall = GUILayout.MaxWidth(25f),
        toggleWidth = GUILayout.MaxWidth(25f);

    private Vector2 scrollPosition; //Scroll position for block list

    #endregion

    #region Unity Methods
    [MenuItem("Window/Procedural Generator")]
    static void Init()
    {
        //Get existing open window or if none, create it
        ProceduralGeneration_Editor window = (ProceduralGeneration_Editor)EditorWindow.GetWindow(typeof(ProceduralGeneration_Editor));
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnEnable()
    {
        RefreshSettings();
    }

    public void OnDisable()
    {
        updateview = false; //Stop updating view

        //Remove Grid Update delegate to stop stacking
        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
    }

    public void OnSceneGUI(SceneView sceneview)
    {
        ProceduralUpdate(sceneview);

        if (pgc) //If the target exists
        {
            //Show the guide of our segments
            if (pgc.segmentManagerObject) //If the segment manager exists
            {
                //Get the children and create a guide according to the children
                Transform reference = pgc.segmentManagerObject.transform;
                int childrencount = reference.transform.childCount;

                if (childrencount > 0) //We have children, so draw the guides
                {
                    DrawAllSegments(reference);
                }
            }

            //Draw Grid Outline
            if (drawGridOutline)
            {
                for (int x = 0; x <= pgc.maximumrows; x++)
                {
                    //Draw Horizontal Grid
                    Vector3[] positions = new Vector3[3];
                    Vector2 xstartpoint = new Vector2(x, 0f);
                    Vector2 xendpoint = new Vector2(pgc.maximumrows, pgc.maximumcolumns);
                    positions = pgc.CalculatePoints(xstartpoint, xendpoint, 0, pgc.creationDirection);
                    Handles.color = Color.black;
                    for (int i = 0; i < positions.Length; i++)
                    {
                        positions[i].y -= 1f;
                    }

                    Handles.DrawLine(positions[0], positions[1]);
                }

                for (int y = 0; y <= pgc.maximumcolumns; y++)
                {
                    //Draw Horizontal Grid
                    Vector3[] positions = new Vector3[3];
                    Vector2 ystartpoint = new Vector2(0f, y);
                    Vector2 yendpoint = new Vector2(pgc.maximumrows, pgc.maximumcolumns);
                    positions = pgc.CalculatePoints(ystartpoint, yendpoint, 0, pgc.creationDirection);

                    //Shift it up to show difference
                    for (int i = 0; i < positions.Length; i++)
                    {
                        positions[i].y -= 1f;
                    }

                    Handles.color = Color.black;
                    Handles.DrawLine(positions[0], positions[3]);
                    Handles.color = Color.white;
                }

                //Draw Grid Properties
                for (int x = 0; x < pgc.maximumrows; x++)
                {
                    //Loop through the columns ( y )
                    for (int y = 0; y < pgc.maximumcolumns; y++)
                    {
                        if (pgc.grid_params[x, y] == 1)
                        {
                            //Draw a cube cape
                            Vector3[] positions = new Vector3[3];
                            Vector2 ystartpoint = new Vector2(x, y);
                            Vector2 yendpoint = new Vector2(x + 1, y + 1);
                            positions = pgc.CalculatePoints(ystartpoint, yendpoint, 0, pgc.creationDirection);

                            //Shift it up to show difference
                            for (int i = 0; i < positions.Length; i++)
                            {
                                positions[i].y -= 1f;
                            }
                            Handles.DrawSolidRectangleWithOutline(positions, Color.black, Color.red);
                        }
                        else
                        {
                            //Handles.DrawSolidRectangleWithOutline(positions, Color.white, Color.black);
                        }
                    }
                }
            }

            DrawAStarGuide();
        }

        //Only repaint the scene if we have to, to save on frame rate
        if (updateview)
        {
            HandleUtility.Repaint();
            updateview = false; //Switch the update date view to false, as we do not need to redraw the grid again
        }
    }

    

    void OnGUI()
    {
        if (SceneView.onSceneGUIDelegate != this.OnSceneGUI)
        {
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
        }

        GUILayout.BeginVertical();

        //Create labels and input for variables-----//
        if (pgc) //Aslong as we have a target
        {
            //Settings---------------------------------//
            GUILayout.Label("-Settings-");
            //Create Width and Height input for grid
            GUILayout.BeginHorizontal();
            pgc.creationDirection = (ProceduralGeneration_Core.CreationDirection)EditorGUILayout.EnumPopup(directionContent, pgc.creationDirection, GUILayout.Width(300f));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Row Size");
            pgc.maximumrows = EditorGUILayout.IntField(pgc.maximumrows, GUILayout.Width(20));
            GUILayout.Label("Column Size");
            pgc.maximumcolumns = EditorGUILayout.IntField(pgc.maximumcolumns, GUILayout.Width(20));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Block Size");
            pgc.blocksize = EditorGUILayout.IntField(pgc.blocksize, GUILayout.Width(20));
            GUILayout.Label("Range");
            pgc.range = EditorGUILayout.IntField(pgc.range, GUILayout.Width(20));
            GUILayout.EndHorizontal();


            //Interactivity------------------------------------------------//
            GUILayout.BeginHorizontal();
            //Create buttons for creating a grid
            if (GUILayout.Button("Create Grid", EditorStyles.miniButtonMid, buttonWidth))
            {
                pgc.CreateGrid(pgc.maximumrows, pgc.maximumcolumns, pgc.blocksize);
                updateview = true;
            }

            //Show toggle for viewing grid outline
            if (GUILayout.Button("Show Grid", EditorStyles.miniButtonMid, buttonWidth))
            {
                //Toggle the grid
                //ToggleGridOutline(pgc.showGrid);
                drawGridOutline = !drawGridOutline;
            }


            //Update drawing
            if (GUILayout.Button("Update", EditorStyles.miniButtonMid, buttonWidth))
            {
                //Allow redrawing
                updateview = true; //Allow view to be updated with this button
                HandleUtility.Repaint();
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            //Add collisions toggle
            pgc.addcollisions = GUILayout.Toggle(pgc.addcollisions, "Collisions", buttonWidth);
            drawGrid = GUILayout.Toggle(drawGrid, "Draw Grid", buttonWidth);
            drawRoom = GUILayout.Toggle(drawRoom, "Draw Room", buttonWidth);
            GUILayout.EndHorizontal();

            //Editor for cuts we want to make
            GUILayout.BeginHorizontal();
            pgc.numberOfCuts = EditorGUILayout.IntField(pgc.numberOfCuts, GUILayout.Width(20f));

            //Create cuts
            if (GUILayout.Button("Create Cuts", EditorStyles.miniButton, buttonWidth))
            {
                //Allow cuts
                pgc.CreateSegmentManager(pgc.maximumrows, pgc.maximumcolumns);
                updateview = true; //Allow view to be updated with this button
            }

            if (GUILayout.Button("Generate Cuts and Rooms", EditorStyles.miniButton, GUILayout.MaxWidth(150f)))
            {
                //Allow cuts
                pgc.CreateSegmentManager(pgc.maximumrows, pgc.maximumcolumns);
                pgc.CreateRooms();
                updateview = true; //Allow view to be updated with this button

            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            //Create Rooms
            if (GUILayout.Button("Record Rooms", EditorStyles.miniButton, buttonWidth))
            {
                pgc.CreateRooms();
            }

            if (GUILayout.Button("Build Rooms", EditorStyles.miniButton, buttonWidth))
            {
                pgc.ActivateRoomBuilding();
            }

            if (GUILayout.Button("Clear level parts", EditorStyles.miniButton, buttonWidth))
            {
                pgc.ClearLevelParts();
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Find Path", EditorStyles.miniButton, buttonWidth))
            {
                AStar astar = pgc.GetComponent<AStar>();

                if (astar)
                {
                    astar.ResetAStar();
                    astar.WorkOutPath(astar.start_point, astar.end_point);
                }
            }

            //Difference allowance, to judge size of boxes
            GUILayout.Label("Difference Cap");
            pgc.DifferenceAllowance = EditorGUILayout.IntField(pgc.DifferenceAllowance, GUILayout.Width(20));

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            //Create spacing label field
            GUILayout.Label("Segment Spacing");
            segment_spacing = EditorGUILayout.IntField(segment_spacing, GUILayout.Width(20));

            //Create cut direction
            if (pgc.CutDirection == 0)
            {
                GUILayout.Label("Cut Direction : Vertical");
            }
            else if (pgc.CutDirection == 1)
            {
                GUILayout.Label("Cut Direction : Horizontal");
            }
            else
                GUILayout.Label("Cut Direction : None");

            GUILayout.EndHorizontal();

            //Show our segments
            //Show the children of the segment manager
            GameObject reference = pgc.segmentManagerObject;

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false, GUILayout.Width(250f), GUILayout.Height(75f));
            GUILayout.Label("Level Collection ", GUILayout.Width(100f));
            GUILayout.BeginHorizontal();
            if (pgc.blockcollection != null)
            {

                int length = pgc.levelparts.Count;

                //Check the length and draw the buttons to represent the objects collected
                if (length > 0)
                {
                    for (int i = 0; i < length; i++)
                    {
                        //Extract gameobject and print out it's name in a button
                        GameObject levelpart = (GameObject)pgc.levelparts[i];

                        //--- Button for name of object ---//
                        GUILayout.Label(levelpart.name);

                    }
                }
            }
            else
            {
                Debug.Log("No block collection :( ");
            }
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();
			
			if(pgc.segmentswithrooms != null)
			{
	            if (pgc.segmentswithrooms.Count > 0)
	            {
	                GUILayout.Label("There are " + pgc.segmentswithrooms.Count.ToString() + " segments with level parts. ");
	                for (int i = 0; i < pgc.segmentswithrooms.Count; i++)
	                {
	                    //Draw the list of segments which have a level part in them
	                    Segment segment = pgc.segmentswithrooms[i];
	                    if (segment)
	                    {
	                        GUILayout.Label("Segment at -> w : " + segment.width + " h : " + segment.height + " waited : " + segment.levelPartCreated);
	                        GUILayout.Label("With  padding: " + segment.paddingguide.ToString());
	                        GUILayout.Label("Block position: " + segment.LevelPartPosition.ToString());
	                    }
	                    
	                }
	            }
	            else
	            {
	                GUILayout.Label("There are no segments with level parts.");
	            }
			}


        }
        else
        {
            //Create object field to get procedural core
            pgc_gameobject = (GameObject)EditorGUILayout.ObjectField(pgc_gameobject, typeof(GameObject), GUILayout.Width(300f));

            if (GUILayout.Button("Apply", EditorStyles.miniButton, buttonWidth))
            {
                InitializeProceduralGenerator();
            }
        }
        GUILayout.EndVertical();

        //Repaint the scene
        //SceneView.RepaintAll();
    }

    #endregion

    #region Class Methods
    public void InitializeProceduralGenerator()
    {
        //PGC should have been registered by now

        updateview = false; //Do not update view
        //eventoccured = false;

        //Add the component to the object
        pgc = pgc_gameobject.GetComponent<ProceduralGeneration_Core>();

        if (pgc)
        {
            //If we have the reference we can carry out other actions to initialize
        }
        else
        {
            //pgc is null
            pgc = pgc_gameobject.AddComponent<ProceduralGeneration_Core>();

        }

        if (PrefabUtility.GetPrefabType(pgc) != PrefabType.Prefab)
        {

            
            //Get the resources
            //pgc.blockcollection = LoadBlocksAtPath("Blocks");

            if (pgc.levelparts == null)
            {
                pgc.levelparts = new List<UnityEngine.Object>();
            }

            if (pgc.levelparts != null)
            {
                //Update them anyway
                UnityEngine.Object[] resource = Resources.LoadAll("LevelParts", typeof(UnityEngine.Object));
                pgc.levelparts.Clear(); //Clear the list
                for (int i = 0; i < resource.Length; i++)
                {
                    pgc.levelparts.Add(resource[i]);
                }
            }
        }
    }

    public void RefreshSettings()
    {
        if (pgc)
        {
            updateview = false; //Do not update view
            //eventoccured = false;

            if (PrefabUtility.GetPrefabType(pgc) != PrefabType.Prefab)
            {
                Debug.Log("Loading level parts");
                //Get the resources
                //pgc.blockcollection = LoadBlocksAtPath("Blocks");

                if (pgc.levelparts == null)
                {
                    pgc.levelparts = new List<UnityEngine.Object>();
                    Debug.Log("Creating new list.");
                }

                if (pgc.levelparts != null)
                {

                    //Update them anyway
                    UnityEngine.Object[] resource = Resources.LoadAll("LevelParts", typeof(UnityEngine.Object));
                    pgc.levelparts.Clear(); //Clear the list
                    Debug.Log("Cleared list to insert content of " + resource.Length + " size.");
                    for (int i = 0; i < resource.Length; i++)
                    {
                        pgc.levelparts.Add(resource[i]);
                    }
                }
            }
        }
    }

    public void ProceduralUpdate(SceneView scene)
    {
        Event e = Event.current; //Grab event
        //e.Use();

        if (pgc)
        {
            //Update grid reference
            pgc.UpdateGridReference();
        }
    }

    

    public void DrawAllSegments(Transform master) //One master that cycles through all the children, grandchildren etc
    {
        int childcount = master.childCount;

        if (childcount > 0)
        {
            //Draw the visual reference, after it has been drawn we then go through its children
            for (int i = 0; i < childcount; i++)
            {
                Transform child = master.GetChild(i);

                filter = 0;

                //Draw visual reference
                //DrawVisualRectangleForSegment(child, i);
                
                //Cycle through the children it has any
                //DrawChildSegment(child); /* Drawing all the child elements really slows it down */
            }

            //Draw visual reference for last set of children
            //Pass in segment A and B to get the list of the last children,
            List<Transform> lastgenerationofchildren = pgc.ReturnListOfLastChildren(master.GetChild(0), master.GetChild(1));

            //Then draw them
            for (int i = 0; i < lastgenerationofchildren.Count; i++)
            {
                if(drawGrid)
                    DrawVisualRectangleForSegment(lastgenerationofchildren[i], i);
                if(drawRoom)
                    DrawRooms(lastgenerationofchildren[i], i);
            }
        }
         
        //After drawing all segments, we draw the room guides
        //DrawRoomGuides(); 
        
        
    }

    public void ToggleGridOutline(bool value)
    {
        //This script only happens as soon the toggle is switched
        if (value)
        {
            //Check if the script exists
            if (pgc.grid_script)
            {
                //Enable it and make sure we make the grid know its visible
                pgc.EnableGridView();
            }
            else
            {
                //No reference to grid object
                Debug.Log("No reference to grid script");
            }
        }
        else
        {
            //Check if the script exists
            if (pgc.grid_script)
            {
                //Disable it and make sure we make the grid know its invisible
                pgc.DisableGridView();
            }
            else
            {
                //No reference to grid object
                Debug.Log("No reference to grid script");
            }
        }

        //Toggle grid showing
        pgc.showGrid = !value;
    }

    public void DrawChildSegment(Transform child)
    {
        //This function will go through this child, draw it's queue and then check if it has anymore children to draw
        int childcount = child.childCount;

        if (childcount > 0)
        {
            for (int i = 0; i < childcount; i++)
            {
                //We get the children of the child we passed in
                Transform parents_child = child.GetChild(i);
                //Draw the visual reference
                DrawVisualRectangleForSegment(parents_child, i);
                
        		//Draw Master Segment Markers ( Upper level rooms to connect )
				DrawMasterSegments(parents_child);
				
                //Draw the child segment
                DrawChildSegment(parents_child);
            }
        }
        
    }
	
	public void DrawMasterSegments(Transform segment)
	{
		Segment script = segment.GetComponent<Segment>();
		if(script)
		{
			
			float difference = pgc.numberOfCuts - 3;
			if(difference <= 0)
				difference = 1;
			
			if(script.segment_index >= difference)
			{
				Vector3 pgcposition = pgc.transform.position;
	            float blocksize = pgc.blocksize;
				Vector3[] positions = new Vector3[3];
				
				//Filter through and apply the positions, according to our map
	            Vector2 startPoint = script.segment_position.startPoint;
	            Vector2 endPoint = script.segment_position.endPoint;
	            int segment_index = script.segment_index;
	            int zposition = segment_index * segment_spacing;
	            Vector2 rightShift = Vector2.zero;//new Vector2(zposition, 0) * -1;
	            
				//positions = pgc.CalculatePoints(startPoint, endPoint, 0, pgc.creationDirection);
				positions = pgc.CalculatePoints(startPoint + rightShift, endPoint + rightShift, 0, pgc.creationDirection);
				
				Handles.color = Color.yellow;
				Handles.DrawAAPolyLine(5f, positions);
				Handles.color = Color.white;
			}
		}
	}

    public void DrawVisualRectangleForSegment(Transform segment, int index)
    {
        Segment script = segment.GetComponent<Segment>();
        //If the script exists, use it's points to create a box
        if (script)
        {
			//Get the z position of the object
            Vector3 pgcposition = pgc.transform.position;
            float blocksize = pgc.blocksize;
			Vector3[] positions = new Vector3[3];
			
			//Filter through and apply the positions, according to our map
            Vector2 startPoint = script.segment_position.startPoint;
            Vector2 endPoint = script.segment_position.endPoint;
            int segment_index = script.segment_index;
            int zposition = segment_index * segment_spacing;
            Vector2 rightShift = Vector2.zero;//new Vector2(zposition, 0) * -1;
            
			//positions = pgc.CalculatePoints(startPoint, endPoint, 0, pgc.creationDirection);
			positions = pgc.CalculatePoints(startPoint + rightShift, endPoint + rightShift, 0, pgc.creationDirection);
			
            Vector3 addition = script.segment_position.startPoint * blocksize;
            Vector3 newposition = pgcposition + addition;
            //Handles.CubeCap(0, positions[0], Quaternion.identity, (float)blocksize);

            Color fill = Color.white;
            filter += segment_index * 20;
            int outlinefilter = filter;

            Color outlinecolor = new Color(outlinefilter, outlinefilter, outlinefilter);

            //See if the number we are at is odd or even
            if (index % 2 == 0)
            {
                //Even
				if(script.width <= 2 || script.height <= 2)
					fill = new Color(0, 50 + filter,0f, 0.5f);
				else
                	fill = new Color(0, 0, 10 + filter,0.1f);
            }
            else
            {
                //Odd
				if(script.width <= 2 || script.height <= 2)
					fill = new Color(0, 50 + filter,0f, 0.5f);
				else
                	fill = new Color(10 + filter, 0, 0, 0.1f);
            }

            //Handles.RectangleCap(0, newposition, Quaternion.identity, blocksize);
            Handles.DrawSolidRectangleWithOutline(positions, fill, outlinecolor);
        }
    }

    public void DrawRooms(Transform segment, int index)
    {
        Segment script = segment.GetComponent<Segment>();
        //If the script exists, use it's points to create a box
        if (!script)
            return;

        //Draw Rooms inside of the segments
        if (script.hasRoom)
        {
            //Draw the rooms
            //Filter through and apply the positions, according to our map
            Room room_script = script.GetComponentInChildren<Room>(); //Mostly likely the first child

            if (!room_script)
                room_script = script.transform.GetChild(0).GetComponent<Room>(); //Try to force the first child

            if (!room_script)
                return;

            if (room_script.room_created) //If the room has been created
            {
                Vector2 room_startPoint = room_script.start_point;
                Vector2 room_endPoint = room_script.end_point;

                Vector2 room_rightShift = Vector2.zero;//new Vector2(zposition, 0) * -1;
                Vector3[] room_positions = new Vector3[3];

                //positions = pgc.CalculatePoints(startPoint, endPoint, 0, pgc.creationDirection);
                room_positions = pgc.CalculatePoints(room_startPoint + room_rightShift, room_endPoint + room_rightShift, 0, pgc.creationDirection);
                Handles.DrawSolidRectangleWithOutline(room_positions, Color.green, Color.red);
            }

        }
    }

    public void DrawAStarGuide()
    {
        if (pgc)
        {
            AStar astar = pgc.GetComponent<AStar>();
            if (astar)
            {
                //Time to draw something
                //Draw the start point
                Vector3[] p = new Vector3[3];
                Vector2 sp = astar.start_point;
                Vector2 ep = new Vector2(sp.x + 1, sp.y + 1);
                p = pgc.CalculatePoints(sp, ep, 0, pgc.creationDirection);

                //Shift it up to show difference
                for (int i = 0; i < p.Length; i++)
                {
                    p[i].y -= 1f;
                }

                Handles.DrawSolidRectangleWithOutline(p, Color.blue, Color.black);

                //Draw the end point
                p = new Vector3[3];
                sp = astar.end_point;
                ep = new Vector2(sp.x + 1, sp.y + 1);
                p = pgc.CalculatePoints(sp, ep, 0, pgc.creationDirection);

                //Shift it up to show difference
                for (int i = 0; i < p.Length; i++)
                {
                    p[i].y -= 1f;
                }

                Handles.DrawSolidRectangleWithOutline(p, Color.red, Color.black);

                //Cycle through the complete path and draw the squares
                List<Vector2> paths = astar.complete_path;
                foreach (Vector2 path in astar.complete_path)
                {
                    //Draw Horizontal Grid
                    Vector3[] positions = new Vector3[3];
                    Vector2 startpoint = path;
                    Vector2 endpoint = new Vector2(path.x + 1, path.y + 1);
                    positions = pgc.CalculatePoints(startpoint, endpoint, 0, pgc.creationDirection);

                    //Shift it up to show difference
                    for (int i = 0; i < positions.Length; i++)
                    {
                        positions[i].y -= 1f;
                    }

                    if (path == astar.end_point)
                    {
                        Handles.DrawSolidRectangleWithOutline(positions, Color.red, Color.black);
                    }
                    else
                        Handles.DrawSolidRectangleWithOutline(positions, Color.cyan, Color.white);
                }

            }
        }
    }

    public void DrawRoomGuides()
    {
        //This script draws the rooms/blocks/objects created in the scene, and where they will be
        //makes it easier to make new cuts and generate new maps without have to create all the parts
        Color fill = Color.white;

        fill = new Color(255, 0, 0, 1f);

        //Filter through the list of vector 3 guides we have and draw a rectangle in those
        //positions
        List<Vector3> listofroomguides = pgc.CreateRoomGuides();

        for (int i = 0; i < listofroomguides.Count; i++)
        {
            //Collect the vector and use it to draw the cube
            Vector3 cube_position = listofroomguides[i];
            Handles.color = fill;
            
            Handles.CubeCap(-1, cube_position, Quaternion.identity, pgc.blocksize);
        }
		
		//Handles.color = Color.black;
		//Handles.DrawAAPolyLine(5f,listofroomguides.ToArray());
		//Handles.color = Color.white;
    }


    public Transform ReturnLastChildOfObject(Transform transform)
    {
        //Would only return the first last child we find in the transform
        int childcount = transform.childCount;

        if (childcount > 0)
        {
            //Cycle through the children and find the transfrom that has the last set of children
            for (int i = 0; i < childcount; i++)
            {
                Transform child = transform.GetChild(i);

                if (child.childCount > 0)
                {
                    //Get the child, and pass it to this object to repeat itself
                    return ReturnLastChildOfObject(child);
                }
            }

            //Return the child we are looking at
        }
        
        //If we do not find anything we return the same transform back
        return transform;
        
     
    }

    public Transform ReturnLastChildOfObjectAt(Transform transform, int index)
    {
        //We return the last child using the ReturnLastChildOfObject
        Transform lastchild = ReturnLastChildOfObject(transform);

        //Then get the parent of that child, and return the child at index
        //First we check to see if we are in bounds, if we are not return the last child
        int childcount = lastchild.parent.childCount;
        if (index > childcount)
        {
            return lastchild.parent.GetChild(childcount - 1);
        }
        else
            return lastchild.parent.GetChild(index); //Else return the child at index

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
            for (int i = 0; i < childcount; i++)
            {
                //Get the children and check if it has children
                Transform child = segment.GetChild(i);

                int children_count = child.childCount;

                if (children_count > 0) //If the child aslo has children we go through again
                {
                    //We call this function again and add whatever whatever children we find to the list
                    listofchildren.AddRange(ReturnListOfLastChildrenInSegment(child));
                }
                else //If it doesnt have children then it is the last generation so we add it to the list
                {
                    listofchildren.Add(child);
                }
                
            }
        }


        return listofchildren;
    }

    public Transform ReturnLastParentOfObject(Transform transform)
    {
        //We find the last child and then return the parent of that child
        Transform lastchild = ReturnLastChildOfObject(transform);

        return lastchild.parent;
    }

    public Transform ReturnLastParentOfObjectAt(Transform transform, int index)
    {
        //Find the parent and then pass it's parent child at index
        Transform lastParent = ReturnLastParentOfObject(transform);

        Transform grandparent = lastParent.parent;

        if (grandparent)
        {
            //Aslong as we have a grand parent we can return the children at index
            int childcount = grandparent.childCount;

            if (index > childcount)
            {
                return grandparent.GetChild(childcount - 1);
            } 
            else return grandparent.GetChild(index);
        }

        return lastParent;
    }

    Object[] LoadBlocksAtPath(string path)
    {
        //This script loads the objec
        Object[] blocklist = Resources.LoadAll(path);
        return blocklist;
    }

    #endregion
}
