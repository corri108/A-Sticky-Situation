using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TextBox = UnityEngine.UI.InputField;

public class ButtonScript : MonoBehaviour {

	public Button createBtn;
	public TextBox roomName;
	public TextBox maxPlayers;
	public Button startCreateBtn;
	public Button startJoinBtn;
	public Button backBtn;
	public Button joinBtn;

	public void Join()
	{
		joinBtn.gameObject.SetActive (false);
		createBtn.gameObject.SetActive (false);
		startJoinBtn.gameObject.SetActive (true);
		roomName.gameObject.SetActive (true);
		backBtn.gameObject.SetActive (true);
	}

	public void Create()
	{
		joinBtn.gameObject.SetActive (false);
		createBtn.gameObject.SetActive (false);
		startCreateBtn.gameObject.SetActive (true);
		roomName.gameObject.SetActive (true);
		maxPlayers.gameObject.SetActive (true);
		backBtn.gameObject.SetActive (true);
	}

	public void Backbtn()
	{
		joinBtn.gameObject.SetActive (true);
		createBtn.gameObject.SetActive (true);
		startCreateBtn.gameObject.SetActive (false);
		startJoinBtn.gameObject.SetActive (false);
		roomName.gameObject.SetActive (false);
		maxPlayers.gameObject.SetActive (false);
		backBtn.gameObject.SetActive (false);
	}
}
