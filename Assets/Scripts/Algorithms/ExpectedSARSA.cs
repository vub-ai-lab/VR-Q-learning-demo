using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Action = Enums.Action;
using VRTK;

public class ExpectedSARSA : Algorithm
{

    private void UpdateQTable(Vector2Int state, Action action, Vector2Int nextState, float reward, bool terminal)
    {
        float blop = terminal ? 0 : discount_factor * expectedSum(nextState);
        float delta = reward + blop - q_table[state.x, state.y][action];
        traces[state.x, state.y][action] = 1;
        for (int x = 0; x < agent.env.gridSizeX; x++)
        {
            for (int y = 0; y < agent.env.gridSizeY; y++)
            {
                foreach (Action a in new List<Action>(q_table[x, y].Keys))
                {
                    q_table[x, y][a] += learning_rate * delta * traces[x, y][a];
                    traces[x, y][a] *= discount_factor * trace_decay;
                }
            }
        }
    }

    private float expectedSum(Vector2Int state)
    {
        float sum = 0;
        List<Action> actions = agent.env.getActions(state);

        foreach (Action action in actions)
        {
            sum += agent.GetPickChance(state, action, actions) * q_table[state.x, state.y][action];
        }

        return sum;
    }


    public override void UpdateValues(Action action, Vector2Int nextState, float reward, bool terminal)
    {
        UpdateQTable(lastState, action, nextState, reward, terminal);
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
