using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

//This makes a custom editor for the inspector and 
//targets it to the Grid Script
/*[CustomEditor(typeof(GridHandler))]*/
public class GridHandlerEditor : Editor
{
    #region inspector variables
    #endregion

    #region public variables
    #endregion

    #region private variables
    //GUI Settings----------------------------------------//
    private static GUIContent
        drawContent = new GUIContent("Start Drawing!", "Turn on drawing mode to create tiles"),
        layerupContent = new GUIContent("^", "Move up a layer"),
        layerdownContent = new GUIContent("v", "Move down a layer"),
        layeraddContent = new GUIContent("+", "Add a new layer"),
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

    private GridHandler grid;
    private GameObject selectedObject, teleportObject = null;
    private string nameoflevel; //Export name for prefab
    private bool drawtiles, keepfunctionality, clipemptyobjects; //Booleans to control behaviour of editor
    private int teleportIndex = -1;
    private Object[] blockcollection; //Used to show what kind of blocks we can use
    private Vector2 scrollPosition; //Scroll position for block list
    #endregion

    #region Unity Methods
    // Use this for initialization
    void Start()
    {
        //Set our draw tile to false to allow drawing tiles
        drawtiles = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    //Override the inspector gui settings to create your own.
    public override void OnInspectorGUI()
    {
        GUILayout.BeginVertical();
        //Settings for the grid handler ---------------------//
        GUILayout.Label("-Settings-");
        GUILayout.BeginHorizontal();
        grid.AutomaticUpdates = GUILayout.Toggle(grid.AutomaticUpdates, "Automatic Upates", GUILayout.Width(100f));
        GUILayout.EndHorizontal();

        //Create Width and Height of Whole Grid fields
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
        if(GUILayout.Button(layerupContent, EditorStyles.miniButtonMid, layerbuttonWidth))
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
            }
            else //If we click in false mode
            {
                //If we are not in draw style mode
                drawContent.text = "Stop Drawing!";
                drawtiles = true;
            }
        }


        //----Creating the list of objects you can cycle through and create
       // GUILayout.BeginArea(new Rect(0, 0, 100, 50));

        scrollPosition = GUILayout.BeginScrollView(scrollPosition,false,false,GUILayout.Width(250f), GUILayout.Height(75f));
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

                    if (GUILayout.Button(block_object.name,GUILayout.Width(50f), GUILayout.Height(50f)))
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
            GUILayout.BeginHorizontal();
            GUILayout.Label("Name of Level:");
            grid.NameOfLevel = GUILayout.TextField(grid.NameOfLevel, 50);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            grid.KeepFunctionality = EditorGUILayout.Toggle(grid.KeepFunctionality, "Keep functionality");
            grid.Clip = EditorGUILayout.Toggle(grid.Clip, "Clip objects");
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Create Level", GUILayout.Width(100f)))
            {
                //Create the level
                MakeLevel(grid.NameOfLevel, grid.KeepFunctionality, grid.Clip);
            }
        }

        GUILayout.EndVertical();
        


        //Repaint the scene
        SceneView.RepaintAll();
    }

    public void OnEnable()
    {
        //Get the reference for the grid class
        grid = (GridHandler)target;

        if (PrefabUtility.GetPrefabType(grid) != PrefabType.Prefab)
        {
            //Create the layers if they haven't been created yet
            if (grid.AutomaticUpdates)
            {
                grid.CreateAndUpdateLayers();
            }

            //Get the resources
            blockcollection = LoadBlocksAtPath("Blocks");


            //Reset teleport index
            teleportIndex = -1;
            teleportObject = null;

            //Allow key input and mouse input
            SceneView.onSceneGUIDelegate += GridUpdate;
        }


    }

    public void OnDisable()
    {
        //If we come off the object, stop our drawing option
        drawtiles = false;
        drawContent.text = "Start Drawing!";
        
        //Remove Grid Update delegate to stop stacking
        SceneView.onSceneGUIDelegate -= GridUpdate;
    }

    public void OnSceneGUI()
    {
        //Draw the guide for the grid
        //Draw lines
        if (grid)
        {
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

        //Stop unity from deselecting
        if (drawtiles)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            //If the mouse clicks on an object store it in the selectedObject

            if (selectedObject)
            {
                //If we have a selected object
                //Do something to show this works for now.
            }
        }
    }

    #endregion

    #region Class Methods
    Object[] LoadBlocksAtPath(string path)
    {
        //This script loads the objec
        Object[] blocklist = Resources.LoadAll(path);
        return blocklist;
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

            }//End of checking if we are still in bounds on our current layer
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

    public void MouseInput(Event e)
    {
        //Cannot draw while using key input and mouse ( unity camera controls
        if (!e.isKey)
        {
            if ((e.isMouse && e.button == 0))
            {
                if (e.type == EventType.MouseDown)
                {
                    if (drawtiles) //If we are in draw tiles mode
                    {
                        DrawTile(grid.hoverIndex); //Old draw tiles method, that only works with the camera facing flat
                    }
                    else
                    {
                        //If we are not in draw tile mode, we should be able to click on other objects
                    }
                }
            }
            else if (e.isMouse && e.button == 1) //Right click
            {
                if (drawtiles)
                {
                    //Allow undo object deletion, to recreate a deleted object
                    Undo.IncrementCurrentEventIndex(); //Allows one by one selection
                    Undo.RegisterSceneUndo("Delete Selected Objects"); //Register undo for deletion

                    //Delete a tile we are look at
                    DeleteTile(grid.hoverIndex);
                }
                else
                {
                    //If we are not in draw tile mode, normal right click method
                }
            }
        }
    }

    public void DrawGuide(int blocksize, int rowsize, int columnsize, Color gridcolor)
    {
        //Debug.Log("HELO");
        //Apply gizmo color
        Handles.color = gridcolor;

        //Work out position to dra
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

        //Repaint view
        HandleUtility.Repaint();
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
                    DestroyImmediate(block);
                }
            }

        }
        else
        {
            Debug.Log("No tile to delete at hover index:" + grid.hoverIndex.ToString(), this);
        }
    }

    private void MakeLevel(string name = "Level", bool functionality = true, bool clip = false)
    {
        //This creates a duplicate of this object with the layer's, rows, and columns.
        //Can export with functionality or without.

        //Create the folder
        if(!Directory.Exists(Application.dataPath + "/Level"))
        {
            //Create the folder
            Directory.CreateDirectory(Application.dataPath + "/Level");
        }

        if (functionality)
        {
            //Make a copy of the current level maker we have as a prefab
            PrefabUtility.CreatePrefab("Assets/Level/" + name + ".prefab", grid.gameObject);

            if (clip)
            {
                for (int i = 0; i < grid.transform.childCount; i++)
                {
                    Transform layer = grid.transform.GetChild(i);

                    if (clip)
                    {
                        ClipObjects(layer);
                    }
                }
            }
        }
        else
        {
            //Make a copy however remove all the scripts that take care of editing
            Object grid_prefab = PrefabUtility.GetPrefabObject(grid.gameObject);
            //GameObject gridhandler_copy = PrefabUtility.InstantiatePrefab(grid_prefab) as GameObject;
            GameObject gridhandler_copy = Instantiate(grid.gameObject) as GameObject;

            //Take all functionality out of layers
            if (gridhandler_copy)
            {
                
                for (int i = 0; i < gridhandler_copy.transform.childCount; i++)
                {
                    Transform layer = gridhandler_copy.transform.GetChild(i);

                    if (clip)
                    {
                        ClipObjects(layer);
                    }

                    DestroyImmediate(layer.GetComponent<LayerHandler>());
                }


                //Take all the functionality out
                DestroyImmediate(gridhandler_copy.GetComponent<GridHandler>());

                //After clearing the scripts create a prefab of the object
                //Create the prefab.
                Object prefab = PrefabUtility.CreatePrefab("Assets/Level/" + name + ".prefab", gridhandler_copy);
                DestroyImmediate(gridhandler_copy); //Destroy the copy
                
            }
            else
            {
                Debug.Log("Could not create clone of game object for making a level without functionality --- ");
            }

            
        }

        //Refresh the asset screen
        AssetDatabase.Refresh();
    }


    //If successful return true
    void ClipObjects(Transform layer)
    {
        //If we want to clip, the we remove all the col's that do not have an object in them
        LayerHandler script = layer.GetComponent<LayerHandler>();

        //Check script
        if (script)
        {
            //Get the rows and columns
            for (int x = 0; x < layer.childCount; x++)
            {
                //Get the rows and get the amount of children they have
                Transform rowobject = script.GetRowObject(x).transform;

                if (rowobject)
                {
                    int rowcount = rowobject.transform.childCount;

                    for (int y = 0; y < rowcount; y++) //Cycle through the childre
                    {
                        //Get the columns
                        int currentcount = rowobject.transform.childCount;

                        if (y < currentcount) //Aslong as we are still in bounds
                        {
                            Transform colobject = rowobject.GetChild(y);
                            if (colobject)
                            {
                                //Remove the box collider from the component
                                DestroyImmediate(colobject.GetComponent<BoxCollider>());

                                int childrencount = colobject.childCount; //number of children

                                //Check to see which column has no children, we do this to remove it 
                                //from the heirarchy as it is not needed anymore. If we destroy it
                                //the list changes so we need t o update our 'y' increment to stop it from
                                //missing objects 
                                //0 1 0 1 0 -> 1 1 (it is a list so we correct our index )
                                if (childrencount <= 0)
                                {
                                    //Remove from row
                                    DestroyImmediate(colobject.gameObject);

                                    //Reset index
                                    if (y != rowcount) //Aslong as we are not equal to the last object
                                    {
                                        y--; //Go back one
                                    }
                                }

                                
                            }
                        }
                    }

                    //Remove the box collider from the layer
                    DestroyImmediate(rowobject.GetComponent<BoxCollider>());
                }
            }
        }

    }
    #endregion
}
