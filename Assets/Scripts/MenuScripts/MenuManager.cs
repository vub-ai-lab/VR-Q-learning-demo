using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

public class MenuManager : MonoBehaviour
{
    public Panel currentPanel = null;

    private List<Panel> panelHistory = new List<Panel>();

    private void Start()
    {
        SetupPanels();
        Debug.Log("Panels have been set up");
    }

    private void SetupPanels()
    {
        Panel[] panels = GetComponentsInChildren<Panel>();


        foreach (Panel panel in panels)
        {
            panel.Setup(this);
        }

        currentPanel.Show();
    }

    public void GoToPrevious()
    {
        if (panelHistory.Count == 0)
            return;

        SetCurrent(panelHistory[panelHistory.Count - 1]);
        panelHistory.RemoveAt(panelHistory.Count - 1);
    }

    public void SetCurrentWithHistory(Panel newPanel)
    {
        Debug.Log("Clicked the button");

        panelHistory.Add(currentPanel);
        SetCurrent(newPanel);
    }

    private void SetCurrent(Panel newPanel)
    {
        currentPanel.Hide();

        currentPanel = newPanel;
        currentPanel.Show();
    }

    //Load the maze scene
    private void ToMaze()
    {

        UnityEngine.SceneManagement.SceneManager.LoadScene("main");

    }

    //Send the selected algorithm to the agent
    public void LoadQlearning()
    {     
        UnityEngine.PlayerPrefs.SetString("Algorithm", "Qlearning");
        UnityEngine.PlayerPrefs.SetString("Policy", "Egreedy");
        ToMaze();
    }

    public void LoadQlearningSoftmax()
    {
        UnityEngine.PlayerPrefs.SetString("Algorithm", "Qlearning");
        UnityEngine.PlayerPrefs.SetString("Policy", "Softmax");
        ToMaze();
    }

    public void LoadSarsa()
    {
        UnityEngine.PlayerPrefs.SetString("Algorithm", "SARSA");
        UnityEngine.PlayerPrefs.SetString("Policy", "Egreedy");
        ToMaze();
    }

    public void LoadSarsaSoftmax()
    {
        UnityEngine.PlayerPrefs.SetString("Algorithm", "SARSA");
        UnityEngine.PlayerPrefs.SetString("Policy", "Softmax");
        ToMaze();
    }
}
