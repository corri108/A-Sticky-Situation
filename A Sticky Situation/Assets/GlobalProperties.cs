using UnityEngine;
using System.Collections;

public class GlobalProperties : MonoBehaviour {

	public static bool IS_NETWORKED = false;
	public static string LEVEL = "None";
	public static float GravityScale = 2f;
	public static string VERSION = "Beta v1.1";
	/// <summary>
	/// NA = no player, Scientist, Big Boy, Thief, Ghost
	/// </summary>
	public static string[] PLAYERCHOICE = new string[]{"NA", "NA", "NA", "NA"};
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
