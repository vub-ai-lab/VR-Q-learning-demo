using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using Action = Enums.Action;
using VRTK;



[Serializable]
public class IceLakeAgent : Agent
{

    //Random number generator
    protected static System.Random rnd = new System.Random();

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


    private void WalkOrSlip(Action chosen_action)
    {
        Vector2Int prevState = algorithm.LastState;
        int steps = 0;

        /*Determine if the user should slip or not
         * 0 = don't slip
         * 1 = slip a random amount of square
         */
        int slip = rnd.Next(2);

        if (slip > 0)
            if(rnd.Next(2) > 0)
            {
                steps = 1;
            } else
            {
                steps = 2;
            }


        while (Act(chosen_action))
        {

            if (steps == 0)
            {
                break;
            }
            else
            {
                steps -= 1;
            }

        }

        envGUI.moveAgentInGameWorld(prevState, algorithm.LastState);

    }


    public override void MoveForward()
    {
        WalkOrSlip(Action.up);
    }

    public override void MoveBackward()
    {
        WalkOrSlip(Action.down);
    }

    public override void MoveLeft()
    {
        WalkOrSlip(Action.left);
    }

    public override void MoveRight()
    {
        WalkOrSlip(Action.right);
    }

    public override void Start()
    {

        // Get the algorithm the user selected in the StartMenu
        algorithmType = UnityEngine.PlayerPrefs.GetString("Algorithm");

        switch (algorithmType)
        {
            case "nstepSARSA":
                algorithm = qlearningWtraces;
                AlgorithmName.text = "Qlearning";
                Debug.Log("LoadedQlearning");
                break;
            case "nstepOffpolicySARSA":
                algorithm = sarsa;
                AlgorithmName.text = "SARSA";
                Debug.Log("Loaded SARSA");
                break;
        }

        //Get the policy the user selected in the StartMenu
        policyType = UnityEngine.PlayerPrefs.GetString("Policy");
        switch (policyType)
        {
            case "Egreedy":
                policy = egreedy;
                PolicyName.text = "ε-greedy";
                Debug.Log("Loaded egreedy");
                break;
            case "Softmax":
                policy = softmax;
                PolicyName.text = "Softmax";
                Debug.Log("Loaded softmax");
                break;
        }

        algorithm.Initialize();
    }

}
