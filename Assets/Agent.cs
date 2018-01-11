using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Agent : MonoBehaviour {

    float [ , ][] q_table;   // The matrix containing the q-value estimates.
    
    // Algorithm parameters
    public float learning_rate = 0.5f; // The rate at which to update the value estimates given a reward.
    public float discount_factor = 1.0f; // Discount factor for calculating Q-target.

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

    Button[] buttons;

    // Environment
    GridWorld env;
    enum Action { Up, Down, Left, Right };
    int actionSize = Enum.GetNames(typeof(Action)).Length;

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

    private IEnumerator WaitAndReset(float time)
    {
        // Deactivate buttons
        DeActivateUIButtons();
        // Pause
        yield return new WaitForSeconds(time);
        // Reset environment
        env.Reset();
        // Activate buttons
        ActivateUIButtons();
    }

    // Use this for initialization
    void Start () {
        
        for(int x=0; x < 5; x++)
        {
            for (int y = 0; y< 5; y++)
            {
                q_table[x, y] = new float[actionSize];
            }
        }
        
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
            StartCoroutine(WaitAndReset(5));
            episodeCount += 1;
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
        episodeCount = 0;
        stepCount = 0;
        q_table = new float[5, 5][];
        lastState = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        buttons = new Button[4] {GameObject.Find("ForwardButton").GetComponent<Button>(),
                                 GameObject.Find("BackwardButton").GetComponent<Button>(),
                                 GameObject.Find("LeftButton").GetComponent<Button>(),
                                 GameObject.Find("RightButton").GetComponent<Button>()};
    }

    void UpdateUI()
    {
        Q_UpEstimText.text =  GetQval(lastState, 0).ToString();
        Q_DownEstimText.text = GetQval(lastState, 1).ToString();
        Q_LeftEstimText.text = GetQval(lastState, 2).ToString();
        Q_RightEstimText.text = GetQval(lastState, 3).ToString();
    }


    public void ActivateUIButtons() { SetInteractableButtons(true); }
    public void DeActivateUIButtons() { SetInteractableButtons(false); }
    private void SetInteractableButtons(bool value)
    {
        foreach(Button button in buttons)
        {
            button.interactable = true;
        }
    }
}
