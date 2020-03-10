using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Action = Enums.Action;
using VRTK;


// Abstract class used to represent an interface for the implementation of the reinforcement learning algorithms
abstract public class Algorithm : MonoBehaviour
{
    // Algorithm parameters
    // These are adjustable in a GUI menu
    [Range(0f, 1f)]
    protected float learning_rate = 0.4f; // The rate at which to update the value estimates given a reward.
    [Range(0f, 1f)]
    protected float discount_factor = 0.9f; // Discount factor for calculating Q-target.
    [Range(0f, 1f)]
    protected float trace_decay = 0.8f; // Factor Lambda to decrease eligibility traces

    [Range(0f, 1f)]
    protected float Epsylon = 0f; // Chance for exploration

    // Learning Memory
    protected Vector2Int lastState;
    protected Dictionary<Action, float>[,] q_table;   // The matrix containing the q-value estimates.
    protected Dictionary<Action, float>[,] traces;  // Matrix containing the eligibility traces

    // Environment
    public GridWorld env;
    public GridWorldGUI envGUI;

    public Vector2Int LastState
    {
        get
        {
            return lastState;
        }

        set
        {
            lastState = value;
            Debug.Log("Set new last state");
        }

    }

    public float Learning_rate
    {
        get
        {
            return learning_rate;
        }

        set
        {
            learning_rate = value;
            Debug.Log("Set new learning rate.");
        }
    }

    public float Discount_factor
    {
        get
        {
            return discount_factor;
        }

        set
        {
            discount_factor = value;
            Debug.Log("Set new discount factor.");
        }
    }

    public float Trace_decay
    {
        get
        {
            return trace_decay;
        }

        set
        {
            trace_decay = value;
            Debug.Log("Set new trace decay.");
        }
    }

    public float GetStateValue(Vector2Int state)
    {
        // we assume a greedy policy
        try
        {
            return q_table[state.x, state.y].Values.Max();
        }
        catch (InvalidOperationException e)
        {
            return 0f;
        }
    }

    public float GetTraceValue(Vector2Int state, Action action)
    {
        return traces[state.x, state.y][action];
    }

    public float GetQval(Vector2Int state, Action action)
    {
        return q_table[state.x, state.y][action];
    }

    public float GetPickChance(Vector2Int state, Action selectedAction, List<Action> actions)
    {

        List<Action> maxList = new List<Action>();
        float maxVal = q_table[state.x, state.y].Values.Max();

        //Construct list of maximum Qvalues
        foreach (Action action in actions)
        {
            if (q_table[state.x, state.y][action] == maxVal)
            {
                maxList.Add(action);
            }
        }

        if (maxList.Count == actions.Count) //In case the Qvalue for every state action pair is equal
        {
            return 1.0f / maxList.Count;
        }
        else if (maxList.Contains(selectedAction))
        {
            return (1.0f - Epsylon) / maxList.Count;
        }
        else
        {
            return Epsylon / (actions.Count - maxList.Count);
        }

    }


    public abstract void UpdateValues(Action action, Vector2Int nextState, float reward, bool terminal);

    public abstract string Test();


    public abstract void Initialize();

    protected void ClearQtable()
    {
        for (int x = 0; x < env.gridSizeX; x++)
        {
            for (int y = 0; y < env.gridSizeY; y++)
            {
                List<Action> actions = env.getActions(new Vector2Int(x, y));
                Dictionary<Action, float> dict = new Dictionary<Enums.Action, float>();

                foreach (Action a in actions)
                    dict.Add(a, 0f);

                q_table[x, y] = dict;
            }
        }
    }

    protected void ClearTraces()
    {
        for (int x = 0; x < env.gridSizeX; x++)
        {
            for (int y = 0; y < env.gridSizeY; y++)
            {
                List<Action> actions = env.getActions(new Vector2Int(x, y));
                Dictionary<Action, float> dict = new Dictionary<Enums.Action, float>();

                foreach (Action a in actions)
                    dict.Add(a, 0f);

                traces[x, y] = dict;
            }
        }
    }

    public void ClearMemory()
    {
        ClearQtable();
        ClearTraces();
    }

    public void RestartLearning()
    {

        ClearMemory();
        ResetEpisode();
    }

    public void ResetEpisode()
    {
        ClearTraces();
        envGUI.ResetEpisode();
        lastState = env.getCurrentState();
        envGUI.moveAgentInGameWorld(lastState, lastState, true);
    }

}
