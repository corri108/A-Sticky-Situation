using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PauseButtons : MonoBehaviour {

	public GameObject pauseScreen;
	public GameObject mainCanvas;

	// Use this for initialization
	void Start () 
	{
		pauseScreen.SetActive (false);
		mainCanvas.SetActive (true);
	}

	public void ReturnToMenu()
	{
		SceneManager.LoadScene ("Menu");
	}

	public void Resume()
	{
		GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");

		for (int x = 0; x < players.Length; x++)
		{
			players [x].GetComponent<PlayerController> ().isPaused = false;
		}

		pauseScreen.SetActive (false);
		mainCanvas.SetActive (true);
	}
}
