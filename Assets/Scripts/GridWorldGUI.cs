using Action = Enums.Action;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using VRTK;

public class GridWorldGUI : MonoBehaviour {
	public GridWorld env;
	public Agent agent;

	public Tilemap tilemap;
	public Gradient tileGradient;

	public VRTK_DashTeleport teleporter;

	// UI vars
	private Text[] texts;
	private Button[] buttons;
	private List<GameObject> chests;
	private Dictionary<Action,GameObject>[,] trace_cones;

	public static bool showQtable = true;
	public static bool showTraces = true;

	private void InitGridGUI()
	{
		for (var y = 0; y < env.gridSizeY; ++y) {
			for (var x = 0; x < env.gridSizeX; ++x) {
				if (env.getLabyrinth (x, y) == GridWorld.wallChar)
					makeWall (x, y);
				else {
					if (env.chestChars.Contains (env.getLabyrinth (x, y))) {
						GameObject chest = makeChest (x, y, env.getLabyrinth (x, y));
						chests.Add (chest);
						env.addChest (x, y, chest);
					}
					SetTile (x,y);
					SetTraceCones (x, y);
				}
			}
		}
	}

	private void SetTile(int x, int y)
	{
		Vector3Int position = new Vector3Int (x, y, 0);
		tilemap.SetTileFlags (position, TileFlags.None);
		tilemap.SetColor (position, Color.black);
	}

	private void SetTraceCones(int x, int y)
	{
		Vector2Int position = new Vector2Int (x, y);
		List<Action> actions = env.getActions (position);
		Dictionary<Action, GameObject> dict = new Dictionary<Action, GameObject> ();
		foreach (Action act in actions) {
			GameObject cone = Cone (position, act);
			dict.Add (act, cone);
		}
		trace_cones [x, y] = dict;
	}

	private GameObject Cone(Vector2Int state, Action act){
		GameObject cone = (GameObject) Instantiate (Resources.Load ("Cone"));
		float trace_val= agent.GetTraceValue(state,act);
		float height = 2.0f;

		Vector3 rel_pos;
		Vector3 rot_vec;
		switch (act) 
		{
			case Action.up:
			    rel_pos = new Vector3 (0, height, 0.5f);
				rot_vec = new Vector3 (90,0,0);
				break;
			case Action.down:
			    rel_pos = new Vector3 (0, height, -0.5f);
				rot_vec = new Vector3 (-90,0,0);
				break;
			case Action.left:
			    rel_pos = new Vector3 (-0.75f, height, 0);
			    rot_vec = new Vector3 (0,-90,0);
				break;
			case Action.right:
			    rel_pos = new Vector3 (0.75f, height, 0);
				rot_vec = new Vector3 (0,90,0);
				break;
			default:
				rel_pos = new Vector3 ();
				rot_vec = new Vector3 ();
				break;
		}
			
		cone.transform.position = tilemap.GetCellCenterWorld (new Vector3Int (state.x, state.y, 0)) + rel_pos;
		cone.transform.localScale = new Vector3 (20*trace_val, 20*trace_val, 20*trace_val);
		cone.transform.Rotate(rot_vec);
		return cone;
	}
		
	public void makeWall(int x, int y)
	{
		GameObject cube = GameObject.CreatePrimitive (PrimitiveType.Cube);
		cube.transform.position = tilemap.GetCellCenterWorld (new Vector3Int (x, y, 0)) + new Vector3 (0, 2.5f, 0);
		cube.transform.localScale = new Vector3 (2, 5, 1.5f);
		Material newMat = Resources.Load("wall_A_d", typeof(Material)) as Material;
		cube.GetComponent<Renderer>().material = newMat;
	}

	public GameObject makeChest(int x, int y, char chest_char){
		GameObject chest;
		// Chest type
		if (chest_char < GridWorld.emptyChestUp) 
			chest = (GameObject) Instantiate (Resources.Load("treasure_chest/treasure_chest"));

		else
			chest = (GameObject) Instantiate (Resources.Load("treasure_chest/treasure_chest_empty"));

		// Position
		chest.transform.position = tilemap.GetCellCenterWorld (new Vector3Int (x, y, 0));

		// Rotation
		Vector3 rot_vec = new Vector3 (0,0,0);
		switch (chest_char % 4) {
		case 0: //right
			rot_vec = new Vector3 (0, 90, 0);
			break;
		case 2: //down
			rot_vec = new Vector3 (0, 180, 0);
			break;
		case 3: // left
			rot_vec = new Vector3 (0, -90, 0);
			break;
		}
		chest.transform.Rotate(rot_vec);

		return chest;
	}
		
	public void moveAgentInGameWorld(Vector2Int from_pos, Vector2Int to_pos, bool teleport = false)
	{
		GameObject from_chest = env.getChest (from_pos);
		if (from_chest != null)
			StartCoroutine (InteractWithChest (from_chest));
		GameObject to_chest = env.getChest (to_pos);
		if (to_chest != null)
			StartCoroutine (InteractWithChest (to_chest));

		Vector3 destination = tilemap.GetCellCenterWorld(new Vector3Int(to_pos.x, to_pos.y, 0));
	
		if (teleport)
			teleporter.ForceTeleport(destination);
		else
			teleporter.Teleport(agent.transform, destination, null, true); // This flies
	}

	private IEnumerator InteractWithChest(GameObject chest){
		yield return new WaitForSeconds (0.1f);
		Animator anim = chest.GetComponent<Animator> ();
		anim.SetBool ("open", !anim.GetBool ("open"));
	}
		
	private void Visualise(object sender, DestinationMarkerEventArgs e){
		VisualiseQTable ();
		VisualiseTraces ();
	}

	private void VisualiseQTable()
	{
		if (showQtable) {
			for (var y = 0; y < env.gridSizeY; ++y) {
				for (var x = 0; x < env.gridSizeX; ++x) {
					Vector2Int pos = new Vector2Int(x, y);
					float v = agent.GetStateValue (pos);
					Vector3Int position = new Vector3Int (pos.x, pos.y, 0);
					tilemap.SetColor (position, tileGradient.Evaluate (v / 10));
				}
			}
		}
	}

	private void VisualiseTraces()
	{
		if (showTraces) {
			for (var y = 0; y < env.gridSizeY; ++y) {
				for (var x = 0; x < env.gridSizeX; ++x) {
					Vector2Int pos = new Vector2Int(x, y);
					foreach (Action a in env.getActions(pos)) {
						float v = agent.GetTraceValue (pos, a);
						trace_cones [x, y] [a].transform.localScale = new Vector3 (v * 20, v * 20, v * 20);
					}
				}
			}
		}
	}

	// Because we use this method as a VRTK teleport event we need the given signature
	public void UpdateUI(object sender, DestinationMarkerEventArgs e)
	{
		// get available actions
		List<Action> actions = env.getActions(env.getCurrentState());
		// set corresponding button active
		// set corresponding text active and update its value
		foreach(Action action in actions){
			buttons [(int) action].gameObject.SetActive(true);
			texts [(int) action].enabled = true;
			texts[(int) action].text = agent.GetQval(env.getCurrentState(), action).ToString("n2");
		}
	}

	// Because we use this method as a VRTK teleport event we need the given signature
	public void ClearUI(object sender, DestinationMarkerEventArgs e)
	{
		foreach (Button button in buttons) {
			button.gameObject.SetActive(false);
		}
		foreach (Text text in texts) {
			text.enabled = false;
		}
	}

	void Awake()
	{
		Debug.Log ("GUI AWAKE");

		buttons = new Button[4] {GameObject.Find("ForwardButton").GetComponent<Button>(),
			GameObject.Find("BackwardButton").GetComponent<Button>(),
			GameObject.Find("LeftButton").GetComponent<Button>(),
			GameObject.Find("RightButton").GetComponent<Button>()};
		texts = new Text[4]{ GameObject.Find("ForwardQvalEstim").GetComponent<Text>(),
			GameObject.Find("BackwardQvalEstim").GetComponent<Text>(),
			GameObject.Find("LeftQvalEstim").GetComponent<Text>(),
			GameObject.Find("RightQvalEstim").GetComponent<Text>()};
		chests = new List<GameObject> ();
		ClearUI (this,new DestinationMarkerEventArgs());
		env.makeGraph ();
		trace_cones = new Dictionary<Action, GameObject>[env.gridSizeX, env.gridSizeY];
	}

	void Start()
	{
		InitGridGUI();
		UpdateUI(this,new DestinationMarkerEventArgs());
	}

	void OnEnable()
	{
		// Enable floor buttons and color visualization
		teleporter.Teleporting += ClearUI;
		teleporter.Teleported += UpdateUI;
		teleporter.Teleported += Visualise;
	}

	void OnDisable()
	{
		// Disable floor buttons and color visualization
		teleporter.Teleporting -= ClearUI;
		teleporter.Teleported -= UpdateUI;
		teleporter.Teleported -= Visualise;
	}

	public void ResetEpisode()
	{
		env.ResetEpisode ();
		//Close chests
		foreach (var chest in chests){
			Animator anim = chest.GetComponent<Animator> ();
			anim.SetBool ("open", false);
		}
			
	}
}
