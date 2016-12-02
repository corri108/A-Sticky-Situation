using UnityEngine;
using System.Collections;

public class GravityLift : MonoBehaviour {

	public float ejectSpeed = 1f;
	public float difference = 1f;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerStay2D(Collider2D c)
	{
		if(c.gameObject.GetComponent<PlayerController>() != null)
		{
			if(GlobalProperties.IS_NETWORKED)
			{
				if(PhotonNetwork.player.ID == c.gameObject.GetComponent<PhotonView>().ownerId)
				{
					Rigidbody2D rb = c.gameObject.GetComponent<Rigidbody2D>();
					float dif = Mathf.Abs(this.transform.position.y - c.gameObject.transform.position.y) / c.gameObject.transform.position.y;
					dif += difference;
					rb.AddForce( Vector2.up * ejectSpeed * dif);

					if(rb.velocity.y > 10)
						rb.velocity = new Vector2(rb.velocity.x, 10);
				}
			}
			else
			{
				Rigidbody2D rb = c.gameObject.GetComponent<Rigidbody2D>();
				float dif = Mathf.Abs(this.transform.position.y - c.gameObject.transform.position.y) / c.gameObject.transform.position.y;
				dif += difference;
				rb.AddForce( Vector2.up * ejectSpeed * dif);
				
				if(rb.velocity.y > 10)
					rb.velocity = new Vector2(rb.velocity.x, 10);
			}
		}
	}
}
