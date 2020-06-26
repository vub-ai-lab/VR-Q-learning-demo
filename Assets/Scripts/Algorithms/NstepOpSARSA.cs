﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Action = Enums.Action;
using System.Linq;

public class NstepOpSARSA : Algorithm
{

    Queue<Vector2Int> savedStates = new Queue<Vector2Int>();
    Queue<Action> savedActions = new Queue<Action>();
    Queue<float> savedRewards = new Queue<float>();


    private void UpdateQTable(Vector2Int state, Action action, bool terminal)
    {
        float blop = terminal ? RewardSum(state, action) : RewardSum(state, action) + Mathf.Pow(discount_factor, nsteps) * q_table[state.x, state.y][action];

        Vector2Int stateToUpdate = savedStates.Dequeue();
        Action actionToUpdate = savedActions.Dequeue();
        float rewardToUpdate = savedRewards.Dequeue();

        Debug.Log("STATE TO UPDATE:" + stateToUpdate);
        float delta = blop - q_table[stateToUpdate.x, stateToUpdate.y][actionToUpdate];
        q_table[stateToUpdate.x, stateToUpdate.y][actionToUpdate] += learning_rate * importanceSamplingRatio()  * delta;

        traces[stateToUpdate.x, stateToUpdate.y][actionToUpdate] = 0.5f;
    }

    private float RewardSum(Vector2Int state, Action action)
    {
        int index = 0;
        float rewardSum = 0;
        foreach (float reward in savedRewards)
        {
            if (index == nsteps)
            {
                break;
            }

            rewardSum += reward * Mathf.Pow(discount_factor, index);
            index++;
        }

        return rewardSum;
    }

    private float importanceSamplingRatio()
    {
        int index = 0;
        float ratio = 1;
        foreach(Action action in savedActions)
        {
            Vector2Int state = savedStates.ElementAt(index);
            //float numerator = pi.GetPickChance(q_table[state.x,state.y], action, agent.env.getActions(state), epsylon, temperature);
            float numerator = MaxValue(q_table[state.x, state.y], action, agent.env.getActions(state));  
            float denominator = agent.GetPickChance(state,action, agent.env.getActions(state));

            Debug.Log("NUMERATOR: " + numerator);
            Debug.Log("denominator: " + denominator);

            ratio *= numerator / denominator;
            index++;
        }

       
        Debug.Log("RATIO: " + ratio);
        return ratio;
    }

    private float MaxValue(Dictionary<Action, float> q_table, Action selectedAction, List<Action> actions)
    {
        List<Action> maxList = new List<Action>();
        float maxVal = q_table.Values.Max();

        //Construct list of maximum Qvalues
        foreach (Action action in actions)
        {
            if (q_table[action] == maxVal)
            {
                maxList.Add(action);
            }
        }

        if (maxList.Contains(selectedAction))
        {
            return 1.0f / maxList.Count();
        }
        else
        {
            return 0f;
        }
    }


    private void FlushRemaining(Vector2Int finalState, Action action)
    {

        //Covers edge case when the amount of steps the user takes is smaller than n
        if (savedStates.Count == 4)
        {
            UpdateQTable(finalState, action, false);
        }

        while (savedStates.Count > 0)
        {
            UpdateQTable(finalState, action, true);
        }

    }


    public override void UpdateValues(Action action, Vector2Int nextState, float reward, bool terminal)
    {

        //Set the trace to 1 to create a trail of n-steps
        traces[lastState.x, lastState.y][action] = 1;

        //Push state action and reward on the corresponding queue
        savedStates.Enqueue(lastState);
        savedActions.Enqueue(action);
        savedRewards.Enqueue(reward);

        if (terminal)
        {
            FlushRemaining(lastState, action);
        }
        else if (savedStates.Count > nsteps)
        {
            UpdateQTable(lastState, action, terminal);
        }

        lastState = nextState;

    }


    public override void Initialize()
    {

        q_table = new Dictionary<Action, float>[agent.env.gridSizeX, agent.env.gridSizeY];
        traces = new Dictionary<Action, float>[agent.env.gridSizeX, agent.env.gridSizeY];
        ClearMemory();
        lastState = agent.env.getCurrentState();
        agent.envGUI.AddPolicySliders();

        // FIXME VRTK teleporter would be preferred here but headset happens not to be enable when this is executed
        transform.localPosition = agent.envGUI.tilemap.GetCellCenterWorld(new Vector3Int(lastState.x, lastState.y, 0));


    }


}
