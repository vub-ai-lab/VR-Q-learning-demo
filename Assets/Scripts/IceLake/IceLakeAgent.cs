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

    private int steps = 0;

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

        //algorithm.UpdateValues(action, nextState, reward, done);
        
        
        if(steps == 0)
        {
            Debug.Log("UPDATED VALUES");
            algorithm.UpdateValues(action, nextState, reward, done);
        }
        

        return true;
    }


    private void WalkOrSlip(Action chosen_action)
    {
        Vector2Int prevState = algorithm.LastState;

        //Let the environment determine how many steps the agent will take
        steps = env.DetermineSteps(chosen_action);

        if(steps >= 0)
        {
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

            

        }

        Debug.Log("MOVED");
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
            case "Qlearning":
                algorithm = qlearningWtraces;
                AlgorithmName.text = "Qlearning";
                Debug.Log("LoadedQlearning");
                break;
            case "SARSA":
                algorithm = sarsa;
                AlgorithmName.text = "SARSA";
                Debug.Log("Loaded SARSA");
                break;
            case "ExpectedSARSA":
                algorithm = expectedSarsa;
                AlgorithmName.text = "E. SARSA";
                Debug.Log("Loaded Expected SARSA");
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
