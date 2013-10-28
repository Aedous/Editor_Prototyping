using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

public class ObjectBuilder : EditorWindow
{
    /*
     * This script is basically the next version of the grid handler, with extra tweaks and 
     * polishing.
     * It will use the existing grid handler's functionality instead of always having to have the
     * object selected in the inspector.
     */
    #region Variables
    /* PUBLIC VARIABLES */
    public GridHandler grid;
	public static Global.ToolSet toolset;
    public Transform gridtransform;


    /* PRIVATE VARIABLES */
    //GUI Settings----------------------------------------//
    private static GUIContent
		directionContent = new GUIContent("Direction", "Pick the direction to build the grid"),
        drawContent = new GUIContent("Start Drawing!", "Turn on drawing mode to create tiles"),
        layerupContent = new GUIContent("^", "Move up a layer"),
        layerdownContent = new GUIContent("v", "Move down a layer"),
        layeraddContent = new GUIContent("+", "Add a new layer"),
        createdgridContent = new GUIContent("Create Grid", "Create a grid"),
        updatelayerContent = new GUIContent("Update Layers", "Update the layers"),
        fixlayerContent = new GUIContent("Sync Layers", "Sync layers to make sure there are no empty objects"),
        insertContent = new GUIContent("+", "insert a copy after this layer"),
        deleteContent = new GUIContent("-", "delete this layer"),
        teleportContent = new GUIContent("T", "Move layer position"),
        deleteBlock = new GUIContent("x", "delete the game object");

    private static GUILayoutOption
        drawbuttonWidth = GUILayout.MaxWidth(255f),
        layerbuttonWidth = GUILayout.MaxWidth(20f),
        updateLayerButton = GUILayout.MaxWidth(255f),
        buttonWidth = GUILayout.MaxWidth(50f),
        buttonWidthSmall = GUILayout.MaxWidth(25f);

    private GameObject selectedObject, teleportObject = null, grid_object;
    private string nameoflevel; //Export name for prefab
    private bool drawtiles, keepfunctionality, clipemptyobjects, leftclickdown, rightclickdown; //Booleans to control behaviour of editor
    private int teleportIndex = -1;
    private Object[] blockcollection; //Used to show what kind of blocks we can use
    private Vector2 scrollPosition; //Scroll position for block list
    

    #endregion

    #region Unity Methods
    [MenuItem("Window/Object Builder")]
    static void Init()
    {
        //Get existing open window or if none, create it
        ObjectBuilder window = (ObjectBuilder)EditorWindow.GetWindow(typeof(ObjectBuilder));
		
		//Set tool set to brush
		toolset = Global.ToolSet.BRUSH;
    }

    void OnGUI()
    {
        if (SceneView.onSceneGUIDelegate != this.OnSceneGUI)
        {
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
        }


        if (grid)
        {
            GridGUIControls();
        }
        else
        {
            //Debug.Log("No Grid Reference, resetting grid reference");
            //If we do not have a grid reference we try and get one from the screen
            if (!grid_object)
            {
                grid_object = GameObject.FindGameObjectWithTag("Grid");
            }

            if (grid_object)
            {
                grid = grid_object.GetComponent<GridHandler>();
                if (!grid)
                {
                    //Add the grid handler component
                    grid_object.tag = "Grid";
                    grid = grid_object.AddComponent<GridHandler>();

                }
            }
            else
            {
                //Debug.Log("Cannot find Grid Object to manipulate");
            }
			
			//Create GUI CONTROLS to select a grid object to manipulate
            //Create Width and Height of Whole Grid fields
            GUILayout.BeginHorizontal();
            grid_object = (GameObject)EditorGUILayout.ObjectField(grid, typeof(GameObject), GUILayout.Width(300f));
            GUILayout.EndHorizontal();

        }
    }

    public void OnEnable()
    {
        drawtiles = false;
		leftclickdown = false;
		rightclickdown = false;
        //Get the resources
        blockcollection = LoadBlocksAtPath("Blocks");

        if (grid == null)
        {
            //If we do not have a grid reference we try and get one from the screen
            GameObject grid_object = GameObject.FindGameObjectWithTag("Grid");

            if (grid_object)
            {
                grid = grid_object.GetComponent<GridHandler>();
            }

            //Set our transform reference
            //gridtransform = grid_object.transform;
        }
    }

    public void OnDisable()
    {
        //If we come off the object, stop our drawing option
        drawtiles = false;
        drawContent.text = "Start Drawing!";
		leftclickdown = false;
		rightclickdown = false;

        //Remove Grid Update delegate to stop stacking
        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;


        //HandleUtility.Repaint(); //Give screen back
    }

    #endregion

    #region Methods
    public void OnSceneGUI(SceneView sceneview)
    {
        if (grid)
        {
            GridUpdate(sceneview);
            //we allow switching on and off of grid
            if (grid.ShowGrid)
            {
                //Apply gizmo color
                Handles.color = grid.guidecolor;

                //Draw Square by Square
                /*
                 * start with the row size, and draw a square from 0 and increment by blocksize
                 * when at row move along the column according to size and shift to the right by blocksize
                 * repeat for each row.
                 * */

                DrawGuide(grid.blocksize, grid.maximumrows, grid.maximumcolumns, grid.guidecolor);

                //Draw Hover Item aslong as we have one

            }
        }

        //If we have a grid reference we can draw its values
        //sceneview.Repaint();
    }

    void GridUpdate(SceneView view)
    {
        //Get the input event
        Event e = Event.current;

        if (grid) //Make sure the grid still exists
        {
            if (grid.CurrentLayer < grid.transform.childCount)
            {
                //Check mouse hovering which returns what cube we are looking at
                MouseHover(e);

                //Mouse Input--------------------------------------------------//
                MouseInput(e);

                //Keyboard Input -------------------------------//
                KeyboardInput(e);

            }//End of checking if we are still in bounds on our current layer
        }

        if (drawtiles)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
        else
        {
            //HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Native));
            //Reset
            //HandleUtility.AddDefaultControl(SceneView.currentDrawingSceneView.GetInstanceID());
        }

    }

    public void GridGUIControls()
    {
        //This function handles the drawing of the controls in the window
        //Draw the fields
        GUILayout.BeginVertical();
        //Settings for the grid handler ---------------------//
        GUILayout.Label("-Settings-");
        GUILayout.BeginHorizontal();
        grid.AutomaticUpdates = GUILayout.Toggle(grid.AutomaticUpdates, "Automatic Upates", GUILayout.Width(200f));
        GUILayout.EndHorizontal();
		
		//Enum for direction
		GUILayout.BeginHorizontal();
		grid.creationDirection = (GridHandler.CreationDirection)EditorGUILayout.EnumPopup(directionContent,grid.creationDirection, GUILayout.Width (300f));
		GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label(" Grid Row Size ");
        grid.maximumrows = EditorGUILayout.IntField(grid.maximumrows, GUILayout.Width(20));
        GUILayout.Label(" Grid Column Size ");
        grid.maximumcolumns = EditorGUILayout.IntField(grid.maximumcolumns, GUILayout.Width(20));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label(" Grid Block Size ");
        grid.blocksize = EditorGUILayout.IntField(grid.blocksize, GUILayout.Width(20));
        GUILayout.Label("Spacing ");
        grid.layerspacing = EditorGUILayout.FloatField(grid.layerspacing, GUILayout.Width(20f));
        GUILayout.EndHorizontal();

        //Create Grid
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(createdgridContent, EditorStyles.miniButton, updateLayerButton))
        {
            if (grid_object.transform.childCount <= 0)
            {
                grid.CreateGrid(grid.maximumrows, grid.maximumcolumns, grid.blocksize, 0, (int)grid.layerspacing);
            }
            else
            {
                //Update the grid with new settings
            }
        }
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        int layertext = grid.CurrentLayer + 1;
        GUILayout.Label(" Layer : " + layertext);

        //Allow Addition of Layers
        if (GUILayout.Button(layeraddContent, EditorStyles.miniButtonLeft, layerbuttonWidth))
        {
            //This function creates a new layer at the end and adds it to the child object
            int endpoint = grid.maxlayers;
            grid.CreateLayer(endpoint);
        }

        //Allow movement of layers
        if (GUILayout.Button(layerupContent, EditorStyles.miniButtonMid, layerbuttonWidth))
        {
            grid.MoveToLayer(grid.CurrentLayer + 1);
        }
        if (GUILayout.Button(layerdownContent, EditorStyles.miniButtonRight, layerbuttonWidth))
        {
            grid.MoveToLayer(grid.CurrentLayer - 1);
        }

        GUILayout.EndHorizontal();

        //Layer Objects -------------------------- //
        GUILayout.BeginVertical();
        int childcount = grid.transform.childCount;


        GUILayout.Label("Number of objects in list of layers : " + childcount);
        for (int i = 0; i < childcount; i++)
        {
            GameObject childobject = grid.GetChildGameObject(i);

            //Horizontal
            GUILayout.BeginHorizontal();
            //Create the layer list
            //If object exists draw the name, else draw a missing object
            if (childobject == null)
            {
                GUILayout.Label("Layer [ " + i + " ] : Missing Game Object");
            }
            else
            {
                GUILayout.Label("Layer Name : " + childobject.name);
            }


            //Create the button to delete and add layer
            if (GUILayout.Button(insertContent, EditorStyles.miniButtonLeft, buttonWidthSmall))
            {
                //Add layer
            }
            if (GUILayout.Button(deleteContent, EditorStyles.miniButtonMid, buttonWidthSmall))
            {
                //Delete layer
                grid.DeleteLayer(i);

                //Update layers
                grid.CreateAndUpdateLayers();
            }

            if (teleportIndex == i)
            {
                teleportContent.text = "o";
            }
            else
            {
                teleportContent.text = "T";
            }

            if (GUILayout.Button(teleportContent, EditorStyles.miniButtonRight, buttonWidthSmall))
            {
                //Check to see if we already have a teleporting element
                if (teleportIndex != -1) //No object selected
                {
                    //If we have an object to teleport,
                    if (teleportIndex == i)
                    {
                        //No teleport if we are on the same index
                        teleportIndex = -1; //Reset teleport index
                    }
                    else
                    {
                        grid.MoveLayers(teleportIndex, i);
                        teleportIndex = -1; //Reset teleport index
                    }
                }
                else
                {
                    //If we do not have a teleporting index set the teleporting index
                    teleportIndex = i;
                }
            }

            if (childobject)
            {
                LayerHandler script = childobject.GetComponent<LayerHandler>();
                script.showDetails = GUILayout.Toggle(script.showDetails, "Details", buttonWidth);
            }

            GUILayout.EndHorizontal();

            //The layers individual objects collected from the children--------------------------//
            //Get the layer script for each layer
            if (childobject != null) //Check if the object exists, which it probably does
            {
                //Get the script we need
                LayerHandler layerScript = childobject.GetComponent<LayerHandler>();

                if (layerScript != null) //Make sure it is valid
                {
                    if (layerScript.showDetails)
                    {
                        //Return a list of child game objects to work with.
                        List<GameObject> objectlist = layerScript.GetAListOfChildren();

                        if (objectlist != null)
                        {
                            //Cycle through its object list to get the information about the objects
                            for (int x = 0; x < objectlist.Count; x++)
                            {
                                GameObject _object = objectlist[x];
                                GUILayout.BeginVertical();
                                //Output the objects the layer is holding in its list.
                                GUILayout.BeginHorizontal();
                                GUILayout.Label(_object.name);
                                //Add delete button
                                if (GUILayout.Button(deleteBlock, EditorStyles.miniButtonMid, buttonWidthSmall))
                                {
                                    //Delete GameObject
                                    layerScript.RemoveObjectInLayer(_object);
                                }
                                GUILayout.EndHorizontal();
                                GUILayout.EndVertical();
                            }
                        }
                        else
                        {
                            Debug.Log("Missing list in game object at layer : " + i);
                        }
                    }
                }
                else
                    Debug.Log("Layer does not exist at index : " + i);
            }
            else
            {
                //The layer in our grid list does not exist in our generated list
                //So we call the update layer function in the grid class to update the layers
                //incase of changes made
                grid.CreateAndUpdateLayers();
            }
        }

        GUILayout.EndVertical();
        //-----------------------------------------//


        GUILayout.BeginHorizontal();
        //Create Update Button For Updating Layers
        if (GUILayout.Button(updatelayerContent, EditorStyles.miniButtonMid, updateLayerButton))
        {
            //Update the layers
            grid.CreateAndUpdateLayers();
        }
        GUILayout.EndHorizontal();

        //------------------------------------------------------//



        //Create Tile Field
        GUILayout.BeginHorizontal();
        GUILayout.Label("Grid Object ");
        grid.tile = (GameObject)EditorGUILayout.ObjectField(grid.tile, typeof(GameObject), GUILayout.Width(150f));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Show Grid ");
        grid.ShowGrid = EditorGUILayout.Toggle(grid.ShowGrid, GUILayout.Width(25f));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Guide Color ");
        grid.guidecolor = EditorGUILayout.ColorField(grid.guidecolor, GUILayout.Width(50f));
        GUILayout.Label("Hover Color ");
        grid.hovercolor = EditorGUILayout.ColorField(grid.hovercolor, GUILayout.Width(50f));
        GUILayout.EndHorizontal();

        //Create Button For Start Draw
        if (GUILayout.Button(drawContent, drawbuttonWidth))
        {
            //Set drawing tiles option to true
            if (drawtiles)
            {
                drawContent.text = "Start Drawing!";
                drawtiles = false;
				leftclickdown = false; //Just incase
				rightclickdown = false;

                //Reset scene mouse input
                //HandleUtility.AddDefaultControl(0);
                
            }
            else //If we click in false mode
            {
                //If we are not in draw style mode
                drawContent.text = "Stop Drawing!";
                drawtiles = true;
				

                //Remove scene mouse input
                //HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            }
        }
		
		//Create the toolset GUI to use
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Brush", GUILayout.Width (50f), GUILayout.Height (50f)))
		{
			//Set the tool set to brush
			toolset = Global.ToolSet.BRUSH;
		}
		
		if(GUILayout.Button("Erase", GUILayout.Width (50f), GUILayout.Height (50f)))
		{
			//Set the tool set to brush
			toolset = Global.ToolSet.ERASE;
		}
		
		if(GUILayout.Button("Fill", GUILayout.Width (50f), GUILayout.Height (50f)))
		{
			//Set the tool set to brush
			toolset = Global.ToolSet.FILL;
		}

        if (GUILayout.Button("Empty", GUILayout.Width(50f), GUILayout.Height(50f)))
        {
            //Set the tool set to brush
            toolset = Global.ToolSet.EMPTY;
        }

        if (GUILayout.Button("Duplicate", GUILayout.Width(50f), GUILayout.Height(50f)))
        {
            //set the toolset to duplicate
            toolset = Global.ToolSet.DUPLICATE;
        }
		
		GUILayout.EndHorizontal();

        //----Creating the list of objects you can cycle through and create
        // GUILayout.BeginArea(new Rect(0, 0, 100, 50));

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false, GUILayout.Width(250f), GUILayout.Height(75f));
        GUILayout.BeginHorizontal();
        if (blockcollection != null)
        {
            int length = blockcollection.Length;
            //Check the length and draw the buttons to represent the objects collected
            if (length > 0)
            {
                for (int i = 0; i < length; i++)
                {
                    //Extract gameobject and print out it's name in a button
                    GameObject block_object = (GameObject)blockcollection[i];

                    //--- Button for name of object ---//
                    if (block_object == grid.tile)
                    {

                    }

                    if (GUILayout.Button(block_object.name, GUILayout.Width(50f), GUILayout.Height(50f)))
                    {
                        //If we click on the button, set our creation object to be this
                        grid.tile = block_object;
                    }

                }
            }
        }

        GUILayout.EndHorizontal();
        GUILayout.EndScrollView();

        //Export Settings------------------------------//
        if (PrefabUtility.GetPrefabType(grid) != PrefabType.Prefab) //Can only save levels if we are not in prefab mode.
        {
            if (grid)
            {
                GUILayout.BeginHorizontal();
                    GUILayout.Label("Name of Level:");
                    grid.NameOfLevel = GUILayout.TextField(grid.NameOfLevel, 100);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                    GUILayout.BeginVertical();
                        grid.KeepFunctionality = GUILayout.Toggle(grid.KeepFunctionality, "Keep functionality");
                        grid.Clip = GUILayout.Toggle(grid.Clip, "Clip objects.");
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical();
                        grid.ClipBlock = GUILayout.Toggle(grid.ClipBlock, "Clip blocks.");
                        GUILayout.EndVertical();
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Create Level", GUILayout.Width(100f)))
                {
                    //Create the level
                    Debug.Log("Clip Object : " + grid.Clip);
                    MakeLevel(grid.NameOfLevel, grid.Clip, grid.KeepFunctionality, grid.ClipBlock);
                }
            }
            else
            {
                //Find the grid again
                Debug.Log("No grid reference ? ");
            }
        }

        GUILayout.EndVertical();
    }

    public void DrawGuide(int blocksize, int rowsize, int columnsize, Color gridcolor)
    {
        //Debug.Log("HELO");
        //Apply gizmo color
        Handles.color = gridcolor;
		
		//Collect the columns in the grid, and draw a cube cap at their position
		if(grid.CurrentLayer < grid.transform.childCount) //Make sure we do not go out of bounds and cause a gui error
		{
			GameObject layerobject = grid.GetChildGameObject(grid.CurrentLayer);
			LayerHandler layer = layerobject.GetComponent<LayerHandler>();
			Vector3 layerposition = layerobject.transform.localPosition;
			
			//Once we have the layer, we need to find the rows and draw the cube and the column
			Vector3 size = Vector3.one * blocksize;
			
			for(int row_index = 0; row_index < layerobject.transform.childCount; row_index++)
			{
				//Filter through the children in the row object (column)
				Transform row_object = layerobject.transform.GetChild(row_index);
				
				for(int col_index = 0; col_index < row_object.childCount; col_index++)
				{
					Transform column_object = row_object.GetChild(col_index);
					Vector3 cube_position = column_object.position;
					//cube_position += layerposition;
				
					if (grid.hoverIndex.x == row_index && grid.hoverIndex.y == col_index)
	                {
	                    Handles.color = grid.hovercolor;
	                    Handles.CubeCap(0, cube_position, Quaternion.identity, (float)blocksize);
	
	                    Handles.color = gridcolor;
	                }
	                else
	                {
	                    Handles.CubeCap(0, cube_position, Quaternion.identity, (float)blocksize);
	                }
				}
				
			}
		}

        //Old drawing of handles which does not take into account the direction the grid is facing
		/*
        if (grid.CurrentLayer < grid.transform.childCount)
        {
            GameObject layerobject = grid.GetChildGameObject(grid.CurrentLayer);
            LayerHandler layer = layerobject.GetComponent<LayerHandler>();

            Vector3 blockposition = layerobject.transform.position;
            Vector3 size = Vector3.one * blocksize;

            //Draw a square for each block according to it's position
            for (int row = 0; row < rowsize; row++)
            {
                //Cycle through each row and then draw a square by each column
                for (int col = 0; col < columnsize; col++)
                {
                    int xmove = col * blocksize;
                    int ymove = row * blocksize;
                    Vector3 newposition = new Vector3(blockposition.x + xmove, blockposition.y + ymove, blockposition.z);

                    if (grid.hoverIndex.x == row && grid.hoverIndex.y == col)
                    {
                        Handles.color = grid.hovercolor;
                        Handles.CubeCap(0, newposition, Quaternion.identity, (float)blocksize);

                        Handles.color = gridcolor;
                    }
                    else
                    {
                        Handles.CubeCap(0, newposition, Quaternion.identity, (float)blocksize);
                    }
                }
            }
        }
        */
		
        //Repaint view
        HandleUtility.Repaint();
    }

    public void KeyboardInput(Event e)
    {
        if (e.isKey)
        {
            //Check what key was pressed
            if (e.keyCode == KeyCode.UpArrow)
            {
                if (e.type == EventType.KeyDown)
                {
                    grid.MoveToLayer(grid.CurrentLayer + 1);
                }
            }
            else if (e.keyCode == KeyCode.DownArrow)
            {
                if (e.type == EventType.KeyDown)
                {
                    grid.MoveToLayer(grid.CurrentLayer - 1);
                }
            }
        }
    }

    public void MouseInput(Event e)
    {
        //Cannot draw while using key input and mouse ( unity camera controls
        if (!e.isKey)
        {
			if(drawtiles)
			{
				DrawingControls(e);
			}
        }
    }

    public void MouseHover(Event e)
    {
        //Get screen mouse coords to world coords
        //makes the bottom left corner (0,0) and top right corner(Camera.current.pixelWidth, Camera.current.pixelHeight)
        //Ray r =  Camera.current.ScreenPointToRay(e.mousePosition);
        Ray r = HandleUtility.GUIPointToWorldRay(new Vector2(e.mousePosition.x, e.mousePosition.y));

        RaycastHit[] hit = Physics.RaycastAll(r);
        Vector3 mousePos = r.origin; //Reference the origin to get mouse position

        //Get the current layer
        GameObject layer_object = grid.GetChildGameObject(grid.CurrentLayer);
        LayerHandler layer = layer_object.GetComponent<LayerHandler>();

        //Cycle through what we have hit and check if it is a row
        if (hit.Length > 0)
        {
            //If we have hit a bunch of objects, only debug information when we
            //hit a row
            for (int i = 0; i < hit.Length; i++)
            {
                Transform hit_transform = hit[i].transform; //Get the transform

                if (hit_transform.tag == "column")
                {
                    //First we extract the row of the column we hit
                    Transform rowobject = hit_transform.parent;

                    //Check to make sure the row is the layer we are currently on
                    if (rowobject.IsChildOf(layer_object.transform))
                    {
                        //Output the name
                        string nameofrow = hit_transform.parent.name;
                        int row_index = layer.FindChildIndex(nameofrow);
                        int col_index = layer.FindChildIndexOfRow(hit_transform.name, hit_transform.parent);
                        //Debug.Log(" [ row ] : [ " + row_index + " ] " + " [ col ] : [ " + col_index + " ] ");

                        //layer.hoverTransform = hit_transform;
                        //Swap row and col around to match grid  ( y : bottom to top, x: right to left )
                        /*
                            * [1,0][1,1]
                            * [0,0][0,1]
                            * 
                            * */

                        //Set our hover index
                        grid.hoverIndex = new Vector2(row_index, col_index);

                    }
                }

            }
        }
        else
        {
            //We have no hover items
            grid.hoverIndex = new Vector2(-1, -1);//Not hovering over any object
        }
    }
	
	void DrawingControls(Event e)
	{
		//This function is used to control the drawing of tiles on the screen.
		if(e.isMouse)
		{
			//Aslong as the event is a mouse key
			//Check what button we pressed
			if(e.button == 0) //Check left click
			{
				if(e.type == EventType.MouseDown)
				{
					//Draw tiles as long as the mouse has not been been released
					leftclickdown = true; //Set to true to simulate holding down the mouse, and set to false once mouse released
				}
				else if(e.type == EventType.MouseUp)
				{
					leftclickdown = false;
				}
				switch(toolset)
				{
					case Global.ToolSet.BRUSH:
						if(leftclickdown)
						{
							DrawTile(grid.hoverIndex);
						}
					break;
					case Global.ToolSet.ERASE:
						if(leftclickdown)
						{
							//Allow undo object deletion, to recreate a deleted object
		                    Undo.IncrementCurrentEventIndex(); //Allows one by one selection
		                    Undo.RegisterSceneUndo("Delete Selected Objects"); //Register undo for deletion
		
		                    //Delete a tile we are look at
		                    DeleteTile(grid.hoverIndex);
						}
					break;
					case Global.ToolSet.FILL:
						//Fill the layer with blocks, if we click the mouse button
						if(e.type == EventType.MouseDown)
						{
							//Fill the layer
							FillLayer(grid.maximumrows, grid.maximumcolumns);
						}
					break;
                    case Global.ToolSet.EMPTY:
                        //Clear the whole layer of objects
                        if (e.type == EventType.MouseDown)
                        {
                            //Clear layer
                            ClearLayer(grid.maximumrows, grid.maximumcolumns);
                        }
                    break;
                    case Global.ToolSet.DUPLICATE:
                        //Duplicate the layer
                        if (e.type == EventType.MouseDown)
                        {
                            DuplicateLayer(grid.CurrentLayer, GridHandler.CreationDirection.UP);
                        }
                    break;
				}	
			}
			
			//Check for right click
			if(e.button == 1)
			{
				if(e.type == EventType.MouseDown)
				{
					rightclickdown = true;
				}
				else if(e.type == EventType.MouseUp)
				{
					rightclickdown = false;
				}
				
				if(rightclickdown)
				{
					//Allow undo object deletion, to recreate a deleted object
                    
                    //Undo.RegisterSceneUndo("Delete Selected Objects"); //Register undo for deletion

                    //Delete a tile we are look at
                    DeleteTile(grid.hoverIndex);
				}
			}
		}
		
	}
	
	void FillLayer(int row_size, int col_size)
	{
		//Filling layer
		//Debug.Log ("Filling layer: " + row_size.ToString() + " x " + col_size.ToString ());
		for(int row = 0; row < row_size; row++)
		{
			for(int col = 0; col < col_size; col++)
			{
				//Debug.Log ("Creating tile at : " + row + " - " + col);
				//Draw tiles to fill the whole layer
				FillTiles(new Vector2(col, row));
			}
		}
	}


    void ClearLayer(int row_size, int col_size)
    {
        //Register undo scene
        Undo.RegisterSceneUndo("Clear Layer");

        //Clearing layer
        for (int row = 0; row < row_size; row++)
        {
            for (int col = 0; col < col_size; col++)
            {
                //Clear the tiles in the whole layer
                //Get the column at the row index and col index
                GameObject go = grid.GetColumnToBuildBlockAt(new Vector2((float)col, (float)row));

                //Once we have the column we have to delete the tile that is in that column
                if (go)
                {
                    //If have a column, grab the block object in there and delete
                    if (go.transform.childCount > 0) //Aslong as we have children
                    {
                        GameObject block = go.transform.GetChild(0).gameObject; //Always the first index, only one block is allowed ( for now );
                        if (block) //If we have a block destroy it.
                        {
                            DestroyImmediate(block);
                        }
                    }

                }
            }
        }
    }

    void FillTiles(Vector2 position_index)
	{
		//This script takes the position of the block and creates and becomes a child of the column we are at ----- //
        GameObject obj; //Reference to our new object

        //Create the prefab at our column position and parent them together
        GameObject column = grid.GetColumnToBuildBlockAt(position_index); //Get column

        if (column)
        {
            //Only create a block if there is space to create one
            if (column.transform.childCount <= 0) //Aslong as we have no children in there
            {
                //Undo Action
                Undo.IncrementCurrentEventIndex();
                //Create the prefab
                obj = (GameObject)PrefabUtility.InstantiatePrefab(grid.tile);

                //Get Depth
                float depth = grid.CurrentLayer * grid.layerspacing;

                if (column) //If the column actually comes back.
                {
                    //Attach the block to the column we are looking at
                    obj.transform.parent = column.transform;
                    obj.transform.position = Vector3.zero; //Zero it out.
                    obj.transform.localPosition = Vector3.zero;
                }
                else
                {
                    //We have a missing column
                    Debug.LogError("Missing column at hover index : " + position_index.ToString(), this);
                }

                //Register the undo action 
                Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);
            }
        }
	}
	
    void DrawTile(Vector2 position_index)
    {
        //This script takes the position of the block and creates and becomes a child of the column we are at ----- //
        GameObject obj; //Reference to our new object

        //Create the prefab at our column position and parent them together
        GameObject column = grid.GetColumnToBuildBlock(); //Get column

        if (column)
        {
            //Only create a block if there is space to create one
            if (column.transform.childCount <= 0) //Aslong as we have no children in there
            {
                //Undo Action
                Undo.IncrementCurrentEventIndex();
                //Create the prefab
                obj = (GameObject)PrefabUtility.InstantiatePrefab(grid.tile);

                //Get Depth
                float depth = grid.CurrentLayer * grid.layerspacing;

                if (column) //If the column actually comes back.
                {
                    //Attach the block to the column we are looking at
                    obj.transform.parent = column.transform;
                    obj.transform.position = Vector3.zero; //Zero it out.
                    obj.transform.localPosition = Vector3.zero;
                }
                else
                {
                    //We have a missing column
                    Debug.LogError("Missing column at hover index : " + grid.hoverIndex.ToString(), this);
                }

                //Register the undo action 
                Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);
            }
        }


    }

    public void DuplicateLayer(int currentlayer, GridHandler.CreationDirection direction)
    {
        //This script takes a layer and duplicates it above or below itself depending on what direction you choose
        //if there is already a layer that exists it will clear the layer and then duplicate the current layer
        
        /* For now the layer only get's duplicated upwards */

        if (grid)
        {
            //Duplicate the current layer
            //Get gameobject from current layer
            GameObject currentlayerobject = grid.GetChildGameObject(currentlayer);
            GameObject newlayer = null;

            if (currentlayerobject)
            {
                //Duplicate layer above current layer
                newlayer = grid.DuplicateLayer(currentlayerobject, GridHandler.CreationDirection.UP);
            }


            if (newlayer)
            {
                //If we have successfully created a new layer
                //We need to change the heirachy of the transform array, swap the last position, with the one before it.

                //We go through the list of children and organize the order in the list
                List<Transform> childlist = grid.ReturnListOfLayers(grid_object.transform);

                //Check if we need to.
                if (childlist.Count > 0)
                {
                    //Grab the last child
                    int indexofnewlayer = -1; //Nothing to start with
                    Transform newlayertransform = newlayer.transform;

                    for (int i = 0; i < childlist.Count; i++)
                    {
                        //Once we have our layer index we can then swap things around
                        if (childlist[i] == newlayertransform)
                        {
                            indexofnewlayer = i;
                        }
                    }

                    //We our going to cycle infront of the index of the layer we are currently at
                    //and swap the new layers index while we go along
                    for (int current = indexofnewlayer; current < childlist.Count; current++)
                    {
                        int next = current + 1;
                        if (next < childlist.Count)
                        {
                            Transform nextlayer = childlist[next];

                            //Swap around
                            //child list
                            childlist[next] = newlayertransform;
                            childlist[current] = nextlayer;
                        }
                       
                    }

                    //After we have rearranged the list we pass it back to the grid to work with
                    grid.SetListOfGameObjects(childlist);
                }
                else
                {
                    Debug.Log("No children to change order in list");
                }

            }
            else
            {
                Debug.Log("Null object passed back");
            }
            
        }

    }

    private void DeleteTile(Vector2 vector2)
    {
        //Get th column object we are looking at
        GameObject column = grid.GetColumnToBuildBlock();

        if (column)
        {
            //If have a column, grab the block object in there and delete
            if (column.transform.childCount > 0) //Aslong as we have children
            {
                GameObject block = column.transform.GetChild(0).gameObject; //Always the first index, only one block is allowed ( for now );
                if (block) //If we have a block destroy it.
                {
                    //Undo.IncrementCurrentEventIndex();
                    Undo.RegisterSceneUndo("Delete Selected Objects"); //Register undo for deletion

                    DestroyImmediate(block);
                }
            }

        }
        else
        {
            Debug.Log("No tile to delete at hover index:" + grid.hoverIndex.ToString(), this);
        }
    }

    Object[] LoadBlocksAtPath(string path)
    {
        //This script loads the objec
        Object[] blocklist = Resources.LoadAll(path);
        return blocklist;
    }

    public void MakeLevel(string name, bool clipobjects, bool keepfunctionality, bool clipblock = false)
    {
        //Create the level
        GameObject level = grid.CreateLevel(name, clipobjects, keepfunctionality, clipblock);

        //Get the script
        ObjectHandler script = level.AddComponent<ObjectHandler>();

        if (script)
        {
            script.SetDimensions(new Vector3(grid.maximumcolumns, grid.maximumrows, grid.maxlayers));
        }

        //Create prefab from level
        //PrefabUtility.CreatePrefab("Resources/" + name + ".prefab", level, ReplacePrefabOptions.Default);
    }

    #endregion

}
