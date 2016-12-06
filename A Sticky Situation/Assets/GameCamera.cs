using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameCamera : MonoBehaviour {

	float startSize;
	private Camera thisCamera;
	private TextMesh thisText;
	public float onPlayerJoinedSize = 5.5f;
	[HideInInspector]
	public bool _playerJoined = false;
	[HideInInspector]
	public bool _gameStarted = false;
	private bool _initTimer = false;
	private bool _gameEnded = false;
	private bool playerStuck = false;

	private Vector3 origScale;
	private GameObject scoreboard;
	private TextMesh scoreboardText;

	public AudioClip countdownNoise;
	public Transform[] spawnPoints;
	//to make sure we dont double occupy a spawn
	[HideInInspector]
	public bool[] spawnsOccupied;
	[HideInInspector]
	public List<PlayerController> playerList;

	[HideInInspector]
	public int gameStartCountdown = 60 * 3;
	[HideInInspector]
	public int gameFinishedTimer = 60 * 10;

	//local
	public GameObject localCratePrefab;
	public AudioClip applause;
	public AudioClip tieNoise;
	public GameObject confetti;
	public Light spotlight;
	bool endingEffects = false;
	int endingTimer = 60 * 5;
	int endingTimerReset = 60 * 5;

	// Use this for initialization
	void Start () 
	{
		if(GlobalProperties.IS_NETWORKED)
		{
			spawnsOccupied = new bool[spawnPoints.Length];
			playerList = new List<PlayerController> ();
			playerList.Clear ();
			thisCamera = this.GetComponent<Camera> ();
			thisText = transform.GetChild (0).GetComponent<TextMesh> ();
			thisText.text = "Joining...";
			startSize = thisCamera.orthographicSize;
			scoreboard = transform.FindChild ("Scoreboard").gameObject;
			scoreboardText = scoreboard.transform.GetChild (0).GetComponent<TextMesh> ();
		}
		else
		{
			playerList = new List<PlayerController> ();
			playerList.Clear ();
			spawnsOccupied = new bool[spawnPoints.Length];
			//make players
			for(int i = 0; i < GlobalProperties.PLAYERCHOICE.Length; ++i)
			{
				if(GlobalProperties.PLAYERCHOICE[i].Equals("NA"))
				{
					//dont make a player
					//turn off ui object
					GameObject canvasObject = GameObject.Find ("Canvas").transform.FindChild("P" + (i + 1).ToString() + "UI").gameObject;
					canvasObject.active = false;
				}
				else
				{
					bool isAI = !GlobalProperties.PLAYERCTRL[i];
					//make a new player
					if(GlobalProperties.PLAYERCHOICE[i].Equals("Scientist"))
					{
						GameObject scientistPref = Resources.Load<GameObject>("FINAL_Scientist");
						PlayerController player = ((GameObject)GameObject.Instantiate(scientistPref, Vector3.zero, Quaternion.identity)).GetComponent<PlayerController>();
						player.playerID = i + 1;
						Rigidbody2D myBody = player.GetComponent<Rigidbody2D> ();
						myBody.gravityScale = GlobalProperties.GravityScale;
						if(isAI)
						{
							player.gameObject.AddComponent<AIComponent>();
						}
						player.LOCAL_SetPlayerNumber(player.playerID);
						playerList.Add(player);
					}
					else if(GlobalProperties.PLAYERCHOICE[i].Equals("BigBoy"))
					{
						GameObject scientistPref = Resources.Load<GameObject>("FINAL_BigBoy");
						PlayerController player = ((GameObject)GameObject.Instantiate(scientistPref, Vector3.zero, Quaternion.identity)).GetComponent<PlayerController>();
						player.playerID = i + 1;
						Rigidbody2D myBody = player.GetComponent<Rigidbody2D> ();
						myBody.gravityScale = GlobalProperties.GravityScale;
						if(isAI)
						{
							player.gameObject.AddComponent<AIComponent>();
						}
						player.LOCAL_SetPlayerNumber(player.playerID);
						playerList.Add(player);
					}
					else if(GlobalProperties.PLAYERCHOICE[i].Equals("Thief"))
					{
						GameObject scientistPref = Resources.Load<GameObject>("FINAL_Thief");
						PlayerController player = ((GameObject)GameObject.Instantiate(scientistPref, Vector3.zero, Quaternion.identity)).GetComponent<PlayerController>();
						player.playerID = i + 1;
						Rigidbody2D myBody = player.GetComponent<Rigidbody2D> ();
						myBody.gravityScale = GlobalProperties.GravityScale;
						if(isAI)
						{
							player.gameObject.AddComponent<AIComponent>();
						}
						player.LOCAL_SetPlayerNumber(player.playerID);
						playerList.Add(player);
					}
					else if(GlobalProperties.PLAYERCHOICE[i].Equals("Ghost"))
					{
						GameObject scientistPref = Resources.Load<GameObject>("FINAL_Ghost");
						PlayerController player = ((GameObject)GameObject.Instantiate(scientistPref, Vector3.zero, Quaternion.identity)).GetComponent<PlayerController>();
						player.playerID = i + 1;
						Rigidbody2D myBody = player.GetComponent<Rigidbody2D> ();
						myBody.gravityScale = GlobalProperties.GravityScale;
						if(isAI)
						{
							player.gameObject.AddComponent<AIComponent>();
						}
						player.LOCAL_SetPlayerNumber(player.playerID);
						playerList.Add(player);
					}
				}
			}

			foreach(var p in playerList)
			{
				p.GetComponent<PlayerController>().LOCAL_SetRandomSpawn();
			}

			thisCamera = this.GetComponent<Camera> ();
			thisText = transform.GetChild (0).GetComponent<TextMesh> ();
			startSize = thisCamera.orthographicSize;
			scoreboard = transform.FindChild ("Scoreboard").gameObject;
			scoreboardText = scoreboard.transform.GetChild (0).GetComponent<TextMesh> ();
		}
	}

	public void JoinedButNoPlayerPicked()
	{
		thisText.text = "";
	}

	void Update()
	{
		if(GlobalProperties.IS_NETWORKED)
		{
			if(Input.GetKey(KeyCode.Tab))
			{
				ShowStatistics();
			}
			else if(Input.GetKeyUp(KeyCode.Tab))
			{
				HideStatistics();
			}
		}
	}

	void ShowStatistics()
	{
		scoreboard.active = true;
		//PlayerController[] players = GameObject.FindObjectsOfType<PlayerController>();
		
		scoreboardText.text = "";
		foreach(var p in playerList)
		{
			scoreboardText.text += p.myStats.ToString() + "\n";
		}
	}

	public void UpdateStatsLocal()
	{
		for(int i = 0; i < playerList.Count; ++i)
		{
			PlayerController p = playerList[i];
			GameObject canvasObject = GameObject.Find ("Canvas").transform.FindChild("P" + (i + 1).ToString() + "UI").gameObject;
			Text roundsWon = canvasObject.transform.GetChild(1).GetComponent<Text>();
			roundsWon.text = p.myStats.RoundsWon.ToString() + "\n";
			Text stats = canvasObject.transform.GetChild(3).GetComponent<Text>();
			stats.text = string.Format("K : {0}\nD : {1}\nC : {2}", p.myStats.Kills, p.myStats.Deaths, p.myStats.CratesPickedUp);
		}
	}

	void HideStatistics()
	{
		scoreboard.active = false;
		scoreboardText.text = "";
	}

	void NetworkFixedUpdate ()
	{
		if(_playerJoined)
		{
			thisCamera.orthographicSize = Mathf.Lerp(thisCamera.orthographicSize, onPlayerJoinedSize, .1f);
			
			if(!_gameStarted && PhotonNetwork.room != null && !_initTimer)
			{
				if(PhotonNetwork.playerList.Length == PhotonNetwork.room.maxPlayers && 
				   PhotonNetwork.playerList.Length == playerList.Count)
				{
					//start countdown
					_initTimer = true;
				}
			}
			else if(!_gameStarted && PhotonNetwork.room != null && _initTimer)
			{
				gameStartCountdown--;
				thisText.text = ( (gameStartCountdown / 60) + 1).ToString() + "...";
				
				if(gameStartCountdown % 60 == 0)
				{
					AudioSource.PlayClipAtPoint(countdownNoise, this.transform.position);
				}
				
				if(gameStartCountdown == 0)
				{
					AudioSource.PlayClipAtPoint(countdownNoise, this.transform.position);
					BeginGameNetwork();
					origScale = thisText.transform.localScale;
					thisText.text = "";
				}
			}
			else if(_gameStarted && !_gameEnded)
			{
				thisText.transform.localScale *= .95f;
				thisText.text = "";
			}
			else if(_gameEnded)
			{
				thisText.transform.localScale = origScale;
				gameFinishedTimer--;
				if(gameFinishedTimer <= 300)
				{
					thisText.text = "Next round in " + ((gameFinishedTimer / 60) + 1).ToString() + "...";
					if(gameFinishedTimer == 0)
					{
						EndRoundNetwork();
					}
				}
			}
		}
		else
		{
			
		}
	}
	
	void LocalFixedUpdate ()
	{
		thisCamera.orthographicSize = Mathf.Lerp(thisCamera.orthographicSize, onPlayerJoinedSize, .1f);

		if(!_gameStarted)
		{
			gameStartCountdown--;
			thisText.text = ((gameStartCountdown / 60) + 1).ToString() + "...";
			
			if(gameStartCountdown % 60 == 0)
			{
				AudioSource.PlayClipAtPoint(countdownNoise, this.transform.position);
			}
			
			if(gameStartCountdown == 0)
			{
				AudioSource.PlayClipAtPoint(countdownNoise, this.transform.position);
				BeginGameLocal();
				Debug.Log("New Round");
				origScale = thisText.transform.localScale;
				thisText.text = "";
			}
		}
		else if(_gameStarted && !_gameEnded)
		{
			thisText.transform.localScale *= .95f;
			thisText.text = "";
		}
		else if(_gameEnded)
		{
			thisText.transform.localScale = origScale;
			gameFinishedTimer--;
			if(gameFinishedTimer <= 300)
			{
				thisText.text = "Next round in " + ((gameFinishedTimer / 60) + 1).ToString() + "...";
				if(gameFinishedTimer == 0)
				{
					EndRoundLocal();
				}
			}
		}

		if(endingEffects)
		{
			endingTimer--;

			if(endingTimer % 5 == 0)
			{
				GameObject.Destroy((GameObject)GameObject.Instantiate(confetti, spotlight.transform.position, Quaternion.identity), 3f);
			}

			if(endingTimer == 40)
			{
				spotlight.GetComponent<WinningSpotlight> ().Close ();
			}

			if(endingTimer == 0)
			{
				endingEffects = false;
			}
		}
	}

	// Update is called once per frame
	void FixedUpdate () 
	{
		if(GlobalProperties.IS_NETWORKED)
		{
			NetworkFixedUpdate ();
		}
		else
		{
			LocalFixedUpdate ();
		}
	}

	public void RoundOver()
	{
		_gameEnded = true;
		thisText.text = "";
		if(GameObject.FindObjectsOfType<PlayerController> ().Length > 0)
		{
			AudioSource.PlayClipAtPoint(applause, GameObject.FindObjectOfType<GameCamera>().transform.position);
			QueueSpotlightLocal ();
		}
		else
		{
			//it was a tie
			AudioSource.PlayClipAtPoint(tieNoise, GameObject.FindObjectOfType<GameCamera>().transform.position);
		}
		UpdateStatsLocal ();
	}

	void QueueSpotlightLocal()
	{
		endingEffects = true;
		if(spotlight == null)
		{
			spotlight = GameObject.FindObjectOfType<WinningSpotlight>().GetComponent<Light>();
		}
		spotlight.GetComponent<WinningSpotlight> ().Open ();
		spotlight.GetComponent<WinningSpotlight> ().winner = GameObject.FindObjectsOfType<PlayerController> () [0].gameObject;
		endingTimer = endingTimerReset;
	}

	void EndRoundNetwork()
	{
		foreach(var p in playerList)
		{
			p.gameObject.active = true;
		}

		_gameEnded = false;
		_gameStarted = false;
		_initTimer = false;
		_playerJoined = false;
		thisText.text = "";
		spawnsOccupied = new bool[spawnPoints.Length];
		foreach(var p in playerList)
		{
			p.GetComponent<PhotonView>().RPC("SetRandomSpawn", PhotonTargets.All, p.playerID);
		}
		_playerJoined = true;
		_initTimer = true;
		gameStartCountdown = 60 * 3;
		gameFinishedTimer = 60 * 10;
	}

	void EndRoundLocal()
	{
		foreach(var p in playerList)
		{
			p.gameObject.active = true;
			p.LOCAL_ResetPlayer();
		}
		
		_gameEnded = false;
		_gameStarted = false;
		thisText.text = "";
		spawnsOccupied = new bool[spawnPoints.Length];
		foreach(var p in playerList)
		{
			p.GetComponent<PlayerController>().LOCAL_SetRandomSpawn();
		}
		gameStartCountdown = 60 * 3;
		gameFinishedTimer = 60 * 10;
	}

	public void BeginGameLocal()
	{
		PlayerController[] allPlayers = GameObject.FindObjectsOfType<PlayerController> ();
		
		foreach(var p in allPlayers)
		{
			p.canMove = true;
		}
		
		foreach(var p in playerList)
		{
			p.GetComponent<PlayerController>().LOCAL_SetCanMove();
		}
		
		thisText.text = "Begin!";
		_gameStarted = true;
		int r = Random.Range(0,spawnPoints.Length);

		//destroy any crates that might be alive for some reason
		StickyCrate[] oldCrates = GameObject.FindObjectsOfType<StickyCrate> ();
		foreach(var c in oldCrates)
		{
			GameObject.Destroy(c.gameObject);
		}

		//make new crate for this round
		GameObject crate = (GameObject)GameObject.Instantiate (localCratePrefab, spawnPoints[r].position, Quaternion.identity);
		GetComponent<CameraTrack> ().enabled = true;
		GetComponent<CameraTrack> ().SetTargets (playerList);
	}

	public void BeginGameNetwork()
	{
		PlayerController[] allPlayers = GameObject.FindObjectsOfType<PlayerController> ();

		foreach(var p in allPlayers)
		{
			p.canMove = true;
		}

		foreach(var p in playerList)
		{
			p.GetComponent<PhotonView>().RPC("SetCanMove", PhotonTargets.All);
		}

		thisText.text = "Begin!";
		_gameStarted = true;

		if(PhotonNetwork.isMasterClient)
		{
			int r = Random.Range(0,spawnPoints.Length);
			GameObject crate = PhotonNetwork.Instantiate ("StickyCrate", spawnPoints[r].position, Quaternion.identity, 0);
			//GameObject crate = PhotonNetwork.Instantiate ("StickyCrate", new Vector3(6,2.5f, 0), Quaternion.identity, 0);
		}

		GetComponent<CameraTrack> ().enabled = true;
		GetComponent<CameraTrack> ().SetTargets (playerList);
	}

	public void PlayerFellLocal (PlayerController player)
	{
		int numPlayersIncludingYou = GameObject.FindObjectsOfType<PlayerController> ().Length;
		
		//AudioSource.PlayClipAtPoint(countdownSound, GameObject.FindObjectOfType<GameCamera>().transform.position);

		if(numPlayersIncludingYou == 1)
		{
			//dont worry, we already won
			PopText.Create ("P" + player.playerID.ToString () + " killed himself out of happiness.", Color.white, 250, 
			                new Vector3(0, 4, 1));
		}
		else if(numPlayersIncludingYou == 2)
		{
			player.myStats.Deaths++;
			player.gameObject.active = false;
			PlayerController winner = GameObject.FindObjectsOfType<PlayerController> ()[0];
			winner.myStats.RoundsWon++;
			//you just made someone else win
			PopText.Create ("ROUND OVER - P" + winner.playerID.ToString() + " WINS!", Color.white, 250, 
			                new Vector3(0, 4, 1));
			GameObject.FindObjectOfType<GameCamera>().RoundOver();
		}
		else
		{
			player.myStats.Deaths++;
			//you died, tragic, but the show must go on.
			PopText.Create ("P" + player.playerID.ToString () + " killed himself.", Color.white, 250, 
			                new Vector3(0, 4, 1));
		}
	}

	public Vector3 GetRandomSpawnPoint()
	{
		return spawnPoints [Random.Range (0, spawnPoints.Length)].position;
	}

	public void BombStuck()
	{
		playerStuck = true;
	}
	public void MyPlayerHasJoined()
	{
		_playerJoined = true;
		thisText.text = "Waiting for Others...";
	}
}
