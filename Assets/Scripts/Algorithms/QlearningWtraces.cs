﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Action = Enums.Action;
using VRTK;

public class QlearningWtraces : Algorithm
{

    // Learning memory
    private Action prevAction;

    private float prevReward;

    private void UpdateQTable(Vector2Int state, Action firstAction, Action secondAction, Vector2Int nextState, float reward, bool terminal)
    {
        float blop = terminal ? 0 : discount_factor * q_table[nextState.x, nextState.y].Values.Max();
        float delta = reward + blop - q_table[state.x, state.y][firstAction];
        traces[state.x, state.y][firstAction] = 1;
        // Check if the player picked the best action
        bool reset = false;
        float bestAction = 0;
        List<Action> actions = agent.env.getActions(nextState);

        // Find the best action
        foreach (Action action in actions)
        {
            float chance = agent.GetPickChance(nextState, action, actions);
            if(chance > bestAction)
            {
                bestAction = chance;
            }
        }

        
        // Check if best action matches with the selected action
        if(terminal || agent.GetPickChance(nextState, secondAction, actions) >= bestAction){
            reset = false;
        }
        else
        {
            reset = true;
        }

    

        for (int x = 0; x < agent.env.gridSizeX; x++)
        {
            for (int y = 0; y < agent.env.gridSizeY; y++)
            {
                foreach (Action a in new List<Action>(q_table[x, y].Keys))
                {
                    q_table[x, y][a] += learning_rate * delta * traces[x, y][a];
                    if (reset)
                    {
                        traces[x, y][a] = 0;
                    }
                    else
                    {
                        traces[x, y][a] *= discount_factor * trace_decay;
                    }
                }
            }
        }
    }



    public override void UpdateValues(Action action, Vector2Int nextState, float reward, bool terminal)
    {
        if (prevState[0] > 0)
        {
            UpdateQTable(prevState, prevAction, action, lastState, prevReward, false);
        }

        prevReward = reward;
        prevState = lastState;
        lastState = nextState;
        prevAction = action;

        if (terminal)
        {
            UpdateQTable(prevState, prevAction, action, lastState, prevReward, true);
        }
    }

    public override void Initialize()
    {


        q_table = new Dictionary<Action, float>[agent.env.gridSizeX, agent.env.gridSizeY];
        traces = new Dictionary<Action, float>[agent.env.gridSizeX, agent.env.gridSizeY];
        ClearMemory();
        lastState = agent.env.getCurrentState();
        prevState.Set(-1, -1);
        agent.envGUI.AddPolicySliders();

        // FIXME VRTK teleporter would be preferred here but headset happens not to be enable when this is executed
        transform.localPosition = agent.envGUI.tilemap.GetCellCenterWorld(new Vector3Int(lastState.x, lastState.y, 0));
        

    }

}
