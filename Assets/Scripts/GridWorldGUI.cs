using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using VRTK;

public class GridWorldGUI : MonoBehaviour {
	// GUI VARIABLES
	// =============
	public GridWorld env;
	public Agent agent;

	public Tilemap tilemap;
	public GameObject coin;
	public Gradient tileGradient;

	public VRTK_DashTeleport teleporter;

	// FIXME: Just make public and rebind to menu
	private bool showQtables = true;
	public bool ShowQtables
	{
		get { return showQtables; }
		set { showQtables = value; }
	}

	public void Awake() {
		Debug.Log ("GUI AWAKE");
		env.makeGraph ();

		InitGridGUI();
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

		if (env.isTerminal(env.getCurrentState())) {
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

	void OnEnable()
	{
		// Enable floor buttons and color visualization
		teleporter.Teleporting += agent.ClearUI;
		teleporter.Teleported += agent.UpdateUI;
		teleporter.Teleported += VisualiseQTable;
	}

	void OnDisable()
	{
		// Disable floor buttons and color visualization
		teleporter.Teleporting -= agent.ClearUI;
		teleporter.Teleported -= agent.UpdateUI;
		teleporter.Teleported -= VisualiseQTable;
	}
}
