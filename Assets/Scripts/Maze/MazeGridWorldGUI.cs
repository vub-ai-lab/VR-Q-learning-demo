using Action = Enums.Action;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using VRTK;

public class MazeGridWorldGUI : GridWorldGUI
{
    protected void MakeWall(int x, int y)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = tilemap.GetCellCenterWorld(new Vector3Int(x, y, 0)) + new Vector3(0, 2.5f, 0);
        cube.transform.localScale = new Vector3(2, 5, 1.5f);
        Material newMat = Resources.Load("wall_A_d", typeof(Material)) as Material;
        cube.GetComponent<Renderer>().material = newMat;
    }

    protected GameObject MakeChest(int x, int y, char chest_char)
    {
        GameObject chest;
        // Chest type
        if (chest_char < GridWorld.emptyChestUp)
            chest = (GameObject)Instantiate(Resources.Load("treasure_chest/treasure_chest"));

        else
            chest = (GameObject)Instantiate(Resources.Load("treasure_chest/treasure_chest_empty"));

        // Position
        chest.transform.position = tilemap.GetCellCenterWorld(new Vector3Int(x, y, 0));

        // Rotation
        Vector3 rot_vec = new Vector3(0, 0, 0);
        switch (chest_char % 4)
        {
            case 0: //right
                rot_vec = new Vector3(0, 90, 0);
                break;
            case 2: //down
                rot_vec = new Vector3(0, 180, 0);
                break;
            case 3: // left
                rot_vec = new Vector3(0, -90, 0);
                break;
        }
        chest.transform.Rotate(rot_vec);

        return chest;
    }


    protected IEnumerator InteractWithChest(GameObject chest)
    {
        yield return new WaitForSeconds(0.1f);
        Animator anim = chest.GetComponent<Animator>();
        anim.SetBool("open", !anim.GetBool("open"));
    }

    protected override void VisualiseTraces()
    {
        if (showTraces)
        {
            for (var y = 0; y < env.gridSizeY; ++y)
            {
                for (var x = 0; x < env.gridSizeX; ++x)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    foreach (Action a in env.getActions(pos))
                    {
                        float v = agent.GetTraceValue(pos, a);

                        if (agent.algorithmType == "nstepSARSA" || agent.algorithmType == "nstepOpSARSA")
                        {
                            Vector3Int position = new Vector3Int(x, y, 0);

                            if (v != 1)
                            {
                                v = 0;
                            }


                        }

                        trace_cones[x, y][a].transform.localScale = new Vector3(v * 20, v * 20, v * 20);
                    }
                }
            }
        }
    }

    protected override void InitGridGUI()
    {
        for (var y = 0; y < env.gridSizeY; ++y)
        {
            for (var x = 0; x < env.gridSizeX; ++x)
            {
                if (env.GetLabyrinth(x, y) == GridWorld.wallChar)
                    MakeWall(x, y);
                else
                {
                    if (env.chestChars.Contains(env.GetLabyrinth(x, y)))
                    {
                        GameObject chest = MakeChest(x, y, env.GetLabyrinth(x, y));
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
