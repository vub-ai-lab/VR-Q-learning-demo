using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using VRTK;
using Action = Enums.Action;

[Serializable]
public class GridWorld : MonoBehaviour{

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
			actionDict.Add (action, neighbor);
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

	//Object vars
	public int gridSizeX = 6;
	public int gridSizeY = 6;
    private List<Node> nodes = new List<Node>();
    private Node currentState;
	private Node  GoalState;
	private Node StartState;

    private bool done = false;

	private GameObject[,] stateValueSpheres;

    public VRTK_DashTeleport teleporter;
	public Agent agent;
    public Grid grid;
	public Tilemap tilemap;
    public GameObject coin;
    public Gradient tileGradient;
    private bool showQtables = true;

    public bool ShowQtables
    {
        get
        {
            return showQtables;
        }

        set
        {
            showQtables = value;
            Debug.Log("Assigned new value to showQtables");
        }
    }

    // Object methods

    public Vector2Int getCurrentState()
    {
		return currentState.getPosition();
    }

    public void Reset()
    {
        // Set agent's position back to start
        currentState = StartState;
		Vector2Int pos = currentState.getPosition();
        teleporter.ForceTeleport(grid.GetCellCenterWorld(new Vector3Int(pos.x, pos.y, 0)));//, agent.transform);
		agent.learning = true;
		agent.lastState = pos;
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
		if (validAction(action, currentState)) 
		{
			currentState = getNextState(currentState, action);
			Debug.Log (String.Concat ("Entering cell ", currentState.getPosition().ToString()));

			// Change position of the agent in the VR world
			moveAgentInGameWorld();

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

		} else {
			throw new InvalidOperationException ();
		}
	}

	/// <summary>
	/// Get the actions for the current state of the environment.
	/// </summary>
	/// <returns>List of the available actions.</returns>
	public List<Action> getActions()
	{
		return currentState.getActions ();
	}

    public List<Action> getActions(Vector2Int state)
    {
        Node node = nodes.Find(n => n.getPosition() == state);
        if (node == null) {
            return null;
        }
        return node.getActions();
    }

    public bool isTerminal()
    {
		return done;
	}
		
	private Node getNextState(Node state, Action action)
    {
		return state.getNeighbor (action);
	}

	private void moveAgentInGameWorld()
    {
		Vector2Int pos = currentState.getPosition ();
		Vector3 destination = grid.GetCellCenterWorld(new Vector3Int(pos.x, pos.y, 0));
		teleporter.Teleport(agent.transform, destination, null, true);
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
		
	/// <summary>
	/// Check whether the action is valid in the current state.
	/// </summary>
	/// <returns><c>true</c>, if action is valid, <c>false</c> otherwise.</returns>
	/// <param name="action">Action.</param>
	/// <param name="state">State.</param>
	private bool validAction(Action action, Node state)
	{
		return state.hasAction (action);
	}

	private void makeGraph()
    {
		// make nodes and add them to the list for future consultations
		Node node00 = new Node(0,0);
        nodes.Add(node00);
		Node node03 = new Node(0,3);
        nodes.Add(node03);
        Node node05 = new Node(0,5);
        nodes.Add(node05);
        Node node15 = new Node(1,5);
        nodes.Add(node15);
        Node node22 = new Node(2,2);
        nodes.Add(node22);
        Node node23 = new Node(2,3);
        nodes.Add(node23);
        Node node30 = new Node(3,0);
        nodes.Add(node30);
        Node node31 = new Node(3,1);
        nodes.Add(node31);
        Node node33 = new Node(3,3);
        nodes.Add(node33);
        Node node34 = new Node(3,4);
        nodes.Add(node34);
        Node node45 = new Node(4,5);
        nodes.Add(node45);
        Node node50 = new Node(5,0);
        nodes.Add(node50);
        Node node53 = new Node(5,3);
        nodes.Add(node53);
        Node node55 = new Node(5,5);
        nodes.Add(node55);

        // link nodes together with actions
        node00.addAction(Action.up,node03);
		node00.addAction (Action.right, node30);

		node03.addAction (Action.up, node05);
		node03.addAction (Action.down, node00);
		node03.addAction (Action.right, node23);

		node05.addAction (Action.down, node03);
		node05.addAction (Action.right, node15);

		node15.addAction (Action.left, node05);

		node22.addAction (Action.up, node23);

		node23.addAction (Action.down, node22);
		node23.addAction (Action.left, node03);
		node23.addAction (Action.right, node33);

		node30.addAction (Action.up, node31);
		node30.addAction (Action.left, node00);
		node30.addAction (Action.right, node50);

		node31.addAction (Action.down, node30);

		node33.addAction (Action.up, node34);
		node33.addAction (Action.left, node23);
		node33.addAction (Action.right, node53);

		node34.addAction (Action.down, node33);

		node45.addAction (Action.right, node55);

		node50.addAction (Action.up, node53);
		node50.addAction (Action.left, node30);

		node53.addAction (Action.up, node55);
		node53.addAction (Action.down, node50);
		node53.addAction (Action.left, node33);

		node55.addAction (Action.down, node53);
		node55.addAction (Action.left, node45);

		//configure start and goal state
		StartState = node00;
		GoalState = node34;
		currentState = StartState;
	}

	private void Awake()
	{
		coin.SetActive(false);
		makeGraph();
        InitTiles();
    }

	void OnEnable()
    {
		teleporter.Teleporting += agent.ClearUI;
		teleporter.Teleported += agent.UpdateUI;
		teleporter.Teleported += VisualiseQTable;
	}

	void OnDisable()
    {
		teleporter.Teleporting -= agent.ClearUI;
		teleporter.Teleported -= agent.UpdateUI;
		teleporter.Teleported -= VisualiseQTable;
	}

    private void InitTiles()
    {
        foreach (Node node in nodes)
        {
            Vector2Int pos = node.getPosition();
            Vector3Int position = new Vector3Int(pos.x, pos.y, 0);
            tilemap.SetTileFlags(position, TileFlags.None);
        }
    }

    private void VisualiseQTable(object sender, DestinationMarkerEventArgs e)
    {
        if (showQtables) {
            foreach (Node node in nodes)
            {
                Vector2Int pos = node.getPosition();
                float v = agent.GetStateValue(pos);
                Vector3Int position = new Vector3Int(pos.x, pos.y, 0);
                tilemap.SetColor(position, tileGradient.Evaluate(v / 10));
            }
        }
	}

    public void restart()
    {
        agent.clearMemory();
        Reset();
    }
}
