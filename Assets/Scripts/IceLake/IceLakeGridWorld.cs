using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Action = Enums.Action;

// This class represents the labyrinth world.
[Serializable]
public class IceLakeGridWorld : GridWorld
{

    public override void makeGraph()
    {
        gridSizeX = lake[0].Length;
        gridSizeY = lake.Count;

        for (var y = 0; y < gridSizeY; ++y)
        {
            nodes.Add(new List<Node>());

            for (var x = 0; x < gridSizeX; ++x)
            {
                Node current = new Node(x, y);
                nodes[y].Add(current);

                if (GetLake(x, y) == goalChar)
                    GoalState = current;

                if (GetLake(x, y) == floorChar)
                    startNodes.Add(current);

                if (GetLake(x, y) == startChar)
                {
                    currentState = StartState = current;
                    startNodes.Add(current);
                }

                if (GetLake(x, y) == holeChar)
                    holes.Add(current);

                if (x > 0)
                {
                    current.addAction(Action.left, nodes[y][x - 1]);
                    nodes[y][x - 1].addAction(Action.right, current);

                }
                if (y > 0)
                {

                    current.addAction(Action.down, nodes[y - 1][x]);
                    nodes[y - 1][x].addAction(Action.up, current);

                }
            }
        }
    }
}
