using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Scientist : MonoBehaviour {

	public bool bombsThrown;

	public int abilityCD = 480;
	int sliderTimer;
	public GameObject abilitySlider;

	// Use this for initialization
	void Start () {
		int id = GetComponent<PlayerController>().playerID;
		abilitySlider = GameObject.FindGameObjectWithTag ("P" + id + "Slider");
		abilitySlider.GetComponent<Slider>().value = abilitySlider.GetComponent<Slider>().maxValue;
	}
	
	// Update is called once per frame
	void FixedUpdate () {

		if (bombsThrown) 
		{
			sliderTimer++;
			abilitySlider.GetComponent<Slider> ().value = sliderTimer/60.0f;
			if (sliderTimer >= abilityCD) 
			{
				sliderTimer = 0;
				bombsThrown = false;
			}
		}
	}
}
