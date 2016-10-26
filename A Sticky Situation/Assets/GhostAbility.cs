using UnityEngine;
using System.Collections;
using Photon;

public class GhostAbility : PunBehaviour {

	public Material material;
	public Material mainMaterial;
	public Material ghostMaterial;
	public GameObject cape;
	public bool abilityAvailable;
	bool used;

	float timer;
	Color tempColor;

	public int wait = 5;

	void Start()
	{
		mainMaterial = transform.GetChild (0).GetChild (0).GetComponent<SpriteRenderer> ().material;
		abilityAvailable = true;
	}

	// Update is called once per frame
	void Update () 
	{
		if (Camera.main.GetComponent<GameCamera> ()._gameStarted) 
		{
			if (Input.GetKeyDown (KeyCode.LeftShift)) 
			{
				GetComponent<PhotonView> ().RPC ("Disappear", PhotonTargets.AllBuffered, null);
			}
		} 
		else 
		{
			abilityAvailable = true;
			used = false;
		}

		if (used) 
		{
			if (timer < wait) 
			{
				transform.GetChild (0).GetChild (0).GetComponent<SpriteRenderer> ().material = ghostMaterial;
				//5
				for(int i = 0; i < 5; ++i)
				{
					transform.GetChild (0).GetChild (0).GetChild (i).GetComponent<SpriteRenderer> ().material = ghostMaterial;
					if(i != 4)
					{
						transform.GetChild (0).GetChild (0).GetChild (i).GetChild(0).GetComponent<SpriteRenderer> ().material = ghostMaterial;
						transform.GetChild (0).GetChild (0).GetChild (i).GetChild(0).GetChild(0).GetComponent<SpriteRenderer> ().material = ghostMaterial;
					}
				}
				cape.SetActive (false);
				timer += Time.deltaTime;
			} 
			else 
			{
				cape.SetActive (true);
				transform.GetChild (0).GetChild (0).GetComponent<SpriteRenderer> ().material = mainMaterial;
				//5
				for(int i = 0; i < 5; ++i)
				{
					transform.GetChild (0).GetChild (0).GetChild (i).GetComponent<SpriteRenderer> ().material = mainMaterial;
					if(i != 4)
					{
						transform.GetChild (0).GetChild (0).GetChild (i).GetChild(0).GetComponent<SpriteRenderer> ().material = mainMaterial;
						transform.GetChild (0).GetChild (0).GetChild (i).GetChild(0).GetChild(0).GetComponent<SpriteRenderer> ().material = mainMaterial;
					}
				}
			}
		}

	}

	[PunRPC]
	void Disappear()
	{
		if (abilityAvailable) 
		{
			used = true;
			abilityAvailable = false;
		}
	}
}
