
using UnityEngine;
using System.Collections;

public class AIComponent : MonoBehaviour {

	[HideInInspector]
	public bool Active = false;
	[HideInInspector]
	public AIState myState = AIState.Idle;
	[HideInInspector]
	public FMState fmState = FMState.None;
	[HideInInspector]
	public int whatIsPlayer;
	[HideInInspector]
	public int whatIsGround;

	//for logic
	private PlayerController myPlayer;
	private StickyCrate crate = null;
	private PlayerController chasePlayer = null;
	private PlayerController stuckPlayer = null;
	private PlayerController hasBombPlayer = null;
	private bool movementFrozen = false;
	private AIP lastNode;
	private Transform[] raycasts;

	bool wasMovingRight = false;
	bool wasMovingLeft = false;
	bool justJumped = false;
	bool justThrew = false;
	//timers for firing events 
	AITimer freezeTimer = new AITimer(45);
	AITimer throwTimer = new AITimer(40);
	AITimer jumpTimer = new AITimer(20);
	AITimer playerChaseTimer = new AITimer(60);
	AITimer confusedTimer = new AITimer(45);
	// Use this for initialization
	void Start () 
	{
		myPlayer = GetComponent<PlayerController> ();
		myPlayer.SetAIGroundedRadius ();
		Transform[] allPR = myPlayer.raycastObjects;
		raycasts = new Transform[]{allPR [1], allPR [2]};
		whatIsPlayer = 512;//BODYPART layer is 9; 2^9 == 512
		whatIsGround = 256;//GROUND layer is 8; 2^8 == 256
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	int directionSwitchedCount = 0;
	AIState confusedState;
	void CheckForConfusion()
	{
		confusedTimer.Decrement ();
		if(confusedTimer.Count() == 0)
		{
			//assess weather we need to switch states
			if(directionSwitchedCount > 4)
			{
				//we are confused. store the current state so we can figure out how to deal with the confusion
				//PopText.Create("Confusion Detected!", Color.white, 120,this.transform.position);
				if(myState != AIState.ConfusedState)
				{
					confusedState = myState;
					//PopText.Create("STATE: " + confusedState.ToString(), Color.white, 120,this.transform.position);
				}
				myState = AIState.ConfusedState;
			}
			directionSwitchedCount = 0;
			confusedTimer.Reset();

			
			if(myState != AIState.ConfusedState)
			{
				//PopText.Create("My State: " + myState.ToString(), Color.white, 120,this.transform.position);
			}
			else
			{
				//PopText.Create("Confused State: " + confusedState.ToString(), Color.white, 120,this.transform.position);
			}
		}
	}

	void Confused()
	{
		int maxFloor = AIP.MaxFloor ();

		if(confusedState == AIState.BombOnGround)
		{
			myLSB = GameObject.FindObjectOfType<LocalStickyBomb> ();

			if(myLSB == null)
			{
				if(myPlayer.hasStickyBomb)
				{
					//i have the bomb again, switch states
					myState = AIState.IHaveBomb;
					
					//find a target based off of distance away
					chasePlayer = FindClosestPlayer();
					
					if(chasePlayer == null)
					{
						Debug.LogError("There is no player that is closest to this AI.");
					}
				}
				else
				{
					//either someone else picked it up, or it exploded.
					crate = GameObject.FindObjectOfType<StickyCrate> ();
					
					if(crate != null)
					{
						//it exploded because a new crate spawned.
						myState = AIState.CrateAvailable;
					}
					else
					{
						//no new crate - someone else picked it up
						myState = AIState.SomeoneElseHasBomb;
						PlayerController hbp = PlayerController.GetPlayerWithBomb();
						
						if(hbp.playerID != myPlayer.playerID)
						{
							hasBombPlayer = hbp;
						}
					}
				}
			}
			else
			{
				AIP cM = AIP.FindClosestMaster(maxFloor, myLSB.transform.position);
				MoveTowards(cM.gameObject);
				//PopText.Create("Master", Color.gray, 50, cM.transform.position);

				if(Vector3.Distance(cM.transform.position, this.transform.position) < .5f)
				{
					//our work here is done.
					myState = AIState.BombOnGround;
				}
			}
		}
		else if(confusedState == AIState.IHaveBomb)
		{
			playerChaseTimer.Decrement ();
			
			if(playerChaseTimer.Count() == 0)
			{
				//ree-evaluate closest player
				chasePlayer = FindClosestPlayer();
				playerChaseTimer.Reset();
			}

			//go to the max floor and figure it out from there.
			if(chasePlayer != null)
			{
				AIP cM = AIP.FindClosestMaster(maxFloor, chasePlayer.transform.position);
				MoveTowards(cM.gameObject);
				//PopText.Create("Master", Color.red, 50, cM.transform.position);
			}
		}
		else if(confusedState == AIState.SomeoneElseHasBomb)
		{
			//go to the max floor and figure it out from there.
			if(hasBombPlayer != null)
			{
				AIP cM = AIP.FindClosestMaster(maxFloor, hasBombPlayer.transform.position);
				MoveTowards(cM.gameObject);
			}
		}
		else if(confusedState == AIState.CrateAvailable)
		{
			myState = AIState.Idle;
		}
	}

	void FixedUpdate()
	{
		CheckForConfusion ();

		if(movementFrozen)
		{
			freezeTimer.Decrement();

			if(freezeTimer.Count() == 0)
			{
				movementFrozen = false;
				fmState = FMState.None;
			}

			FrozenMove();
		}

		if(justJumped)
		{
			jumpTimer.Decrement();

			if(jumpTimer.Count() == 0)
			{
				justJumped = false;
				jumpTimer.Reset();
			}
		}

		switch(myState)
		{
		case AIState.Idle:
			Idle();
			break;
		case AIState.CrateAvailable:
			GoToCrate();
			break;
		case AIState.IHaveBomb:
			IHaveBomb();
			break;
		case AIState.IThrewBomb:
			IThrewBomb();
			break;
		case AIState.SomeoneElseStuck:
			RunFromStuckPlayer();
			break;
		case AIState.SomeoneElseHasBomb:
			SomeoneElseHasBomb();
			break;
		case AIState.BombOnGround:
			GoToBomb();
			break;
		case AIState.IAmStuck:
			IAmStuck();
			break;
		case AIState.SomeoneElseThrewBomb:
			AvoidBombMidAir();
			break;
		case AIState.ConfusedState:
			Confused();
			break;
		}

		//always check for if u get stuck
		if(myState != AIState.IAmStuck)
		{
			if(myPlayer.isStuck)
			{
				myState = AIState.IAmStuck;
				chasePlayer = FindClosestPlayer();
			}
		}

		RealizeErrors ();
	}

	void IAmStuck()
	{
		playerChaseTimer.Decrement ();
		
		if(playerChaseTimer.Count() == 0)
		{
			//ree-evaluate closest player
			chasePlayer = FindClosestPlayer();
			playerChaseTimer.Reset();
		}
		
		if(chasePlayer != null)
		{
			if(chasePlayer.lastPoint != null && lastNode != null
			   && chasePlayer.lastPoint.FLOOR > lastNode.FLOOR)
			{
				//move to master node of player floor
				MoveTowards(AIP.FindClosestMaster(chasePlayer.lastPoint.FLOOR, chasePlayer.transform.position).gameObject);
			}
			else if(chasePlayer.lastPoint != null && lastNode != null
			        && chasePlayer.lastPoint.FLOOR < lastNode.FLOOR)
			{
				//move to master node of player floor
				MoveTowards(AIP.FindClosestMaster(chasePlayer.lastPoint.FLOOR, chasePlayer.transform.position).gameObject);
			}
			else
			{
				MoveTowards(chasePlayer.gameObject);
			}
			
			if(myPlayer.gameObject.active && !myPlayer.isStuck)
			{
				//youre not stuck anymore! run for it!
				myState = AIState.SomeoneElseStuck;
				stuckPlayer = PlayerController.GetStuckPlayer();
			}
		}
	}

	void Idle()
	{
		crate = GameObject.FindObjectOfType<StickyCrate> ();
		
		if(crate == null)
		{
			//wait it hasnt spawned yet
		}
		else
		{
			//go get that crate!
			myState = AIState.CrateAvailable;
		}
	}

	LocalStickyBomb myLSB;
	void IThrewBomb()
	{
		//i just threw the bomb. wait until it either hits someone or hits the ground before deciding what to do.
		if(myLSB != null)
		{
			if(myLSB.GetComponent<StickyBomb>().ownerID == myPlayer.playerID)
			{
				//its ur bomb
				//check to see if it landed on the ground or stuck someone
				
				if(myLSB.GetComponent<StickyBomb>().hitGround)
				{
					//it hit the ground
					myState = AIState.BombOnGround;
					//PopText.Create("MY BOMB ON GROUND", Color.white, 120,this.transform.position);
				}
				else if(myLSB.GetComponent<StickyBomb>().stuckID > 0)
				{
					myState = AIState.SomeoneElseStuck;
					stuckPlayer = PlayerController.GetByPlayerID(myLSB.GetComponent<StickyBomb>().stuckID);
					//PopText.Create("SOMEONE ELSE STUCK", Color.white, 120,this.transform.position);
				}
			}
			else
			{
				//its not ur bomb. see if ur bomb even ecists
				LocalStickyBomb[] allbombs = GameObject.FindObjectsOfType<LocalStickyBomb> ();

				bool myBombExists = false;
				foreach(var b in allbombs)
				{
					if(b.GetComponent<StickyBomb>().ownerID == myPlayer.playerID)
					{
						//we are good
						myBombExists = true;
						myLSB = b;
					}
				}

				if(!myBombExists)
				{
					//what happened to my bomb??
				}
			}
		}
		else
		{
			myLSB = GameObject.FindObjectOfType<LocalStickyBomb> ();

			if(myLSB == null)
			{
				//what happened to my bomb??
			}
		}
	}

	LocalStickyBomb theirLSB;
	void AvoidBombMidAir()
	{
		//someone just threw the bomb. wait until it either hits someone or hits the ground before deciding what to do.
		if(theirLSB != null)
		{
			if(theirLSB.GetComponent<StickyBomb>().ownerID == hasBombPlayer.playerID)
			{
				//its their bomb
				//check to see if it landed on the ground or stuck someone
				
				if(theirLSB.GetComponent<StickyBomb>().hitGround)
				{
					//it hit the ground
					myState = AIState.BombOnGround;
					//PopText.Create("MY BOMB ON GROUND", Color.white, 120,this.transform.position);
				}
				else if(theirLSB.GetComponent<StickyBomb>().stuckID > 0)
				{
					if(theirLSB.GetComponent<StickyBomb>().stuckID != myPlayer.playerID)
					{
						myState = AIState.SomeoneElseStuck;
						stuckPlayer = PlayerController.GetByPlayerID(theirLSB.GetComponent<StickyBomb>().stuckID);
						//PopText.Create("SOMEONE ELSE STUCK", Color.white, 120,this.transform.position);
					}
				}
				else if(Vector3.Distance(theirLSB.transform.position, this.transform.position) < 2.5f)//if its getting close to us
				{
					//try and jump away
					if(myPlayer.IsGrounded() && !justJumped)
					{
						myPlayer.COMMAND_Jump ();
						justJumped = true;
					}
				}
			}
			else
			{
				//its not their bomb. see if their bomb even ecists
				LocalStickyBomb[] allbombs = GameObject.FindObjectsOfType<LocalStickyBomb> ();
				
				bool theirBombExists = false;
				foreach(var b in allbombs)
				{
					if(b.GetComponent<StickyBomb>().ownerID == hasBombPlayer.playerID)
					{
						//we are good
						theirBombExists = true;
						theirLSB = b;
					}
				}
				
				if(!theirBombExists)
				{
					//what happened to my bomb??
				}
			}
		}
		else
		{
			theirLSB = GameObject.FindObjectOfType<LocalStickyBomb> ();
			
			if(theirLSB == null)
			{
				//what happened to my bomb??
			}
		}
	}

	void IHaveBomb()
	{
		playerChaseTimer.Decrement ();

		if(playerChaseTimer.Count() == 0)
		{
			//ree-evaluate closest player
			chasePlayer = FindClosestPlayer();
			playerChaseTimer.Reset();
		}

		float minThrowD = 12.5f;
		
		//if(Vector3.Distance(this.transform.position, chasePlayer.transform.position) <= minThrowD)
		//{
			if(myPlayer.xInput > 0)
				TryToThrow(true, minThrowD, 8f);
			else 
				TryToThrow(false, minThrowD, 8f);
		//}

		if(chasePlayer != null)
		{
			if(chasePlayer.lastPoint != null && lastNode != null
			   && chasePlayer.lastPoint.FLOOR > lastNode.FLOOR)
			{
				//move to master node of player floor
				MoveTowards(AIP.FindClosestMaster(chasePlayer.lastPoint.FLOOR, chasePlayer.transform.position).gameObject);
				//PopText.Create("Moving to top floor.", Color.white, 120,this.transform.position);
			}
			else if(chasePlayer.lastPoint != null && lastNode != null
				   && chasePlayer.lastPoint.FLOOR < lastNode.FLOOR)
			{
				//move to master node of player floor
				MoveTowards(AIP.FindClosestMaster(chasePlayer.lastPoint.FLOOR, chasePlayer.transform.position).gameObject);
				//PopText.Create("Moving to bottom floor.", Color.white, 120,this.transform.position);
			}
			else
			{
				MoveTowards(chasePlayer.gameObject);
			}
		}
	}

	private void TryToThrow(bool toTheRight, float minD, float jumpDCutoff)
	{
		foreach(var t in raycasts)
		{
			Vector2 twoDPos = new Vector2(t.position.x, t.position.y);
			if(Diagnostics.DrawDebugOn)
			{
				Debug.DrawLine(twoDPos, toTheRight ? twoDPos + Vector2.right * minD : twoDPos + Vector2.left * minD, Color.red);
			}

			//first check for ground in the way
			RaycastHit2D rh = Physics2D.Raycast(twoDPos, toTheRight ? Vector2.right : Vector2.left, minD, whatIsGround);
			
			if(rh.collider == null)
			{
				//now check for players! no ground is obstructing our throw
				RaycastHit2D rh2 = Physics2D.Raycast(twoDPos, toTheRight ? Vector2.right : Vector2.left, minD, whatIsPlayer);
				
				if(rh2.collider != null)
				{
					//we detected a collision. throw our bomb nigga
					myPlayer.COMMAND_LocalThrowBomb();
					myState = AIState.IThrewBomb;
					break;
				}
			}
			else
			{
				//now check to see if player distance is less than that of ground distance
				RaycastHit2D rh2 = Physics2D.Raycast(twoDPos, toTheRight ? Vector2.right : Vector2.left, minD, whatIsPlayer);
				
				if(rh2.collider != null && rh.distance > rh2.distance)
				{
					//we detected a collision. throw our bomb nigga
					myPlayer.COMMAND_LocalThrowBomb();
					myState = AIState.IThrewBomb;

					//if player is farther away, jump when u throw

					if(rh2.distance > jumpDCutoff)
					{
						if(myPlayer.IsGrounded() && !justJumped)
						{
							myPlayer.COMMAND_Jump ();
							justJumped = true;
						}
					}
					break;
				}
			}
		}
	}

	private PlayerController FindClosestPlayer()
	{
		PlayerController[] allP = GameObject.FindObjectsOfType<PlayerController> ();
		PlayerController theP = null;
		float closest = float.MaxValue;

		foreach(var p in allP)
		{
			if(p.playerID != myPlayer.playerID)
			{
				float d = Vector3.Distance(p.transform.position, myPlayer.transform.position);

				if(d < closest)
				{
					closest = d;
					theP = p;
				}
			}
		}

		return theP;
	}

	void RunFromStuckPlayer()
	{
		if(stuckPlayer != null && stuckPlayer.gameObject.active && stuckPlayer.isStuck)
		{
			//keep running from him
			MoveAwayFrom(stuckPlayer.gameObject);
		}
		else if(stuckPlayer != null && stuckPlayer.gameObject.active && !stuckPlayer.isStuck)
		{
			//someone else is stuck! is it us?
			if(myPlayer.isStuck)
			{
				myState = AIState.IAmStuck;
			}
			else
			{
				//find out who is stuck and run away from them
				stuckPlayer = PlayerController.GetStuckPlayer();

				if(stuckPlayer == null)
				{
					//someone must have been the bigboy. go to idle to get new crate
					myState = AIState.Idle;
				}
			}
		}
		else if(stuckPlayer != null && !stuckPlayer.gameObject.active)
		{
			//he should be dead. look for next spawned crate by going to idle.
			myState = AIState.Idle;
		}
		else
		{
			//i give up. go to idle.
			myState = AIState.Idle;
			//PopText.Create("GOING TO IDLE", Color.white, 120,this.transform.position);
		}
	}

	void GoToBomb()
	{
		myLSB = GameObject.FindObjectOfType<LocalStickyBomb> ();
		
		if(myLSB == null)
		{
			if(myPlayer.hasStickyBomb)
			{
				//i have the bomb again, switch states
				myState = AIState.IHaveBomb;
				
				//find a target based off of distance away
				chasePlayer = FindClosestPlayer();
				
				if(chasePlayer == null)
				{
					Debug.LogError("There is no player that is closest to this AI.");
				}
			}
			else
			{
				//either someone else picked it up, or it exploded.
				crate = GameObject.FindObjectOfType<StickyCrate> ();
				
				if(crate != null)
				{
					//it exploded because a new crate spawned.
					myState = AIState.CrateAvailable;
				}
				else
				{
					//no new crate - someone else picked it up
					myState = AIState.SomeoneElseHasBomb;
					PlayerController hbp = PlayerController.GetPlayerWithBomb();
					
					if(hbp.playerID != myPlayer.playerID)
					{
						hasBombPlayer = hbp;
					}
				}
			}
		}
		else
		{
			AIP pointNearBomb = AIP.PointNear(myLSB.transform.position);
			//PopText.Create("Bomb", Color.blue, 50, myLSB.transform.position);
			//PopText.Create("Point", Color.green, 50, pointNearBomb.transform.position);

			int floorGuess = 1;

			if(myLSB.transform.position.y - this.transform.position.y > 4)
				floorGuess = 2;

			if(pointNearBomb != null && lastNode != null
			   && pointNearBomb.FLOOR > lastNode.FLOOR)
			{
				//move to master node of player floor
				MoveTowards(AIP.FindClosestMaster(pointNearBomb.FLOOR, myLSB.transform.position).gameObject);
				//PopText.Create("Master", Color.red, 50, AIP.FindClosestMaster(pointNearBomb.FLOOR, myLSB.transform.position).gameObject.transform.position);
				//PopText.Create("Moving to top floor.", Color.white, 120,this.transform.position);
			}
			else if(pointNearBomb != null && lastNode != null && floorGuess == 2)
			{
				MoveTowards(AIP.FindClosestMaster(2, myLSB.transform.position).gameObject);
				//PopText.Create("Master", Color.red, 50, AIP.FindClosestMaster(2, myLSB.transform.position).gameObject.transform.position);
			}
			else if(pointNearBomb != null && lastNode != null
			        && pointNearBomb.FLOOR < lastNode.FLOOR)
			{
				//move to master node of player floor
				MoveTowards(AIP.FindClosestMaster(pointNearBomb.FLOOR, myLSB.transform.position).gameObject);
				//PopText.Create("Moving to bottom floor.", Color.white, 120,this.transform.position);
				//PopText.Create("Master", Color.red, 50, AIP.FindClosestMaster(pointNearBomb.FLOOR, myLSB.transform.position).gameObject.transform.position);
			}
			else
			{
				MoveTowards(myLSB.gameObject);
			}
		}
	}

	public void Reset()
	{
		myState = AIState.Idle;
		stuckPlayer = null;
		crate = null;
		chasePlayer = null;
		hasBombPlayer = null;
		myLSB = null;
		theirLSB = null;
		movementFrozen = false;
		lastNode = null;
		//bools for timers
		wasMovingRight = false;
		wasMovingLeft = false;
		justJumped = false;
		justThrew = false;
		//timers for firing events 
		freezeTimer = new AITimer (45);
		throwTimer = new AITimer (40);
		jumpTimer = new AITimer (20);
		playerChaseTimer = new AITimer (60);
		confusedTimer = new AITimer(30);

		directionSwitchedCount = 0;
	}

	void SomeoneElseHasBomb()
	{
		if(hasBombPlayer != null && hasBombPlayer.hasStickyBomb)
		{
			//keep running from him
			MoveAwayFrom(hasBombPlayer.gameObject);
		}
		else if(hasBombPlayer != null && !hasBombPlayer.hasStickyBomb)
		{
			PlayerController hbp = PlayerController.GetPlayerWithBomb();

			//someone might have stolen it
			if(hbp != null)
			{
				hasBombPlayer = hbp;
			}
			else
			{
				//nope, no one stole it. move to the SomeoneThrewBomb state
				myState = AIState.SomeoneElseThrewBomb;
			}
		}
		else
		{
			//i give up. go to idle.
			myState = AIState.Idle;
			//PopText.Create("GOING TO IDLE", Color.white, 120,this.transform.position);
		}
	}

	void GoToCrate()
	{
		crate = GameObject.FindObjectOfType<StickyCrate> ();

		if(crate == null)
		{
			if(GetComponent<PlayerController>().hasStickyBomb)
			{
				//i have the bomb, switch states
				myState = AIState.IHaveBomb;

				//find a target based off of distance away
				chasePlayer = FindClosestPlayer();

				if(chasePlayer == null)
				{
					Debug.LogError("There is no player that is closest to this AI.");
				}
			}
			else
			{
				//well now we know someone else has the bomb. so lets switch states
				myState = AIState.SomeoneElseHasBomb;
				PlayerController hbp = PlayerController.GetPlayerWithBomb();

				if(hbp.playerID != myPlayer.playerID)
				{
					hasBombPlayer = hbp;
				}
			}
		}
		else
		{
			//go get that crate!
			MoveTowards(crate.gameObject);
		}
	}

	void MoveTowards(GameObject go)
	{
		if (movementFrozen)
			return;

		float fauxInput = 0;

		Vector3 dV = go.transform.position - this.transform.position;

		//to see if we changed directions
		bool lastMoveRight = wasMovingRight;
		bool lastMoveLeft = wasMovingLeft;

		if(dV.x > 0)
		{
			fauxInput = 1;
			wasMovingRight = true;
			wasMovingLeft = false;
		}
		else if(dV.x < 0)
		{
			fauxInput = -1;
			wasMovingRight = false;
			wasMovingLeft = true;
		}
		else
		{
			wasMovingRight = false;
			wasMovingLeft = false;
		}

		Teleporter[] tp1 = Teleporter.FindClosestPair (this.transform.position);
		float minTeleDistance = 10;
		float meAndPlayer = Vector3.Distance (this.transform.position, go.transform.position);
		float meAndTP1 = Vector3.Distance (this.transform.position, tp1[0].transform.position);
		float meAndTP2 = Vector3.Distance (this.transform.position, tp1[1].transform.position);
		float himAndTP1 = Vector3.Distance (go.transform.position, tp1[0].transform.position);
		float himAndTP2 = Vector3.Distance (go.transform.position, tp1[1].transform.position);

		//to figure out if we should take a portal, add the distance between:
		//you and the portal closest to you + him and portal closest to him
		//IFF portals are different AND added distance is LESS than distance(you, him)
		//only then will you take the portal.

		if(meAndTP1 < meAndTP2 && himAndTP1 < himAndTP2)
		{
			//this is no good, because we are both closer to the same portal.
		}
		else if(meAndTP2 < meAndTP1 && himAndTP2 <  himAndTP1)
		{
			//also no good, same case but with different portals
		}
		else if(meAndTP1 < meAndTP2 && himAndTP2 < himAndTP1)
		{
			//now we are getting somewhere. i am closer to TP1. He is closer to TP2.
			if(meAndTP1 + himAndTP2 < meAndPlayer)
			{
				//NICE! we should take the portal. move to tp1
				Vector3 dP = tp1[0].transform.position - this.transform.position;
				if(dP.x > 0)
				{
					fauxInput = 1;
					wasMovingRight = true;
					wasMovingLeft = false;
				}
				else if(dP.x < 0)
				{
					fauxInput = -1;
					wasMovingRight = false;
					wasMovingLeft = true;
				}

				//PopText.Create("TAKE PORTAL", Color.white, 120,this.transform.position);
			}
		}
		else if(meAndTP2 < meAndTP1 && himAndTP1 < himAndTP2)
		{
			//now i am closer to TP2. He is closer to TP1.
			if(meAndTP2 + himAndTP1 < meAndPlayer)
			{
				//NICE! we should take the portal.
				Vector3 dP = tp1[1].transform.position - this.transform.position;
				if(dP.x > 0)
				{
					fauxInput = 1;
					wasMovingRight = true;
					wasMovingLeft = false;
				}
				else if(dP.x < 0)
				{
					fauxInput = -1;
					wasMovingRight = false;
					wasMovingLeft = true;
				}

				//PopText.Create("TAKE PORTAL", Color.white, 120,this.transform.position);
			}
		}

		//did we change directions?
		if(lastMoveRight != wasMovingRight && lastMoveLeft != wasMovingLeft)
		{
			//we changed
			directionSwitchedCount++;
		}

		bool shouldSprint = ShouldISprint ();
		
		myPlayer.xInput = fauxInput;
		myPlayer.xInput *= myPlayer.accelerationSpeed;
		
		if(shouldSprint && myPlayer.xInput != 0 && !myPlayer.exhausted)
		{
			myPlayer.COMMAND_Sprint();
		}
		else
		{
			myPlayer.COMMAND_DontSprint();
		}
	}

	void MoveAwayFrom(GameObject go)
	{
		if (movementFrozen)
			return;
		
		float fauxInput = 0;
		
		Vector3 dV = go.transform.position - this.transform.position;

		//to see if we changed directions
		bool lastMoveRight = wasMovingRight;
		bool lastMoveLeft = wasMovingLeft;

		if(dV.x < 0)
		{
			fauxInput = 1;
			wasMovingRight = true;
			wasMovingLeft = false;
		}
		else if(dV.x > 0)
		{
			fauxInput = -1;
			wasMovingRight = false;
			wasMovingLeft = true;
		}
		else
		{
			wasMovingRight = false;
			wasMovingLeft = false;
		}
		
		Teleporter[] tp1 = Teleporter.FindClosestPair (this.transform.position);
		float minTeleDistance = 10;
		float meAndPlayer = Vector3.Distance (this.transform.position, go.transform.position);
		float meAndTP1 = Vector3.Distance (this.transform.position, tp1[0].transform.position);
		float meAndTP2 = Vector3.Distance (this.transform.position, tp1[1].transform.position);
		float himAndTP1 = Vector3.Distance (go.transform.position, tp1[0].transform.position);
		float himAndTP2 = Vector3.Distance (go.transform.position, tp1[1].transform.position);
		
		//to figure out if we should take a portal, add the distance between:
		//you and the portal closest to you + him and portal closest to him
		//IFF portals are different AND added distance is LESS than distance(you, him)
		//only then will you take the portal.
		
		if(meAndTP1 < meAndTP2 && himAndTP1 < himAndTP2)
		{
			//we are both closer to same portal - so dont go towards a portal. just keep running
		}
		else if(meAndTP2 < meAndTP1 && himAndTP2 <  himAndTP1)
		{
			//also no good, same case but with different portals
		}
		if(meAndTP1 < meAndTP2 && himAndTP2 < himAndTP1)
		{
			//now we are getting somewhere. i am closer to TP1. He is closer to TP2.
			if(meAndTP1 + himAndTP2 < meAndPlayer)
			{
				//our combined teleporter distances are greater than our actual distance. go towards the portal
				Vector3 dP = go.transform.position - this.transform.position;
				if(dP.x > 0)
				{
					fauxInput = 1;
					wasMovingRight = true;
					wasMovingLeft = false;
				}
				else if(dP.x < 0)
				{
					fauxInput = -1;
					wasMovingRight = false;
					wasMovingLeft = true;
				}
				
				//PopText.Create("TAKE PORTAL", Color.white, 120,this.transform.position);
			}
		}
		else if(meAndTP2 < meAndTP1 && himAndTP1 < himAndTP2)
		{
			//now i am closer to TP2. He is closer to TP1.
			if(meAndTP2 + himAndTP1 < meAndPlayer)
			{
				//NICE! we should take the portal.
				Vector3 dP = go.transform.position - this.transform.position;
				if(dP.x > 0)
				{
					fauxInput = 1;
					wasMovingRight = true;
					wasMovingLeft = false;
				}
				else if(dP.x < 0)
				{
					fauxInput = -1;
					wasMovingRight = false;
					wasMovingLeft = true;
				}
				
				//PopText.Create("TAKE PORTAL", Color.white, 120,this.transform.position);
			}
		}

		//did we change directions?
		if(lastMoveRight != wasMovingRight && lastMoveLeft != wasMovingLeft)
		{
			//we changed
			directionSwitchedCount++;
		}

		bool shouldSprint = ShouldISprint ();
		
		myPlayer.xInput = fauxInput;
		myPlayer.xInput *= myPlayer.accelerationSpeed;
		
		if(shouldSprint && myPlayer.xInput != 0 && !myPlayer.exhausted)
		{
			myPlayer.COMMAND_Sprint();
		}
		else
		{
			myPlayer.COMMAND_DontSprint();
		}
	}

	//public methods to communitcate with player controller
	public void BlockCollide(bool isRight)
	{

	}

	public bool ShouldISprint()
	{
		float juice = myPlayer.SprintRatio ();
		int stickyTime = 10;
		bool grounded = false;

		if(GameObject.FindObjectOfType<LocalStickyBomb>() != null)
		{
			LocalStickyBomb sb = GameObject.FindObjectOfType<LocalStickyBomb>();
			stickyTime = sb.GetTimerInSeconds(false);
			grounded = sb.GetComponent<StickyBomb>().hitGround;
		}

		if(stickyTime <= 2)
		{
			return true;
		}
		else if(stickyTime <= 4)
		{
			if(myState == AIState.IAmStuck || myState == AIState.SomeoneElseStuck)
			{
				return true;
			}
			else
			{
				if(juice > .5f)
					return true;
				else return false;
			}
		}
		else if(stickyTime <= 7)
		{
			if(myState == AIState.IAmStuck || myState == AIState.SomeoneElseStuck)
			{
				if(juice > .8f)
					return true;

				return false;
			}
			return false;
		}

		if(grounded)
		{
			if(juice > .5f)
			{
				return true;
			}
			else return false;
		}
		if(stickyTime == 10)
		{
			if(myState == AIState.IHaveBomb || myState == AIState.SomeoneElseHasBomb ||
			   myState == AIState.SomeoneElseThrewBomb)
			{
				if(juice > .7f)
					return true;
			}
		}

		return false;
	}

	void RealizeErrors()
	{
		if(myPlayer.hasStickyBomb && myState != AIState.IHaveBomb)
		{
			myState = AIState.IHaveBomb;
			chasePlayer = FindClosestPlayer();
			
			if(chasePlayer == null)
			{
				Debug.LogError("There is no player that is closest to this AI.");
			}
		}
	}

	void FreezeMovement()
	{
		movementFrozen = true;
		freezeTimer.Reset ();
	}

	void FrozenMove()
	{
		if (fmState == FMState.None)
			return;

		int fauxInput = 0;

		switch(fmState)
		{
		case FMState.JumpToLeft:
			fauxInput = -1;
			break;
		case FMState.JumpToRight:
			fauxInput = 1;
			break;
		case FMState.MoveToLeft:
			fauxInput = -1;
			break;
		case FMState.MoveToRight:
			fauxInput = 1;
			break;
		}

		bool shouldSprint = ShouldISprint ();
		bool shouldJump = fmState == FMState.JumpToLeft || fmState == FMState.JumpToRight;
		
		myPlayer.xInput = fauxInput;
		myPlayer.xInput *= myPlayer.accelerationSpeed;
		
		if(shouldSprint && myPlayer.xInput != 0 && !myPlayer.exhausted)
		{
			myPlayer.COMMAND_Sprint();
		}
		else
		{
			myPlayer.COMMAND_DontSprint();
		}

		//constantly try to jump
		if(myPlayer.IsGrounded() && !justJumped && shouldJump)
		{
			myPlayer.COMMAND_Jump ();
			justJumped = true;
		}
	}

	void OnTriggerEnter2D(Collider2D c)
	{
		if(c.GetComponent<AIP>() != null)
		{
			AIP point = c.GetComponent<AIP>();
			lastNode = point;
			FreezeMovement();

			//usually we want to jump.. UNLESS we are looking for a crate / bomb and the crate / bomb is below us.
			bool shouldJump = point.jump;

			if(myState == AIState.CrateAvailable)
			{
				if(crate != null)
				{
					if(this.transform.position.y - crate.transform.position.y > 0)
					{
						//dont jump
						shouldJump = false;
					}
				}
			}
			else if(myState == AIState.IHaveBomb && chasePlayer != null)
			{
				if(chasePlayer != null)
				{
					if(this.transform.position.y - chasePlayer.transform.position.y > 1)
					{
						//dont jump
						shouldJump = false;
					}
				}
				//if you are going down to the floor
				if(chasePlayer.lastPoint != null && lastNode != null
				   && chasePlayer.lastPoint.FLOOR < lastNode.FLOOR)
				{
					//dont jump!
					shouldJump = false;
				}
			}
			else if(myState == AIState.BombOnGround)
			{
				if(myLSB != null)
				{
					if(this.transform.position.y - myLSB.transform.position.y > 0)
					{
						//dont jump
						shouldJump = false;
					}
				}
			}

			if(shouldJump)
			{
				if(point.type == AIPType.AlwaysLeft)
				{
					fmState = FMState.JumpToLeft;
				}
				else if(point.type == AIPType.AlwaysRight)
				{
					fmState = FMState.JumpToRight;
				}
				else if(point.type == AIPType.InheritVelocity)
				{
					if(myState == AIState.CrateAvailable)
					{
						//go to the direction the crate is
						if(this.transform.position.x - crate.transform.position.x > 0)
						{
							//dont jump
							fmState = FMState.JumpToLeft;
						}
						else 
						{
							fmState = FMState.JumpToRight;
						}
					}
					else if(myState == AIState.IHaveBomb && chasePlayer != null)
					{
						//go to the direction the player is
						if(this.transform.position.x - chasePlayer.transform.position.x > 0)
						{
							fmState = FMState.JumpToLeft;
						}
						else 
						{
							fmState = FMState.JumpToRight;
						}
					}
					else if(myState == AIState.BombOnGround && myLSB != null)
					{
						//go to the direction the crate is
						if(this.transform.position.x - myLSB.transform.position.x > 0)
						{
							//dont jump
							fmState = FMState.JumpToLeft;
						}
						else 
						{
							fmState = FMState.JumpToRight;
						}
					}
					else if(wasMovingLeft)
					{
						fmState = FMState.JumpToLeft;
					}
					else if(wasMovingRight)
					{
						fmState = FMState.JumpToRight;
					}
				}
				else if(point.type == AIPType.OnlyIfGoingLeft)
				{
					if(wasMovingLeft)
					{
						fmState = FMState.JumpToLeft;
					}
				}
				else if(point.type == AIPType.OnlyIfGoingRight)
				{
					if(wasMovingRight)
					{
						fmState = FMState.JumpToRight;
					}
				}
			}
			else if(point.move)
			{
				if(point.type == AIPType.AlwaysLeft)
				{
					fmState = FMState.MoveToLeft;
				}
				else if(point.type == AIPType.AlwaysRight)
				{
					fmState = FMState.MoveToRight;
				}
				else if(point.type == AIPType.InheritVelocity)
				{
					if(myState == AIState.CrateAvailable)
					{
						//go to the direction the crate is
						if(this.transform.position.x - crate.transform.position.x > 0)
						{
							//dont jump
							fmState = FMState.MoveToLeft;
						}
						else 
						{
							fmState = FMState.MoveToRight;
						}
					}
					else if(myState == AIState.IHaveBomb && chasePlayer != null)
					{
						//go to the direction the player is
						if(this.transform.position.x - chasePlayer.transform.position.x > 0)
						{
							fmState = FMState.MoveToLeft;
						}
						else 
						{
							fmState = FMState.MoveToRight;
						}
					}
					else if(myState == AIState.BombOnGround && myLSB != null)
					{
						//go to the direction the crate is
						if(this.transform.position.x - myLSB.transform.position.x > 0)
						{
							//dont jump
							fmState = FMState.MoveToLeft;
						}
						else 
						{
							fmState = FMState.MoveToRight;
						}
					}
					else if(wasMovingLeft)
					{
						fmState = FMState.MoveToLeft;
					}
					else if(wasMovingRight)
					{
						fmState = FMState.MoveToRight;
					}
				}
				else if(point.type == AIPType.OnlyIfGoingLeft)
				{
					if(wasMovingLeft)
					{
						fmState = FMState.MoveToLeft;
					}
				}
				else if(point.type == AIPType.OnlyIfGoingRight)
				{
					if(wasMovingRight)
					{
						fmState = FMState.MoveToRight;
					}
				}
			}
		}
	}
}

public enum FMState
{
	JumpToLeft,
	JumpToRight,
	MoveToLeft,
	MoveToRight,
	None
}

public enum AIState
{
	IHaveBomb,
	SomeoneElseHasBomb,
	CrateAvailable,
	IThrewBomb,
	SomeoneElseThrewBomb,
	BombOnGround,
	Idle,
	SomeoneElseStuck,
	IAmStuck,
	ConfusedState
}