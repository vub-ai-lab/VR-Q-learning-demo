using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// This class represents the labyrinth world.
[Serializable]
public class GridWorld : MonoBehaviour {

	public enum Action { up = 0, down = 1, left = 2, right = 3 };

	// String representation of maze layout
	// Note: Origin is at top-left corner
	static List<string> labyrinth2 = new List<string> {
		"xxxxxxxx",
		"x.lxxr.x",
		"x.xxDx.x",
		"x......x",
		"x.xuxx.x",
		"x.xxdx.x",
		"xs.....x",
		"xxxxxxxx"
	};

	// In the future I'd like to have a more difficult maze
	static List<string> labyrinth1 = new List<string> {
		"xxxxxxxxxxxx",
		"xdx.......xx",
		"x.xUx.xxx.lx",
		"x.xxx.xdx.xx",
		"x.........xx",
		"x.xx.xxxx.lx",
		"x.xr.x.lx.xx",
		"x.xx.x.xx..x",
		"x....x.xxx.x",
		"x.xx.x.lxr.x",
		"x.xx.x.xxx.x",
		"xs.........x",
		"xxxxxxxxxxxx"
	};

	static List<string> labyrinth = labyrinth2;

	public char getLabyrinth(int x, int y)
	{
		var labY = gridSizeY - y - 1;
		return labyrinth[labY][x];
	}
		
	public static char wallChar  = 'x';
	public static char floorChar = '.';
	public static char startChar = 's';

	public static char goldChestUp = 'U';
	public static char goldChestDown = 'D';
	public static char goldChestLeft = 'L';
	public static char goldChestRight = 'R';
	public static char emptyChestUp = 'u';
	public static char emptyChestDown = 'd';
	public static char emptyChestLeft = 'l';
	public static char emptyChestRight = 'r';
	public static List<char> goalChars = new List<char>{'U','D','L','R'};

	public HashSet<char> chestChars = new HashSet<char> {goldChestUp, goldChestDown, goldChestLeft, goldChestRight, emptyChestUp, emptyChestDown, emptyChestLeft, emptyChestRight}; 

	private List<Node> startNodes = new List<Node> ();
	private static System.Random rnd = new System.Random();

	public delegate void EnvEvent();
	public static event EnvEvent OnGoalReached;

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
    public int stateSpace;

	//Adjacency list graph
    private List<List<Node>> nodes = new List<List<Node>>();

    private Node currentState;
	private Node StartState;
	private List<Node> GoalStates = new List<Node>();

    public int posToInt(Vector2Int pos)
    {
        return pos.y * gridSizeX + pos.x;
    }
    public Vector2Int intToPos(int s)
    {
        var pos = new Vector2Int();
        pos.y = s / gridSizeX;
        pos.x = s - pos.y * gridSizeX;
        return pos;
    }

    // Object methods
    public int getCurrentState()
    {
        return posToInt(currentState.getPosition());
    }

	public bool isTerminal()
	{
		return GoalStates.Contains(currentState);
	}
		
    public void ResetEpisode()
    {
		currentState = startNodes [rnd.Next(startNodes.Count)];
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
		if (GoalStates.Contains(currentState)) {
			if(OnGoalReached != null)
				OnGoalReached ();
			return 10f;
		}
		else 
			// By default we give zero reward
			return 0; 
	}

    public List<Action> getActions(int s)
    {
        var pos = intToPos(s);
		Node node = nodes [pos.y][pos.x];
        return node.getActions();
    }

	public GameObject getChest(int s){
		var pos = intToPos(s);
		return nodes [pos.y][pos.x].getChest();
	}

	public void addChest(int x, int y, GameObject obj){
		nodes [y] [x].setChest (obj);
	}
		
	public void makeGraph()
	{
		gridSizeX = labyrinth[0].Length;
		gridSizeY = labyrinth.Count;
		stateSpace = gridSizeX * gridSizeY;

		for (var y = 0; y < gridSizeY; ++y) {
			nodes.Add (new List<Node>());

			for (var x = 0; x < gridSizeX; ++x) {
				Node current = new Node (x, y);
				nodes [y].Add (current);

				if (getLabyrinth(x, y) == wallChar)
					continue;
					
				if (goalChars.Contains(getLabyrinth (x, y)))
					GoalStates.Add(current);
				
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
