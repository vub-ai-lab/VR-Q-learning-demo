using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Action = Enums.Action;
using VRTK;

public class SARSA : Algorithm
{


    // Learning memory
    private Vector2Int prevState;

    private Action prevAction;

    private float prevReward;

    private void UpdateQTable(Vector2Int state, Action firstAction, Action secondAction, Vector2Int nextState, float reward, bool terminal)
    {
        float blop = terminal ? 0 : discount_factor * q_table[nextState.x, nextState.y][secondAction];
        float delta = reward + blop - q_table[state.x, state.y][firstAction];
        traces[state.x, state.y][firstAction] = 1;
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


        q_table = new Dictionary<Action, float>[env.gridSizeX, env.gridSizeY];
        traces = new Dictionary<Action, float>[env.gridSizeX, env.gridSizeY];
        ClearMemory();
        lastState = env.getCurrentState();
        //prevState is still non existant
        prevState.Set(-1, -1);
        epsylon = 0.2f;
        envGUI.AddPolicySliders();

        // FIXME VRTK teleporter would be preferred here but headset happens not to be enable when this is executed
        transform.localPosition = envGUI.tilemap.GetCellCenterWorld(new Vector3Int(lastState.x, lastState.y, 0));

    }



}
