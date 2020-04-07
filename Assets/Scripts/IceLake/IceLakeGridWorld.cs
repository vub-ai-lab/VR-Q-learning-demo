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

                if (getLake(x, y) == wallChar)
                    continue;

                if (getLake(x, y) == goalChar)
                    GoalState = current;

                if (getLake(x, y) == floorChar)
                    startNodes.Add(current);

                if (getLake(x, y) == startChar)
                {
                    currentState = StartState = current;
                    startNodes.Add(current);
                }

                if (getLake(x, y) == holeChar)
                    holes.Add(current);

                if (x > 0)
                {
                    if (getLake(x - 1, y) != wallChar)
                    {
                        current.addAction(Action.left, nodes[y][x - 1]);
                        nodes[y][x - 1].addAction(Action.right, current);
                    }
                }
                if (y > 0)
                {
                    if (getLake(x, y - 1) != wallChar)
                    {
                        current.addAction(Action.down, nodes[y - 1][x]);
                        nodes[y - 1][x].addAction(Action.up, current);
                    }
                }
            }
        }
    }
}
