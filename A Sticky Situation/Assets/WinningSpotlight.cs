using UnityEngine;
using System.Collections;

public class WinningSpotlight : MonoBehaviour {

	int openSpotAngle = 40;
	float openIntensity = 2.5f;
	Light l;
	bool open = false;
	[HideInInspector]
	public GameObject winner;
	// Use this for initialization
	void Start () 
	{
		l = GetComponent<Light> ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(winner != null)
		{
			Vector3 pwoz = new Vector3( winner.transform.position.x,  winner.transform.position.y, this.transform.position.z);
			this.transform.position = Vector3.Lerp (this.transform.position, pwoz, .1f);
		}

		if(open)
		{
			l.intensity = Mathf.Lerp (l.intensity, openIntensity, .1f);
			l.spotAngle = Mathf.Lerp (l.spotAngle, openSpotAngle, .1f);
		}
		else
		{
			l.intensity = Mathf.Lerp (l.intensity, 0, .1f);
			l.spotAngle = Mathf.Lerp (l.spotAngle, 0, .1f);
		}
	}

	public void Open()
	{
		open = true;
	}

	public void Close()
	{
		open = false;
	}
}
