using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BigBoy : MonoBehaviour {

	[HideInInspector]
	public bool alreadyHit = false;

	public GameObject abilitySlider;

	public GameObject abilityImage;
	public Sprite heartSprite;

	// Use this for initialization
	void Start () {
		int id = GetComponent<PlayerController>().playerID;
		abilitySlider = GameObject.FindGameObjectWithTag ("P" + id + "Slider");
		abilityImage = GameObject.Find ("P" + id + "AbilityImage");
		abilityImage.GetComponent<Image> ().sprite = heartSprite;
	}
	
	// Update is called once per frame
	void Update () {
		if (alreadyHit) 
		{
			abilitySlider.SetActive (false);
			abilityImage.SetActive (false);
		} 
		else 
		{
			abilitySlider.SetActive (true);
			abilityImage.SetActive (true);
		}
	}
}
