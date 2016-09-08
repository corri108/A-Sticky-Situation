using UnityEngine;
using System.Collections;

public class Torch : MonoBehaviour {

	Light pointLight;
	float maxI;
	public float iRange = 1f;
	public float iAmplitude = .075f;
	// Use this for initialization
	void Start () {
		pointLight = transform.GetChild (0).GetComponent<Light> ();
		maxI = pointLight.intensity;
	}
	
	// Update is called once per frame
	void Update () 
	{
		pointLight.intensity += Random.Range (-iAmplitude, iAmplitude);
		pointLight.intensity = Mathf.Clamp (pointLight.intensity, maxI - iRange, maxI + iRange);
	}
}
