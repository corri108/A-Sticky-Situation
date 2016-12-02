using UnityEngine;
using System.Collections;

public class Introduction : MonoBehaviour {

	private Vector3 origScale;

	// Use this for initialization
	void Start () {
		origScale = this.transform.localScale;
		this.transform.localScale = Vector3.zero;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		this.transform.localScale = Vector3.Lerp (this.transform.localScale, origScale, .1f);
	}

	void OnDisable()
	{
		this.transform.localScale = Vector3.zero;
	}
}
