using UnityEngine;
using System.Collections;
using Photon;

public class ThiefAbility : PunBehaviour {

	public bool abilityAvailable;
	bool used;

	public bool inRange;
	GameObject target;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Camera.main.GetComponent<GameCamera> ()._gameStarted) 
		{
			if (abilityAvailable) 
			{
				if (inRange) 
				{
					if (Input.GetKeyDown (KeyCode.LeftShift))
					{
						if (target.GetComponent<PlayerController> ().hasStickyBomb)
						{
							target.GetComponent<PlayerController> ().hasStickyBomb = false;
							this.GetComponent<PlayerController> ().hasStickyBomb = true;
							target.GetComponent<PlayerController> ().bombStatus.GetComponent<SpriteRenderer>().sprite= target.GetComponent<PlayerController> ().noBombSprite;
							this.GetComponent<PlayerController> ().bombStatus.GetComponent<SpriteRenderer>().sprite = this.GetComponent<PlayerController> ().hasBombSprite;
							abilityAvailable = false;
						}
					}
				} 
			}
		} 
		else 
		{
			abilityAvailable = true;
		}
	}

	[PunRPC]
	public void SetInRange()
	{
		inRange = true;
	}

	[PunRPC]
	public void SetOutRange()
	{
		inRange = false;
	}

	void OnTriggerStay2D(Collider2D col)
	{
		if (col.gameObject.tag == "Player") 
		{
			this.GetComponent<PhotonView>().RPC("SetInRange", PhotonTargets.All);
			target = col.gameObject;
		}
	}
	void OnTriggerExit2D(Collider2D col)
	{
		if (col.gameObject.tag == "Player") 
		{
			this.GetComponent<PhotonView>().RPC("SetOutRange", PhotonTargets.All);
			target = null;
		}
	}
}
