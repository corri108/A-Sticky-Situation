using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class InstructionPageManager : MonoBehaviour {

	public GameObject firstPage;
	public GameObject secondPage;
	public GameObject thirdPage;
	public GameObject fourthPage;
	public GameObject backBtn;
	public GameObject nextBtn;
	public AudioClip click;
	public AudioClip clickBack;

	int currentPage;

	// Use this for initialization
	void Start () 
	{
		currentPage = 1;
		backBtn.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void MenuButton()
	{
		AudioSource.PlayClipAtPoint (clickBack, this.transform.position);
		SceneManager.LoadScene ("Menu");
	}

	public void NextButton()
	{
		AudioSource.PlayClipAtPoint (click, this.transform.position);
		if (currentPage == 1) 
		{
			backBtn.SetActive (true);
			firstPage.SetActive (false);
			secondPage.SetActive (true);
			currentPage = 2;
		}
		else if (currentPage == 2) 
		{
			secondPage.SetActive (false);
			thirdPage.SetActive (true);
			currentPage = 3;
		}
		else if (currentPage == 3) 
		{
			thirdPage.SetActive (false);
			fourthPage.SetActive (true);
			nextBtn.SetActive (false);
			currentPage = 4;
		}
	}

	public void BackButton()
	{
		AudioSource.PlayClipAtPoint (click, this.transform.position);
		if (currentPage == 4) 
		{
			nextBtn.SetActive (true);
			fourthPage.SetActive (false);
			thirdPage.SetActive (true);
			currentPage = 3;
		}
		else if (currentPage == 3) 
		{
			thirdPage.SetActive (false);
			secondPage.SetActive (true);
			currentPage = 2;
		}
		else if (currentPage == 2) 
		{
			secondPage.SetActive (false);
			firstPage.SetActive (true);
			backBtn.SetActive (false);
			currentPage = 1;
		}
	}
}
