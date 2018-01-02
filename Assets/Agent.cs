using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class Agent : MonoBehaviour {

    public float [ , ][] q_table;   // The matrix containing the q-value estimates.
    public float learning_rate = 0.5f; // The rate at which to update the value estimates given a reward.
    int action = -1;
    int actionSize = 4; // Up, down, left, right
    public float discount_factor = 0.99f; // Discount factor for calculating Q-target.
    Vector2Int lastState;

    GridWorld env;

    /// <summary>
    /// Gets the current Estimate of the State Value
    /// </summary>
    /// <returns>The V-value of the current state.</returns>
	public float GetStateValue()
    {
        return GetStateValue(lastState);
    }

    private float GetStateValue(Vector2Int state)
    {
        float result = q_table[state.x, state.y].Average();
        return result;
    }

    /// <summary>
    /// Updates the value estimate matrix given a new experience (state, action, reward).
    /// </summary>
    /// <param name="state">The environment state the experience happened in.</param>
    /// <param name="reward">The reward recieved by the agent from the environment for it's action.</param>
    /// <param name="done">Whether the episode has ended</param>
    public void SendState(Vector2Int nextState, float reward, bool done) {
        if (action != -1)
        {
            if (done == true)
            {
                q_table[lastState.x, lastState.y][action] += learning_rate * (reward - q_table[lastState.x, lastState.y][action]);
            }
            else
            {
                float q_max_nextState = q_table[nextState.x, nextState.y].Max();
                q_table[lastState.x,lastState.y][action] += learning_rate * (reward + discount_factor * q_max_nextState - q_table[lastState.x,lastState.y][action]);
            }
        }
        lastState = nextState;
    }

    // Use this for initialization
    void Start () {
        q_table = new float[4, 4][];
        for(int x=0; x < 4; x++)
        {
            for (int y = 0; y< 4; y++)
            {
                q_table[x, y] = new float[actionSize];
            }
        }
        lastState = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
    }

    private void Learn(int action)
    {
        float reward = env.Step(action);
        Vector2Int nextState = env.getCurrentState();
        bool done = env.done;
        SendState(nextState, reward, done);
    }

    public void MoveForward()
    {
        Learn(0);

    }

    public void MoveBackward()
    {
        Learn(1);

    }

    public void MoveLeft()
    {
        Learn(2);

    }

    public void MoveRight()
    {
        Learn(3);

    }

    private void Awake()
    {
        env = GameObject.Find("GridWorld").GetComponent<GridWorld>();
    }
}
