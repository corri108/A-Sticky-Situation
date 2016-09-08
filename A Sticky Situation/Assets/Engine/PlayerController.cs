using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	private Rigidbody2D myBody;
	private Animator animator;
	private TextMesh nametag;
	private float kineticFriction = .5f;
	private Transform footTransform;
	private bool isGrounded = false;
	private bool wasGrounded = false;
	private float groundedRadius = .2f;
	private Transform[] raycastObjects;

	//hidden public vars
	[HideInInspector]
	public bool canMove = false;
	[HideInInspector]
	public int playerID = 1;

	//public vars
	public string playerName = "Bob";
	public float maxSpeed = 2f;
	public float accelerationSpeed = .12f;
	public float jumpForce = 100f;
	public LayerMask whatIsGround;
	//sounds
	public AudioClip foot1;
	public AudioClip foot2;
	public AudioClip land;
	public AudioClip jump;

	//for keeping movement
	float xInput = 0;
	// Use this for initialization
	void Start () 
	{
		Diagnostics.DrawDebugOn = true;
		myBody = GetComponent<Rigidbody2D> ();
		animator = this.transform.GetChild (0).GetComponent<Animator> ();
		nametag = transform.FindChild ("Nametag").GetComponent<TextMesh> ();
		nametag.color = transform.GetChild (0).GetChild (0).GetComponent<SpriteRenderer> ().material.color;
		footTransform = transform.FindChild ("FootPosition");
		nametag.text = playerName;

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
			animator.SetBool ("Walking", true);
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
			GameObject.Destroy(c.gameObject);
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

	public void SetPlayerNumber(int id)
	{
		playerID = id;
		//nametag.text = "P" + playerID.ToString();
	}
}
