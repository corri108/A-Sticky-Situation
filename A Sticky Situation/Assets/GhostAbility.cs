using UnityEngine;
using System.Collections;
using Photon;

public class GhostAbility : PunBehaviour {

	public Material material;
	public Material mainMaterial;
	public Material ghostMaterial;
	public GameObject[] hideObjects;
	public bool abilityAvailable;
	[HideInInspector]
	public AbilityStatus abs = null;
	bool used;

	float timer;
	Color tempColor;

	public int wait = 5;

	void Start()
	{
		mainMaterial = transform.GetChild (0).GetChild (0).GetComponent<SpriteRenderer> ().material;
		abilityAvailable = true;
	}

	// Update is called once per frame
	void Update () 
	{
		if (Camera.main.GetComponent<GameCamera> ()._gameStarted) 
		{

		} 
		else 
		{
			abilityAvailable = true;
			used = false;
		}

		if (used) 
		{
			if (timer < wait) 
			{
				transform.GetChild (0).GetChild (0).GetComponent<SpriteRenderer> ().material = ghostMaterial;
				//5
				for(int i = 0; i < 5; ++i)
				{
					transform.GetChild (0).GetChild (0).GetChild (i).GetComponent<SpriteRenderer> ().material = ghostMaterial;
					if(i != 4)
					{
						transform.GetChild (0).GetChild (0).GetChild (i).GetChild(0).GetComponent<SpriteRenderer> ().material = ghostMaterial;
						transform.GetChild (0).GetChild (0).GetChild (i).GetChild(0).GetChild(0).GetComponent<SpriteRenderer> ().material = ghostMaterial;
					}
				}
				foreach(var ho in hideObjects)
				{
					ho.SetActive (false);
				}
				timer += Time.deltaTime;
			} 
			else 
			{
				foreach(var ho in hideObjects)
				{
					ho.SetActive (true);
				}
				transform.GetChild (0).GetChild (0).GetComponent<SpriteRenderer> ().material = mainMaterial;
				//5
				for(int i = 0; i < 5; ++i)
				{
					transform.GetChild (0).GetChild (0).GetChild (i).GetComponent<SpriteRenderer> ().material = mainMaterial;
					if(i != 4)
					{
						transform.GetChild (0).GetChild (0).GetChild (i).GetChild(0).GetComponent<SpriteRenderer> ().material = mainMaterial;
						transform.GetChild (0).GetChild (0).GetChild (i).GetChild(0).GetChild(0).GetComponent<SpriteRenderer> ().material = mainMaterial;
					}
				}

				timer = 0;
				used = false;
				abilityAvailable = true;
			}
		}

	}

	public void SetABS(AbilityStatus abs)
	{
		this.abs = abs;
	}

	public void LOCAL_Reset()
	{
		foreach(var ho in hideObjects)
		{
			ho.SetActive (true);
		}
		transform.GetChild (0).GetChild (0).GetComponent<SpriteRenderer> ().material = mainMaterial;
		//5
		for(int i = 0; i < 5; ++i)
		{
			transform.GetChild (0).GetChild (0).GetChild (i).GetComponent<SpriteRenderer> ().material = mainMaterial;
			if(i != 4)
			{
				transform.GetChild (0).GetChild (0).GetChild (i).GetChild(0).GetComponent<SpriteRenderer> ().material = mainMaterial;
				transform.GetChild (0).GetChild (0).GetChild (i).GetChild(0).GetChild(0).GetComponent<SpriteRenderer> ().material = mainMaterial;
			}
		}
		
		timer = 0;
		used = false;
		abilityAvailable = true;
	}

	public void LOCAL_Disappear()
	{
		if (abilityAvailable) 
		{
			used = true;
			abilityAvailable = false;
		}
		else
		{
			//play noise
		}
	}

	[PunRPC]
	void Disappear()
	{
		if (abilityAvailable) 
		{
			used = true;
			abilityAvailable = false;
		}
	}
}
