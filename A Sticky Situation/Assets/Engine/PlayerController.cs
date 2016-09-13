using UnityEngine;
using System.Collections;

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
	private Transform[] raycastObjects;
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

	//public vars
	public string playerName = "Bob";
	public float maxSpeed = 2f;
	public float accelerationSpeed = .12f;
	public float jumpForce = 100f;
	public LayerMask whatIsGround;
	public Sprite hasBombSprite;
	public Sprite noBombSprite;
	//sounds
	public AudioClip foot1;
	public AudioClip foot2;
	public AudioClip land;
	public AudioClip jump;

	
	//for colors
	public Material[] playerMaterials;

	//for keeping movement
	float xInput = 0;
	// Use this for initialization
	void Start () 
	{
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
		scale = new Vector3 (transform.localScale.x, transform.localScale.y, transform.localScale.z);
		minScale = new Vector3 (-transform.localScale.x, transform.localScale.y, transform.localScale.z);

		//raycasting setup
		Transform raycast = transform.FindChild ("Raycasts");

		raycastObjects = new Transform[raycast.childCount];
		for(int i = 0; i < raycast.childCount; ++i)
		{
			raycastObjects[i] =	raycast.GetChild (i);
		}

		//tell camera we have joined
		GameObject.FindObjectOfType<GameCamera> ().MyPlayerHasJoined ();
	}
	
	//input and logic goes here
	void Update () 
	{
		if (!canMove)
			return;

	    xInput = Input.GetAxis ("Horizontal");
		xInput *= accelerationSpeed;

		if (xInput > 0 || xInput < 0)
		{
			animator.SetBool ("Walking", true);

			if(xInput > 0)
			{
				this.transform.localScale = scale;
				nametag.transform.localScale = nametagScale;
			}
			else
			{
				this.transform.localScale = minScale;
				nametag.transform.localScale = nametagMinScale;
			}
		}
		else
			animator.SetBool ("Walking", false);

		isGrounded = Physics2D.OverlapCircle(footTransform.position, groundedRadius, whatIsGround);
		animator.SetBool ("Grounded", isGrounded);

		if(isGrounded)
		{
			if(Input.GetButtonDown("Jump"))
			{
				isGrounded = false;
				animator.SetBool("Grounded", false);
				animator.SetBool("Jumped", true);
				myBody.AddForce(Vector2.up * jumpForce);
				AudioSource.PlayClipAtPoint(jump, this.transform.position);
			}

			if(!wasGrounded)
			{
				AudioSource.PlayClipAtPoint(land, this.transform.position);
			}
		}

		float raycastD = .55f;
		//check raycasting for objects to the left and right of player
		if(xInput > 0)
		{
			//right
			if(RaycastCollision(true, raycastD))
			{
				myBody.velocity = new Vector2(0, myBody.velocity.y);
				xInput = 0;
			}
		}
		else if(xInput < 0)
		{
			//left
			if(RaycastCollision(false, raycastD))
			{
				myBody.velocity = new Vector2(0, myBody.velocity.y);
				xInput = 0;
			}
		}

		if(hasStickyBomb)
		{
			if(Input.GetMouseButtonDown(0))
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
		}

		wasGrounded = isGrounded;
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
	
	//physics must be done here
	void FixedUpdate()
	{
		if (!canMove)
			return;

		float newX = Mathf.Clamp ((myBody.velocity.x + xInput) * kineticFriction, -maxSpeed, maxSpeed);
		myBody.velocity = new Vector2 (newX, myBody.velocity.y);

		if (Mathf.Abs (xInput) > .1f && isGrounded)
			WalkingSounds ();
		else
			walkingTimer = 0;
	}

	void OnCollisionEnter2D(Collision2D c)
	{
		if(c.gameObject.GetComponent<StickyCrate>() != null)
		{
			this.GetComponent<PhotonView>().RPC("PickupBomb", PhotonTargets.All);
			PhotonNetwork.Destroy(c.gameObject);
		}
	}
	
	[PunRPC]
	void PickupBomb()
	{
		Debug.Log("Picked up Sticky");
		hasStickyBomb = true;
		bombStatus.GetComponent<SpriteRenderer>().sprite = hasBombSprite;
	}

	[PunRPC]
	void ThrowBomb()
	{
		Debug.Log("Threw a bomb");
		hasStickyBomb = false;
		bombStatus.GetComponent<SpriteRenderer>().sprite = noBombSprite;
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
	void SetPlayerNumber(int id)
	{
		Color c = playerMaterials [id - 1].color;
		playerID = id;
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
	}
}
