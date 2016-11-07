using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

	// Use this for initialization
	void Start () 
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

	public void JoinedButNoPlayerPicked()
	{
		thisText.text = "";
	}

	void Update()
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

	void HideStatistics()
	{
		scoreboard.active = false;
		scoreboardText.text = "";
	}
	
	// Update is called once per frame
	void FixedUpdate () 
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
					BeginGame();
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
						EndRound();
					}
				}
			}
		}
		else
		{

		}
	}

	public void RoundOver()
	{
		_gameEnded = true;
		thisText.text = "";
	}

	void EndRound()
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

	public void BeginGame()
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
