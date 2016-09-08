using UnityEngine;
using System.Collections;
using Photon;

public class NetworkingObject : Photon.PunBehaviour {

	// Use this for initialization
	void Start () 
	{
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
		GameObject[] allPlayers = GameObject.FindGameObjectsWithTag ("Player");
		if(allPlayers.Length > 0)
		{
			yourPlayerID = allPlayers.Length;
		}
		else yourPlayerID = 1;

		Debug.Log ("Your ID: " + yourPlayerID);

		GameObject monster = PhotonNetwork.Instantiate ("TestPlayer", Vector3.zero, Quaternion.identity, 0);
		PlayerController controller = monster.GetComponent<PlayerController> ();
		controller.enabled = true;
		controller.SetPlayerNumber (yourPlayerID);
		Rigidbody2D myBody = monster.GetComponent<Rigidbody2D> ();
		myBody.gravityScale = GlobalProperties.GravityScale;
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
