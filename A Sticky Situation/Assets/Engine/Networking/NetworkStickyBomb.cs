using UnityEngine;
using System.Collections;
using Photon;
using System.Collections.Generic;

public class NetworkStickyBomb : Photon.PunBehaviour {

	public GameObject blowupPS;
	[HideInInspector]
	public bool justTransfered = false;
	int justTransferedTimer = 45;
	int justTransferedTimerReset = 45;

	private Vector3 correctBombPos;
	private Quaternion correctBombRot;
	private StickyBomb sb;

	private int groundedTimer = 5 * 60;
	private int groundedTimerReset = 5 * 60;

	private int countdownTimer = 10 * 60;
	private int countdownTimerReset = 10 * 60;
	private bool bursting = false;

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

					if(stuckPlayer.GetComponent<BigBoy>() != null && 
					   !stuckPlayer.GetComponent<BigBoy>().alreadyHit)
					{
						//take away a life but dont kill him
						GetComponent<PhotonView>().RPC("BigBoyHit", PhotonTargets.All, stuckPlayer.playerID, gotKillPlayer.playerID);
					}
					else
					{
						GetComponent<PhotonView>().RPC("DisplayKill", PhotonTargets.All, stuckPlayer.playerID, gotKillPlayer.playerID);
					}

					//check to see if we should spawn a crate
					players = GameObject.FindObjectsOfType<PlayerController>();
					if(players.Length == 1)
					{
						//dont spawn crate
					}
					else
					{
						//spawn crate
						//spawn a crate if we are master
						GameObject crate = PhotonNetwork.Instantiate ("StickyCrate", 
						                   GameObject.FindObjectOfType<GameCamera>().GetRandomSpawnPoint(), Quaternion.identity, 0);
					}
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
			else if(sb.hitGround)
			{
				groundedTimer--;
				
				if(groundedTimer % 60 == 0)
				{
					GetComponent<PhotonView>().RPC("DisplayCountdown", PhotonTargets.All, groundedTimer / 60);
				}

				if(groundedTimer <= 180 && groundedTimer % 30 == 0)
				{
					GetComponent<PhotonView>().RPC("Bursting", PhotonTargets.All, bursting);
					bursting = !bursting;
				}
				
				if(groundedTimer == 0)
				{
					GetComponent<PhotonView>().RPC("Blowup", PhotonTargets.All);

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

					//TODO: stuck player doesnt exist...loop thru all players and calculate their positions to see if they are affected 
					//by the bomb
					/*
					if(stuckPlayer.GetComponent<BigBoy>() != null && 
					   !stuckPlayer.GetComponent<BigBoy>().alreadyHit)
					{
						//take away a life but dont kill him
						GetComponent<PhotonView>().RPC("BigBoyHit", PhotonTargets.All, stuckPlayer.playerID, gotKillPlayer.playerID);
					}
					else
					{
						GetComponent<PhotonView>().RPC("DisplayKill", PhotonTargets.All, stuckPlayer.playerID, gotKillPlayer.playerID);
					}*/
					int[] hits = new int[]{-1,-1,-1,-1};
					int[] pID = new int[]{-1,-1,-1,-1};
					for(int i = 0; i < players.Length; ++i)
					{
						PlayerController p = players[i];
						//pID[i] = p.playerID;
						if(Vector3.Distance(this.transform.position, p.transform.position) < 1.8f)
						{
							hits[i] = p.playerID;
						}
					}

					GetComponent<PhotonView>().RPC("DisplayKillGround", PhotonTargets.All, gotKillPlayer.playerID,
					                              hits[0], hits[1], hits[2], hits[3]);
					
					//check to see if we should spawn a crate
					players = GameObject.FindObjectsOfType<PlayerController>();
					if(players.Length == 1 && PhotonNetwork.room.playerCount != 1)
					{
						//dont spawn crate
					}
					else
					{
						GameObject crate = PhotonNetwork.Instantiate ("StickyCrate", 
						                   GameObject.FindObjectOfType<GameCamera>().GetRandomSpawnPoint(), Quaternion.identity, 0);
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
	void Bursting(bool b)
	{
		if(b)
		{
			GetComponent<SpriteRenderer>().material.color = Color.white;
		}
		else
		{
			GetComponent<SpriteRenderer>().material.color = Color.red;
		}
	}

	[PunRPC]
	void HitGround()
	{
		GetComponent<StickyBomb> ().hitGround = true;
		PopText.Create ("Hit Ground!", Color.white, 60, transform.position + Vector3.up * .5f);
		GetComponent<Rigidbody2D> ().velocity = Vector2.zero;
		GetComponent<Rigidbody2D> ().velocity = Vector2.zero;
	}

	[PunRPC]
	void DisplayKill(int killed, int killer)
	{
		PlayerController stuckPlayer = null;
		PlayerController gotKillPlayer = null;
		PlayerController[] players = GameObject.FindObjectsOfType<PlayerController>();
		
		foreach(var p in players)
		{
			if(p.playerID == sb.stuckID)
			{
				stuckPlayer = p;
			}
		}
		foreach(var p in players)
		{
			if(p.playerID == sb.ownerID)
			{
				gotKillPlayer = p;
			}
		}

		//there isnt always a killed player - if there is, lets kill him but if not move on
		if(killed != -1)
		{
			stuckPlayer.isStuck = false;
			stuckPlayer.gameObject.active = false;

			gotKillPlayer.myStats.Kills++;
			stuckPlayer.myStats.Deaths++;

			players = GameObject.FindObjectsOfType<PlayerController>();
			if(players.Length == 1)
			{
				PopText.Create ("ROUND OVER - P" + killer.ToString() + " WINS!", Color.white, 250, 
				                new Vector3(0, 4, 1));
				gotKillPlayer.myStats.RoundsWon++;
				GameObject.FindObjectOfType<GameCamera>().RoundOver();
			}
			else
			{
				PopText.Create ("P" + killed.ToString () + " was killed by P" + killer.ToString(), Color.white, 250, 
				                new Vector3(0, 4, 1));
			}
		}

		this.gameObject.active = false;
	}

	[PunRPC]
	void DisplayKillGround(int threwTheBomb, int op1, int op2, int op3, int op4)
	{
		PlayerController gotKillPlayer = null;
		PlayerController[] players = GameObject.FindObjectsOfType<PlayerController> ();
		List<PlayerController> deadPlayers = new List<PlayerController> ();

		foreach(var p in players)
		{
			if(p.playerID == sb.ownerID)
			{
				gotKillPlayer = p;
			}
			if(p.playerID == op1)
				deadPlayers.Add(p);
			else if(p.playerID == op2)
				deadPlayers.Add(p);
			else if(p.playerID == op3)
				deadPlayers.Add(p);
			else if(p.playerID == op4)
				deadPlayers.Add(p);
		}

		foreach(PlayerController dp in deadPlayers)
		{
			dp.isStuck = false;
			dp.gameObject.active = false;
			if(dp != gotKillPlayer)
				gotKillPlayer.myStats.Kills++;
			dp.myStats.Deaths++;
		}
		
		players = GameObject.FindObjectsOfType<PlayerController>();
		if(players.Length == 1 && PhotonNetwork.playerList.Length != 1)
		{
			PopText.Create ("ROUND OVER - P" + gotKillPlayer.playerID.ToString() + " WINS!", Color.white, 250, 
			                new Vector3(0, 4, 1));
			gotKillPlayer.myStats.RoundsWon++;
			GameObject.FindObjectOfType<GameCamera>().RoundOver();
		}
		else if(deadPlayers.Count == 1)
		{
			if(deadPlayers[0].playerID == gotKillPlayer.playerID)
			{
				PopText.Create ("P" + deadPlayers[0].playerID.ToString () + " killed himself.", Color.white, 250, 
				                new Vector3(0, 4, 1));

				if(players.Length == 0)
				{
					//PopText.Create ("ROUND OVER - Tie!", Color.white, 250, 
					                //new Vector3(0, 4, 1));
					GameObject.FindObjectOfType<GameCamera>().RoundOver();
				}
			}
			else
			{
				PopText.Create ("P" + deadPlayers[0].playerID.ToString () + " was killed by P" + gotKillPlayer.playerID.ToString(), Color.white, 250, 
				                new Vector3(0, 4, 1));
			}
		}
		else if(players.Length == 0)
		{
			PopText.Create ("ROUND OVER - Tie!", Color.white, 250, 
			                new Vector3(0, 4, 1));
			GameObject.FindObjectOfType<GameCamera>().RoundOver();
		}
		else if(deadPlayers.Count > 1)
		{
			PopText.Create ("P" + gotKillPlayer.playerID.ToString() + " got a " + deadPlayers.Count.ToString () + " player multikill!", Color.white, 250, 
			                new Vector3(0, 4, 1));
		}
		
		this.gameObject.active = false;
	}

	[PunRPC]
	void BigBoyHit(int killed, int killer)
	{
		PlayerController stuckPlayer = null;
		PlayerController gotKillPlayer = null;
		PlayerController[] players = GameObject.FindObjectsOfType<PlayerController>();
		
		foreach(var p in players)
		{
			if(p.playerID == sb.stuckID)
			{
				stuckPlayer = p;
			}
		}
		foreach(var p in players)
		{
			if(p.playerID == sb.ownerID)
			{
				gotKillPlayer = p;
			}
		}
		
		stuckPlayer.isStuck = false;
		stuckPlayer.GetComponent<BigBoy> ().alreadyHit = true;

		PopText.Create ("P" + killed.ToString () + " was hit by P" + killer.ToString(), Color.white, 250, 
			                new Vector3(0, 4, 1));
		
		this.gameObject.active = false;
	}

	[PunRPC]
	void Blowup()
	{	
		this.gameObject.transform.SetParent (null);
		GetComponent<SpriteRenderer> ().material.color = Color.white;
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
