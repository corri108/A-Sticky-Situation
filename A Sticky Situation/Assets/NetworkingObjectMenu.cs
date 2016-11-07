
using UnityEngine;
using System.Collections;
using Photon;
using Button = UnityEngine.UI.Button;
using TextBox = UnityEngine.UI.InputField;
using Dropdown = UnityEngine.UI.Dropdown;

public class NetworkingObjectMenu : Photon.PunBehaviour {

	public AudioClip click;
	public AudioClip clickBack;
	public Button connect;
	public Button about;
	public Button quit;
	public Button join;
	public Button create;
	public TextBox roomName;
	public TextBox maxPlayers;
	public Dropdown levelChoose;

	// Use this for initialization
	void Start () 
	{

	}
	
	public override void OnJoinedLobby()
	{
		//turn off old options
		connect.gameObject.active = false;
		quit.gameObject.active = false;
		about.gameObject.active = false;
		//turn on new options
		join.gameObject.active = true;
		create.gameObject.active = true;

		AudioSource.PlayClipAtPoint (click, this.transform.position);
		PopText.Create ("Connected to game server.", Color.black, 150, this.transform.position);
	}
	
	//for joining random room
	void OnPhotonRandomJoinFailed()
	{

	}
	
	public override void OnJoinedRoom ()
	{
		PhotonNetwork.isMessageQueueRunning = false;

		string levelString = "TestScene";
		if(levelChoose.captionText.text.Equals("Boxlands"))
			Application.LoadLevel ("TestScene");
		else
			Application.LoadLevel ("Level2");
	}
	
	public override void OnPhotonPlayerConnected (PhotonPlayer newPlayer)
	{

	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	public void ClickConnect()
	{
		AudioSource.PlayClipAtPoint (click, this.transform.position);
		PhotonNetwork.ConnectUsingSettings (GlobalProperties.VERSION);
	}

	public void ClickJoin()
	{
		AudioSource.PlayClipAtPoint (click, this.transform.position);
		PhotonNetwork.JoinRoom (roomName.text);
		PopText.Create ("Joining room \"" + roomName.text + "\"...", Color.white, 200, this.transform.position);
	}
	
	public void ClickCreate()
	{
		string level = levelChoose.captionText.text;
		Debug.Log ("LEVEL: " + level);
		GlobalProperties.LEVEL = level;
		RoomOptions ro = new RoomOptions ();
		ro.MaxPlayers = byte.Parse(maxPlayers.text);
		AudioSource.PlayClipAtPoint (click, this.transform.position);
		PhotonNetwork.JoinOrCreateRoom (roomName.text, ro, TypedLobby.Default);
		PopText.Create ("Creating and joining room \"" + roomName.text + "\"...", Color.white, 200, this.transform.position);
	}
	
	public void ClickQuit()
	{
		AudioSource.PlayClipAtPoint (clickBack, this.transform.position);
		Application.Quit ();
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
