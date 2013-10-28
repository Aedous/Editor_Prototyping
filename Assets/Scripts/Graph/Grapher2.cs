using UnityEngine;
using System.Collections;

public class Grapher2 : MonoBehaviour
{
    #region Enums
    public enum FunctionOption
    {
        Linear,
        Exponential,
        Parabola,
        Sine,
        Ripple
    }
    #endregion

    #region inspector variables
    public FunctionOption function;
    public int resolution = 10; //Set the resolution of the graph

    #endregion

    
    #region public variables
    #endregion

    #region private variables
    //DELEGATES
    private delegate float FunctionDelegate(Vector3 p, float x);
    private static FunctionDelegate[] functionDelegates = {
                                                              Linear,
                                                              Exponential,
                                                              Parabola,
                                                              Sine,
                                                              Ripple
                                                          };
    //---------------------------//
    private int currentResolution;
    private ParticleSystem.Particle[] points;
    
    #endregion

    #region Unity Methods
    // Use this for initialization
    void Start()
    {
        //Create the particle points
        CreatePoints();
    }

    // Update is called once per frame
    void Update()
    {
        //If we change the resolution in real time update the particles

        if (currentResolution != resolution)
        {
            CreatePoints();
        }

        //USE DELEGATE
        FunctionDelegate f = functionDelegates[(int)function];
        float t = Time.timeSinceLevelLoad;
        //Create the Y axis particles
        for (int i = 0; i < points.Length; i++)
        {
            Vector3 p = points[i].position;
            p.y = f(p, t); //Set the Y position to that of X
            points[i].position = p; //Remap points

            //Change color with y value
            Color c = points[i].color;
            c.g = p.y;
            points[i].color = c;
        }
        //We need to pass our particles in that we have just created.
        //with the size of the particles. (resolution '10')
        particleSystem.SetParticles(points, points.Length);
    }

    #endregion

    #region Class Methods
    public void CreatePoints()
    {
        //Cap the resolution so it never causes any errors
        if (resolution < 2)
        {
            resolution = 2;
        }
        else if (resolution > 50) //Cap at 50 to stop from slowing down
        {
            resolution = 50;
        }
        currentResolution = resolution;

        //Make a square point array size
        points = new ParticleSystem.Particle[resolution * resolution];

        //The points in between 0 and 1 along the x axis
        float increment = 1f / (resolution - 1);

        int i = 0; //We are going to use another dimension
        for (int x = 0; x < resolution; x++)
        {
            for (int z = 0; z < resolution; z++)
            {
                Vector3 p = new Vector3(x * increment, 0f, z * increment);
                //Use the value of x to set position and color
                points[i].position = p;
                points[i].color = new Color(p.x, 0f, p.z);
                points[i].size = 0.1f;
                i++; //Move to next index
            }
        }
    }

    //Does not require an object to function that is why it is
    //static
    private static float Linear(Vector3 p,float t)
    {
        return p.x;
    }

    private static float Exponential(Vector3 p, float t)
    {
        return p.x * p.x;
    }

    private static float Parabola(Vector3 p, float t)
    {
        p.x = (2f * p.x) - 1f;
        p.z = (2f * p.z) - 1;
        return 1f - p.x * p.x * p.z * p.z;
    }

    private static float Sine(Vector3 p, float t)
    {
        return 0.50f +
                0.25f * Mathf.Sin(4 * Mathf.PI * p.x + 4 * t) * Mathf.Sin(2 * Mathf.PI * p.z + t) +
                0.10f * Mathf.Cos(3 * Mathf.PI * p.x + 5 * t) * Mathf.Cos(5 * Mathf.PI * p.z + 3 * t) +
                0.15f * Mathf.Sin(Mathf.PI * p.x + 0.6f * t);
    }

    private static float Ripple(Vector3 p, float t)
    {
        float squareRadius = (p.x - 0.5f) * (p.x - 0.5f) + (p.z - 0.5f) * (p.z - 0.5f);
        return 0.5f + Mathf.Sin(15 * Mathf.PI * squareRadius - 2f * t) / (2f + 100f * squareRadius);
    }

    #endregion
}
