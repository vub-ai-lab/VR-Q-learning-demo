using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Action = Enums.Action;

public class Softmax : Policy
{
    // Gives back the chance a policy assigns to a given state action pair
    public override float GetPickChance(Dictionary<Action, float> q_table, Action selectedAction, List<Action> actions, float epsylon, float temperature)
    {
        float scaledTemperature = (float)Math.Pow(10, temperature);
        float allStateSum = 0;

        foreach (Action action in actions)
        {
            allStateSum += (float)Math.Pow(Math.E, q_table[action] / scaledTemperature);
        }

        return (float)Math.Pow(Math.E, q_table[selectedAction] / scaledTemperature) / allStateSum;

    }

    public override void prepareSettingsMenu(string algorithm)
    {
        string[] sliders;

        if (algorithm == "nstepSARSA" || algorithm == "nstepOpSARSA")
        {
            sliders = new string[] { "LRslider", "DFslider", "TDslider", "Tslider", "Nslider", "Ptoggle" };
        }
        else
        {
            sliders = new string[] { "LRslider", "DFslider", "TDslider", "Tslider", "Ptoggle" };
        }

        menu.InitializeMenu(sliders);
    }
}
