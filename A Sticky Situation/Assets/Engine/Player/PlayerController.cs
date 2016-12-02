using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

	private Rigidbody2D myBody;
	private Animator animator;
	public GameObject bombStatus;
	private TextMesh nametag;
	private Vector3 nametagScale;
	private Vector3 nametagMinScale;
	private float kineticFriction = .5f;
	private Transform footTransform;
	private bool isGrounded = false;
	private bool wasGrounded = false;
	private float groundedRadius = .2f;
	[HideInInspector]
	public Transform[] raycastObjects;
	private Vector3 scale;
	private Vector3 minScale;

	//hidden public vars
	[HideInInspector]
	public bool canMove = false;
	[HideInInspector]
	public int playerID = 1;
	[HideInInspector]
	public bool hasStickyBomb = false;
	[HideInInspector]
	public bool isStuck = false;
	[HideInInspector]
	public Vector3 stuckPosition;
	[HideInInspector]
	public StickyBomb currentStuck = null;
	[HideInInspector]
	public Scorecard myStats;


	public GameObject sprintSlider;
	public GameObject bombImage;

	//public vars
	public string playerName = "Bob";
	public float maxSpeed = 2f;
	public float accelerationSpeed = .12f;
	private float sprintSpeed = 1.65f;
	private GameObject sprintParticle;
	public float jumpForce = 100f;
	public LayerMask whatIsGround;
	public Sprite hasBombSprite;
	public Sprite noBombSprite;
	public GameObject teleportParticle;
	public GameObject specialParticleEffect;
	//sounds
	public AudioClip foot1;
	public AudioClip foot2;
	public AudioClip land;
	public AudioClip jump;
	public AudioClip pickupBomb;
	public AudioClip pickupCrate;
	public AudioClip transferBomb;
	public AudioClip getStuck;
	public AudioClip threwBomb;
	public AudioClip specialNoise;

	Xbox360Controller XBox;
	//for colors
	public Material[] playerMaterials;

	private AbilityStatus abs;
	private Vector3 absScale;
	private Vector3 absMinScale;
	private SpriteRenderer sprintBar;
	private Vector3 sprintScale;
	private Vector3 sprintMinScale;
	private bool isSprinting = false;
	private int sprintJuice = 60 * 4;
	private int sprintJuiceMax = 60 * 4;
	public bool exhausted = false;
	private int exhaustedResetMax = 60 * 3;
	private int exhausedResetTimer = 60 * 3;
	private int giveSprintBack = 0;
	private Color playercolor;
	//for keeping movement
	[HideInInspector]
	public float xInput = 0;

	//AI and pathfinding
	private AIComponent AI;
	[HideInInspector]
	public AIP lastPoint;
	// Use this for initialization
	void Start () 
	{
		sprintSlider = GameObject.Find ("P" + playerID + "SprintSlider");
		sprintSlider.GetComponent<Slider> ().value = sprintSlider.GetComponent<Slider> ().maxValue;

		bombImage = GameObject.Find("P" + playerID + "BombImage");
		bombImage.GetComponent<Image> ().color = Color.white;
		bombImage.SetActive (false);

		Diagnostics.DrawDebugOn = true;
		myBody = GetComponent<Rigidbody2D> ();
		animator = this.transform.GetChild (0).GetComponent<Animator> ();
		if(nametag == null)
		{
			nametag = transform.FindChild ("Nametag").GetComponent<TextMesh> ();
			nametag.color = transform.GetChild (0).GetChild (0).GetComponent<SpriteRenderer> ().material.color;
			nametag.text = playerName;
		}
		footTransform = transform.FindChild ("FootPosition");
		nametagScale = new Vector3 (nametag.transform.localScale.x, nametag.transform.localScale.y, nametag.transform.localScale.z);
		nametagMinScale = new Vector3 (-nametag.transform.localScale.x, nametag.transform.localScale.y, nametag.transform.localScale.z);
		abs = this.transform.GetComponentInChildren<AbilityStatus> ();
		absScale = new Vector3 (abs.transform.localScale.x, abs.transform.localScale.y, abs.transform.localScale.z);
		absMinScale = new Vector3 (-abs.transform.localScale.x, abs.transform.localScale.y, abs.transform.localScale.z);
		scale = new Vector3 (transform.localScale.x, transform.localScale.y, transform.localScale.z);
		minScale = new Vector3 (-transform.localScale.x, transform.localScale.y, transform.localScale.z);
		sprintParticle = Resources.Load<GameObject> ("DustParticle");
		sprintBar = this.transform.FindChild ("SprintStatus").GetComponent<SpriteRenderer> ();
		sprintBar.color = playercolor;
		sprintScale = new Vector3 (sprintBar.transform.localScale.x, sprintBar.transform.localScale.y, sprintBar.transform.localScale.z);
		sprintMinScale = new Vector3 (-sprintBar.transform.localScale.x, sprintBar.transform.localScale.y, sprintBar.transform.localScale.z);

		//raycasting setup
		Transform raycast = transform.FindChild ("Raycasts");

		raycastObjects = new Transform[raycast.childCount];
		for(int i = 0; i < raycast.childCount; ++i)
		{
			raycastObjects[i] =	raycast.GetChild (i);
		}

		if(GlobalProperties.IS_NETWORKED)
		{
			//tell camera we have joined
			GameObject.FindObjectOfType<GameCamera> ().MyPlayerHasJoined ();
		}
		//this will be null forever if you are player controller player instead of AI
		AI = GetComponent<AIComponent> ();
	}

	public static PlayerController GetByPlayerID (int id)
	{
		PlayerController[] allPlayers = GameObject.FindObjectsOfType<PlayerController>();

		foreach(var p in allPlayers)
		{
			if(p.playerID == id)
				return p;
		}

		return null;
	}

	public static PlayerController GetStuckPlayer ()
	{
		PlayerController[] allPlayers = GameObject.FindObjectsOfType<PlayerController>();
		
		foreach(var p in allPlayers)
		{
			if(p.isStuck)
				return p;
		}
		
		return null;
	}

	public static PlayerController GetPlayerWithBomb ()
	{
		PlayerController[] allPlayers = GameObject.FindObjectsOfType<PlayerController>();
		
		foreach(var p in allPlayers)
		{
			if(p.hasStickyBomb)
				return p;
		}
		
		return null;
	}

	void SpecialCharacterMovesNetwork()
	{
		if (Input.GetKeyDown (KeyCode.LeftShift)) 
		{
			if(GetComponent<GhostAbility>() != null)
				GetComponent<PhotonView> ().RPC ("Disappear", PhotonTargets.AllBuffered, null);
		}
	}

	void SpecialCharacterMovesLocal()
	{
		if(AI == null)
		{
			if (XBox.PressedSpecial()) 
			{
				COMMAND_SpecialMoves ();
			}
		}
	}

	public float SprintRatio ()
	{
		return (float)sprintJuice / sprintJuiceMax;
	}

	public void SetAIGroundedRadius()
	{
		groundedRadius = .05f;
	}

	public void COMMAND_SpecialMoves ()
	{
		if (GetComponent<GhostAbility> () != null && abs.ability_ready) 
		{
			GetComponent<GhostAbility> ().LOCAL_Disappear ();
			GetComponent<GhostAbility> ().SetABS (abs);
			abs.ability_ready = false;
			abs.UpdateReady ();
			AudioSource.PlayClipAtPoint (specialNoise, GameObject.FindObjectOfType<GameCamera> ().transform.position);
			GameObject.Destroy (GameObject.Instantiate (specialParticleEffect, this.transform.position, Quaternion.identity), 5f);
		}
		else if (GetComponent<ThiefAbility> () != null && abs.ability_ready) 
		{
			GetComponent<ThiefAbility> ().abilityAvailable = true;
			GetComponent<ThiefAbility> ().LOCAL_Steal ();
			GetComponent<ThiefAbility> ().SetABS (abs);
			abs.ability_ready = false;
			abs.UpdateReady ();
			AudioSource.PlayClipAtPoint (specialNoise, GameObject.FindObjectOfType<GameCamera> ().transform.position);
			GameObject.Destroy (GameObject.Instantiate (specialParticleEffect, this.transform.position, Quaternion.identity), 5f);
		}
	}

	public bool IsGrounded()
	{
		return isGrounded;
	}
	
	//input and logic goes here
	void Update () 
	{
		if (!canMove)
			return;

		//get horizontal movement
		if(GlobalProperties.IS_NETWORKED)
		{
		    xInput = Input.GetAxis ("Horizontal");
			xInput *= accelerationSpeed;
		}
		else
		{
			if(AI == null)
			{
				xInput = XBox.HorizontalAxis ();
				xInput *= accelerationSpeed;

				if(XBox.SprintPressed() && xInput != 0 && !exhausted)
				{
					COMMAND_Sprint();
				}
				else
				{
					COMMAND_DontSprint();
				}
			}
		}

		if(GlobalProperties.IS_NETWORKED)
		{
			SpecialCharacterMovesNetwork ();
		}
		else
		{
			SpecialCharacterMovesLocal();
		}

		CalculateSprintScale (false/*xInput > 0*/);

		if (xInput > 0 || xInput < 0)
		{
			animator.SetBool ("Walking", true);

			if(xInput > 0)
			{
				this.transform.localScale = scale;
				nametag.transform.localScale = nametagScale;
				abs.transform.localScale = absScale;
			}
			else
			{
				this.transform.localScale = minScale;
				nametag.transform.localScale = nametagMinScale;
				abs.transform.localScale = absMinScale;
			}
		}
		else
			animator.SetBool ("Walking", false);

		isGrounded = Physics2D.OverlapCircle(footTransform.position, groundedRadius, whatIsGround);
		animator.SetBool ("Grounded", isGrounded);

		if(isGrounded)
		{
			if(GlobalProperties.IS_NETWORKED)
			{
				if(Input.GetButtonDown("Jump"))
				{
					isGrounded = false;
					animator.SetBool("Grounded", false);
					animator.SetBool("Jumped", true);
					myBody.AddForce(Vector2.up * jumpForce);
					AudioSource.PlayClipAtPoint(jump, this.transform.position);
					animator.SetBool ("InAir", true);
				}
			}
			else
			{
				if(AI == null)
				{
					if(XBox.PressedJump())
					{
						COMMAND_Jump ();
					}
				}
			}

			if(!wasGrounded)
			{
				AudioSource.PlayClipAtPoint(land, this.transform.position);
				animator.SetBool ("InAir", false);
			}
		}
		else animator.SetBool ("InAir", true);

		float raycastD = .55f;
		//check raycasting for objects to the left and right of player
		if(xInput > 0)
		{
			//right
			if(RaycastCollision(true, raycastD))
			{
				myBody.velocity = new Vector2(0, myBody.velocity.y);
				xInput = 0;

				if(AI != null)
				{
					AI.BlockCollide(true);
				}
			}
		}
		else if(xInput < 0)
		{
			//left
			if(RaycastCollision(false, raycastD))
			{
				myBody.velocity = new Vector2(0, myBody.velocity.y);
				xInput = 0;

				if(AI != null)
				{
					AI.BlockCollide(false);
				}
			}
		}

		if (hasStickyBomb) 
		{
			if (AI == null) 
			{
				bombImage.SetActive (true);
				if (GlobalProperties.IS_NETWORKED && Input.GetMouseButtonDown (0))
				{
					NetworkThrowBomb ();
				} 
				else if (!GlobalProperties.IS_NETWORKED && XBox.PressedThrow ()) 
				{
					COMMAND_LocalThrowBomb ();
				}
			}
		} 
		else if (isStuck) 
		{
			bombImage.SetActive (true);
			bombImage.GetComponent<Image> ().color = Color.red;
		}
		else 
		{
			bombImage.SetActive (false);
		}

		wasGrounded = isGrounded;
	}

	public void COMMAND_Sprint()
	{
		xInput *= sprintSpeed;
		animator.SetFloat("Speed" , 1f);
		GameObject sp = (GameObject)GameObject.Instantiate(sprintParticle, footTransform.position, Quaternion.identity);
		sp.GetComponent<ParticleSystem>().startColor = playercolor;
		GameObject.Destroy(sp, 3.5f);
		isSprinting = true;
	}

	public void COMMAND_DontSprint()
	{
		animator.SetFloat("Speed" , 0f);
		isSprinting = false;
	}

	public void COMMAND_Jump ()
	{
		isGrounded = false;
		animator.SetBool ("Grounded", false);
		animator.SetBool ("Jumped", true);
		myBody.velocity = new Vector2 (myBody.velocity.x, 0);
		myBody.AddForce (Vector2.up * jumpForce);
		AudioSource.PlayClipAtPoint (jump, this.transform.position);
		animator.SetBool ("InAir", true);
	}

	void NetworkThrowBomb ()
	{
		if(GetComponent<Scientist>() == null)
		{
			//throw bomb
			this.GetComponent<PhotonView>().RPC("ThrowBomb", PhotonTargets.All);
			GameObject theBomb = PhotonNetwork.Instantiate("StickyBomb", this.transform.position, Quaternion.identity, 0);
			this.GetComponent<PhotonView>().RPC("SetBomb", PhotonTargets.All, theBomb.GetComponent<PhotonView>().viewID, playerName);
			
			if(transform.localScale.x > 0)
			{
				//throw to right
				theBomb.GetComponent<Rigidbody2D>().AddForce(new Vector2(200,-50));
			}
			else
			{
				theBomb.GetComponent<Rigidbody2D>().AddForce(new Vector2(-200,-50));
			}
		}
		//WAIT if you are the scientist, let us throw two instead!
		else if(GetComponent<Scientist>() != null)
		{
			//throw one bomb
			this.GetComponent<PhotonView>().RPC("ThrowBomb", PhotonTargets.All);
			GameObject theBomb = PhotonNetwork.Instantiate("StickyBomb", this.transform.position, Quaternion.identity, 0);
			this.GetComponent<PhotonView>().RPC("SetBomb", PhotonTargets.All, theBomb.GetComponent<PhotonView>().viewID, playerName);
			
			if(transform.localScale.x > 0)
			{
				//throw to right
				theBomb.GetComponent<Rigidbody2D>().AddForce(new Vector2(200,-75));
			}
			else
			{
				theBomb.GetComponent<Rigidbody2D>().AddForce(new Vector2(-200,-75));
			}
			
			//throw another
			GameObject secondBomb = PhotonNetwork.Instantiate("StickyBomb", this.transform.position, Quaternion.identity, 0);
			this.GetComponent<PhotonView>().RPC("SetBomb", PhotonTargets.All, secondBomb.GetComponent<PhotonView>().viewID, playerName);
			AudioSource.PlayClipAtPoint(specialNoise, GameObject.FindObjectOfType<GameCamera>().transform.position);
			
			if(transform.localScale.x > 0)
			{
				//throw to right
				secondBomb.GetComponent<Rigidbody2D>().AddForce(new Vector2(200,75));
			}
			else
			{
				secondBomb.GetComponent<Rigidbody2D>().AddForce(new Vector2(-200,75));
			}
		}
	}

	public static bool SomeoneHasSticky()
	{
		PlayerController[] pcs = GameObject.FindObjectsOfType<PlayerController> ();

		foreach(var p in pcs)
		{
			if (p.hasStickyBomb)
				return true;
		}

		return false;
	}

	public void COMMAND_LocalThrowBomb()
	{

		//WAIT if you are the scientist, let us throw two instead!
		if(GetComponent<Scientist>() != null && abs.ability_ready)
		{
			//throw bomb
			hasStickyBomb = false;
			bombStatus.GetComponent<SpriteRenderer>().sprite = noBombSprite;

			if (abs.ability_ready) 
			{
				GetComponent<Scientist> ().bombsThrown = true;
			}
			abs.ability_ready = false;
			abs.UpdateReady();
			
			GameObject localStickyPrefab = Resources.Load<GameObject>("LocalStickyBomb");
			GameObject theBomb = (GameObject)GameObject.Instantiate(localStickyPrefab, this.transform.position, Quaternion.identity);
			StickyBomb sb = theBomb.GetComponent<StickyBomb>();
			sb.ownerID = playerID;
			AudioSource.PlayClipAtPoint(threwBomb, GameObject.FindObjectOfType<GameCamera>().transform.position);
			GameObject.Destroy (GameObject.Instantiate (specialParticleEffect, this.transform.position, Quaternion.identity), 5f);
			
			if(transform.localScale.x > 0)
			{
				//throw to right
				theBomb.GetComponent<Rigidbody2D>().AddForce(new Vector2(600,-75));
			}
			else
			{
				theBomb.GetComponent<Rigidbody2D>().AddForce(new Vector2(-600,-75));
			}
			
			//throw another
			GameObject secondBomb = (GameObject)GameObject.Instantiate(localStickyPrefab, this.transform.position, Quaternion.identity);
			StickyBomb sb2 = secondBomb.GetComponent<StickyBomb>();
			sb2.ownerID = playerID;
			
			if(transform.localScale.x > 0)
			{
				//throw to right
				secondBomb.GetComponent<Rigidbody2D>().AddForce(new Vector2(600,75));
			}
			else
			{
				secondBomb.GetComponent<Rigidbody2D>().AddForce(new Vector2(-600,75));
			}
		}
		else
		{
			//throw bomb
			hasStickyBomb = false;
			bombStatus.GetComponent<SpriteRenderer>().sprite = noBombSprite;
			
			GameObject localStickyPrefab = Resources.Load<GameObject>("LocalStickyBomb");
			GameObject theBomb = (GameObject)GameObject.Instantiate(localStickyPrefab, this.transform.position, Quaternion.identity);
			StickyBomb sb = theBomb.GetComponent<StickyBomb>();
			sb.ownerID = playerID;
			AudioSource.PlayClipAtPoint(threwBomb, GameObject.FindObjectOfType<GameCamera>().transform.position);
			
			if(transform.localScale.x > 0)
			{
				//throw to right
				theBomb.GetComponent<Rigidbody2D>().AddForce(new Vector2(600,-50));
			}
			else
			{
				theBomb.GetComponent<Rigidbody2D>().AddForce(new Vector2(-600,-50));
			}
		}
	}

	private bool RaycastCollision(bool toTheRight, float raycastD)
	{
		foreach(var t in raycastObjects)
		{
			Vector2 twoDPos = new Vector2(t.position.x, t.position.y);
			if(Diagnostics.DrawDebugOn)
			{
				Debug.DrawLine(twoDPos, toTheRight ? twoDPos + Vector2.right * raycastD : twoDPos + Vector2.left * raycastD, Color.green);
			}

			RaycastHit2D rh = Physics2D.Raycast(twoDPos, toTheRight ? Vector2.right : Vector2.left,raycastD, whatIsGround);

			if(rh.collider != null)
			{
				//we detected a collision
				return true;
			}
		}

		//no collisions were detected
		return false;
	}

	public bool SprintOverAmount(float amt)
	{
		return false;
		//return amt <= (float)sprintJuice / sprintJuiceMax;
	}

	void CalculateSprintScale(bool flip)
	{
		float xScale = (float)sprintJuice / sprintJuiceMax;//# betwenn 0 and 1

		if(flip)
		{
			//Vector3 newV = xScale * sprintMinScale;
			//sprintBar.transform.localScale = new Vector3(newV.x, sprintBar.transform.localScale.y, sprintBar.transform.localScale.z);
		}
		else
		{
			Vector3 newV = xScale * sprintScale;
			sprintBar.transform.localScale = new Vector3(newV.x, sprintBar.transform.localScale.y, sprintBar.transform.localScale.z);
		}
	}
	
	//physics must be done here
	void FixedUpdate()
	{
		if (!canMove)
			return;

		float newX = Mathf.Clamp ((myBody.velocity.x + xInput) * kineticFriction, -maxSpeed, maxSpeed);
		myBody.velocity = new Vector2 (newX, myBody.velocity.y);

		if(isSprinting && sprintJuice > 0)
		{
			sprintJuice--;
			sprintSlider.GetComponent<Slider> ().value = (sprintJuice / 60.0f);

			if(sprintJuice == 0)
			{
				exhausted = true;
				isSprinting = false;
			}
		}
		else if(!exhausted && sprintJuice > 0)
		{
			if(giveSprintBack % 3 == 0)
			{
				sprintJuice++;
				sprintSlider.GetComponent<Slider> ().value = (sprintJuice / 60.0f);

				if(sprintJuice > sprintJuiceMax)
					sprintJuice = sprintJuiceMax;
			}
		}
		else if(exhausted)
		{
			exhausedResetTimer--;

			if(exhausedResetTimer == 0)
			{
				exhausedResetTimer = exhaustedResetMax;
				exhausted = false;
				sprintJuice = 1;
			}
		}

		//for giving sprint back slowly
		giveSprintBack++;

		if (giveSprintBack == 1000)
			giveSprintBack = 0;

		if (Mathf.Abs (xInput) > .1f && isGrounded)
			WalkingSounds ();
		else
			walkingTimer = 0;
	}

	void OnCollisionEnter2D(Collision2D c)
	{
		if(GlobalProperties.IS_NETWORKED)
		{
			if(c.gameObject.GetComponent<StickyCrate>() != null)
			{
				this.GetComponent<PhotonView>().RPC("PickupBomb", PhotonTargets.All);
				PhotonNetwork.Destroy(c.gameObject);
			}
		}
		else
		{
			if(c.gameObject.GetComponent<StickyCrate>() != null)
			{
				LOCAL_PickupBomb();
				GameObject.Destroy(c.gameObject);
			}
		}
	}

	void OnTriggerEnter2D(Collider2D c)
	{
		if(c.GetComponent<KillBox>() != null)
		{
			//youre dead
			GameObject.FindObjectOfType<GameCamera>().PlayerFellLocal(this);
		}
		if(c.GetComponent<AIP>() != null)
		{
			lastPoint = c.GetComponent<AIP>();
		}
	}

	public Xbox360Controller GetXBox ()
	{
		return XBox;
	}
	
	[PunRPC]
	void PickupBomb()
	{
		Debug.Log("Picked up Sticky");
		hasStickyBomb = true;
		bombStatus.GetComponent<SpriteRenderer>().sprite = hasBombSprite;
		AudioSource.PlayClipAtPoint(pickupCrate, GameObject.FindObjectOfType<GameCamera>().transform.position);
	}

	void LOCAL_PickupBomb()
	{
		Debug.Log("Picked up Sticky");
		hasStickyBomb = true;
		bombStatus.GetComponent<SpriteRenderer>().sprite = hasBombSprite;
		myStats.CratesPickedUp++;
		GameObject.FindObjectOfType<GameCamera> ().UpdateStatsLocal ();
		AudioSource.PlayClipAtPoint(pickupCrate, GameObject.FindObjectOfType<GameCamera>().transform.position);
	}

	[PunRPC]
	void ThrowBomb()
	{
		Debug.Log("Threw a bomb");
		hasStickyBomb = false;
		bombStatus.GetComponent<SpriteRenderer>().sprite = noBombSprite;
		AudioSource.PlayClipAtPoint(threwBomb, GameObject.FindObjectOfType<GameCamera>().transform.position);
	}

	[PunRPC]
	void SetBomb(int stickyID, string pName)
	{
		StickyBomb[] stickys = GameObject.FindObjectsOfType<StickyBomb> ();
		StickyBomb theSticky = null;
		
		foreach(var s in stickys)
		{
			if(s.GetComponent<PhotonView>().viewID == stickyID)
			{
				theSticky = s;
				break;
			}
		}
		theSticky.GetComponent<StickyBomb>().ownerID = playerID;
	}

	[PunRPC]
	void GetStuck(Vector3 stuckPos, Vector3 scale, int stickyID)
	{
		Debug.Log("Got stuck");
		isStuck = true;
		stuckPosition = stuckPos;

		StickyBomb[] stickys = GameObject.FindObjectsOfType<StickyBomb> ();
		StickyBomb theSticky = null;

		foreach(var s in stickys)
		{
			if(s.GetComponent<PhotonView>().viewID == stickyID)
			{
				theSticky = s;
				break;
			}
		}

		if(theSticky != null)
		{
			AudioSource.PlayClipAtPoint(getStuck, GameObject.FindObjectOfType<GameCamera>().transform.position);
			currentStuck = theSticky.GetComponent<StickyBomb>();
			theSticky.transform.SetParent(this.transform);
			theSticky.transform.localScale = scale * 9;
			theSticky.transform.position = stuckPos;
			theSticky.GetComponent<Rigidbody2D>().isKinematic = true;
			theSticky.isStuck = true;
			theSticky.GetComponent<StickyBomb>().stuckID = playerID;
			PopText.Create("STUCK!", Color.white, 120, this.transform.position + Vector3.up * .5f);
			Debug.Log("Current stuck: " + currentStuck.name);
		}
	}

	[PunRPC]
	void PickupBombGround(Vector3 stuckPos, Vector3 scale, int stickyID)
	{
		StickyBomb[] stickys = GameObject.FindObjectsOfType<StickyBomb> ();
		StickyBomb theSticky = null;
		
		foreach(var s in stickys)
		{
			if(s.GetComponent<PhotonView>().viewID == stickyID)
			{
				theSticky = s;
				break;
			}
		}
		
		if(theSticky != null)
		{
			AudioSource.PlayClipAtPoint(pickupBomb, GameObject.FindObjectOfType<GameCamera>().transform.position);
			//pickup the bomb, and destroy the bomb
			hasStickyBomb = true;
			bombStatus.GetComponent<SpriteRenderer>().sprite = hasBombSprite;
			PhotonNetwork.Destroy(theSticky.gameObject);
		}
	}

	[PunRPC]
	void Teleported(Vector3 t1, Vector3 t2)
	{
		GameObject.Destroy (GameObject.Instantiate (teleportParticle, t1, Quaternion.identity), 2f);
		GameObject.Destroy (GameObject.Instantiate (teleportParticle, t2, Quaternion.identity), 2f);
	}

	public void LOCAL_Teleported(Vector3 t1, Vector3 t2)
	{
		GameObject.Destroy (GameObject.Instantiate (teleportParticle, t1, Quaternion.identity), 2f);
		GameObject.Destroy (GameObject.Instantiate (teleportParticle, t2, Quaternion.identity), 2f);
		AudioSource.PlayClipAtPoint(threwBomb, GameObject.FindObjectOfType<GameCamera>().transform.position);
	}

	[PunRPC]
	void SwitchOwners(Vector3 stuckPos, int stickyID)
	{
		isStuck = true;
		stuckPosition = stuckPos;
		
		StickyBomb[] stickys = GameObject.FindObjectsOfType<StickyBomb> ();
		StickyBomb theSticky = null;
		
		foreach(var s in stickys)
		{
			if(s.GetComponent<PhotonView>().viewID == stickyID)
			{
				theSticky = s;
				break;
			}
		}
		
		if(theSticky != null)
		{
			AudioSource.PlayClipAtPoint(transferBomb, GameObject.FindObjectOfType<GameCamera>().transform.position);
			currentStuck = theSticky.GetComponent<StickyBomb>();
			theSticky.transform.SetParent(this.transform);
			theSticky.transform.position = stuckPos;
			theSticky.GetComponent<Rigidbody2D>().isKinematic = true;
			theSticky.isStuck = true;
			//the person that ran into you is now the potential killer
			theSticky.GetComponent<StickyBomb>().ownerID = theSticky.GetComponent<StickyBomb>().stuckID;
			theSticky.GetComponent<StickyBomb>().stuckID = playerID;
			theSticky.GetComponent<NetworkStickyBomb>().TransferBomb();
			PopText.Create("STUCK!", Color.white, 120, this.transform.position + Vector3.up * .5f);
			//get the new owner and tell him he isnt stuck anymore

			PlayerController newOwner = null;
			PlayerController[] players = GameObject.FindObjectsOfType<PlayerController>();
			
			foreach(var p in players)
			{
				if(p.playerID == theSticky.ownerID)
				{
					newOwner = p;
				}
			}

			newOwner.isStuck = false;
			newOwner.currentStuck = null;
		}
		GameObject.Find ("Main Camera").GetComponent<GameCamera> ().BombStuck ();
	}

	int walkingTimer = 0;
	int timeToPlayWalk = 20;
	void WalkingSounds()
	{
		walkingTimer ++; 

		if(walkingTimer == timeToPlayWalk)
		{
			if(Random.Range(0,2) == 0)
			{
				AudioSource.PlayClipAtPoint(foot1, this.transform.position);
			}
			else
			{
				AudioSource.PlayClipAtPoint(foot2, this.transform.position);
			}
			walkingTimer = 0;
		}
	}

	[PunRPC]
	void SetCanMove()
	{
		GameCamera gc = GameObject.FindObjectOfType<GameCamera> ();
		
		foreach(var p in gc.playerList)
		{
			p.canMove = true;
		}
	}

	public void LOCAL_SetCanMove()
	{
		this.canMove = true;
	}

	[PunRPC]
	void SetRandomSpawn(int pID)
	{
		GameCamera gc = GameObject.FindObjectOfType<GameCamera> ();

		foreach(var p in gc.playerList)
		{
			p.gameObject.active = true;
			gc._playerJoined = true;
			gc.gameStartCountdown = 60 * 3;
			gc.gameFinishedTimer = 60 * 10;
		}
		//use a spawn point to tell other player where to go
		int r = Random.Range(0, gc.spawnPoints.Length);
		PlayerController correctPlayer = this;
		
		while(gc.spawnsOccupied[r])
		{
			r = Random.Range(0, gc.spawnPoints.Length);
		}
		
		correctPlayer.transform.position = gc.spawnPoints[r].position;
		gc.spawnsOccupied[r] = true;
		correctPlayer.gameObject.active = true;
		correctPlayer.canMove = false;

		//destroy all bombs
		StickyBomb[] allBombs = GameObject.FindObjectsOfType<StickyBomb> ();
		foreach(var b in allBombs)
		{
			PhotonNetwork.Destroy(b.gameObject);
		}
	}

	public void LOCAL_PickupBombGround(StickyBomb theSticky)
	{
		//pickup the bomb, and destroy the bomb
		hasStickyBomb = true;
		bombStatus.GetComponent<SpriteRenderer>().sprite = hasBombSprite;
		AudioSource.PlayClipAtPoint(pickupBomb, GameObject.FindObjectOfType<GameCamera>().transform.position);
		GameObject.Destroy(theSticky.gameObject);
	}

	public void LOCAL_GetStuck(Vector3 stuckPos, Vector3 scale, StickyBomb theSticky)
	{
		isStuck = true;
		stuckPosition = stuckPos;

		if(theSticky != null)
		{
			AudioSource.PlayClipAtPoint(getStuck, GameObject.FindObjectOfType<GameCamera>().transform.position);
			currentStuck = theSticky.GetComponent<StickyBomb>();
			theSticky.transform.SetParent(this.transform);
			theSticky.transform.localScale = scale * 9;
			theSticky.transform.position = stuckPos;
			theSticky.GetComponent<Rigidbody2D>().isKinematic = true;
			theSticky.isStuck = true;
			theSticky.GetComponent<LocalStickyBomb>().TransferBomb();
			theSticky.GetComponent<StickyBomb>().stuckID = playerID;
			PopText.Create("STUCK!", Color.white, 120, this.transform.position + Vector3.up * .5f);
			Debug.Log("Current stuck: " + currentStuck.name);
		}
	}

	public void LOCAL_SwitchOwners(Vector3 stuckPos, Vector3 scale, StickyBomb theSticky)
	{
		isStuck = true;
		stuckPosition = stuckPos;
		
		if(theSticky != null)
		{
			currentStuck = theSticky.GetComponent<StickyBomb>();
			theSticky.transform.SetParent(this.transform);
			theSticky.transform.position = stuckPos;
			theSticky.GetComponent<Rigidbody2D>().isKinematic = true;
			theSticky.isStuck = true;
			//the person that ran into you is now the potential killer
			theSticky.GetComponent<StickyBomb>().ownerID = theSticky.GetComponent<StickyBomb>().stuckID;
			theSticky.GetComponent<StickyBomb>().stuckID = playerID;
			theSticky.GetComponent<LocalStickyBomb>().TransferBomb();
			PopText.Create("STUCK!", Color.white, 120, this.transform.position + Vector3.up * .5f);
			//get the new owner and tell him he isnt stuck anymore
			AudioSource.PlayClipAtPoint(transferBomb, GameObject.FindObjectOfType<GameCamera>().transform.position);
			PlayerController newOwner = null;
			PlayerController[] players = GameObject.FindObjectsOfType<PlayerController>();
			
			foreach(var p in players)
			{
				if(p.playerID == theSticky.ownerID)
				{
					newOwner = p;
				}
			}
			
			newOwner.isStuck = false;
			newOwner.currentStuck = null;
		}
		GameObject.Find ("Main Camera").GetComponent<GameCamera> ().BombStuck ();
	}

	public void LOCAL_SetRandomSpawn()
	{
		GameCamera gc = GameObject.FindObjectOfType<GameCamera> ();
		
		foreach(var p in gc.playerList)
		{
			p.gameObject.active = true;
			gc._playerJoined = true;
			gc.gameStartCountdown = 60 * 3;
			gc.gameFinishedTimer = 60 * 10;
		}
		//use a spawn point to tell other player where to go
		int r = Random.Range(0, gc.spawnPoints.Length);
		
		while(gc.spawnsOccupied[r])
		{
			r = Random.Range(0, gc.spawnPoints.Length);
		}
		
		this.transform.position = gc.spawnPoints[r].position;
		gc.spawnsOccupied[r] = true;
		this.gameObject.active = true;
		this.canMove = false;
		
		//destroy all bombs
		StickyBomb[] allBombs = GameObject.FindObjectsOfType<StickyBomb> ();
		foreach(var b in allBombs)
		{
			GameObject.Destroy(b.gameObject);
		}
	}

	[PunRPC]
	void SetPlayerNumber(int id)
	{
		Color c = playerMaterials [id - 1].color;
		playerID = id;
		myStats = new Scorecard (id);
		if(nametag == null)
		{
			nametag = transform.FindChild ("Nametag").GetComponent<TextMesh> ();
			//change player to dif color
			//body
			transform.GetChild (0).GetChild (0).GetComponent<SpriteRenderer> ().material = playerMaterials[id - 1];
			//5
			for(int i = 0; i < 5; ++i)
			{
				transform.GetChild (0).GetChild (0).GetChild (i).GetComponent<SpriteRenderer> ().material = playerMaterials[id - 1];
				if(i != 4)
				{
					transform.GetChild (0).GetChild (0).GetChild (i).GetChild(0).GetComponent<SpriteRenderer> ().material = playerMaterials[id - 1];
					transform.GetChild (0).GetChild (0).GetChild (i).GetChild(0).GetChild(0).GetComponent<SpriteRenderer> ().material = playerMaterials[id - 1];
				}
			}
			nametag.color = c;
			nametag.text = "P" + id.ToString ();

		}

		GameObject.FindObjectOfType<GameCamera> ().playerList.Add (this);
	}

	public void LOCAL_ResetPlayer()
	{
		animator.SetBool ("Walking", false);
		animator.SetBool ("InAir", false);
		animator.SetFloat ("Speed", 0);
		hasStickyBomb = false;

		bombImage.SetActive (false);
		bombImage.GetComponent<Image> ().color = Color.white;

		bombStatus.GetComponent<SpriteRenderer>().sprite = noBombSprite;
		sprintJuice = sprintJuiceMax;
		exhausedResetTimer = exhaustedResetMax;
		exhausted = false;
		giveSprintBack = 0;
		CalculateSprintScale (false);

		if(AI != null)
		{
			AI.Reset();
		}

		if(GetComponent<BigBoy>() != null)
		{
			GetComponent<BigBoy>().alreadyHit = false;
			transform.FindChild("BigBoyStatus").GetComponent<SpriteRenderer>().enabled = true;
		}
		else if(GetComponent<Scientist>() != null)
		{
			abs.ability_ready = true;
			abs.UpdateReady();
		}
		else if(GetComponent<ThiefAbility>() != null)
		{
			GetComponent<ThiefAbility>().LOCAL_Reset();
			abs.ability_ready = true;
			abs.UpdateReady();
		}
		else if(GetComponent<GhostAbility>() != null)
		{
			GetComponent<GhostAbility>().LOCAL_Reset();
			abs.ability_ready = true;
			abs.UpdateReady();
		}
	}

	public void LOCAL_SetPlayerNumber(int id)
	{
		Color c = playerMaterials [id - 1].color;
		playercolor = c;
		playerID = id;
		myStats = new Scorecard (id);
		XBox = new Xbox360Controller (id);
		if(nametag == null)
		{
			nametag = transform.FindChild ("Nametag").GetComponent<TextMesh> ();
			//change player to dif color
			//body
			transform.GetChild (0).GetChild (0).GetComponent<SpriteRenderer> ().material = playerMaterials[id - 1];
			//5
			for(int i = 0; i < 5; ++i)
			{
				transform.GetChild (0).GetChild (0).GetChild (i).GetComponent<SpriteRenderer> ().material = playerMaterials[id - 1];
				if(i != 4)
				{
					transform.GetChild (0).GetChild (0).GetChild (i).GetChild(0).GetComponent<SpriteRenderer> ().material = playerMaterials[id - 1];
					transform.GetChild (0).GetChild (0).GetChild (i).GetChild(0).GetChild(0).GetComponent<SpriteRenderer> ().material = playerMaterials[id - 1];
				}
			}
			//nametag.color = c;
			nametag.text = "P" + id.ToString ();

			AI = GetComponent<AIComponent>();
			if(AI != null)
			{
				nametag.text = "CP" + id.ToString ();
			}
		}
	}
}
