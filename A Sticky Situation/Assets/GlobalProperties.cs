
using UnityEngine;
using System.Collections;

public class GlobalProperties : MonoBehaviour {

	public static bool IS_NETWORKED = false;
	public static string LEVEL = "Boxlands";
	public static float GravityScale = 2f;
	public static string VERSION = "Beta v1.5";
	/// <summary>
	/// NA = no player, Scientist, Big Boy, Thief, Ghost
	/// </summary>
	public static string[] PLAYERCHOICE = new string[]{"Scientist", "Thief", "BigBoy", "Ghost"};
	public static bool[] PLAYERCTRL = new bool[]{true,true,true,true};
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
