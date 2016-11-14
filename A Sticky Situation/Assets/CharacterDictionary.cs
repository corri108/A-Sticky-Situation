using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterDictionary : MonoBehaviour {

	public List<string> charNames;
	public List<Sprite> charPics;

	public Sprite GetByText(string s)
	{
		for(int i = 0; i < charNames.Count; ++i)
		{
			if(charNames[i].Equals(s))
			{
				return charPics[i];
			}
		}

		Debug.LogError("The dictionary does not contain: " + s);
		return null;
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
