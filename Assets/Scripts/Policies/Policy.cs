using System.Collections;
using System.Collections.Generic;
using Action = Enums.Action;
using UnityEngine;

public abstract class Policy : MonoBehaviour
{
    // Menu
    public MenuCreator menu;

    public abstract float GetPickChance(Dictionary<Action, float> q_table, Action selectedAction, List<Action> actions, float epsylon, float temperature);

    public abstract void prepareSettingsMenu(string algorithm);

}
