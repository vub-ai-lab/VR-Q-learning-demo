using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Action = Enums.Action;
using VRTK;

public class QlearningWtraces : Algorithm
{

    public override void UpdateQTable(Vector2Int state, Action action, Vector2Int nextState, float reward, bool terminal)
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


    public override string Test()
    {
        return "Henlo world";
    }


}
