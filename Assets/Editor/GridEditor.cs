using UnityEngine;
using UnityEditor;
using System.Collections;

//This makes a custom editor for the inspector and 
//targets it to the Grid Script
[CustomEditor(typeof(Grid))]
public class GridEditor : Editor
{
    #region inspector variables
    #endregion

    #region public variables
    #endregion

    #region private variables
    //GUI Settings----------------------------------------//
    private static GUIContent
        drawContent = new GUIContent("Start Drawing!", "Turn on drawing mode to create tiles");
    private static GUILayoutOption
        drawbuttonWidth = GUILayout.MaxWidth(100f);

    private Grid grid;
    private GameObject selectedObject;
    private bool drawtiles;
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
        
        //Create width field
        GUILayout.BeginHorizontal();
        GUILayout.Label(" Block Width ");
        grid.width = EditorGUILayout.FloatField(grid.width, GUILayout.Width(50));
        GUILayout.EndHorizontal();

        //Create height field
        GUILayout.BeginHorizontal();
        GUILayout.Label(" Block Height ");
        grid.height = EditorGUILayout.FloatField(grid.height, GUILayout.Width(50));
        GUILayout.EndHorizontal();

        //Create Tile Field
        GUILayout.BeginHorizontal();
        GUILayout.Label(" Grid Object ");
        grid.tile = (GameObject)EditorGUILayout.ObjectField(grid.tile,typeof(GameObject), GUILayout.Width(255));
        GUILayout.EndHorizontal();

        

        //Create Editor Window Button
        if (GUILayout.Button("Open Grid Window", GUILayout.Width(255)))
        {
            //Create window
            GridWindow window = (GridWindow)EditorWindow.GetWindow(typeof(GridWindow));

            //Call the grid window function to initialize
            window.Init();
        }

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


        //Repaint the scene
        SceneView.RepaintAll();
    }

    public void OnEnable()
    {
        //Get the reference for the grid class
        grid = (Grid)target;

        //Allow key input and mouse input
        SceneView.onSceneGUIDelegate += GridUpdate;

        
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
    void GridUpdate(SceneView sceneView)
    {
        //Get the input event
        Event e = Event.current;

        //Get screen mouse coords to world coords
        //makes the bottom left corner (0,0) and top right corner(Camera.current.pixelWidth, Camera.current.pixelHeight)
        Ray r = Camera.current.ScreenPointToRay(new Vector3(e.mousePosition.x, -e.mousePosition.y + Camera.current.pixelHeight));
        Vector3 mousePos = r.origin; //Reference the origin to get mouse position


        //If we press the a mouse button
        if ((e.isMouse && e.button == 0))
        {
            if (e.type == EventType.MouseDown)
            {
                if (drawtiles) //If we are in draw tiles mode
                {
                    //When in this mode, we check if the tile we have clicked on has a block there
                    //if not we can create one, if there is, we can tweak the values of the block
                    //Create a reference for our new object
                    GameObject obj;

                    //Get the prefab of the game object
                    Object prefab = PrefabUtility.GetPrefabParent((Object)grid.tile);

                    //Check to see if the prefab exists
                    //Instatiate it as long as we are in the scene view
                    if (prefab)
                    {
                        //This code allows the undo action to affect an object
                        //one by one.
                        Undo.IncrementCurrentEventIndex();

                        //Create prefab
                        obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                        //Align it to the grid
                        Vector3 aligned = new Vector3(Mathf.Floor(mousePos.x / grid.width) * grid.width + grid.width / 2.0f,
                                                       Mathf.Floor(mousePos.y / grid.height) * grid.height + grid.height / 2.0f, 0.0f);

                        //Create the prefab at mouse position
                        obj.transform.position = aligned;

                        //Register the undo action 
                        Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);
                    }
                }
                else
                {
                    //If we are not in draw tile mode, we should be able to click on other objects
                }
            }
        }
        else if(e.isMouse && e.button == 1) //Right click
        {
            if (drawtiles)
            {
                //Allow undo object deletion, to recreate a deleted object
                Undo.IncrementCurrentEventIndex(); //Allows one by one selection
                Undo.RegisterSceneUndo("Delete Selected Objects"); //Register undo for deletion
                foreach (GameObject obj in Selection.gameObjects)
                {
                    DestroyImmediate(obj);
                }
            }
            else
            {
                //If we are not in draw tile mode, normal right click method
            }
        }
    }

    #endregion
}
