using Action = Enums.Action;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using VRTK;

abstract public class GridWorldGUI : MonoBehaviour
{
    public GridWorld env;
    public Agent agent;

    public Tilemap tilemap;
    public Gradient tileGradient;

    public VRTK_DashTeleport teleporter;

    // Disabling controllers during teleportation
    public ButtonEvents leftController;
    public ButtonEvents rightController;

    // Checking the status (dirty bug fix)
    public GameObject popUpMenu;

    // UI vars
    protected Text[] texts;
    protected Button[] buttons;
    protected Slider[] sliders;
    protected List<GameObject> chests;
    protected Dictionary<Action, GameObject>[,] trace_cones;

    public static bool showQtable = true;
    public static bool showTraces = true;

    protected abstract void InitGridGUI();

    protected void SetTile(int x, int y)
    {
        Vector3Int position = new Vector3Int(x, y, 0);
        tilemap.SetTileFlags(position, TileFlags.None);
        tilemap.SetColor(position, Color.black);
    }

    protected void SetTraceCones(int x, int y)
    {
        Vector2Int position = new Vector2Int(x, y);
        List<Action> actions = env.getActions(position);
        Dictionary<Action, GameObject> dict = new Dictionary<Action, GameObject>();
        foreach (Action act in actions)
        {
            GameObject cone = Cone(position, act);
            dict.Add(act, cone);
        }
        trace_cones[x, y] = dict;
    }

    protected GameObject Cone(Vector2Int state, Action act)
    {
        GameObject cone = (GameObject)Instantiate(Resources.Load("Cone"));
        float trace_val = agent.GetTraceValue(state, act);
        float height = 2.0f;

        Vector3 rel_pos;
        Vector3 rot_vec;
        switch (act)
        {
            case Action.up:
                rel_pos = new Vector3(0, height, 0.5f);
                rot_vec = new Vector3(90, 0, 0);
                break;
            case Action.down:
                rel_pos = new Vector3(0, height, -0.5f);
                rot_vec = new Vector3(-90, 0, 0);
                break;
            case Action.left:
                rel_pos = new Vector3(-0.75f, height, 0);
                rot_vec = new Vector3(0, -90, 0);
                break;
            case Action.right:
                rel_pos = new Vector3(0.75f, height, 0);
                rot_vec = new Vector3(0, 90, 0);
                break;
            default:
                rel_pos = new Vector3();
                rot_vec = new Vector3();
                break;
        }

        cone.transform.position = tilemap.GetCellCenterWorld(new Vector3Int(state.x, state.y, 0)) + rel_pos;
        cone.transform.localScale = new Vector3(20 * trace_val, 20 * trace_val, 20 * trace_val);
        cone.transform.Rotate(rot_vec);
        return cone;
    }

    public abstract void moveAgentInGameWorld(Vector2Int from_pos, Vector2Int to_pos, bool teleport = false); 

    protected void Visualise(object sender, DestinationMarkerEventArgs e)
    {
        VisualiseQTable();
        VisualiseTraces();
    }

    protected void VisualiseQTable()
    {
        if (showQtable)
        {
            for (var y = 0; y < env.gridSizeY; ++y)
            {
                for (var x = 0; x < env.gridSizeX; ++x)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    float v = agent.GetStateValue(pos);
                    Vector3Int position = new Vector3Int(pos.x, pos.y, 0);
                    tilemap.SetColor(position, tileGradient.Evaluate(v / 10));
                }
            }
        }
    }

    protected abstract void VisualiseTraces();


    // Because we use this method as a VRTK teleport event we need the given signature
    public void UpdateUI(object sender, DestinationMarkerEventArgs e)
    {
        // get available actions
        List<Action> actions = env.getActions(env.getCurrentState());
        // set corresponding button active
        // set corresponding text active and update its value
        foreach (Action action in actions)
        {
            buttons[(int)action].gameObject.SetActive(true);
            texts[(int)action].enabled = true;
            texts[(int)action].text = agent.GetQval(env.getCurrentState(), action).ToString("n2");
        }

    }

    protected void UpdatePolicySliders(object sender, DestinationMarkerEventArgs e)
    {
        // get available actions
        List<Action> actions = env.getActions(env.getCurrentState());
        // set corresponding sliders active and update its value
        foreach (Action action in actions)
        {
            sliders[(int)action].gameObject.SetActive(true);
            sliders[(int)action].value = agent.GetPickChance(env.getCurrentState(), action, actions);

            //Change the color of the slider so that it represents the value better
            foreach (Slider slider in sliders)
            {
                float SliderValue = slider.value;

                if (SliderValue < 0.25f)
                {
                    slider.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = Color.red;
                }
                else if (SliderValue < 0.5f)
                {
                    slider.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = new Color(1.0f, 0.64f, 0.0f);
                }
                else
                {
                    slider.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = Color.green;
                }

            }

        }
    }

    protected void DisablePolicySliders()
    {
        // get available actions
        List<Action> actions = env.getActions(env.getCurrentState());

        // Disable sliders
        foreach (Action action in actions)
        {
            sliders[(int)action].gameObject.SetActive(false);
        }

    }

    public void AddPolicySliders()
    {
        UpdatePolicySliders(this, new DestinationMarkerEventArgs());
        teleporter.Teleported += UpdatePolicySliders;
    }

    public void RemovePolicySliders()
    {
        DisablePolicySliders();
        teleporter.Teleported -= UpdatePolicySliders;
    }

    public void ToggleSliders(bool toggled)
    {
        if (toggled)
        {
            AddPolicySliders();
        }
        else
        {
            RemovePolicySliders();
        }
    }

    // Because we use this method as a VRTK teleport event we need the given signature
    public void ClearUI(object sender, DestinationMarkerEventArgs e)
    {
        foreach (Button button in buttons)
        {
            button.gameObject.SetActive(false);
        }
        foreach (Text text in texts)
        {
            text.enabled = false;
        }
        foreach (Slider slider in sliders)
        {
            slider.gameObject.SetActive(false);
        }
    }

    // Because we use this method as a VRTK teleport event we need the given signature
    public void DisableControllers(object sender, DestinationMarkerEventArgs e)
    {
        leftController.SetDisabled(true);
        rightController.SetDisabled(true);
    }

    // Because we use this method as a VRTK teleport event we need the given signature
    public void ReEnableControllers(object sender, DestinationMarkerEventArgs e)
    {
        // unless a pop-up menu has been acivated (due to goal reached event, this is a dirty bug fix to be done cleaner)
        if (popUpMenu.activeSelf)
            return;
        leftController.SetDisabled(false);
        rightController.SetDisabled(false);
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

    public void ResetEpisode()
    {
        env.ResetEpisode();
        //Close chests
        foreach (var chest in chests)
        {
            Animator anim = chest.GetComponent<Animator>();
            anim.SetBool("open", false);
        }

    }

    public virtual void ResetPosition()
    {
        env.ResetPosition();
    }
}
