using Action = Enums.Action;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using VRTK;

public class MazeGridWorldGUI : GridWorldGUI
{
    protected override void InitGridGUI()
    {
        for (var y = 0; y < env.gridSizeY; ++y)
        {
            for (var x = 0; x < env.gridSizeX; ++x)
            {
                if (env.getLabyrinth(x, y) == GridWorld.wallChar)
                    makeWall(x, y);
                else
                {
                    if (env.chestChars.Contains(env.getLabyrinth(x, y)))
                    {
                        GameObject chest = makeChest(x, y, env.getLabyrinth(x, y));
                        chests.Add(chest);
                        env.addChest(x, y, chest);
                    }
                    SetTile(x, y);
                    SetTraceCones(x, y);
                }
            }
        }
    }

    public override void moveAgentInGameWorld(Vector2Int from_pos, Vector2Int to_pos, bool teleport = false)
    {

        Debug.Log("MOVE TO" + to_pos);
        GameObject from_chest = env.getChest(from_pos);
        if (from_chest != null)
            StartCoroutine(InteractWithChest(from_chest));
        GameObject to_chest = env.getChest(to_pos);
        if (to_chest != null)
            StartCoroutine(InteractWithChest(to_chest));

        Vector3 destination = tilemap.GetCellCenterWorld(new Vector3Int(to_pos.x, to_pos.y, 0));

        if (teleport)
            teleporter.ForceTeleport(destination);
        else
            teleporter.Teleport(agent.transform, destination, null, true); // This flies
    }


    void Awake()
    {
        Debug.Log("GUI AWAKE");

        buttons = new Button[4] {GameObject.Find("ForwardButton").GetComponent<Button>(),
            GameObject.Find("BackwardButton").GetComponent<Button>(),
            GameObject.Find("LeftButton").GetComponent<Button>(),
            GameObject.Find("RightButton").GetComponent<Button>()};
        texts = new Text[4]{ GameObject.Find("ForwardQvalEstim").GetComponent<Text>(),
            GameObject.Find("BackwardQvalEstim").GetComponent<Text>(),
            GameObject.Find("LeftQvalEstim").GetComponent<Text>(),
            GameObject.Find("RightQvalEstim").GetComponent<Text>()};
        sliders = new Slider[4] {GameObject.Find("ForwardSlider").GetComponent<Slider>(),
            GameObject.Find("BackwardSlider").GetComponent<Slider>(),
            GameObject.Find("LeftSlider").GetComponent<Slider>(),
            GameObject.Find("RightSlider").GetComponent<Slider>()};
        chests = new List<GameObject>();
        ClearUI(this, new DestinationMarkerEventArgs());
        env.makeGraph();
        trace_cones = new Dictionary<Action, GameObject>[env.gridSizeX, env.gridSizeY];
    }

    void Start()
    {
        Debug.Log("GUI STARTED");
        InitGridGUI();
        UpdateUI(this, new DestinationMarkerEventArgs());
    }

    void OnEnable()
    {
        // Enable floor buttons and color visualization
        teleporter.Teleporting += DisableControllers;
        teleporter.Teleporting += ClearUI;
        teleporter.Teleported += UpdateUI;
        teleporter.Teleported += Visualise;
        teleporter.Teleported += ReEnableControllers;

    }

    void OnDisable()
    {
        // Disable floor buttons and color visualization
        teleporter.Teleporting -= DisableControllers;
        teleporter.Teleporting -= ClearUI;
        teleporter.Teleported -= UpdateUI;
        teleporter.Teleported -= Visualise;
        teleporter.Teleported -= ReEnableControllers;
    }

}
