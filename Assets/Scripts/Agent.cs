using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using Action = Enums.Action;
using VRTK;



[Serializable]
abstract public class Agent : MonoBehaviour
{


    // Algorithms
    public QlearningWtraces qlearningWtraces;
    public SARSA sarsa;
    public ExpectedSARSA expectedSarsa;
    protected String algorithmType;
    protected Algorithm algorithm;

    //Policies
    public Softmax softmax;
    public Egreedy egreedy;
    protected String policyType;
    protected Policy policy;

    //Textfields
    public Text AlgorithmName;
    public Text PolicyName;

    //Environment
    public GridWorld env;
    public GridWorldGUI envGUI;

    public float Learning_rate
    {
        get
        {
            return algorithm.Learning_rate;
        }

        set
        {
            algorithm.Learning_rate = value;
            Debug.Log("Set new learning rate.");
        }
    }

    public float Discount_factor
    {
        get
        {
            return algorithm.Discount_factor;
        }

        set
        {
            algorithm.Discount_factor = value;
            Debug.Log("Set new discount factor.");
        }
    }

    public float Trace_decay
    {
        get
        {
            return algorithm.Trace_decay;
        }

        set
        {
            algorithm.Trace_decay = value;
            Debug.Log("Set new trace decay.");
        }
    }

    public float Epsylon
    {
        get
        {
            return algorithm.Epsylon;
        }

        set
        {
            algorithm.Epsylon = value;
            Debug.Log("Set new epsylon");
        }
    }

    public float Temperature
    {
        get
        {
            return algorithm.Temperature;
        }

        set
        {
            algorithm.Temperature = value;
            Debug.Log("Set new temperature");
        }
    }
    /// <summary>
    /// Gets the current Estimate of the State Value
    /// </summary>
    /// <returns>The V-value of the current state.</returns>
    public float GetCurrentStateValue()
    {
        return GetStateValue(algorithm.LastState);
    }

    public float GetStateValue(Vector2Int state)
    {
        return algorithm.GetStateValue(state);
        ;
    }

    public float GetQval(Vector2Int state, Action action)
    {
        return algorithm.GetQval(state, action);
    }


    public float GetTraceValue(Vector2Int state, Action action)
    {
        return algorithm.GetTraceValue(state, action);
    }

    public float GetPickChance(Vector2Int state, Action selectedAction, List<Action> actions)
    {
        return policy.GetPickChance(algorithm.q_table[state.x, state.y], selectedAction, actions, algorithm.Epsylon, algorithm.Temperature);
    }

    /// <summary>
    /// Updates the value estimate matrix given a new experience (state, action, reward).
    /// </summary>
    /// <param name="state">The environment state the experience happened in.</param>
    /// <param name="reward">The reward recieved by the agent from the environment for it's action.</param>
    /// <param name="done">Whether the episode has ended</param>

    // Handle player movement
    protected abstract bool Act(Action action);

    public abstract void MoveForward();

    public abstract void MoveBackward();

    public abstract void MoveLeft();

    public abstract void MoveRight();


    abstract public void Start();

    public void InitializeMenu()
    {
        policy.prepareSettingsMenu();
    }

    public void ClearMemory()
    {
        algorithm.ClearMemory();
    }

#if DEBUG
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
            MoveForward();
        else if (Input.GetKeyDown(KeyCode.S))
            MoveBackward();
        else if (Input.GetKeyDown(KeyCode.A))
            MoveLeft();
        else if (Input.GetKeyDown(KeyCode.D))
            MoveRight();
        else if (Input.GetKeyDown(KeyCode.E))
            ResetEpisode();
    }
#endif


    public void RestartLearning()
    {
        algorithm.RestartLearning();
    }

    public void ResetEpisode()
    {
        algorithm.ResetEpisode();
    }

    public void ResetPosition()
    {
        algorithm.ResetPosition();
    }

    public void BackToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("StartMenu");
    }
}


