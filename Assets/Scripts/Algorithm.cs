using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Action = Enums.Action;


// Abstract class used to represent an interface for the implementation of the reinforcement learning algorithms
abstract public class Algorithm : MonoBehaviour
{
    public abstract float GetTraceValue(Vector2Int state, Action action);

    public abstract float GetStateValue(Vector2Int state);

    public abstract float GetQval(Vector2Int state, Action action);

    public abstract string Test();
}
