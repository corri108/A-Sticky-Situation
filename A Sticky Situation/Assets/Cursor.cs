using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Cursor : MonoBehaviour {

	static int SignedInCount = 0;
	private Xbox360Controller x3;
	public int ID = 1;
	public Image select;
	private Color desired;
	private string desiredText = "";
	private float speed = .08f;
	CharacterSelect colliding = null;
	LevelSelect collidingLevel = null;
	LevelSelect prevCollidingLevel = null;

	bool SignedIn = false;
	// Use this for initialization
	void Start () 
	{
		x3 = new Xbox360Controller (ID);
		this.GetComponent<SpriteRenderer> ().enabled = false;
		desired = select.color;
		select.color = Color.white;
		desiredText = select.transform.GetChild (0).GetComponent<Text> ().text;
		//select.transform.GetChild (0).GetComponent<Text> ().text = "";
	}

	public bool ReadyToPlay()
	{
		//is anyone signed in?
		if (SignedInCount <= 1)
			return false;

		if (!SignedIn)
			return true;
		else if (SignedIn && !GlobalProperties.PLAYERCHOICE [ID - 1].Equals ("NA"))
			return true;

		return false;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(x3 == null)
			x3 = new Xbox360Controller (ID);

		if(SignedIn)
		{
			float hor = x3.HorizontalAxis ();
			float ver = x3.VerticalAxis ();
			this.transform.position += new Vector3 (hor, -ver) * speed;
		}
		else
		{
			if(x3.StartPressed())
			{
				SignedIn = true;
				this.GetComponent<SpriteRenderer> ().enabled = true;
				select.transform.GetChild (1).GetComponent<Text> ().text = "";
				select.color = desired;
				SignedInCount++;
				AudioSource.PlayClipAtPoint(GameObject.FindObjectOfType<NetworkingObjectMenu>().click, GameObject.FindGameObjectWithTag("MainCamera").transform.position);
			}
		}

		if(colliding != null)
		{
			if(x3.PressedJump())
			{
				CharacterType character = colliding.ct;
				GlobalProperties.PLAYERCHOICE[ID - 1] = character.ToString();
				select.transform.GetChild(2).GetComponent<Image>().sprite = GameObject.FindObjectOfType<CharacterDictionary>().GetByText(character.ToString());
				select.transform.GetChild (1).GetComponent<Text> ().text = "\n" + character.ToString();
				select.transform.GetChild (1).GetComponent<Text> ().alignment = TextAnchor.UpperRight; 	
				AudioSource.PlayClipAtPoint(colliding.noise, GameObject.FindGameObjectWithTag("MainCamera").transform.position);
			}
		}

		if (collidingLevel != null) 
		{
			if(x3.PressedJump())
			{
				LevelType level = collidingLevel.lt;
				GlobalProperties.LEVEL = level.ToString();
				collidingLevel.GetComponent<SpriteRenderer>().color = Color.red;
				AudioSource.PlayClipAtPoint(colliding.noise, GameObject.FindGameObjectWithTag("MainCamera").transform.position);

				if(prevCollidingLevel == null || prevCollidingLevel == collidingLevel)
				{
					prevCollidingLevel = collidingLevel;
				}
				else
				{
					prevCollidingLevel.GetComponent<SpriteRenderer>().color = Color.white;
					prevCollidingLevel = collidingLevel;
				}
			}
		}
	}

	void OnTriggerEnter2D(Collider2D c)
	{
		if(c.gameObject.GetComponent<CharacterSelect>() != null)
		{
			colliding = c.gameObject.GetComponent<CharacterSelect>();
		}
		if(c.gameObject.GetComponent<LevelSelect>() != null)
		{
			collidingLevel = c.gameObject.GetComponent<LevelSelect>();
		}
	}
}
