using UnityEngine;
using System;
using System.Collections;

/*
 * This class makes a star on the screen
 * */

//A mesh needs a mesh filter and renderer and allow the behaviour
//to run in the edit mode
[ExecuteInEditMode, RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Star : MonoBehaviour
{
    //Children classes
    [Serializable]
    public class Point
    {
        //Color and offset of points
        public Color color;
        public Vector3 offset;
    }

    #region inspector variables
    public Point[] points;
    public int frequency = 1;
    public Color centerColor;
    #endregion

    #region public variables
    #endregion

    #region private variables
    private Mesh mesh; //We use this to create a triangle fan mesh.
    private Vector3[] vertices; //Create the vertices
    private Color[] colors; //Color for the vertices
    public int[] triangles;
    #endregion

    #region Unity Methods
    // Use this for initialization
    public void UpdateStar()
    {
        //Create mesh and assign a mesh filter to it.
        if (mesh == null)
        {
            mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;
            mesh.name = "Star Mesh";

            //Stop unity from saving the mesh as we create it dynamically
            mesh.hideFlags = HideFlags.HideAndDontSave;
        }


        //Check to make sure frequency is never negative
        if (frequency < 1)
        {
            frequency = 1;
        }

        //Check to see if points are also larger than 0
        if (points == null || points.Length == 0)
        {
            //Set all array points to vector3.up
            points = new Point[] { new Point() };
        }

        int numberOfPoints = frequency * points.Length;

        if (vertices == null || vertices.Length != numberOfPoints + 1)
        {
            //Create the vertices array
            vertices = new Vector3[numberOfPoints + 1];

            //Create the colors
            colors = new Color[numberOfPoints + 1];

            //Create triangles ( set of 3's)
            triangles = new int[numberOfPoints * 3];
        }

        //The correct angle inverted
        float angle = -360f / numberOfPoints;

        //Set the center color
        colors[0] = centerColor;
        for (int iF = 0, v = 1, t = 1; iF < frequency; iF++)
        {
            for (int iP = 0; iP < points.Length; iP += 1, v++, t += 3)
            {
                vertices[v] = Quaternion.Euler(0f, 0f, angle * (v - 1)) * points[iP].offset;
                colors[v] = points[iP].color;
                triangles[t] = v;
                triangles[t + 1] = v + 1;
            }
        }
        triangles[triangles.Length - 1] = 1;

        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.triangles = triangles;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnEnable()
    {
        UpdateStar();
    }

    void OnDisable()
    {
        //Clean up the mesh as we are in edit mode
        if (Application.isEditor)
        {
            //Clean the mesh up so no reports of missing mesh
            GetComponent<MeshFilter>().mesh = null;
            DestroyImmediate(mesh);//Destroy the mesh
        }
    }

    //Reset star with editor button
    void Reset()
    {
        UpdateStar();
    }

    #endregion

    #region Class Methods
    #endregion
}
