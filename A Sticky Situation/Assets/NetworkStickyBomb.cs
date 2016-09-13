using UnityEngine;
using System.Collections;
using Photon;

public class NetworkStickyBomb : Photon.PunBehaviour {

	public GameObject blowupPS;
	[HideInInspector]
	public bool justTransfered = false;
	int justTransferedTimer = 45;
	int justTransferedTimerReset = 45;

	private Vector3 correctBombPos;
	private Quaternion correctBombRot;
	private StickyBomb sb;

	private int countdownTimer = 10 * 60;
	private int countdownTimerReset = 10 * 60;

	void Start()
	{
		sb = GetComponent<StickyBomb> ();
	}

	public void TransferBomb()
	{
		justTransfered = true;
		justTransferedTimer = justTransferedTimerReset;
	}
	
	// Update is called once per frame
	void Update()
	{
		if (sb == null)
			sb = GetComponent<StickyBomb> ();
		if (sb.isStuck)
			return;

		if (!photonView.isMine)
		{
			transform.position = Vector3.Lerp(transform.position, this.correctBombPos, Time.deltaTime * 20);
			transform.rotation = Quaternion.Lerp(transform.rotation, this.correctBombRot, Time.deltaTime * 20);
		}
	}

	void FixedUpdate()
	{
		if(PhotonNetwork.isMasterClient)
		{
			if (sb == null)
				sb = GetComponent<StickyBomb> ();

			if(sb.isStuck)
			{
				countdownTimer--;

				if(countdownTimer % 60 == 0)
				{
					GetComponent<PhotonView>().RPC("DisplayCountdown", PhotonTargets.All, countdownTimer / 60);
				}

				if(countdownTimer == 0)
				{
					GetComponent<PhotonView>().RPC("Blowup", PhotonTargets.All);
					PhotonNetwork.Destroy(this.gameObject);

					PlayerController stuckPlayer = null;
					PlayerController gotKillPlayer = null;
					PlayerController[] players = GameObject.FindObjectsOfType<PlayerController>();

					foreach(var p in players)
					{
						if(p.playerID == sb.stuckID)
						{
							stuckPlayer = p;
						}
						if(p.playerID == sb.ownerID)
						{
							gotKillPlayer = p;
						}
					}

					GetComponent<PhotonView>().RPC("DisplayKill", PhotonTargets.All, stuckPlayer.playerID, gotKillPlayer.playerID);
				}

				if(justTransfered)
				{
					justTransferedTimer--;

					if(justTransferedTimer == 0)
					{
						justTransfered = false;
					}
				}
			}
		}
	}

	[PunRPC]
	void DisplayCountdown(int num)
	{
		PopText.Create (num.ToString (), Color.white, 60, transform.position + Vector3.up * .5f);
	}

	[PunRPC]
	void DisplayKill(int killed, int killer)
	{
		PopText.Create ("P" + killed.ToString () + " was killed by P" + killer.ToString(), Color.white, 250, 
		                new Vector3(0, 4, 1));

		PlayerController stuckPlayer = null;
		PlayerController[] players = GameObject.FindObjectsOfType<PlayerController>();
		
		foreach(var p in players)
		{
			if(p.playerID == sb.stuckID)
			{
				stuckPlayer = p;
			}
		}

		stuckPlayer.gameObject.active = false;
	}

	[PunRPC]
	void Blowup()
	{
		GameObject.Destroy ((GameObject)GameObject.Instantiate (blowupPS, this.transform.position, Quaternion.identity), 1f);
	}
	
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (sb == null)
			sb = GetComponent<StickyBomb> ();

		if (sb.isStuck)
			return;

		if (stream.isWriting)
		{
			// We own this player: send the others our data
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
		}
		else
		{
			// Network player, receive data
			this.correctBombPos = (Vector3)stream.ReceiveNext();
			this.correctBombRot = (Quaternion)stream.ReceiveNext();
		}
	}
}
