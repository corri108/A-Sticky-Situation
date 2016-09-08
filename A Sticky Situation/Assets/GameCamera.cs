using UnityEngine;
using System.Collections;

public class GameCamera : MonoBehaviour {

	float startSize;
	private Camera thisCamera;
	private TextMesh thisText;
	public float onPlayerJoinedSize = 5.5f;
	private bool _playerJoined = false;
	private bool _gameStarted = false;
	
	// Use this for initialization
	void Start () 
	{
		thisCamera = this.GetComponent<Camera> ();
		thisText = transform.GetChild (0).GetComponent<TextMesh> ();
		thisText.text = "Joining...";
		startSize = thisCamera.orthographicSize;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(_playerJoined)
		{
			thisCamera.orthographicSize = Mathf.Lerp(thisCamera.orthographicSize, onPlayerJoinedSize, .1f);

			if(!_gameStarted && PhotonNetwork.room != null)
			{
				if(PhotonNetwork.playerList.Length == PhotonNetwork.room.maxPlayers)
				{
					//start countdown
					BeginGame();
				}
			}
		}
		else
		{

		}
	}

	public void BeginGame()
	{
		PlayerController[] allPlayers = GameObject.FindObjectsOfType<PlayerController> ();

		foreach(var p in allPlayers)
		{
			p.canMove = true;
		}

		thisText.text = "Begin!";
		_gameStarted = true;
	}

	public void MyPlayerHasJoined()
	{
		_playerJoined = true;
		thisText.text = "Waiting for Others...";
	}
}
