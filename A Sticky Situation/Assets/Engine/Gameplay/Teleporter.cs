using UnityEngine;
using System.Collections;

public class Teleporter : MonoBehaviour {

	public int TeleID = 1;
	bool justTP = false;
	PlayerController tpTarg = null;
	public bool reachable = true;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter2D(Collider2D c)
	{
		if(c.gameObject.GetComponent<PlayerController>() != null)
		{
			if(GlobalProperties.IS_NETWORKED)
			{
				if(PhotonNetwork.player.ID == c.gameObject.GetComponent<PhotonView>().ownerId && !justTP)
				{
					Teleporter next = GetTeleporterWithID(TeleID, c.gameObject.GetComponent<PlayerController>());
					c.gameObject.transform.position = next.transform.position;
					c.gameObject.GetComponent<PhotonView>().RPC("Teleported", PhotonTargets.All, this.transform.position, next.transform.position);
				}
			}
			else
			{
				if(!justTP)
				{
					Teleporter next = GetTeleporterWithID(TeleID, c.gameObject.GetComponent<PlayerController>());
					c.gameObject.transform.position = next.transform.position;
					c.gameObject.GetComponent<PlayerController>().LOCAL_Teleported(this.transform.position, next.transform.position);
				}
			}
		}
	}

	void OnTriggerExit2D(Collider2D c)
	{
		if(c.gameObject.GetComponent<PlayerController>() != null)
		{
			if(GlobalProperties.IS_NETWORKED)
			{
				if(PhotonNetwork.player.ID == c.gameObject.GetComponent<PhotonView>().ownerId && justTP)
				{
					justTP = false;
				}
			}
			else
			{
				if(justTP)
				{
					justTP = false;
				}
			}
		}
	}

	public static Teleporter[] FindClosestPair(Vector3 pos)
	{
		int pairID = -1;

		Teleporter[] list = GameObject.FindObjectsOfType<Teleporter> ();
		float closestTP = float.MaxValue;

		foreach(var t in list)
		{
			float d = Vector3.Distance(t.transform.position, pos);
			if(d < closestTP)
			{
				closestTP = d;
				pairID = t.TeleID;
			}
		}

		return FindPair (pairID);
	}

	public static Teleporter[] FindPair(int id)
	{
		Teleporter[] empty = new Teleporter[]{null , null};

		Teleporter[] list = GameObject.FindObjectsOfType<Teleporter> ();
		bool gotOne = false;

		for(int i = 0; i < list.Length; ++i)
		{
			Teleporter tp = list[i];
			if(tp.TeleID == id)
			{
				if(!gotOne)
				{
					empty[0] = tp;
					gotOne = true;
				}
				else
				{
					empty[1] = tp;
					break;
				}
			}
		}

		return empty;
	}

	Teleporter GetTeleporterWithID(int other, PlayerController player)
	{
		Teleporter[] tp = GameObject.FindObjectsOfType<Teleporter> ();

		foreach(var t in tp)
		{
			if(t.TeleID == other && !t.Equals(this))
			{
				//dont let this player teleport until oncollisionexit
				t.justTP = true;
				t.tpTarg = player;
				return t;
			}
		}

		return null;
	}
}
