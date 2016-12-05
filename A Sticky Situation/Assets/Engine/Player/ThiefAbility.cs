using UnityEngine;
using System.Collections;
using Photon;
using UnityEngine.UI;

public class ThiefAbility : PunBehaviour {

	public bool abilityAvailable;
	bool used;
	[HideInInspector]
	public AbilityStatus abs = null;
	public bool inRange;
	GameObject target;
	bool trySteal = false;

	private int stealTimer = 30;
	private int stealTimerReset = 30;

	bool startSlider;
	public int abilityCD = 510;
	int sliderTimer;
	public GameObject abilitySlider;

	public GameObject abilityImage;
	public Sprite ThiefSprite;

	// Use this for initialization
	void Start () 
	{
		int id = GetComponent<PlayerController>().playerID;
		abilitySlider = GameObject.FindGameObjectWithTag ("P" + id + "Slider");
		abilitySlider.GetComponent<Slider>().maxValue = 8.5f;
		abilitySlider.GetComponent<Slider>().value = abilitySlider.GetComponent<Slider>().maxValue;

		abilityImage = GameObject.Find ("P" + id + "AbilityImage");
		abilityImage.GetComponent<Image> ().sprite = ThiefSprite;
		abilityImage.GetComponent<Image> ().color = Color.black;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Camera.main.GetComponent<GameCamera> ()._gameStarted) 
		{
			if (abilityAvailable) 
			{
				if (inRange && target != null) 
				{
					if(GlobalProperties.IS_NETWORKED)
					{
						NetworkLogic();
					}
					else
					{
						LocalLogic();
					}
				} 
			}
		} 
		else 
		{
			abilityAvailable = true;
		}
	}

	void FixedUpdate()
	{
		if (!GetComponent<PlayerController> ().isPaused)
		{
			if (trySteal)
			{
				//countdown until steal no longer works
				stealTimer--;
				if (stealTimer == 0)
				{
					stealTimer = stealTimerReset;
					trySteal = false;
				}
			}
			if (startSlider)
			{
				sliderTimer++;
				abilitySlider.GetComponent<Slider> ().value = sliderTimer / 60.0f;
				if (sliderTimer >= abilityCD)
				{
					sliderTimer = 0;
					startSlider = false;
					abilityImage.SetActive (true);
				}
			}
		}
	}

	public void LOCAL_Reset()
	{
		abilityAvailable = true;
		trySteal = false;
		target = null;
	}

	public void LOCAL_Steal()
	{
		trySteal = true;
		startSlider = true;
		abilityImage.SetActive (false);
	}

	public void SetABS(AbilityStatus abs)
	{
		this.abs = abs;
	}

	void NetworkLogic()
	{
		if (Input.GetKeyDown (KeyCode.LeftShift))
		{
			if (target.GetComponent<PlayerController> ().hasStickyBomb)
			{
				target.GetComponent<PlayerController> ().hasStickyBomb = false;
				this.GetComponent<PlayerController> ().hasStickyBomb = true;
				target.GetComponent<PlayerController> ().bombStatus.GetComponent<SpriteRenderer>().sprite= target.GetComponent<PlayerController> ().noBombSprite;
				this.GetComponent<PlayerController> ().bombStatus.GetComponent<SpriteRenderer>().sprite = this.GetComponent<PlayerController> ().hasBombSprite;
				abilityAvailable = false;
			}
		}
	}

	void LocalLogic()
	{
		Xbox360Controller xc = GetComponent<PlayerController> ().GetXBox ();

		if (trySteal)
		{
			if (target.GetComponent<PlayerController> ().hasStickyBomb)
			{
				target.GetComponent<PlayerController> ().hasStickyBomb = false;
				this.GetComponent<PlayerController> ().hasStickyBomb = true;
				target.GetComponent<PlayerController> ().bombStatus.GetComponent<SpriteRenderer>().sprite= target.GetComponent<PlayerController> ().noBombSprite;
				this.GetComponent<PlayerController> ().bombStatus.GetComponent<SpriteRenderer>().sprite = this.GetComponent<PlayerController> ().hasBombSprite;
				abilityAvailable = false;
				PopText.Create ("SNATCHED!", Color.white, 60, transform.position + Vector3.up * .5f);
			}
		}
	}

	[PunRPC]
	public void SetInRange()
	{
		inRange = true;
	}

	[PunRPC]
	public void SetOutRange()
	{
		inRange = false;
	}

	void OnTriggerEnter2D(Collider2D col)
	{
		if (col.gameObject.tag == "Player") 
		{
			if(GlobalProperties.IS_NETWORKED)
			{
				this.GetComponent<PhotonView>().RPC("SetInRange", PhotonTargets.All);
				target = col.gameObject;
			}
			else
			{
				inRange = true;
				target = col.gameObject;
			}
		}
	}
	void OnTriggerExit2D(Collider2D col)
	{
		if (col.gameObject.tag == "Player") 
		{
			if(GlobalProperties.IS_NETWORKED)
			{
				this.GetComponent<PhotonView>().RPC("SetOutRange", PhotonTargets.All);
				target = null;
			}
			else
			{
				inRange = false;
				target = null;
			}
		}
	}
}
