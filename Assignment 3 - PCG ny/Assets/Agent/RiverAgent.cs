using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using Utils;

public class RiverAgent : MonoBehaviour
{
    private List<Vector2> path;
    private Vector2 highestPoint;
    private Vector2 lowestPoint;
    private Vector2 position;
    private float[,] heightMap;
    private float[,] riverMap;
    private bool[,] bfsMap;
    private int mapHeight;
    private int mapWidth;
    private float amplitude;
    private System.Random rand;
    private Vector2 prevPosition;
    private int steps;
    private int currentTokens;
    private int currentRiverWidth;



    [Header("General  Properties")]
    [SerializeField] private float seaLevel;

    [Header("River Properties")]
    [SerializeField] private int riverExpansionRate;
    [SerializeField] private float riverDepth;
    [SerializeField] private int riverWidth;    

    [Header("Agent Properties")]
    [SerializeField] private int movesBetwenWiggle;
    [SerializeField] private int tokens;
    [SerializeField] private float waterLevel;
    [Range(0, 1)]
    [SerializeField] private float maxTravesable;

    void Awake()
    {
        rand = new System.Random();
        prevPosition = Vector2.zero;
        highestPoint = Vector2.zero;
        lowestPoint = Vector2.zero;
    }



    public float[,] generateRiver(int mapHeight, int mapWidth, float[,] heightMap, float amplitude)
    {
        this.mapHeight = mapHeight;
        this.mapWidth = mapWidth;
        this.heightMap = heightMap;
        this.amplitude = amplitude;
        bfsMap = new bool[mapWidth, mapHeight];
        riverMap = new float[mapWidth, mapHeight];
        path = new List<Vector2>();
        steps = 0;
        currentTokens = tokens;
        currentRiverWidth = riverWidth;

        init();
        startTraversing();

        return riverMap;
    }

    public void startTraversing()
    {
        position = highestPoint;
        //dijkstra();

        int wiggleCounter = 0;
        while (!checkIfMinHeightReached(position))
        {

            if (wiggleCounter >= movesBetwenWiggle)
            {
                wiggleCounter = movesBetwenWiggle;
                makeMove(findWiggleMove(GetDirection(lowestPoint)));
            }
            else
            {
                wiggleCounter++;
                makeMove(findMove(GetDirection(lowestPoint)));

            }

            if (currentTokens <= 0) break;
        }

        for (int i = 0; i < path.Count; i++)
        {
            updateRiverExpansion();
            broadenRiver(i);
            position = path[i];
        }       
    }
    private void updateRiverExpansion()
    {
        steps++;
        if (steps >= riverExpansionRate)
        {
            steps = 0;
            currentRiverWidth++;
        }
    }

    private Vector2 GetDirection(Vector2 target)
    {
        Vector2 temp = (target - position).normalized;
        float x = Mathf.Round(temp.x);
        float y = Mathf.Round(temp.y);
        return new Vector2(x, y);
    }

    private Vector2 GetDirectionAway(Vector2 repellerPoint, Vector2 position)
    {
        Vector2 temp = (position - repellerPoint).normalized;
        float x = Mathf.Round(temp.x);
        float y = Mathf.Round(temp.y);
        return new Vector2(x, y);
    }

    private Vector2 findMove(Vector2 direction)
    {
        Vector2 move = Vector2.zero;
        switch (direction)
        {
            case Vector2 v when v.Equals(Vector2.up):
                move = chooseTile(new Vector2(1, 1), new Vector2(0, 1), new Vector2(-1, 1));
                break;
            case Vector2 v when v.Equals(Vector2.down):
                move = chooseTile(new Vector2(1, -1), new Vector2(0, -1), new Vector2(-1, -1));
                break;
            case Vector2 v when v.Equals(Vector2.left):
                move = chooseTile(new Vector2(-1, 1), new Vector2(-1, -1), new Vector2(-1, 0));
                break;
            case Vector2 v when v.Equals(Vector2.right):
                move = chooseTile(new Vector2(1, 1), new Vector2(1, -1), new Vector2(1, 0));
                break;
            case Vector2 v when v.Equals(new Vector2(1, 1)):
                move = chooseTile(new Vector2(1, 1), new Vector2(0, 1), new Vector2(1, 0));
                break;
            case Vector2 v when v.Equals(new Vector2(1, -1)):
                move = chooseTile(new Vector2(1, 0), new Vector2(1, -1), new Vector2(0, -1));
                break;
            case Vector2 v when v.Equals(new Vector2(-1, -1)):
                move = chooseTile(new Vector2(-1, 0), new Vector2(-1, -1), new Vector2(0, -1));
                break;
            case Vector2 v when v.Equals(new Vector2(-1, 1)):
                move = chooseTile(new Vector2(-1, 0), new Vector2(-1, 1), new Vector2(0, 1));
                break;
            default:
                Debug.Log("Couldnt find switch for: " + position.x + ", " + position.y);
                break;
        }
        return move;
    }

    private Vector2 findWiggleMove(Vector2 direction)
    {
        Vector2 move = Vector2.zero;
        switch (direction)
        {
            case Vector2 v when v.Equals(Vector2.up):
                move = chooseWiggleTile(new Vector2(1, 1), new Vector2(0, 1), new Vector2(-1, 1));
                break;
            case Vector2 v when v.Equals(Vector2.down):
                move = chooseWiggleTile(new Vector2(1, -1), new Vector2(0, -1), new Vector2(-1, -1));
                break;
            case Vector2 v when v.Equals(Vector2.left):
                move = chooseWiggleTile(new Vector2(-1, 1), new Vector2(-1, -1), new Vector2(-1, 0));
                break;
            case Vector2 v when v.Equals(Vector2.right):
                move = chooseWiggleTile(new Vector2(1, 1), new Vector2(1, -1), new Vector2(1, 0));
                break;
            case Vector2 v when v.Equals(new Vector2(1, 1)):
                move = chooseWiggleTile(new Vector2(1, 1), new Vector2(0, 1), new Vector2(1, 0));
                break;
            case Vector2 v when v.Equals(new Vector2(1, -1)):
                move = chooseWiggleTile(new Vector2(1, 0), new Vector2(1, -1), new Vector2(0, -1));
                break;
            case Vector2 v when v.Equals(new Vector2(-1, -1)):
                move = chooseWiggleTile(new Vector2(-1, 0), new Vector2(-1, -1), new Vector2(0, -1));
                break;
            case Vector2 v when v.Equals(new Vector2(-1, 1)):
                move = chooseWiggleTile(new Vector2(-1, 0), new Vector2(-1, 1), new Vector2(0, 1));
                break;
            default:
                Debug.Log("Couldnt find switch for: " + position.x + ", " + position.y);
                break;
        }
        return move;
    }
    private Vector2 chooseWiggleTile(Vector2 pos1, Vector2 pos2, Vector2 pos3)
    {
        List<Vector2> list = new List<Vector2>();
        if (pos1 != prevPosition) list.Add(pos1);
        if (pos2 != prevPosition) list.Add(pos2);
        if (pos3 != prevPosition) list.Add(pos3);
        return list[rand.Next(0, list.Count)];
    }
    private bool checkIfBelowMaxHeight()
    {
        int x = (int)position.x;
        int y = (int)position.y;
        return heightMap[x, y] / amplitude < maxTravesable;
    }

    private void makeMove(Vector2 move)
    {
        if (checkIfBelowMaxHeight())
        {
            position = position + move;
            path.Add(position);
            currentTokens--;
            prevPosition = move;
        }
        else
        {
            position = position + move;
            prevPosition = move;
        }
    }
    private Vector2 findDirectionNext(int i)
    {
        Vector2 temp = new Vector2(position.x - path[i].x, position.y - path[i].y);
        if (temp.magnitude > 2)
        {
            temp = temp.normalized;
            temp = new Vector2(Mathf.Round(temp.x), Mathf.Round(temp.y));
        }

        return temp;
    }
    private void broadenRiver(int i)
    {
        Vector2 direction = findDirectionNext(i);
        findBroaderTiles(direction);
    }

    public void findBroaderTiles(Vector2 direction)
    {
        assignBroaderValues(Vector2.zero, 0);

        if (direction.x == 0 || direction.y == 0) //Horizontal or vertical diretion
        {
            int offset1 = 0;
            int offset2 = 0;
            Vector2 offset = new Vector2(direction.y, direction.x); //flip x and y from direction
            for (int i = currentRiverWidth - 1; i > 0; i--)
            {
                assignBroaderValues(offset * i, i);
                assignBroaderValues(offset * -i, i);
            }
        }
        else // diagonal direction
        {
            bfsExpandRiver(position, currentRiverWidth);
        }
    }

    private void assignBroaderValues(Vector2 offset, int i)
    {
        int x = (int)(position.x + offset.x);
        int y = (int)(position.y + offset.y);

        float riverDepthScaler = (seaLevel - riverDepth) / currentRiverWidth;
        float depthValue = riverDepth + riverDepthScaler * i;
        if (riverMap[x, y] == 0 || riverMap[x, y] > depthValue) riverMap[x, y] = depthValue;
    }
    private void bfsExpandRiver(Vector2 startPos, int riverWidth)
    {
        resetGraph();

        Queue<Vector2> que = new Queue<Vector2>();
        que.Enqueue(startPos);

        while (que.Count != 0)
        {
            Vector2 temp = que.Dequeue();


            if (Vector2.Distance(startPos, temp) < riverWidth && !bfsMap[(int)temp.x, (int)temp.y])
            {
                float riverDepthScaler = (seaLevel - riverDepth) / riverWidth;
                float distance = (Vector2.Distance(startPos, temp));

                float depthValue = seaLevel + riverDepthScaler * (distance - riverWidth);

                if (riverMap[(int)temp.x, (int)temp.y] == 0 || riverMap[(int)temp.x, (int)temp.y] > depthValue)
                    riverMap[(int)temp.x, (int)temp.y] = depthValue;

                bfsMap[(int)temp.x, (int)temp.y] = true;

                getAdjecent(que, temp);
            }
        }
    }
    private void resetGraph()
    {
        for (int i = 0; i < mapHeight; i++)
        {
            for (int j = 0; j < mapWidth; j++)
            {
                bfsMap[i, j] = false;
            }
        }
    }
    private Vector2 chooseTile(Vector2 pos1, Vector2 pos2, Vector2 pos3)
    {


        float pos1Value = float.MaxValue;
        float pos2Value = float.MaxValue;
        float pos3Value = float.MaxValue;
        if (checkOutOfBounds(pos1))
            pos1Value = heightMap[(int)(pos1.x + position.x), (int)(pos1.y + position.y)];
        if (checkOutOfBounds(pos2))
            pos2Value = heightMap[(int)(pos2.x + position.x), (int)(pos2.y + position.y)];
        if (checkOutOfBounds(pos3))
            pos3Value = heightMap[(int)(pos3.x + position.x), (int)(pos3.y + position.y)];


        if (pos2Value <= pos3Value && pos2Value <= pos1Value)
            return pos2;
        if (pos1Value <= pos2Value && pos1Value <= pos3Value)
            return pos1;
        else
            return pos3;
    }



    private bool checkOutOfBounds(Vector2 pos1)
    {
        if ((pos1.x + position.x) >= mapWidth || (pos1.y + position.y) >= mapHeight || (pos1.y + position.y) < 0 || (pos1.x + position.x) < 0)
        {
            Debug.Log("Out of bounds: " + pos1.x + position.x + ", " + pos1.y + position.y);
            return false;
        }
        return true;
    }
    private bool checkIfUnderWater(Vector2 move)
    {
        if (heightMap[(int)(move.x + position.x), (int)(move.y + position.y)] <= waterLevel * amplitude)
            return true;
        return false;
    }
    private bool checkIfMinHeightReached(Vector2 move)
    {
        return heightMap[(int)move.x, (int)move.y] / amplitude < 0.2f;
    }

    public void init() //Needs to be optimized or changed
    {
        float lowest = float.MaxValue;
        float highest = float.MinValue;

        for (int i = 0; i < mapHeight; i++)
        {
            for (int j = 0; j < mapWidth; j++)
            {
                riverMap[i, j] = 0;
                if (lowest > heightMap[i, j])
                {
                    lowest = heightMap[i, j];
                    lowestPoint = new Vector2(i, j);
                }
                if (highest < heightMap[i, j])
                {
                    highest = heightMap[i, j];
                    highestPoint = new Vector2(i, j);

                }
            }
        }
    }



    #region  dijkstra

    private void dijkstra()
    {
        PriorityQueue<Vector2, float> prio = new PriorityQueue<Vector2, float>();
        prio.Enqueue(position, heightMap[(int)position.x, (int)position.y]);
        HashSet<Vector2> set = new HashSet<Vector2>();

        while (prio.Count != 0)
        {
            Vector2 temp = prio.Dequeue();

            float value = heightMap[(int)temp.x, (int)temp.y];
            if (value < .20f * amplitude)
            {
                break;
            }
            if (!set.Contains(temp))
            {
                getAdjecent(prio, temp);
                set.Add(temp);
                riverMap[(int)temp.x, (int)temp.y] = riverDepth;
            }
        }
    }

    private void getAdjecent(PriorityQueue<Vector2, float> prio, Vector2 next)
    {
        if (checkbounds(new Vector2((int)(next.x + 1), (int)(next.y))))
            prio.Enqueue(new Vector2((int)(next.x + 1), (int)(next.y)), heightMap[(int)(next.x + 1), (int)(next.y)]);
        if (checkbounds(new Vector2((int)(next.x - 1), (int)(next.y))))
            prio.Enqueue(new Vector2((int)(next.x - 1), (int)(next.y)), heightMap[(int)(next.x - 1), (int)(next.y)]);
        if (checkbounds(new Vector2((int)(next.x), (int)(next.y + 1))))
            prio.Enqueue(new Vector2((int)(next.x), (int)(next.y + 1)), heightMap[(int)(next.x), (int)(next.y + 1)]);
        if (checkbounds(new Vector2((int)(next.x), (int)(next.y - 1))))
            prio.Enqueue(new Vector2((int)(next.x), (int)(next.y - 1)), heightMap[(int)(next.x), (int)(next.y - 1)]);
    }
    private void getAdjecent(Queue<Vector2> prio, Vector2 next)
    {
        if (checkbounds(new Vector2((int)(next.x + 1), (int)(next.y))))
            prio.Enqueue(new Vector2((int)(next.x + 1), (int)(next.y)));
        if (checkbounds(new Vector2((int)(next.x - 1), (int)(next.y))))
            prio.Enqueue(new Vector2((int)(next.x - 1), (int)(next.y)));
        if (checkbounds(new Vector2((int)(next.x), (int)(next.y + 1))))
            prio.Enqueue(new Vector2((int)(next.x), (int)(next.y + 1)));
        if (checkbounds(new Vector2((int)(next.x), (int)(next.y - 1))))
            prio.Enqueue(new Vector2((int)(next.x), (int)(next.y - 1)));
    }
    private bool checkbounds(Vector2 pos)
    {
        if (pos.x < 0 || pos.x >= heightMap.GetLength(1))
        {
            return false;
        }

        if (pos.y < 0 || pos.y >= heightMap.GetLength(1))
        {
            return false;
        }
        return true;
    }


    #endregion

    #region SubRivers
    //private void FindSubRivers()
    //{
    //    position = path[0];
    //    int stepsTaken = 0;
    //    int riversCreated = 1;
    //    while (!checkIfMinHeightReached(path[stepsTaken]) && stepsTaken < path.Count)
    //    {
    //        stepsTaken++;
    //        if (stepsTaken > minDistaneBetweenRivers * riversCreated)
    //        {
    //            CreateSubRiver(path[stepsTaken]);
    //        }

    //    }
    //}

    //private void CreateSubRiver(Vector2 repellerPoint)
    //{
    //    List<Vector2> subRiver = new List<Vector2>();

    //    Vector2 move = findFirstSubRiverMove(repellerPoint);

    //    Vector2 previousMove = repellerPoint;
    //    while (CheckIfMoveIsInclined(previousMove + move, previousMove) && !checkIfMinHeightReached(previousMove + move))
    //    {
    //        heightMap[(int)(move.x + previousMove.x), (int)(move.y + previousMove.y)] = 0.2f;
    //        subRiver.Add(previousMove + move);
    //        previousMove = previousMove + move;
    //        //move = findMove(GetDirectionAway(repellerPoint));
    //    }

    //    ExpandSubRiver();
    //}

    //private void ExpandSubRiver()
    //{

    //}

    //private Vector2 findFirstSubRiverMove(Vector2 position)
    //{


    //    return Vector2.zero;
    //}

    //private bool CheckIfMoveIsInclined(Vector2 move, Vector2 previousMove)
    //{
    //    return heightMap[(int)move.x, (int)move.y] < heightMap[(int)previousMove.x, (int)previousMove.y];
    //}
}
#endregion