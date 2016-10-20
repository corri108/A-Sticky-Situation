using UnityEngine;
using System.Collections;
using Photon;

public class GhostAbility : PunBehaviour {

	public Material material;
	public GameObject cape;
	bool abilityAvailable;
	bool used;
	bool timerStart;

	float timer;
	Color tempColor;

	public int wait = 5;

	void Start()
	{
		Debug.Log (transform.GetChild (0).GetChild (0).name);
		material = transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer> ().material;
		abilityAvailable = true;
	}

	// Update is called once per frame
	void Update () 
	{
		if (Input.GetKeyDown (KeyCode.LeftShift)) 
		{
			GetComponent<PhotonView> ().RPC ("Disappear", PhotonTargets.AllBuffered, null);
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
