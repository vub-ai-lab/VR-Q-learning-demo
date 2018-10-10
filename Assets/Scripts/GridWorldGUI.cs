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
	public GameObject coin;
	public Gradient tileGradient;

	public VRTK_DashTeleport teleporter;

	// UI vars
	public Text Q_UpEstimText;
	public Text Q_DownEstimText;
	public Text Q_LeftEstimText;
	public Text Q_RightEstimText;

	private Text[] texts;
	private Button[] buttons;

	// FIXME: Just make public and rebind to menu
	private bool showQtables = true;
	public bool ShowQtables
	{
		get { return showQtables; }
		set { showQtables = value; }
	}

	private void InitGridGUI()
	{
		coin.SetActive(false);
		for (var y = 0; y < env.gridSizeY; ++y) {
			for (var x = 0; x < env.gridSizeX; ++x) {
				Vector3Int position = new Vector3Int (x, y, 0);

				if (env.getLabyrinth(x, y) == env.wallChar)
					makeWall (x, y);
				
				//tilemap.SetTile (position, ScriptableObject.CreateInstance<TileBase>());
				tilemap.SetTileFlags (position, TileFlags.None);
				tilemap.SetColor (position, Color.black);
			}
		}
	}
		
	public void makeWall(int x, int y)
	{
		GameObject cube = GameObject.CreatePrimitive (PrimitiveType.Cube);
		cube.transform.position = tilemap.GetCellCenterWorld (new Vector3Int (x, y, 0)) + new Vector3 (0, 2.5f, 0);
		cube.transform.localScale = new Vector3 (2, 5, 1.5f);
	}
		
	public void moveAgentInGameWorld(Vector2Int pos, bool teleport = false)
	{
		Vector3 destination = tilemap.GetCellCenterWorld(new Vector3Int(pos.x, pos.y, 0));

		if (teleport)
			teleporter.ForceTeleport(tilemap.GetCellCenterWorld(new Vector3Int(pos.x, pos.y, 0))); // This teleports
		else
			teleporter.Teleport(agent.transform, destination, null, true); // This flies

		if (env.isTerminal()) {
			StartCoroutine (ShowCoinTemporarily());
		}
	}

	private IEnumerator ShowCoinTemporarily ()
	{
		yield return new WaitForSeconds (0.05f);
		coin.SetActive (true);
		yield return new WaitForSeconds (2f);
		coin.SetActive (false);
	}

	private void VisualiseQTable(object sender, DestinationMarkerEventArgs e)
	{
		if (showQtables) {
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
		texts = new Text[4]{ Q_UpEstimText, Q_DownEstimText, Q_LeftEstimText, Q_RightEstimText };
		ClearUI (this,new DestinationMarkerEventArgs());

		env.makeGraph ();
		InitGridGUI();
	}

	void Start()
	{
		UpdateUI(this,new DestinationMarkerEventArgs());
	}

	void OnEnable()
	{
		// Enable floor buttons and color visualization
		teleporter.Teleporting += ClearUI;
		teleporter.Teleported += UpdateUI;
		teleporter.Teleported += VisualiseQTable;
	}

	void OnDisable()
	{
		// Disable floor buttons and color visualization
		teleporter.Teleporting -= ClearUI;
		teleporter.Teleported -= UpdateUI;
		teleporter.Teleported -= VisualiseQTable;
	}
}
