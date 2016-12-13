using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayTutorialMovie : MonoBehaviour {

	public MovieTexture[] clips;
	public RawImage img;
	MovieTexture mov;
	int count = 0;

	// Use this for initialization
	void Start () {
		mov = img.texture as MovieTexture;
		mov = clips [0];
		mov.Play ();
		img.texture = mov;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (!mov.isPlaying)
		{
			count ++;
			if (count < clips.Length)
			{
				mov = clips [count];
				mov.Play ();
				img.texture = mov;
			}
		}
	}
}
