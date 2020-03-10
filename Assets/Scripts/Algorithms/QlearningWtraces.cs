using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Action = Enums.Action;
using VRTK;

public class QlearningWtraces : Algorithm
{

    private void UpdateQTable(Vector2Int state, Action action, Vector2Int nextState, float reward, bool terminal)
    {
        float blop = terminal ? 0 : discount_factor * q_table[nextState.x, nextState.y].Values.Max();
        float delta = reward + blop - q_table[state.x, state.y][action];
        traces[state.x, state.y][action] = 1;
        for (int x = 0; x < env.gridSizeX; x++)
        {
            for (int y = 0; y < env.gridSizeY; y++)
            {
                foreach (Action a in new List<Action>(q_table[x, y].Keys))
                {
                    q_table[x, y][a] += learning_rate * delta * traces[x, y][a];
                    traces[x, y][a] *= discount_factor * trace_decay;
                }
            }
        }
    }


    public override void UpdateValues(Action action, Vector2Int nextState, float reward, bool terminal)
    {
        UpdateQTable(lastState, action, nextState, reward, terminal);
        lastState = nextState;

    }

    public override void Initialize()
    {


        q_table = new Dictionary<Action, float>[env.gridSizeX, env.gridSizeY];
        traces = new Dictionary<Action, float>[env.gridSizeX, env.gridSizeY];
        ClearMemory();
        lastState = env.getCurrentState();

        // FIXME VRTK teleporter would be preferred here but headset happens not to be enable when this is executed
        transform.localPosition = envGUI.tilemap.GetCellCenterWorld(new Vector3Int(lastState.x, lastState.y, 0));


    }

    public override string Test()
    {
        return "QLEARNING TEST";
    }


}
