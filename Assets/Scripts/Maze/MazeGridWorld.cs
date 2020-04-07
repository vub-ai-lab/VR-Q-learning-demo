using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Action = Enums.Action;

// This class represents the labyrinth world.
[Serializable]
public class MazeGridWorld : GridWorld {

	
	public override void makeGraph()
	{
		gridSizeX = labyrinth[0].Length;
		gridSizeY = labyrinth.Count;

		for (var y = 0; y < gridSizeY; ++y) {
			nodes.Add (new List<Node>());

			for (var x = 0; x < gridSizeX; ++x) {
				Node current = new Node (x, y);
				nodes [y].Add (current);

				if (getLabyrinth(x, y) == wallChar)
					continue;
					
				if (getLabyrinth (x, y) == goalChar)
					GoalState = current;
				
				if (getLabyrinth (x,y) == floorChar)
					startNodes.Add (current);
				
				if (getLabyrinth (x, y) == startChar) {
					currentState = StartState = current;
					startNodes.Add (current);
				}
				if (x > 0) {
					if (getLabyrinth(x-1, y) != wallChar) {
						current.addAction (Action.left, nodes [y] [x - 1]);
						nodes [y] [x - 1].addAction (Action.right, current);
					}
				}
				if (y > 0) {
					if (getLabyrinth(x, y-1) != wallChar) {
						current.addAction (Action.down, nodes [y - 1] [x]);
						nodes [y - 1] [x].addAction (Action.up, current);
					}
				}
			}
		}
	}
}
