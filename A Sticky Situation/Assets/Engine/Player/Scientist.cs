using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Scientist : MonoBehaviour {

	public bool bombsThrown;

	public int abilityCD = 480;
	int sliderTimer;
	public GameObject abilitySlider;
	public GameObject abilityImage;
	public Sprite scientistSprite;

	// Use this for initialization
	void Start () {
		int id = GetComponent<PlayerController>().playerID;
		abilitySlider = GameObject.FindGameObjectWithTag ("P" + id + "Slider");
		abilitySlider.GetComponent<Slider>().value = abilitySlider.GetComponent<Slider>().maxValue;

		abilityImage = GameObject.Find ("P" + id + "AbilityImage");
		abilityImage.GetComponent<Image> ().sprite = scientistSprite;
	}
	
	// Update is called once per frame
	void FixedUpdate () {

		if (bombsThrown) 
		{
			abilityImage.SetActive (false);
			sliderTimer++;
			abilitySlider.GetComponent<Slider> ().value = sliderTimer/60.0f;
			if (sliderTimer >= abilityCD) 
			{
				abilityImage.SetActive (true);
				sliderTimer = 0;
				bombsThrown = false;
			}
		}
	}
}
