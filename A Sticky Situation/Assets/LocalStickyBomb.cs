using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LocalStickyBomb : MonoBehaviour {

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

	public GameObject localCratePrefab;
	public AudioClip blowupSound;
	public AudioClip countdownSound;
	
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

	}

	public int GetTimerInSeconds(bool grounded)
	{
		if(!grounded)
		{
			return countdownTimer / 60;
		}
		else
		{
			return groundedTimer / 60;
		}
	}
	
	void FixedUpdate()
	{
		if (sb == null)
			sb = GetComponent<StickyBomb> ();
		
		if(sb.isStuck)
		{
			PopText.Create("o", Color.cyan, 30, this.transform.position);
			countdownTimer--;
			
			if(countdownTimer % 60 == 0)
			{
				LOCAL_DisplayCountdown(countdownTimer / 60);
			}
			
			if(countdownTimer == 0)
			{
				bool lastBomb = true;
				
				StickyBomb[] allStickys = GameObject.FindObjectsOfType<StickyBomb>();
				if(allStickys.Length > 1)
					lastBomb = false;
				if(PlayerController.SomeoneHasSticky())
					lastBomb = false;

				Blowup();
				GameObject.Destroy(this.gameObject);
				
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
					BigBoyHit(stuckPlayer.playerID, gotKillPlayer.playerID);
				}
				else
				{
					if(stuckPlayer != null && gotKillPlayer != null)
						DisplayKill(stuckPlayer.playerID, gotKillPlayer.playerID);
				}
				
				//check to see if we should spawn a crate
				players = GameObject.FindObjectsOfType<PlayerController>();
				if(players.Length == 1)
				{
					//dont spawn crate
					Debug.Log("DIDNT SPAWN CRATE BECAUSE ONLY ONE PLAYER!");
				}
				else if(lastBomb)
				{
					//spawn crate
					GameObject crate = (GameObject)GameObject.Instantiate (localCratePrefab, 
					                                              GameObject.FindObjectOfType<GameCamera>().GetRandomSpawnPoint(), Quaternion.identity);
				}

				GameObject.FindObjectOfType<GameCamera>().UpdateStatsLocal();
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
				LOCAL_DisplayCountdown(groundedTimer / 60);
			}
			
			if(groundedTimer <= 180 && groundedTimer % 30 == 0)
			{
				Bursting(bursting);
				bursting = !bursting;
			}
			
			if(groundedTimer == 0)
			{
				bool lastBomb = true;

				StickyBomb[] allStickys = GameObject.FindObjectsOfType<StickyBomb>();
				if(allStickys.Length > 1)
					lastBomb = false;
				if(PlayerController.SomeoneHasSticky())
					lastBomb = false;

				Blowup();
				
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

				if(gotKillPlayer != null)
				{
					DisplayKillGround(gotKillPlayer.playerID,
				                               hits[0], hits[1], hits[2], hits[3]);
				}
				
				//check to see if we should spawn a crate
				players = GameObject.FindObjectsOfType<PlayerController>();
				if(players.Length == 1)
				{
					//dont spawn crate, round is over
				}
				else if(lastBomb)//only spawn a crate if all the bombs are gone (there might be a scientist on the map)
				{
					GameObject crate = (GameObject)GameObject.Instantiate (localCratePrefab, 
					                                                       GameObject.FindObjectOfType<GameCamera>().GetRandomSpawnPoint(), Quaternion.identity);
				}
			}
		}
		else if(!sb.hitGround)
		{
			PopText.Create("o", Color.cyan, 30, this.transform.position);
		}
	}

	void LOCAL_DisplayCountdown(int num)
	{
		PopText.Create (num.ToString (), Color.white, 60, transform.position + Vector3.up * .5f);
		AudioSource.PlayClipAtPoint(countdownSound, GameObject.FindObjectOfType<GameCamera>().transform.position);
	}

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

	public void HitGround()
	{
		GetComponent<StickyBomb> ().hitGround = true;
		PopText.Create ("Hit Ground!", Color.white, 60, transform.position + Vector3.up * .5f);
		GetComponent<Rigidbody2D> ().velocity = Vector2.zero;
		GetComponent<Rigidbody2D> ().velocity = Vector2.zero;
		GetComponent<Rigidbody2D> ().gravityScale = 0;
	}

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
				
				if(players.Length == 0 || players.Length == 1)
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
		stuckPlayer.transform.FindChild("BigBoyStatus").GetComponent<SpriteRenderer>().enabled = false;
		
		PopText.Create ("P" + killed.ToString () + " was hit by P" + killer.ToString(), Color.white, 250, 
		                new Vector3(0, 4, 1));
		
		this.gameObject.active = false;
	}

	void Blowup()
	{	
		this.gameObject.transform.SetParent (null);
		AudioSource.PlayClipAtPoint(blowupSound, GameObject.FindObjectOfType<GameCamera>().transform.position);
		GetComponent<SpriteRenderer> ().material.color = Color.white;
		GameObject.Destroy ((GameObject)GameObject.Instantiate (blowupPS, this.transform.position, Quaternion.identity), 1f);
	}
}
