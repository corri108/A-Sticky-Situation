using UnityEngine;
using System.Collections;

public class AbilityStatus : MonoBehaviour {

	private PlayerController pc;
	[HideInInspector]
	public bool ability_ready = true;
	public Sprite charging;
	public Sprite ready;
	public AudioClip specialReadyNoise;
	private float chargingSpinSpeed = 1.2f;
	private int timerCooldown = 60 * 8;
	private int timerCooldownR = 60 * 8;
	// Use this for initialization
	void Start () 
	{
		pc = this.transform.root.GetComponent<PlayerController> ();
		this.GetComponent<SpriteRenderer> ().color = this.transform.root.FindChild("SprintStatus").GetComponent<SpriteRenderer> ().material.color;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void FixedUpdate()
	{
		if(ability_ready)
		{
			this.transform.rotation = Quaternion.Lerp(this.transform.rotation, Quaternion.identity, .1f);
		}
		else
		{
			this.transform.Rotate(Vector3.forward * chargingSpinSpeed);

			timerCooldown--;

			if(timerCooldown == 0)
			{
				timerCooldown = timerCooldownR;
				ability_ready = true;
				UpdateReady();
			}
		}
	}

	public void UpdateReady()
	{
		if(ability_ready)
		{
			GetComponent<SpriteRenderer>().sprite = ready;
			timerCooldown = timerCooldownR;
			AudioSource.PlayClipAtPoint(specialReadyNoise, GameObject.FindObjectOfType<GameCamera>().transform.position);
		}
		else
		{
			GetComponent<SpriteRenderer>().sprite = charging;
		}
	}
}
