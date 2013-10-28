using UnityEngine;
using System.Collections;

public class Grapher : MonoBehaviour
{
    #region Enums
    public enum FunctionOption
    {
        Linear,
        Exponential,
        Parabola,
        Sine
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
    private delegate float FunctionDelegate(float x);
    private static FunctionDelegate[] functionDelegates = {
                                                              Linear,
                                                              Exponential,
                                                              Parabola,
                                                              Sine
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
        //Create the Y axis particles
        for (int i = 0; i < resolution; i++)
        {
            Vector3 p = points[i].position;
            p.y = f(p.x); //Set the Y position to that of X
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

        currentResolution = resolution;
        points = new ParticleSystem.Particle[resolution];

        //The points in between 0 and 1 along the x axis
        float increment = 1f / (resolution - 1);

        for (int i = 0; i < resolution; i++)
        {
            //multiply i to increment to get next value
            float x = i * increment;

            //Use the value of x to set position and color
            points[i].position = new Vector3(x, 0f, 0f);
            points[i].color = new Color(x, 0f, 0f);
            points[i].size = 0.1f;
        }
    }

    //Does not require an object to function that is why it is
    //static
    private static float Linear(float x)
    {
        return x;
    }

    private static float Exponential(float x)
    {
        return x * x;
    }

    private static float Parabola(float x)
    {
        x = (2f * x) - 1f;
        return x * x;
    }

    private static float Sine(float x)
    {
        return 0.5f + 0.5f * Mathf.Sin(2 * Mathf.PI * x * Time.timeSinceLevelLoad);
    }
    #endregion
}
