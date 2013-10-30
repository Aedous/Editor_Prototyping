using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


[ExecuteInEditMode]
public class AStar : MonoBehaviour
{
    //This class handles the a star algorithm to determine which path is best to follow
    //The theory follows this example http://www.policyalmanac.org/games/aStarTutorial.htm

    //To sum it up, this will make reference to a grid system, collect point A and point B,
    //decide the best path to take and then return a list of path points to take
    [Serializable]
    public class BlockScore
    {
        public int points_to_move;
        public int heuristic;
        public int total_move;
        public Vector2 point, parent_point;
        public int indexinlist; //index the score is at on the list

        public BlockScore()
        {
            points_to_move = -1;
            heuristic = -1;
            total_move = -1;
            point = Vector2.zero;
        }

        public BlockScore(int pointstomove, int h, int totalmove, Vector2 _point)
        {
            points_to_move = pointstomove;
            heuristic = h;
            total_move = totalmove;
            point = _point;
        }
    }

    public struct GridPosition
    {
        public Vector2 startPoint;
        public Vector2 endPoint;
    }

    #region Inspector Variables
    public const int blockscore = 10;
    public const int blockscore_diagonal = 14;
    #endregion

    #region Public Variables
    public int grid_width;
    public int grid_height;
    public Vector2 current_point, start_point, end_point;
    public List<Vector2> complete_path;
    public List<BlockScore> open_points;
    public List<BlockScore> closed_points;
    public bool completedsearch;

    #endregion

    #region Private Variables
    #endregion

    #region Unity Classes
    void Start()
    {

    }

    void Update()
    {

    }
    #endregion

    #region Public Classes
    public void InitAStar(int width, int height)
    {
        grid_width = width;
        grid_height = height;
    }

    public void ResetAStar()
    {
        complete_path.Clear();
        open_points.Clear();
        closed_points.Clear();
        completedsearch = false;
    }

    public void WorkOutPath(Vector2 pointA, Vector2 pointB)
    {
        completedsearch = false;
        complete_path = new List<Vector2>();
        open_points = new List<BlockScore>();
        closed_points = new List<BlockScore>();

        //First thing we do is set our current point to pointA
        current_point = pointA;
        start_point = pointA;
        end_point = pointB;
        //Add the pointA to the closed list so we do not check the spot we are on
        BlockScore start_blockscore = CreateBlockScoreFromPoint(pointA, pointB, start_point);
        start_blockscore.indexinlist = 0;
        closed_points.Add(start_blockscore);

        //Loop through to figure out our path to take
        BlockScore last_point = PathLoop(start_point, end_point);

        //Once that is complete we should have the complete list of points for a path
        Debug.Log("From " + pointA + " to " + pointB);
        Debug.Log(complete_path.ToString());
        Debug.Log("Path Completed in " + complete_path.Count + " steps.");
    }
    #endregion

    #region Private Classes
    private BlockScore PathLoop(Vector2 c_point, Vector2 e_point)
    {
        //Find out the path to take
        BlockScore next_point = CheckPointsFromPoint(c_point);
        complete_path.Add(next_point.point);

        //Add this point to our list
        //Once we have calculate a move, we check to see if that next_point is our end point
        if (completedsearch)
        {
            return next_point;
        }

        if (next_point.point == e_point)
        {
            //We have arrived at our destination
            completedsearch = true;
            return next_point;
        }
        else
        {
            //Debug and log out the open_points list
            return PathLoop(next_point.point, e_point);
        }
        
    }

    private Vector2[] CreatePointsToCheck(Vector2 starting_point)
    {
        Vector2[] points = new Vector2[8];

        //Calculate the up right down and left points
        Vector2 up_point = new Vector2(starting_point.x, starting_point.y + 1);
        Vector2 right_point = new Vector2(starting_point.x + 1, starting_point.y);
        Vector2 down_point = new Vector2(starting_point.x, starting_point.y - 1);
        Vector2 left_point = new Vector2(starting_point.x - 1, starting_point.y);

        //Diagonals up right, down right, down left, up left ( clockwise )
        Vector2 upright_point = new Vector2(starting_point.x + 1, starting_point.y + 1);
        Vector2 downright_point = new Vector2(starting_point.x + 1, starting_point.y - 1);
        Vector2 downleft_point = new Vector2(starting_point.x - 1, starting_point.y - 1);
        Vector2 upleft_point = new Vector2(starting_point.x - 1, starting_point.y + 1);

        //Save points in an array in a clockwise order
        points[0] = up_point;
        points[1] = upright_point;
        points[2] = right_point;
        points[3] = downright_point;
        points[4] = down_point;
        points[5] = downleft_point;
        points[6] = left_point;
        points[7] = upleft_point;

        return points;

    }

    private BlockScore CreateBlockScoreFromPoint(Vector2 start_point, Vector2 end_point, Vector2 score_point)
    {
        //start_point is the current position we are trying to move from
        //score_point is the position that we want to create a blockscore
        //end_position is where we are trying to move to

        BlockScore new_blockscore = new BlockScore();

        //Create the parameters for the block score
        //Work the difference between the two points, and get an absolute value
        //if x_diff and y_diff both have a number higher than 0 then we moving diagonally
        int x_diff = (int)Mathf.Abs(start_point.x - score_point.x);
        int y_diff = (int)Mathf.Abs(start_point.y - score_point.y);

        if (x_diff > 0 && y_diff > 0)
        {
            //We are moving diagonally
            new_blockscore.points_to_move = 14; //
        }
        else
            new_blockscore.points_to_move = 10; //

        new_blockscore.heuristic = WorkoutHeuristic(score_point, end_point);
        new_blockscore.total_move = new_blockscore.points_to_move + new_blockscore.heuristic;
        new_blockscore.point = score_point;

        return new_blockscore;
    }



    private BlockScore CheckPointsFromPoint(Vector2 starting_point)
    {
        //This function takes a point and checks the spaces along it's edges ( diagonally as well )
        Vector2[] points;
        points = CreatePointsToCheck(starting_point);

        //Check which points are valid to check against a move
        for (int p = 0; p < points.Length; p++)
        {
            Vector2 point_to_check = points[p];

            if (IsPointValid(point_to_check))
            {
                //If the point is valid we convert it into something we can understand BlockScore
                //we can use the struct to save more information in one point
                BlockScore block_score = CreateBlockScoreFromPoint(starting_point, end_point, point_to_check);
                block_score.indexinlist = p;
                //Assign the parent to this starting_point
                block_score.parent_point = starting_point;

                if (CanAddToOpenList(block_score))
                    open_points.Add(block_score);
            }
        }
        
        //Once we have a list of validated and converted points ( to blockscore ) we need to select the lowest score
        BlockScore lowest_score = new BlockScore();
        int index = 0;
        for (int i = 0; i < open_points.Count; i++)
        {
            if (lowest_score.total_move == -1)
            {
                lowest_score = open_points[i];
                continue;
            }
            else
            {
                //Check if lower_score.total_move is greater than the current block_score
                if (open_points[i].point == end_point)
                {
                    lowest_score = open_points[i];
                    index = i;
                    completedsearch = true;
                }
                else if (lowest_score.total_move > open_points[i].total_move)
                {
                    lowest_score = open_points[i];
                    index = i;
                }
            }
        }
        //Drop the lowest score from the open list and add to the closed list
        if(index < open_points.Count) //Stop any invalid values been popped out
            open_points.RemoveAt(index);

        lowest_score.indexinlist = closed_points.Count + 1; //Keep track of the index for the new list
        closed_points.Add(lowest_score);
        

        //Once we get the lowest score to move to, we can return to it and move to that spot
        return lowest_score;
    }

    private bool CanAddToOpenList(BlockScore item)
    {
        //This code makes sure the item doesn't already exist in the list
        foreach (BlockScore bscore in open_points)
        {
            //Check the bscore against the item
            if (bscore.point == item.point)
            {
                //Already exists
                return false;
            }
        }

        foreach (BlockScore bscore in closed_points)
        {
            //Check the bscore against the item
            if (bscore.point == item.point)
            {
                //Already exists
                return false;
            }
        }

        //If we loop through everything and we are okay
        return true;
    }

    private bool IsPointValid(Vector2 point) //Basic validation check to make sure the space is actually a valid space to consider
    {
        //This makes sure that the point we are checking is a valid point on the grid
        //Check against width
        if (point.x > grid_width || point.x < 0)
            return false;
        //Check against height
        if (point.y > grid_height || point.y < 0)
            return false;

        //Check if point is valid
        ProceduralGeneration_Core pgc_reference = GetComponent<ProceduralGeneration_Core>();
        if (pgc_reference)
        {
            //Check if path is valid to move on
            if (pgc_reference.grid_params[(int)point.x, (int)point.y] == 1)
            {
                return false;
            }
        }
        return true;
    }

    private int WorkoutHeuristic(Vector2 start_point, Vector2 end_point)
    {
        int score = 0;

        //Take the start_point and subtract it from the end_point, make sure to return the absolute value
        //then take the difference between them and add them together
        int x_diff = (int)Mathf.Abs(start_point.x - end_point.x);
        int y_diff = (int)Mathf.Abs(start_point.y - end_point.y);

        score = x_diff + y_diff;

        return score;
    }
    #endregion

}

