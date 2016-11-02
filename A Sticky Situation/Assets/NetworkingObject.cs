using UnityEngine;
using System.Collections;
using Photon;

public class NetworkingObject : Photon.PunBehaviour {

	public bool debugMode = false;
	//for spawns
	public Transform[] spawnPoints;
	//to make sure we dont double occupy a spawn
	[HideInInspector]
	public bool[] spawnsOccupied;
	// Use this for initialization

	//for accessing the select screen
	public GameObject selectScreen;
	public GameObject level;
	public byte debugCharacterSize = 2;
	private CharacterType characterSelection;
	void Start () 
	{
		//set select screen inactive until we join
		selectScreen.active = false;
		//set level active until we join
		level.active = true;

		spawnsOccupied = new bool[spawnPoints.Length];
		if(debugMode)
			PhotonNetwork.ConnectUsingSettings(GlobalProperties.VERSION);
		else
		{
			//we are in real mode, so we are already connected and in the room. lets set the select screen
			selectScreen.active = true;
			//set level active so we can see the select screen better.
			level.active = false;
			GameObject.FindObjectOfType<GameCamera>().JoinedButNoPlayerPicked();
		}
	}

	public void ReadyUp()
	{
		//you already have joined the room. init player now
		InitializePlayer(characterSelection);
		PhotonNetwork.isMessageQueueRunning = true;
		selectScreen.active = false;
		//set level active again for waiting stage
		level.active = true;
	}

	//character selection methods
	public void ChooseScientist()
	{
		characterSelection = CharacterType.Scientist;
	}
	public void ChooseGhost()
	{
		characterSelection = CharacterType.Ghost;
	}
	public void ChooseBigBoy()
	{
		characterSelection = CharacterType.BigBoy;
	}
	public void ChooseThief()
	{
		characterSelection = CharacterType.Thief;
	}

	public override void OnJoinedLobby()
	{
		if(debugMode)
			PhotonNetwork.JoinRandomRoom();
		else
		{

		}
	}

	//for joining random room
	void OnPhotonRandomJoinFailed()
	{
		if(debugMode)
		{
			Debug.Log("Can't join random room!");
			RoomOptions ro = new RoomOptions ();
			ro.MaxPlayers = debugCharacterSize;
			PhotonNetwork.CreateRoom("Dev Test Room" , ro, null);
		}
	}

	private void InitializePlayer(CharacterType type)
	{
		int yourPlayerID = -1;
		//get how many players there are first to figure out which player you are
		PhotonPlayer[] allPlayers = PhotonNetwork.playerList;
		if(allPlayers.Length > 0)
		{
			yourPlayerID = allPlayers.Length;
		}
		else 
		{
			yourPlayerID = 1;
		}
		
		Debug.Log ("Your ID: " + yourPlayerID);
		
		GameObject myPlayer = PhotonNetwork.Instantiate (type.ToString(), Vector3.zero, Quaternion.identity, 0);
		PlayerController controller = myPlayer.GetComponent<PlayerController> ();
		controller.enabled = true;
		controller.GetComponent<PhotonView>().RPC("SetPlayerNumber", PhotonTargets.AllBuffered, yourPlayerID);
		Rigidbody2D myBody = myPlayer.GetComponent<Rigidbody2D> ();
		myBody.gravityScale = GlobalProperties.GravityScale;
		if (type == CharacterType.Thief)
			myPlayer.GetComponent<ThiefAbility> ().enabled = true;
		
		if(controller.GetComponent<PhotonView>().owner.isMasterClient)
		{
			//spawn a crate if we are master
			//GameObject crate = PhotonNetwork.Instantiate ("StickyCrate", new Vector3(6,2.5f, 0), Quaternion.identity, 0);
			
			//also, use a spawn point, but only if we are master.
			int r = Random.Range(0, spawnPoints.Length);
			controller.transform.position = spawnPoints[r].position;
			spawnsOccupied[r] = true;
		}
	}

	public override void OnJoinedRoom ()
	{
		if(debugMode)
		{
			//since we are in debug mode, set select screen active here instead of up in Start()
			selectScreen.active = true;
			GameObject.FindObjectOfType<GameCamera>().JoinedButNoPlayerPicked();
			level.active = false;
		}
	}

	public override void OnPhotonPlayerConnected (PhotonPlayer newPlayer)
	{
		if(PhotonNetwork.isMasterClient)
		{
			//use a spawn point to tell other player where to go
			int r = Random.Range(0, spawnPoints.Length);
			PlayerController correctPlayer = null;
			PlayerController[] allPlayers = GameObject.FindObjectsOfType<PlayerController>();

			foreach(var p in allPlayers)
			{
				if(PhotonPlayer.Find(p.GetComponent<PhotonView>().ownerId) != null)
				{
					correctPlayer = p;
				}
			}

			while(spawnsOccupied[r])
			{
				r = Random.Range(0, spawnPoints.Length);
			}

			correctPlayer.transform.position = spawnPoints[r].position;
			spawnsOccupied[r] = true;
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	void OnGUI()
	{
		if(Diagnostics.ShowNetworkStats)
		{
			GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());

			if(PhotonNetwork.room != null)
			{
				GUILayout.Label(PhotonNetwork.room.ToString());
			}
		}
	}
}
