
using UnityEngine;
using System.Collections;
using Photon;

public class NetworkingObject : Photon.PunBehaviour {

	//for spawns
	public Transform[] spawnPoints;
	//to make sure we dont double occupy a spawn
	[HideInInspector]
	public bool[] spawnsOccupied;
	// Use this for initialization
	void Start () 
	{
		spawnsOccupied = new bool[spawnPoints.Length];
		PhotonNetwork.ConnectUsingSettings("Alpha v0.1");
	}

	public override void OnJoinedLobby()
	{
		PhotonNetwork.JoinRandomRoom();
	}

	//for joining random room
	void OnPhotonRandomJoinFailed()
	{
		Debug.Log("Can't join random room!");
		RoomOptions ro = new RoomOptions ();
		ro.MaxPlayers = 2;
		PhotonNetwork.CreateRoom("Dev Test Room" , ro, null);
	}

	public override void OnJoinedRoom ()
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

		GameObject myPlayer = PhotonNetwork.Instantiate ("TestPlayer", Vector3.zero, Quaternion.identity, 0);
		PlayerController controller = myPlayer.GetComponent<PlayerController> ();
		controller.enabled = true;
		controller.GetComponent<PhotonView>().RPC("SetPlayerNumber", PhotonTargets.AllBuffered, yourPlayerID);
		Rigidbody2D myBody = myPlayer.GetComponent<Rigidbody2D> ();
		myBody.gravityScale = GlobalProperties.GravityScale;

		if(controller.GetComponent<PhotonView>().owner.isMasterClient)
		{
			//spawn a crate if we are master
			GameObject crate = PhotonNetwork.Instantiate ("StickyCrate", new Vector3(6,2.5f, 0), Quaternion.identity, 0);

			//also, use a spawn point, but only if we are master.
			int r = Random.Range(0, spawnPoints.Length);
			controller.transform.position = spawnPoints[r].position;
			spawnsOccupied[r] = true;
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
