using UnityEngine;
using System.Collections;

public class Grapher3 : MonoBehaviour
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
    public bool absolute;
    public float threshold = 0.5f;
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

        if (absolute)
        {
            for (int i = 0; i < points.Length; i++)
            {
                Color c = points[i].color;
                c.a = f(points[i].position, t) >= threshold ? 1f : 0f;
                points[i].color = c;
            }
        }
        else
        {
            //Create the Y axis particles
            for (int i = 0; i < points.Length; i++)
            {
                //Change color with y value
                Color c = points[i].color;
                c.a = f(points[i].position, t);
                points[i].color = c;
            }
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
        else if (resolution > 14) //Cap at 50 to stop from slowing down
        {
            //My computer can only handle 14 * (x,y,z) points
            resolution = 14;
        }
        currentResolution = resolution;

        //Make a square point array size x, y, and z
        points = new ParticleSystem.Particle[resolution * resolution * resolution];

        //The points in between 0 and 1 along the x axis
        float increment = 1f / (resolution - 1);

        int i = 0; //We are going to use another dimension
        for (int x = 0; x < resolution; x++)
        {
            //loop through z axis
            for (int z = 0; z < resolution; z++)
            {
                //loop through y axis
                for (int y = 0; y < resolution; y++)
                {
                    //Get next point
                    Vector3 p = new Vector3(x, y, z) * increment;

                    //Apply to particle (point)
                    points[i].position = p;
                    points[i].color = new Color(p.x, p.y, p.z);
                    points[i].size = 0.1f;
                    i++; //Move to next index
                }
            }
        }
    }

    //Does not require an object to function that is why it is
    //static
    private static float Linear(Vector3 p, float t)
    {
        return 1f - p.x - p.y - p.z + 0.5f * Mathf.Sin(t);
    }

    private static float Exponential(Vector3 p, float t)
    {
        return 1f - p.x * p.x - p.y * p.y - p.z * p.z + 0.5f * Mathf.Sin(t);
    }

    private static float Parabola(Vector3 p, float t)
    {
        p.x = 2f * p.x - 1f;
        p.z = 2f * p.z - 1f;
        return 1f - p.x * p.x - p.z * p.z + 0.5f * Mathf.Sin(t);
    }

    private static float Sine(Vector3 p, float t)
    {
        float x = Mathf.Sin(2 * Mathf.PI * p.x);
        float y = Mathf.Sin(2 * Mathf.PI * p.y);
        float z = Mathf.Sin(2 * Mathf.PI * p.z + (p.y > 0.5f ? t : -t));
        return x * x * y * y * z * z;
    }

    private static float Ripple(Vector3 p, float t)
    {
        float squareRadius =
            (p.x - 0.5f) * (p.x - 0.5f) +
            (p.y - 0.5f) * (p.y - 0.5f) +
            (p.z - 0.5f) * (p.z - 0.5f);
        return Mathf.Sin(4 * Mathf.PI * squareRadius - 2f * t);
    }

    #endregion
}
