using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using VRTK;
using Action = Enums.Action;

// This class represents the labyrinth world.
[Serializable]
public class GridWorld : MonoBehaviour{
	// Note: Origin is at top-left corner
	List<string> labyrinth = new List<string> {
	//        5,3
		"..xx..",
		".xxcx.",
		"......",
		".x.xx.",
		".xx.x.",
		"......"
	// 0,0
	};
		
	char coinChar  = 'c';
	char wallChar  = 'x';
	char floorChar = '.';

	// This is used to represent each accessible tile in the grid.
	// It contains the available actions and adjacent Nodes.
	class Node{
		private Vector2Int position;
		private Dictionary<Action,Node> actionDict;

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
	}

	// Object vars
	// ===========

	public int gridSizeX;
	public int gridSizeY;

    private List<List<Node>> nodes = new List<List<Node>>();

    private Node currentState;
	private Node GoalState;
	private Node StartState;

    private bool done = false;

	public Agent agent;

	// GUI VARIABLES
	// =============
	public Tilemap tilemap;
    public GameObject coin;
    public Gradient tileGradient;
    
	public VRTK_DashTeleport teleporter;

	// FIXME: Just make public and rebind to menu
	private bool showQtables = true;
	public bool ShowQtables
    {
        get { return showQtables; }
        set { showQtables = value; }
    }

    // Object methods

    public Vector2Int getCurrentState()
    {
		return currentState.getPosition();
    }

    public void Reset()
    {
		currentState = StartState;
		moveAgentInGameWorld (currentState, true);

		agent.learning = true;
		agent.lastState = currentState.getPosition();
        // Set environment back to not done
        done = false;
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
		
		Debug.Log ("Before:");
		Debug.Log (currentState.getPosition ());
		currentState = currentState.getNeighbor(action);
		Debug.Log ("After:");
		Debug.Log (currentState.getPosition ());

		// Change position of the agent in the VR world
		moveAgentInGameWorld(currentState);

		// Provide reward
		if (currentState == GoalState) {
			Debug.Log ("Reached goal state");
			done = true;
			return 10f;
		} else {
			// Since we don't automatically reset the episode when the goal state is reached, 
			// we need to ensure this for sanity when we decide to continue walking around after the goal has been reached
			done = false;
			// By default we give zero reward
			return 0; 
		}
	}

    public List<Action> getActions(Vector2Int state)
    {
		Node node = nodes [state.y][state.x];
        return node.getActions();
    }

    public bool isTerminal()
    {
		return done;
	}
		
	public void restart()
	{
		agent.clearMemory();
		Reset();
	}

	// Unity calls this when it initializes everything.
	private void Awake()
	{
		makeGraph();
        InitGridGUI();
    }

	private void makeGraph()
	{
		gridSizeX = labyrinth[0].Length;
		gridSizeY = labyrinth.Count;
		for (var y = 0; y < gridSizeY; ++y) {
			var labY = gridSizeY - y - 1;

			nodes.Add (new List<Node>());

			for (var x = 0; x < gridSizeX; ++x) {
				Node current = new Node (x, y);
				nodes [y].Add (current);

				if (labyrinth [labY] [x] == wallChar) 
				{
					GameObject cube = GameObject.CreatePrimitive (PrimitiveType.Cube);
					cube.transform.position = tilemap.GetCellCenterWorld (new Vector3Int (x, y, 0)) + new Vector3 (0, 2.5f, 0);
					cube.transform.localScale = new Vector3 (2, 5, 1.5f);
					continue;
				}
					
				if (labyrinth [labY] [x] == coinChar)
					GoalState = current;

				if (x > 0) {
					if (labyrinth [labY] [x - 1] != wallChar) {
						current.addAction (Action.left, nodes [y] [x - 1]);
						nodes [y] [x - 1].addAction (Action.right, current);
					}
				}
				if (y > 0) {
					if (labyrinth [labY + 1] [x] != wallChar) {
						current.addAction (Action.down, nodes [y - 1] [x]);
						nodes [y - 1] [x].addAction (Action.up, current);
					}
				}
			}
		}

		//configure start and goal state
		StartState = nodes[0][0];
		currentState = StartState;
	}

	private void InitGridGUI()
	{
		coin.SetActive(false);
		foreach (Node node in nodes.SelectMany(l => l))
		{
			Vector2Int pos = node.getPosition();
			Vector3Int position = new Vector3Int(pos.x, pos.y, 0);
			//tilemap.SetTile (position, ScriptableObject.CreateInstance<TileBase>());
			tilemap.SetTileFlags(position, TileFlags.None);
			tilemap.SetColor(position, Color.black);
		}
	}

	// GUI DEPENDENT STUFF
	// ===================

	private void moveAgentInGameWorld(Node target, bool teleport = false)
	{
		Vector2Int pos = target.getPosition ();
		Vector3 destination = tilemap.GetCellCenterWorld(new Vector3Int(pos.x, pos.y, 0));

		if (teleport)
			teleporter.ForceTeleport(tilemap.GetCellCenterWorld(new Vector3Int(pos.x, pos.y, 0))); // This teleports
		else
			teleporter.Teleport(agent.transform, destination, null, true); // This flies

		if (currentState == GoalState) {
			StartCoroutine (ShowCoinTemporarily());
		}
	}

	private IEnumerator ShowCoinTemporarily ()
	{
		yield return new WaitForSeconds (0.05f);
		coin.SetActive (true);
		yield return new WaitForSeconds (2f);
		coin.SetActive (false);
	}

	private void VisualiseQTable(object sender, DestinationMarkerEventArgs e)
	{
		if (showQtables) {
			foreach (Node node in nodes.SelectMany(l => l))
			{
				Vector2Int pos = node.getPosition();
				float v = agent.GetStateValue(pos);
				Vector3Int position = new Vector3Int(pos.x, pos.y, 0);
				tilemap.SetColor(position, tileGradient.Evaluate(v / 10));
			}
		}
	}
		
	void OnEnable()
	{
		// Enable floor buttons and color visualization
		teleporter.Teleporting += agent.ClearUI;
		teleporter.Teleported += agent.UpdateUI;
		teleporter.Teleported += VisualiseQTable;
	}

	void OnDisable()
	{
		// Disable floor buttons and color visualization
		teleporter.Teleporting -= agent.ClearUI;
		teleporter.Teleported -= agent.UpdateUI;
		teleporter.Teleported -= VisualiseQTable;
	}
}
