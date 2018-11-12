using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Action = Enums.Action;

// This class represents the labyrinth world.
[Serializable]
public class GridWorld : MonoBehaviour {

	// String representation of maze layout
	// Note: Origin is at top-left corner
	static List<string> labyrinth = new List<string> {
		".gxxh.",
		".xxbx.",
		"......",
		".xexx.",
		".xxfx.",
		"s....."
	};

	// In the future I'd like to have a more difficult maze
	static List<string> labyrinth2 = new List<string> {
		".x.......x",
		".x.xxxxx..",
		".xxxxxgx.x",
		".........x",
		".xx.xxxx..",
		".x..x..x.x",
		".xx.x.xx..",
		"....x.xxx.",
		".xx.x..x..",
		".xx.x.xxx.",
		"s........."
	};

	public char getLabyrinth(int x, int y)
	{
		var labY = gridSizeY - y - 1;
		return labyrinth[labY][x];
	}
		
	public static char wallChar  = 'x';
	public static char floorChar = '.';
	public static char startChar = 's';
	public static char goalChar = 'b';

	public static char goldChestUp = 'a';
	public static char goldChestDown = 'b';
	public static char goldChestLeft = 'c';
	public static char goldChestRight = 'd';
	public static char emptyChestUp = 'e';
	public static char emptyChestDown = 'f';
	public static char emptyChestLeft = 'g';
	public static char emptyChestRight = 'h';
	public HashSet<char> chestChars = new HashSet<char> {goldChestUp, goldChestDown, goldChestLeft, goldChestRight, emptyChestUp, emptyChestDown, emptyChestLeft, emptyChestRight}; 

	private static System.Random rnd = new System.Random();

	// This is used to represent each accessible tile in the grid.
	// It contains the available actions and adjacent Nodes.
	class Node{
		private Vector2Int position;
		private Dictionary<Action,Node> actionDict;
		private GameObject chest;

		public Node(int x, int y)
        {
			position = new Vector2Int(x,y);
			actionDict = new Dictionary<Action, Node>();
		}

		public Vector2Int getPosition()
        {
			return position;
		}

		public List<Action> getActions()
        {
			return new List<Action>(actionDict.Keys);
		}

		public void addAction(Action action, Node neighbor)
        {
			actionDict.Add(action, neighbor);
		}

		public Node getNeighbor(Action action)
        {
			return actionDict [action];
		}

		public bool hasAction(Action a)
        {
			return actionDict.ContainsKey (a);
		}

		public GameObject getChest(){
			return chest;
		}

		public void setChest(GameObject chest_obj){
			chest = chest_obj;
		}
	}

	// Object vars
	// ===========
	[HideInInspector]
	public int gridSizeX;
	[HideInInspector]
	public int gridSizeY;

	//Adjacency list graph
    private List<List<Node>> nodes = new List<List<Node>>();

    private Node currentState;
	private Node GoalState;
	private Node StartState;

    // Object methods
    public Vector2Int getCurrentState()
    {
		return currentState.getPosition();
    }

	public bool isTerminal()
	{
		return currentState == GoalState;
	}
		
    public void ResetEpisode()
    {
		//currentState = nodes [rnd.Next(nodes.Count)];

		currentState = StartState;

    }

	/// <summary>
	/// Perform the specified action in the environment.
	/// Throws an InvalidOperationException if the action is not allowed in the environment.
	/// </summary>
	/// <param name="action">Action.</param>
	public float Step(Action action)
    {
		if (!currentState.hasAction(action))
			throw new InvalidOperationException ();

		currentState = currentState.getNeighbor(action);

		// Provide reward
		if (currentState == GoalState) {
			Debug.Log ("Reached goal state");
			return 10f;
		} else {
			// By default we give zero reward
			return 0; 
		}
	}

    public List<Action> getActions(Vector2Int state)
    {
		Node node = nodes [state.y][state.x];
        return node.getActions();
    }

	public GameObject getChest(Vector2Int state){
		return nodes [state.y][state.x].getChest();
	}

	public void addChest(int x, int y, GameObject obj){
		nodes [y] [x].setChest (obj);
	}
		
	public void makeGraph()
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
					
				if (getLabyrinth(x, y) == goalChar)
					GoalState = current;
				
				if (getLabyrinth (x, y) == startChar)
					currentState = StartState = current;

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
