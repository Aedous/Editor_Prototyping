using UnityEngine;
using UnityEditor;
using System.Collections;

//This makes a custom editor for the inspector and 
//targets it to the Star Script, also allow multiple object editing
[CanEditMultipleObjects, CustomEditor(typeof(Star))]
public class StarInspector : Editor
{
    #region inspector variables
    #endregion

    #region public variables
    #endregion

    #region private variables
    //Snap point for scene editing
    private static Vector3 pointSnap = Vector3.one * 0.1f;

    //Static variables for point gui settings
    private static GUIContent
        insertContent = new GUIContent("+", "duplicate this point"),
        deleteContent = new GUIContent("-", "delete this point"),
        pointContent = GUIContent.none,
        teleportContent = new GUIContent("T"),
        activeContent = new GUIContent("X"),
        swapContent = new GUIContent("S");
    private static GUILayoutOption 
        buttonWidth = GUILayout.MaxWidth(20f),
        colorWidth = GUILayout.MaxWidth(50f);

    private SerializedObject star;
    private SerializedProperty points, frequency, centerColor;
    private int teleportingElement, swappingElement, selectedElement; //Keep track of what we are dragging
    
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
        //Serialize the gameobject this editor is attached to
        star = new SerializedObject(targets); //use targets because there may be more than one object selected

        //Get the variables with a serialized object
        points = star.FindProperty("points");
        frequency = star.FindProperty("frequency");
        centerColor = star.FindProperty("centerColor");

        //Set the drag element to -1 (no dragged object), and tooltip
        teleportingElement = -1;
        teleportContent.tooltip = "Start teleporting this point";
    }

    //Override our inspector GUI
    public override void OnInspectorGUI()
    {
        //Update the star to get up to date changes made.
        star.Update();

        //Create the layout
        GUILayout.Label("Points");
        //Draw the individual points manually
        for (int i = 0; i < points.arraySize; i++)
        {
            SerializedProperty
                point = points.GetArrayElementAtIndex(i),
                offset = point.FindPropertyRelative("offset");

            //Check to see if an offset exists before allowing 
            //tweak of values
            if (offset == null)
            {
                break;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(offset, pointContent);
            EditorGUILayout.PropertyField(point.FindPropertyRelative("color"), pointContent, colorWidth);

            //Create teleport buttons (drag and drop)
            //If we are the selected teleporting element
            if (teleportingElement == i)
            {
                //Create a GUI Button to show it has been selected
                if (GUILayout.Button(activeContent, EditorStyles.miniButtonLeft, buttonWidth))
                {
                    //Click back on cancel button removes teleportingElement
                    teleportingElement = -1;
                    teleportContent.tooltip = "start teleporting this point";
                }
            }
            else
            {
                //Draw normal gui
                if (GUILayout.Button(teleportContent, EditorStyles.miniButtonLeft, buttonWidth))
                {
                    if (teleportingElement >= 0)
                    {
                        points.MoveArrayElement(teleportingElement, i);
                        teleportingElement = -1;
                        teleportContent.tooltip = "start teleporting this point";
                    }
                    else
                    {
                        teleportingElement = i;
                        teleportContent.tooltip = "teleport here";
                    }
                }
            }


            //Create buttons to add points and delete points
            if (GUILayout.Button(insertContent, EditorStyles.miniButtonMid, buttonWidth))
            {
                points.InsertArrayElementAtIndex(i);
            }
            if (GUILayout.Button(deleteContent, EditorStyles.miniButtonRight, buttonWidth))
            {
                points.DeleteArrayElementAtIndex(i);
            }

            EditorGUILayout.EndHorizontal();
        }

        if (teleportingElement >= 0)
        {
            GUILayout.Label("teleporting point " + teleportingElement);
        }

        //Property fields for points
        EditorGUILayout.PropertyField(frequency);
        EditorGUILayout.PropertyField(centerColor);

        //Apply properties to star if changed by inspector
        if (
            star.ApplyModifiedProperties() ||
            (
                Event.current.type == EventType.ValidateCommand &&
                Event.current.commandName == "UndoRedoPerformed")
            )
        {
            //Update each star selected
            foreach (Star s in targets)
            {
                //Check to make sure it is not a prefab when updating
                if (PrefabUtility.GetPrefabType(s) != PrefabType.Prefab)
                {
                    //Cast it back to a star and update it's methods
                    s.UpdateStar();
                }
            }
        }
    }

    
    //Allows tweaking in scene view
    void OnSceneGUI()
    {
        //Cannot use serialized property on scene gui
        Star star = (Star)target;
        Transform starTransform = star.transform;

        //Allow undo
        Undo.SetSnapshotTarget(star, "Move Star Point");

        float angle = -360f / (star.frequency * star.points.Length);

        //Loop through each point
        for (int i = 0; i < star.points.Length; i++)
        {
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle * i);
            Vector3 oldPoint = starTransform.TransformPoint(rotation * star.points[i].offset),
                    newPoint = Handles.FreeMoveHandle
                                            (
                                            oldPoint,
                                            Quaternion.identity,
                                            0.04f,
                                            pointSnap,
                                            Handles.DotCap
                                            );

            //Check to see if the new position is different from old
            //then update star
            if (oldPoint != newPoint)
            {
                //Convert the offset back to local space rather than world space
                star.points[i].offset =
                    Quaternion.Inverse(rotation) * starTransform.InverseTransformPoint(newPoint);
                star.UpdateStar();

            }
        }
    }
    

    #endregion

    #region Class Methods
    #endregion
}
