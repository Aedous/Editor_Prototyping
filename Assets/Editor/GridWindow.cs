using UnityEngine;
using UnityEditor;
using System.Collections;

//This makes a custom editor for editor window
public class GridWindow : EditorWindow
{
    #region inspector variables
    #endregion

    #region public variables
    #endregion

    #region private variables
    Grid grid;
    #endregion

    #region Unity Methods
    void OnGUI()
    {
        grid.color = EditorGUILayout.ColorField(grid.color, GUILayout.Width(200));
    }
    #endregion

    #region Class Methods
    public void Init()
    {
        //Attach the grid script into reference
        grid = (Grid)FindObjectOfType(typeof(Grid));
    }
    #endregion
}
