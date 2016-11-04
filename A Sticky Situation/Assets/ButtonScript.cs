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
	public GameObject title;

	public void Join()
	{
		title.SetActive (false);
		joinBtn.gameObject.SetActive (false);
		createBtn.gameObject.SetActive (false);
		startJoinBtn.gameObject.SetActive (true);
		roomName.gameObject.SetActive (true);
		backBtn.gameObject.SetActive (true);
		AudioSource.PlayClipAtPoint (GetComponent<NetworkingObjectMenu>().click, this.transform.position);
	}

	public void Create()
	{
		title.SetActive (false);
		joinBtn.gameObject.SetActive (false);
		createBtn.gameObject.SetActive (false);
		startCreateBtn.gameObject.SetActive (true);
		roomName.gameObject.SetActive (true);
		maxPlayers.gameObject.SetActive (true);
		backBtn.gameObject.SetActive (true);
		AudioSource.PlayClipAtPoint (GetComponent<NetworkingObjectMenu>().click, this.transform.position);
	}

	public void Backbtn()
	{
		title.SetActive (true);
		joinBtn.gameObject.SetActive (true);
		createBtn.gameObject.SetActive (true);
		startCreateBtn.gameObject.SetActive (false);
		startJoinBtn.gameObject.SetActive (false);
		roomName.gameObject.SetActive (false);
		maxPlayers.gameObject.SetActive (false);
		backBtn.gameObject.SetActive (false);
		AudioSource.PlayClipAtPoint (GetComponent<NetworkingObjectMenu>().clickBack, this.transform.position);

	}

	public void AboutButton()
	{
		SceneManager.LoadScene ("HowToScene");
	}
}