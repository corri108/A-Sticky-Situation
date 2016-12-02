using UnityEngine;
using System.Collections;

public class BigBoy : MonoBehaviour {

	[HideInInspector]
	public bool alreadyHit = false;

	public GameObject abilitySlider;

	// Use this for initialization
	void Start () {
		int id = GetComponent<PlayerController>().playerID;
		abilitySlider = GameObject.FindGameObjectWithTag ("P" + id + "Slider");
	}
	
	// Update is called once per frame
	void Update () {
		if (alreadyHit) 
		{
			abilitySlider.SetActive (false);
		} 
		else 
		{
			abilitySlider.SetActive (true);
		}
	}
}
