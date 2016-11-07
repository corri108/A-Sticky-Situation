
using UnityEngine;
using System.Collections;

public class BodyPart : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter2D(Collider2D c)
	{
		if (c.gameObject.tag.Equals("StickyBomb"))
		{
			if(PhotonNetwork.isMasterClient)
			{
				PlayerController pc = this.transform.root.GetComponent<PlayerController>();
				StickyBomb sb = c.gameObject.GetComponent<StickyBomb>();
				Debug.Log("Sticky : " + sb.ownerID + ", Owner: " + pc.playerID);

				if(sb.hitGround)
				{
					PopText.Create("Get Bomb!", Color.black, 60, sb.transform.position + Vector3.up);
					Vector3 scale = new Vector3(c.gameObject.transform.localScale.x,c.gameObject.transform.localScale.y,c.gameObject.transform.localScale.z);
					//this transform, scale, stickyID
					pc.GetComponent<PhotonView> ().RPC ("PickupBombGround", PhotonTargets.All, sb.transform.position, scale, sb.GetComponent<PhotonView>().viewID);
				}
				else
				{
					if(!pc.playerID.Equals(sb.ownerID) && !sb.isStuck)
					{
						Debug.Log("Sucess!");
						Vector3 scale = new Vector3(c.gameObject.transform.localScale.x,c.gameObject.transform.localScale.y,c.gameObject.transform.localScale.z);
						//this transform, scale, stickyID
						pc.GetComponent<PhotonView> ().RPC ("GetStuck", PhotonTargets.All, sb.transform.position, scale, sb.GetComponent<PhotonView>().viewID);
					}

					//for switching owners
					else if(sb.isStuck && !sb.GetComponent<NetworkStickyBomb>().justTransfered && 
					        !pc.playerID.Equals(sb.stuckID))
					{
						Debug.Log("Sucess!");
						Vector3 scale = new Vector3(c.gameObject.transform.localScale.x,c.gameObject.transform.localScale.y,c.gameObject.transform.localScale.z);
						//this transform, scale, stickyID
						pc.GetComponent<PhotonView> ().RPC ("SwitchOwners", PhotonTargets.All, sb.transform.position, sb.GetComponent<PhotonView>().viewID);

						//call first on master client so transfer doesnt occur multiple times
						sb.GetComponent<NetworkStickyBomb>().TransferBomb();
					}
				}
			}
		}
	}
}
