using UnityEngine;
using System.Collections;

public class ObjectHandler : MonoBehaviour
{

    #region inspector variables
    //VARIABLES//

    //-----------------------------------//
    #endregion

    #region public variables
    //GETTER AND SETTER//
    public Vector3 ObjectDimensions;
    public Vector3 center;
    public int Width 
    {
        get
        {
            return (int)ObjectDimensions.x;
        }
        set
        {
            ObjectDimensions =new Vector3((float)value, ObjectDimensions.y, ObjectDimensions.z);
        }
    }

    public int Depth
    {
        get
        {
            return (int)ObjectDimensions.y;
        }
        set
        {
            ObjectDimensions = new Vector3( ObjectDimensions.x,(float)value, ObjectDimensions.z);
        }
    }

    public int Height
    {
        get
        {
            return (int)ObjectDimensions.z;
        }
        set
        {
            ObjectDimensions = new Vector3(ObjectDimensions.x, ObjectDimensions.y,(float)value);
        }
    }
    //------------------------------------//

    #endregion

    #region private variables
    #endregion

    #region Unity Methods
    // Use this for initialization
    void Start()
    {

    }

    void OnDrawGizmos()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    #endregion

    #region Class Methods
    public void SetDimensions(Vector3 dimensions)
    {
        ObjectDimensions = dimensions;
    }

    public void SetDimensions(int x, int y, int z)
    {
        Vector3 dimensions = new Vector3((float)x, (float)y, (float)z);
        ObjectDimensions = dimensions;
    }

    public void SetCenter()
    {
        //Get the box collider center point
        //Work out the center positions
        float centerx = Width / 2;
        float centery = Height / 2;
        float centerz = Depth / 2;

        center = new Vector3(centerx, centery, centerz);
    }

    #endregion
}
