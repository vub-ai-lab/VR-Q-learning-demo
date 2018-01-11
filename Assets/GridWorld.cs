using System;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

[Serializable]
public class GridWorld : MonoBehaviour{

    Vector2Int currentState;
	List<Vector2Int> blockingCells = new List<Vector2Int>() { new Vector2Int(2,2), new Vector2Int(1,1), new Vector2Int(3,3), new Vector2Int(1,3), new Vector2Int(3,1)}; 
    int actionSize = 4;
    Vector2Int GoalState = new Vector2Int(4, 4);
    Vector2Int StartState = new Vector2Int(0, 0);
    public bool done = false;

    public VRTK_DashTeleport teleporter;

    Agent agent;
    Grid grid;
    GameObject coin;
    private int gridSizeX = 5;
    private int gridSizeY = 5;

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

    public float Step(int action)
    {
        // Change position if valid action index
        if(action < actionSize)
        {
            Vector2Int nextState;
            switch (action)
            {
                case 0:
                    // Up
                    nextState = currentState + Vector2Int.up;
                    break;
                case 1:
                    // Down
                    nextState = currentState + Vector2Int.down;
                    break;
                case 2:
                    // Left
                    nextState = currentState + Vector2Int.left;
                    break;
                case 3:
                    // Right
                    nextState = currentState + Vector2Int.right;
                    break;
                default:
                    // Nothing happens
                    nextState = currentState;
                    break;
            }
            // Dirty check if we have a valid transition
            if ((!blockingCells.Contains(nextState)) && 
                (-1 < nextState.x) && 
                (nextState.x < gridSizeX) &&
                (-1 < nextState.y) && 
                (nextState.y < gridSizeY))

            {
                currentState = nextState;
                Debug.Log(String.Concat("Entering cell ", currentState.ToString()));
            }
        }

        // Change position of the agent in the actual world
        agent.DeActivateUIButtons();
        Vector3 destination = grid.GetCellCenterWorld(new Vector3Int(currentState.x, currentState.y, 0));
        teleporter.Teleport(agent.transform, destination);
        agent.ActivateUIButtons();

        // Provide reward
        if(currentState == GoalState) {
            Debug.Log("Reached goal state");
            done = true;
            coin.SetActive(true);
            return 10f;
        }
        else { return 0; }
    }
    private void Awake()
    {
        agent = GameObject.Find("Agent").GetComponent<Agent>();
        grid = GameObject.Find("FloorGrid").GetComponent<Grid>();
        coin = GameObject.Find("Coin");
        coin.SetActive(false);
    }
}
