using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Action = Enums.Action;

public class QlearningWtraces : Algorithm
{
    public override float GetTraceValue(Vector2Int state, Action action)
    {
        return 1;
    }

    public override float GetStateValue(Vector2Int state)
    {
        return 1;
    }

    public override float GetQval(Vector2Int state, Action action)
    {
        return 1;
    }

    public override string Test()
    {
        return "Henlo world";
    }

}
