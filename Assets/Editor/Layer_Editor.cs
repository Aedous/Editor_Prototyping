using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

//This makes a custom editor for the inspector and 
//targets it to the Grid Script

[CustomEditor(typeof(LayerHandler))]
public class Layer_Editor : Editor
{
    #region inspector variables
    #endregion

    #region public variables
    #endregion

    #region private variables
    //Gui Options
    private static GUILayoutOption
      buttonWidth = GUILayout.MaxWidth(255f),
      labelWidth = GUILayout.MaxWidth(255f),
      inputWidth = GUILayout.MaxWidth(255f),
      mediumWidth = GUILayout.MaxWidth(125f),
      smallbuttonWidth = GUILayout.MaxWidth(50f);

    //----------------------------------//

    private LayerHandler layer; //layer reference
    private bool drawguides; //Draw the block guide
    private bool showRowDetails; //show row details for each layer
    #endregion

    #region Unity Methods
    // Use this for initialization
    void Start()
    {
        drawguides = true; //Draw guides set to true
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnEnable()
    {
        //Get the reference for the grid class
        
        layer = (LayerHandler)target;

        if (PrefabUtility.GetPrefabType(layer) != PrefabType.Prefab)
        {
            //Allow key input and mouse input
            SceneView.onSceneGUIDelegate += LayerUpdate;
        }
    }

    public void OnDisable()
    {

        //Remove from scene delegate
        SceneView.onSceneGUIDelegate -= LayerUpdate;
    }

    public void OnSceneGUI()
    {
        //Draw lines
        if (layer)
        {
            //we allow switching on and off of grid
            if (layer.drawguide)
            {
                //Apply gizmo color
                Gizmos.color = layer.guidecolor;

                //Get Transform position
                Vector3 position = layer.transform.position;

                //Draw Square by Square
                /*
                 * start with the row size, and draw a square from 0 and increment by blocksize
                 * when at row move along the column according to size and shift to the right by blocksize
                 * repeat for each row.
                 * 
                 */
                

                DrawGuide(layer.blocksize, layer.numberofrows, layer.numberofcolumns, layer.guidecolor);

                //Draw Hover Item aslong as we have one

            }
        }
    }

    public void DrawGuide(int blocksize, int rowsize, int columnsize, Color gridcolor)
    {
        //Debug.Log("HELO");
        //Apply gizmo color
        Handles.color = gridcolor;

        //Work out position to draw
        Vector3 blockposition = layer.transform.position;
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

                if (layer.hoverIndex.x == row && layer.hoverIndex.y == col)
                {
                    Color hoverColor = Color.blue;
                    hoverColor.a = 0.3f;
                    Handles.color = hoverColor;
                    //Gizmos.color = hoverColor;
                    //Gizmos.DrawCube(newposition, size);
                    Debug.Log(layer.hoverIndex.ToString());
                    Handles.CubeCap(0, newposition, Quaternion.identity, (float)blocksize);

                    Handles.color = gridcolor;
                }
                else
                {
                    Handles.CubeCap(0, newposition, Quaternion.identity, (float)blocksize);
                }
            }
        }

        //Repaint view
        HandleUtility.Repaint();

        

    }


    public override void OnInspectorGUI()
    {
        //Create Width and Height of Whole Grid fields
        GUILayout.BeginVertical();
        GUILayout.Label("Layer Position: " + layer.currentIndex);

        GUILayout.BeginHorizontal();
        GUILayout.Label(" Block Size ");
        //Change these to just labels, when added to Grid Handler
        layer.blocksize = EditorGUILayout.IntField(layer.blocksize, GUILayout.Width(50));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label(" Row Amount ");
        layer.numberofrows = EditorGUILayout.IntField(layer.numberofrows, GUILayout.Width(50));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label(" Column Amount ");
        layer.numberofcolumns = EditorGUILayout.IntField(layer.numberofcolumns, GUILayout.Width(50));
        GUILayout.EndHorizontal();

        //Horizontal Layout for toggle
        GUILayout.BeginHorizontal();
        GUILayout.Label(" Draw Guide ");
        layer.drawguide = EditorGUILayout.Toggle(layer.drawguide, GUILayout.Width(50f));
        GUILayout.Label(" Guide Color ");
        layer.guidecolor = EditorGUILayout.ColorField(layer.guidecolor, GUILayout.Width(100f));
        GUILayout.EndHorizontal();
        //Finish Horizontal



        //Draw Box Collision settings which are in the rows
        GUILayout.BeginHorizontal();
        GUILayout.Label(" Show Rows ");
        showRowDetails = EditorGUILayout.Toggle(showRowDetails, GUILayout.Width(50f));
         GUILayout.EndHorizontal();
        
        if (showRowDetails)
        {
            //Get a list of the children from the layer which are the rows
            List<GameObject> children = layer.GetAListOfChildren();

            for (int i = 0; i < children.Count; i++)
            {
                //Draw the details of the row with button's
                GUILayout.BeginHorizontal();
                GameObject child = layer.transform.GetChild(i).gameObject;

                //If the game object exists, get the collider and other details we need to show to user
                if (child)
                {
                    BoxCollider collider = child.GetComponent<BoxCollider>();
                    string rowdetail = "[" + i + "]" + " Row : " + 
                                       "[ " + 
                                       collider.size.x + " , " +
                                       collider.size.y + " , " + 
                                       collider.size.z +
                                       " ] ";
                    GUILayout.Label(rowdetail); //Create the label for information about the row

                    //Create a button to delete the row-------------------//
                    if(GUILayout.Button("-", smallbuttonWidth))
                    {
                        //Delete the row
                    }
                    //----------------------------------------------------//
                }
                GUILayout.EndHorizontal();
            }
        }
        
        //Create Update Buttons -- For Prototyping
        if (GUILayout.Button("Update Layer", buttonWidth))
        {
            //Update the layer
            layer.CreateAndUpdateRows();
        }

        GUILayout.EndVertical();


        //Repaint the scene
        SceneView.RepaintAll();
        

    }


    #endregion

    #region Class Methods
    void LayerUpdate(SceneView view)
    {
        //Get the input event
        Event e = Event.current;

        //Get screen mouse coords to world coords
        //makes the bottom left corner (0,0) and top right corner(Camera.current.pixelWidth, Camera.current.pixelHeight)
        //Ray r =  Camera.current.ScreenPointToRay(e.mousePosition);
        Ray r = HandleUtility.GUIPointToWorldRay(new Vector2(e.mousePosition.x, e.mousePosition.y));
        
        RaycastHit[] hit = Physics.RaycastAll(r);
        Vector3 mousePos = r.origin; //Reference the origin to get mouse position

        //Cycle through what we have hit and check if it is a row
        if (hit.Length > 0)
        {
            //If we have hit a bunch of objects, only debug information when we
            //hit a row
            for (int i = 0; i < hit.Length; i++)
            {
                Transform hit_transform = hit[i].transform; //Get the transform

                //Figure out which transform this is
                if (hit_transform.tag == "column")
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


                    layer.hoverIndex = new Vector2(row_index, col_index);
                }
            }
        }
        else
        {
            //We have no hover items
            layer.hoverIndex = new Vector2(-1, -1);//Not hovering over any object
        }


    }
    #endregion
}
