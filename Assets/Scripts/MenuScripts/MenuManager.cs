using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public Panel currentPanel = null;

    private List<Panel> panelHistory = new List<Panel>();
    
    private void Start(){
        SetupPanels();
        Debug.Log("Panels have been set up");
    }

    private void SetupPanels(){
        Panel[] panels = GetComponentsInChildren<Panel>();


        foreach (Panel panel in panels)
        {
            panel.Setup(this);
        }

        currentPanel.Show();
    }

    public void GoToPrevious(){
        if(panelHistory.Count == 0)
            return;

        SetCurrent(panelHistory[panelHistory.Count - 1]);
        panelHistory.RemoveAt(panelHistory.Count - 1);
    }

    public void SetCurrentWithHistory(Panel newPanel){
        Debug.Log("Clicked the button");

        panelHistory.Add(currentPanel);
        SetCurrent(newPanel);
    }

    private void SetCurrent(Panel newPanel){
        currentPanel.Hide();

        currentPanel = newPanel;
        currentPanel.Show();
    }
}
