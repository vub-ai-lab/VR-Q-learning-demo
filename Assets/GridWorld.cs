using System;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using Action = Enums.Action;

[Serializable]
public class GridWorld : MonoBehaviour{

	//Object vars

	public int gridSizeX = 5;
	public int gridSizeY = 5;
    private Vector2Int currentState;
	private Vector2Int GoalState = new Vector2Int(4, 4);
	private Vector2Int StartState = new Vector2Int(0, 0);

	private List<Vector2Int> blockingCells = new List<Vector2Int>() { new Vector2Int(2,2), new Vector2Int(1,1), new Vector2Int(3,3), new Vector2Int(1,3), new Vector2Int(3,1)}; // Cells of the grid we cannot visit (walls)
    
    public bool done = false;
	private Dictionary<Vector2Int,List<Action> > actionMap = new Dictionary<Vector2Int, List<Action> >(); // containing special cases where only a subset of actions is available

    public VRTK_DashTeleport teleporter;

    private Agent agent;
    private Grid grid;
    private GameObject coin;
    
	// Object methods

    public Vector2Int getCurrentState()
    {
        return currentState;
    }

    public void Reset()
    {
        // Set agent's position back to start
        currentState = StartState;
        agent.DeActivateUIButtons();
		teleporter.ForceTeleport(grid.GetCellCenterWorld(new Vector3Int(currentState.x, currentState.y, 0)));
        agent.ActivateUIButtons();
        // De-activate coin
        coin.SetActive(false);
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
			Vector2Int nextState = getNextState (currentState, action);

			// Dirty check if we have a valid transition
			if ((!blockingCells.Contains(nextState)) &&
			             (-1 < nextState.x) &&
			             (nextState.x < gridSizeX) &&
			             (-1 < nextState.y) &&
			             (nextState.y < gridSizeY)) {
				currentState = nextState;
				Debug.Log (String.Concat ("Entering cell ", currentState.ToString ()));
				// Change position of the agent in the actual world
				moveAgentInGameWorld();
			}
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
	/// Get the actions for the given state.
	/// </summary>
	/// <returns>List of the available actions.</returns>
	/// <param name="state">State</param>
	public List<Action> getActions(Vector2Int state)
	{
		if (actionMap.ContainsKey (state)) {
			return actionMap [state];
		} else {
				return new List<Action> (){ Action.up, Action.down, Action.left, Action.right };
		}

	}
		
	private Vector2Int getNextState(Vector2Int state, Action action){
		// In which direction will we move?
		Vector2Int stepDirection;
		switch (action)
		{
		case Action.up:
			stepDirection = Vector2Int.up;
			break;
		case Action.down:
			stepDirection = Vector2Int.down;
			break;
		case Action.left:
			stepDirection = Vector2Int.left;
			break;
		case Action.right:
			stepDirection = Vector2Int.right;
			break;
		default:
			// Nothing happens
			stepDirection = Vector2Int.zero;
			break;
		}

		Vector2Int nextState = currentState + stepDirection;
		return nextState;
	}

	private void moveAgentInGameWorld(){
		agent.ClearUI ();
		Vector3 destination = grid.GetCellCenterWorld(new Vector3Int(currentState.x, currentState.y, 0));
		teleporter.Teleport(agent.transform, destination,null,true);
		if (currentState == GoalState) {
			coin.SetActive (true);
		}

	}
		
	/// <summary>
	/// Check whether the action is valid in the current state.
	/// </summary>
	/// <returns><c>true</c>, if action is valid, <c>false</c> otherwise.</returns>
	/// <param name="action">Action.</param>
	/// <param name="state">State.</param>
	private bool validAction(Action action, Vector2Int state)
	{
		if (actionMap.ContainsKey(state)) {
			return actionMap[state].Contains(action);
		} 
		else {
			return true;
		}
	}

	private void fillActionMap(){
		actionMap.Add (StartState, new List<Action> (){ Action.up, Action.right });
		actionMap.Add (GoalState, new List<Action> (){ Action.down, Action.left });
		actionMap.Add (new Vector2Int (0, 1), new List<Action> (){ Action.up, Action.down });

	}

		
	private void Awake()
	{
		agent = GameObject.Find("Agent").GetComponent<Agent>();
		grid = GameObject.Find("FloorGrid").GetComponent<Grid>();
		coin = GameObject.Find("Coin");
		coin.SetActive(false);
		currentState = StartState;
		fillActionMap ();
	}

	private void Start()
	{
		

	}

}
