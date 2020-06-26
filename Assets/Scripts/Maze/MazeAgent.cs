using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using Action = Enums.Action;
using VRTK;



[Serializable]
public class MazeAgent : Agent
{

    // Handle player movement
    protected override bool Act(Action action)
    {

        float reward;
        try
        {
            reward = env.Step(action);
        }
        catch (InvalidOperationException e)
        {
            Debug.Log("Action invalid");
            return false;
        }
        Debug.Log("Received reward: " + reward.ToString());
        Vector2Int nextState = env.getCurrentState();
        bool done = env.isTerminal();

        algorithm.UpdateValues(action, nextState, reward, done);
        return true;
    }


    private void WalkUntilCrossing(Action chosen_action, Action crossing_action1, Action crossing_action2)
    {
        Vector2Int prevState = algorithm.LastState;

        while (Act(chosen_action))
        {
            var actions = env.getActions(env.getCurrentState());
            if (actions.Contains(crossing_action1) ||
                actions.Contains(crossing_action2))
                break;
        }

        envGUI.moveAgentInGameWorld(prevState, algorithm.LastState);
    }


    public override void MoveForward()
    {
        WalkUntilCrossing(Action.up, Action.left, Action.right);
    }

    public override void MoveBackward()
    {
        WalkUntilCrossing(Action.down, Action.left, Action.right);
    }

    public override void MoveLeft()
    {
        WalkUntilCrossing(Action.left, Action.up, Action.down);
    }

    public override void MoveRight()
    {
        WalkUntilCrossing(Action.right, Action.up, Action.down);
    }

}


