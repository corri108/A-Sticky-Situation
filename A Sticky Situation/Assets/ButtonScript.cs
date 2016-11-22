using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TextBox = UnityEngine.UI.InputField;

public class ButtonScript : MonoBehaviour {

	public Button createBtn;
	public TextBox roomName;
	public TextBox maxPlayers;
	public Button startCreateBtn;
	public Button startJoinBtn;
	public Button backBtn;
	public Button joinBtn;
	public Dropdown levelSelect;
	public GameObject title;

	public GameObject[] initalScreenItems;
	public Button localLevelSelect;
	public Cursor[] cursors;
	public GameObject[] playerSelectObjects;
	public GameObject[] levelSelectObjects;
	public Button playLocalButton;

	Xbox360Controller[] xcl;

	void Start()
	{
		cursors = GameObject.FindObjectsOfType<Cursor> ();
		xcl = new Xbox360Controller[]
		{
			new Xbox360Controller(1),
			new Xbox360Controller(2),
			new Xbox360Controller(3),
			new Xbox360Controller(4)
		};
	}

	void Update()
	{
		localLevelSelect.gameObject.active = true;
		foreach(var c in cursors)
		{
			if(!c.ReadyToPlay())
			{
				localLevelSelect.gameObject.active = false;
			}
		}

		if(playLocalButton.gameObject.active)
		{
			foreach(var c in xcl)
			{
				if(c.StartPressed())
				{
					StartLocalGame();
				}
			}
		}
	}

	public void Join()
	{
		title.SetActive (false);
		joinBtn.gameObject.SetActive (false);
		createBtn.gameObject.SetActive (false);
		levelSelect.gameObject.SetActive (false);
		startJoinBtn.gameObject.SetActive (true);
		roomName.gameObject.SetActive (true);
		backBtn.gameObject.SetActive (true);
		AudioSource.PlayClipAtPoint (GetComponent<NetworkingObjectMenu>().click, GameObject.FindGameObjectWithTag("MainCamera").transform.position);
	}

	public void Create()
	{
		title.SetActive (false);
		joinBtn.gameObject.SetActive (false);
		createBtn.gameObject.SetActive (false);
		startCreateBtn.gameObject.SetActive (true);
		levelSelect.gameObject.SetActive (true);
		roomName.gameObject.SetActive (true);
		maxPlayers.gameObject.SetActive (true);
		backBtn.gameObject.SetActive (true);
		AudioSource.PlayClipAtPoint (GetComponent<NetworkingObjectMenu>().click, GameObject.FindGameObjectWithTag("MainCamera").transform.position);
	}

	public void LocalPlay()
	{
		title.SetActive (false);
		joinBtn.gameObject.SetActive (false);
		createBtn.gameObject.SetActive (false);
		startCreateBtn.gameObject.SetActive (false);
		levelSelect.gameObject.SetActive (false);
		roomName.gameObject.SetActive (false);
		maxPlayers.gameObject.SetActive (false);
		backBtn.gameObject.SetActive (false);

		foreach(var f in initalScreenItems)
		{
			f.SetActive(false);
		}

		foreach(var f in playerSelectObjects)
		{
			f.SetActive(true);
		}

		AudioSource.PlayClipAtPoint (GetComponent<NetworkingObjectMenu>().click, GameObject.FindGameObjectWithTag("MainCamera").transform.position);
	}

	public void LevelSelect()
	{	
		foreach(var f in playerSelectObjects)
		{
			if(f.GetComponent<Cursor>() == null)
				f.SetActive(false);
		}

		foreach(var f in levelSelectObjects)
		{
			f.SetActive(true);
		}
		playLocalButton.gameObject.SetActive (true);
		
		AudioSource.PlayClipAtPoint (GetComponent<NetworkingObjectMenu>().click, GameObject.FindGameObjectWithTag("MainCamera").transform.position);
	}

	public void StartLocalGame()
	{	
		GlobalProperties.IS_NETWORKED = false;
		if(GlobalProperties.LEVEL.Equals(""))
		{
			PopText.Create("Choose a level!", Color.red, 120, playLocalButton.transform.position);
		}
		else
		{
			switch(GlobalProperties.LEVEL)
			{
			case "Boxlands":
				Application.LoadLevel("The Cottage");
				break;
			case  "Plains":
				Application.LoadLevel("Level2");
				break;
			case  "TwoRooms":
				Application.LoadLevel("Level3");
				break;
			}
		}
		AudioSource.PlayClipAtPoint (GetComponent<NetworkingObjectMenu>().click, GameObject.FindGameObjectWithTag("MainCamera").transform.position);
	}

	public void Backbtn()
	{
		title.SetActive (true);
		joinBtn.gameObject.SetActive (true);
		createBtn.gameObject.SetActive (true);
		startCreateBtn.gameObject.SetActive (false);
		startJoinBtn.gameObject.SetActive (false);
		levelSelect.gameObject.SetActive (false);
		roomName.gameObject.SetActive (false);
		maxPlayers.gameObject.SetActive (false);
		backBtn.gameObject.SetActive (false);
		AudioSource.PlayClipAtPoint (GetComponent<NetworkingObjectMenu>().click, GameObject.FindGameObjectWithTag("MainCamera").transform.position);
	}

	public void AboutButton()
	{
		SceneManager.LoadScene ("HowToScene");
	}
}