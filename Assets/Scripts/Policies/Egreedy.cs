using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using Action = Enums.Action;

public class Egreedy : Policy
{

    // Gives back the chance a policy assigns to a given state action pair
    public override float GetPickChance(Dictionary<Action, float> q_table, Action selectedAction, List<Action> actions, float epsylon, float temperature)
    {

        List<Action> maxList = new List<Action>();
        float maxVal = q_table.Values.Max();

        //Construct list of maximum Qvalues
        foreach (Action action in actions)
        {
            if (q_table[action] == maxVal)
            {
                maxList.Add(action);
            }
        }

        if (maxList.Count == actions.Count) //In case the Qvalue for every state action pair is equal
        {
            return 1.0f / maxList.Count;
        }
        else if (maxList.Contains(selectedAction))
        {
            return (1.0f - epsylon) / maxList.Count + epsylon / actions.Count;
        }
        else
        {
            return epsylon / actions.Count;
        }

    }


    public override void prepareSettingsMenu()
    {
        string[] sliders = new string[] { "LRslider", "DFslider", "TDslider", "Eslider", "Ptoggle" };
        menu.InitializeMenu(sliders);
    }

}
