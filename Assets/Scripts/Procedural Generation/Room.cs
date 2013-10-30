using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class Room : MonoBehaviour
{
    //This script is a specific type of segment, which is in charge of creating rooms,
    //it can only live inside of a segment, and is in charge of decorating the segment giving to it.
    #region Structures
    [Serializable]
    public struct SegmentPosition
    {
        public Vector2 startPoint;
        public Vector2 endPoint;
    }

    [Serializable]
    public enum RoomBuildType
    {
        //The type of room building to use
        //Simple - build out like square
        //Grow - grow like a plant ( cellular automata <-- research )
        TEST = 0,
        SIMPLE,
        GROW
    }
    #endregion

    #region inspector variables
    public Vector2 room_pickpoint, start_point, end_point; //random position picked along the segment ( horizontally or vertically )
    public RoomBuildType roombuildtype = RoomBuildType.TEST; //Set to simple room building
    public Segment parent_segment;
    //public SegmentPosition room_position;
    public int width, height;
    public bool room_created = false; //set this value to true when a room is finished creating itself
    #endregion

    #region public variables

    #endregion

    #region private variables
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

    #endregion

    #region Class Methods
    public void InitRoom()
    {
        //The init actions that must be taken for a room to fully be ready to
        //function
        parent_segment = transform.parent.GetComponent<Segment>();

        if (!parent_segment)
        {
            Debug.LogError("No Parent Segment found for InitRoom()");
            return;
        }

        //Adjust the position of this transform
        
    }

    public void CreateRoom()
    {
        
        //Make sure room has reset it's values to match the segment (parent) above
        switch (roombuildtype)
        {
            case RoomBuildType.TEST:
                    MiddleRoomCreation();
                    //RandomRoomCreation();
                break;
            case RoomBuildType.SIMPLE:
                    //Simple room growth after testing middle room theory
                break;
            case RoomBuildType.GROW:
                break;
        }

        //Update the grid
        parent_segment.procedural_reference.FillGridParam(start_point, end_point, 1);
    }

    public void RandomRoomCreation()
    {
        //Pick a random point on the map to start the creating
        
        //Once we have our position, we can create a block at that position by adding it to the transform position
        ProceduralGeneration_Core procedural_reference = parent_segment.procedural_reference;

        Vector3[] blockposition = procedural_reference.CalculatePoints(parent_segment.startPoint, parent_segment.endPoint, 0, procedural_reference.creationDirection);

        //Work out the middle point according to the grid
        //Middle of two numbers is (add them together and divide by two)
        Vector2 added_points = parent_segment.startPoint + parent_segment.endPoint;
        int randomx = (int)UnityEngine.Random.Range(parent_segment.startPoint.x, parent_segment.endPoint.x);
        int randomy = (int)UnityEngine.Random.Range(parent_segment.startPoint.y, parent_segment.endPoint.y);
        room_pickpoint = new Vector2((float)randomx, (float)randomy);

        //To convert the points to values in the 3d world we always need to multiply it by the block size
        //float shiftx = (parent_segment.width * procedural_reference.blocksize) / 2;
        //float shifty = (parent_segment.height * procedural_reference.blocksize) / 2;
        //Vector3 middle = new Vector3(blockposition[0].x + shiftx, blockposition[0].y, blockposition[0].z + shifty);

        //From the middle just to test, spread it out to make a larger point to look at
        //room_position.startPoint = new Vector2(room_pickpoint.x - 1, room_pickpoint.y - 1);
        //room_position.endPoint = new Vector2(room_pickpoint.x + 1, room_pickpoint.y + 1);

        int[] shiftpoints = SpreadAmount();

        //Pick a random value between the numbers to spread out by
        int shiftup = UnityEngine.Random.Range(4, shiftpoints[0]);
        int shiftright = UnityEngine.Random.Range(4, shiftpoints[1]);
        int shiftdown = UnityEngine.Random.Range(4, shiftpoints[2]);
        int shiftleft = UnityEngine.Random.Range(4, shiftpoints[3]);

        start_point = new Vector2(room_pickpoint.x - shiftleft, room_pickpoint.y - shiftdown);
        end_point = new Vector2(room_pickpoint.x + shiftright, room_pickpoint.y + shiftup);

        //Set width and height of room
        CalculateWidth((int)start_point.x, (int)end_point.x);
        CalculateHeight((int)start_point.y, (int)end_point.y);

        //To create the box mesh we need to find out the
        //width in this case - applies to length
        //
        float boxheight = UnityEngine.Random.Range(5f, 20f);
        boxheight *= procedural_reference.blocksize;
        CreateBoxMesh(width * procedural_reference.blocksize, boxheight, height * procedural_reference.blocksize);

        room_created = true;

        PositionRoomTransformMiddle(boxheight/2); //Position the element according to the start point and end point
    }

    public void MiddleRoomCreation()
    {
        //Pick a point to determine middle of the layout given (segment)
        //This creates an array of points for the handles to use to draw its guide

        //Once we have our position, we can create a block at that position by adding it to the transform position
        ProceduralGeneration_Core procedural_reference = parent_segment.procedural_reference;

        Vector3[] blockposition = procedural_reference.CalculatePoints(parent_segment.startPoint, parent_segment.endPoint, 0, procedural_reference.creationDirection);
        
        //Work out the middle point according to the grid
        //Middle of two numbers is (add them together and divide by two)
        Vector2 added_points = parent_segment.startPoint + parent_segment.endPoint;
        int middlex = (int)Mathf.Abs(added_points.x / 2);
        int middley = (int)Mathf.Abs(added_points.y / 2);
        room_pickpoint = new Vector2((float)middlex,(float)middley);
        
        //To convert the points to values in the 3d world we always need to multiply it by the block size
        //float shiftx = (parent_segment.width * procedural_reference.blocksize) / 2;
        //float shifty = (parent_segment.height * procedural_reference.blocksize) / 2;
        //Vector3 middle = new Vector3(blockposition[0].x + shiftx, blockposition[0].y, blockposition[0].z + shifty);

        //From the middle just to test, spread it out to make a larger point to look at
        //room_position.startPoint = new Vector2(room_pickpoint.x - 1, room_pickpoint.y - 1);
        //room_position.endPoint = new Vector2(room_pickpoint.x + 1, room_pickpoint.y + 1);

        int[] shiftpoints = SpreadAmount();

        //Pick a random value between the numbers to spread out by
        int shiftup = UnityEngine.Random.Range(2, shiftpoints[0]);
        int shiftright = UnityEngine.Random.Range(2, shiftpoints[1]);
        int shiftdown = UnityEngine.Random.Range(2, shiftpoints[2]);
        int shiftleft = UnityEngine.Random.Range(2, shiftpoints[3]);

        start_point = new Vector2(room_pickpoint.x - shiftleft, room_pickpoint.y - shiftdown);
        end_point = new Vector2(room_pickpoint.x + shiftright, room_pickpoint.y + shiftup);

        //Set width and height of room
        CalculateWidth((int)start_point.x, (int)end_point.x);
        CalculateHeight((int)start_point.y, (int)end_point.y);

        //To create the box mesh we need to find out the
        //width in this case - applies to length
        //
        float boxheight = UnityEngine.Random.Range(1f, 2f);
        boxheight *= procedural_reference.blocksize;
        CreateBoxMesh(width * procedural_reference.blocksize, boxheight, height * procedural_reference.blocksize);

        room_created = true;

        PositionRoomTransformMiddle(boxheight/2); //Position the element according to the start point and end point

    }

    public int[] SpreadAmount()
    {
        //Spread amount - this is used for checking how far the room can be built up, down, left and right
        //[0]UP - [1]RIGHT - [2]DOWN - [3]LEFT <-- ALWAYS THAT ORDER

        //To calculate the spread amount we use our pickpoint and subtract it from our parent segment start and end points
        int[] shiftpoints = new int[4];

        int shiftup = (int)Mathf.Abs(parent_segment.endPoint.y - room_pickpoint.y);
        int shiftdown = (int)Mathf.Abs(room_pickpoint.y - parent_segment.startPoint.y);
        int shiftleft = (int)Mathf.Abs(room_pickpoint.x - parent_segment.startPoint.x);
        int shiftright = (int)Mathf.Abs(parent_segment.endPoint.x - room_pickpoint.x);

        //Store in the array to pass back using the same order , up, right, down, left
        shiftpoints[0] = shiftup;
        shiftpoints[1] = shiftright;
        shiftpoints[2] = shiftdown;
        shiftpoints[3] = shiftleft;

        return shiftpoints;
    }

    public void PositionRoomTransformMiddle(float shiftheight = 0f)
    {
        //This positions the segment in the middle of it's segment's guide
        ProceduralGeneration_Core procedural_reference = parent_segment.procedural_reference;
        Vector3[] blockposition = procedural_reference.CalculatePoints(start_point, end_point, 0, procedural_reference.creationDirection);

        float shiftx = (width * procedural_reference.blocksize) / 2;
        float shifty = (height * procedural_reference.blocksize) / 2;
        Vector3 middle = new Vector3(blockposition[0].x + shiftx, blockposition[0].y + shiftheight, blockposition[0].z + shifty);

        transform.position = middle;
    }

    public void PositionRoomTransform()
    {
        //This positions the segment transform at the 0,0 of it's segment guide
        ProceduralGeneration_Core procedural_reference = parent_segment.procedural_reference;
        Vector3[] blockposition = procedural_reference.CalculatePoints(start_point, end_point, 0, procedural_reference.creationDirection);

        transform.position = blockposition[0];
    }

    public void CalculateWidth(int startpointx, int endpointx)
    {
        //Calculate the difference between the two points which will return the width
        width = Mathf.Abs(startpointx - endpointx);
    }

    public void CalculateHeight(int startpointy, int endpointy)
    {
        //Calculate the difference between the two points which will return the height
        height = Mathf.Abs(startpointy - endpointy);
    }

    public void CreateBoxMesh(float length = 1f, float width = 1f, float height = 1f)
    {
        // You can change that line to provide another MeshFilter
        MeshRenderer meshrender = gameObject.AddComponent<MeshRenderer>();
        meshrender.material.color = Color.red;
        
        MeshFilter filter = gameObject.AddComponent<MeshFilter>();
        Mesh mesh = filter.mesh;
        mesh.Clear();

        #region Vertices
        Vector3 p0 = new Vector3(-length * .5f, -width * .5f, height * .5f);
        Vector3 p1 = new Vector3(length * .5f, -width * .5f, height * .5f);
        Vector3 p2 = new Vector3(length * .5f, -width * .5f, -height * .5f);
        Vector3 p3 = new Vector3(-length * .5f, -width * .5f, -height * .5f);

        Vector3 p4 = new Vector3(-length * .5f, width * .5f, height * .5f);
        Vector3 p5 = new Vector3(length * .5f, width * .5f, height * .5f);
        Vector3 p6 = new Vector3(length * .5f, width * .5f, -height * .5f);
        Vector3 p7 = new Vector3(-length * .5f, width * .5f, -height * .5f);

        Vector3[] vertices = new Vector3[]
        {
	        // Bottom
	        p0, p1, p2, p3,
 
	        // Left
	        p7, p4, p0, p3,
 
	        // Front
	        p4, p5, p1, p0,
 
	        // Back
	        p6, p7, p3, p2,
 
	        // Right
	        p5, p6, p2, p1,
 
	        // Top
	        p7, p6, p5, p4
        };
        #endregion

        #region Normales
        Vector3 up = Vector3.up;
        Vector3 down = Vector3.down;
        Vector3 front = Vector3.forward;
        Vector3 back = Vector3.back;
        Vector3 left = Vector3.left;
        Vector3 right = Vector3.right;

        Vector3[] normales = new Vector3[]
        {
	        // Bottom
	        down, down, down, down,
 
	        // Left
	        left, left, left, left,
 
	        // Front
	        front, front, front, front,
 
	        // Back
	        back, back, back, back,
 
	        // Right
	        right, right, right, right,
 
	        // Top
	        up, up, up, up
        };
        #endregion

        #region UVs
        Vector2 _00 = new Vector2(0f, 0f);
        Vector2 _10 = new Vector2(1f, 0f);
        Vector2 _01 = new Vector2(0f, 1f);
        Vector2 _11 = new Vector2(1f, 1f);

        Vector2[] uvs = new Vector2[]
        {
	        // Bottom
	        _11, _01, _00, _10,
 
	        // Left
	        _11, _01, _00, _10,
 
	        // Front
	        _11, _01, _00, _10,
 
	        // Back
	        _11, _01, _00, _10,
 
	        // Right
	        _11, _01, _00, _10,
 
	        // Top
	        _11, _01, _00, _10,
        };
        #endregion

        #region Triangles
        int[] triangles = new int[]
        {
	        // Bottom
	        3, 1, 0,
	        3, 2, 1,			
 
	        // Left
	        3 + 4 * 1, 1 + 4 * 1, 0 + 4 * 1,
	        3 + 4 * 1, 2 + 4 * 1, 1 + 4 * 1,
 
	        // Front
	        3 + 4 * 2, 1 + 4 * 2, 0 + 4 * 2,
	        3 + 4 * 2, 2 + 4 * 2, 1 + 4 * 2,
 
	        // Back
	        3 + 4 * 3, 1 + 4 * 3, 0 + 4 * 3,
	        3 + 4 * 3, 2 + 4 * 3, 1 + 4 * 3,
 
	        // Right
	        3 + 4 * 4, 1 + 4 * 4, 0 + 4 * 4,
	        3 + 4 * 4, 2 + 4 * 4, 1 + 4 * 4,
 
	        // Top
	        3 + 4 * 5, 1 + 4 * 5, 0 + 4 * 5,
	        3 + 4 * 5, 2 + 4 * 5, 1 + 4 * 5,
 
        };
                #endregion

        mesh.vertices = vertices;
        mesh.normals = normales;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.Optimize();
    }

    #endregion
}

