using UnityEngine;
using System.Collections;

public class ExplosionRadius : MonoBehaviour {

	int throwerID = -1;
	int tt = 5;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () 
	{
		tt--;

		if(tt == 0)
		{
			GameObject.Destroy(this.gameObject);
		}
	}

	public void SetThrowerID(int id)
	{
		throwerID = id;

		PlayerController stuckPlayer = null;
		PlayerController gotKillPlayer = null;
		PlayerController[] players = GameObject.FindObjectsOfType<PlayerController>();
		
		/*foreach(var p in players)
		{
			if(p.playerID == sb.stuckID)
			{
				stuckPlayer = p;
			}
			if(p.playerID == sb.ownerID)
			{
				gotKillPlayer = p;
			}
		}*/
	}

	void OnTriggerEnter2D(Collider2D c)
	{
		if(PhotonNetwork.isMasterClient)
		{
			if(c.gameObject.tag.Equals("Player"))
			{

			}
		}
	}
}
