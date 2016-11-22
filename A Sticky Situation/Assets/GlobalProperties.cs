using UnityEngine;
using System.Collections;

public class GlobalProperties : MonoBehaviour {

	public static bool IS_NETWORKED = false;
	public static string LEVEL = "Boxlands";
	public static float GravityScale = 2f;
	public static string VERSION = "Beta v1.3";
	/// <summary>
	/// NA = no player, Scientist, Big Boy, Thief, Ghost
	/// </summary>
	public static string[] PLAYERCHOICE = new string[]{"Thief", "Scientist", "NA", "NA"};
	public static bool[] PLAYERCTRL = new bool[]{true,false,false,false};
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
