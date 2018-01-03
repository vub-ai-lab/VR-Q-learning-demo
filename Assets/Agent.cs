using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Agent : MonoBehaviour {

    float [ , ][] q_table;   // The matrix containing the q-value estimates.
    int actionSize = 4; // Up, down, left, right

    // Algorithm parameters
    public float learning_rate = 0.5f; // The rate at which to update the value estimates given a reward.
    public float discount_factor = 0.99f; // Discount factor for calculating Q-target.

    // Misc 
    Vector2Int lastState;
    private int episodeCount;
    private int stepCount;

    // UI vars
    public Text episodeCountText;
    public Text stepCountText;

    public Text Q_UpEstimText;
    public Text Q_DownEstimText;
    public Text Q_LeftEstimText;
    public Text Q_RightEstimText;

    // Environment ref
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

    float GetQval(Vector2Int state, int action)
    {
        return q_table[state.x, state.y][action];
    }

    /// <summary>
    /// Updates the value estimate matrix given a new experience (state, action, reward).
    /// </summary>
    /// <param name="state">The environment state the experience happened in.</param>
    /// <param name="reward">The reward recieved by the agent from the environment for it's action.</param>
    /// <param name="done">Whether the episode has ended</param>
    public void UpdateQTable(int action, Vector2Int state, float reward, bool done) {
        if (action != -1)
        {
            if (done == true)
            {
                q_table[lastState.x, lastState.y][action] += learning_rate * (reward - q_table[lastState.x, lastState.y][action]);  
            }
            else
            {
                float q_max_nextState = q_table[state.x, state.y].Max();
                q_table[lastState.x,lastState.y][action] += learning_rate * (reward + discount_factor * q_max_nextState - q_table[lastState.x,lastState.y][action]);
            }
        }
    }

    private IEnumerator Waiter()
    {
        Debug.Log(Time.time);
        yield return new WaitForSecondsRealtime(5);
        Debug.Log(Time.time);
    }

    // Use this for initialization
    void Start () {
        episodeCount = 0;
        stepCount = 0;
        q_table = new float[4, 4][];
        for(int x=0; x < 4; x++)
        {
            for (int y = 0; y< 4; y++)
            {
                q_table[x, y] = new float[actionSize];
            }
        }
        lastState = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        UpdateUI();
    }

    private void Act(int action)
    {
        float reward = env.Step(action);
        Debug.Log("Received reward: " + reward.ToString());
        Vector2Int nextState = env.getCurrentState();
        bool done = env.done;
        UpdateQTable(action, nextState, reward, done);
        lastState = nextState;
        if (done)
        {
            Debug.Log("Resetting environment");
            StartCoroutine(Waiter());
            env.Reset();
            episodeCount = 0;
            stepCount = 0;
        }
        else
        {
            stepCount += 1;
        }
        UpdateUI();

    }

    public void MoveForward()
    {
        Act(0);

    }

    public void MoveBackward()
    {
        Act(1);

    }

    public void MoveLeft()
    {
        Act(2);

    }

    public void MoveRight()
    {
        Act(3);

    }

    private void Awake()
    {
        env = GameObject.Find("GridWorld").GetComponent<GridWorld>();
    }

    void UpdateUI()
    {
        Q_UpEstimText.text = "Estimated Q-value: " + GetQval(lastState, 0);
        Q_DownEstimText.text = "Estimated Q-value: " + GetQval(lastState, 1);
        Q_LeftEstimText.text = "Estimated Q-value: " + GetQval(lastState, 2);
        Q_RightEstimText.text = "Estimated Q-value: " + GetQval(lastState, 3);

        //episodeCountText.text = "Episode: " + episodeCount.ToString();
        //stepCountText.text = "Step: " + stepCount.ToString();
    }
}
