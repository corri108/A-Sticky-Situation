using UnityEngine;
using System.Collections;

public class PopText : MonoBehaviour {

	public bool isMaster = false;
	[HideInInspector]
	public int timer = -1;
	private static GameObject prefab;
	// Use this for initialization
	void Awake () {
		prefab = Resources.Load<GameObject> ("PopText");
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void FixedUpdate()
	{
		if (isMaster)
			return;

		timer--;
		if(timer < 70)
		{
			this.gameObject.transform.localScale *= .95f;
		}
		if(timer == 0)
		{
			GameObject.Destroy(this.gameObject);
		}
	}

	public static void Create(string s, Color c, int t, Vector3 pos)
	{
		GameObject newPop = (GameObject)GameObject.Instantiate (prefab, pos, Quaternion.identity);
		newPop.GetComponent<TextMesh> ().text = s;
		newPop.GetComponent<TextMesh> ().color = c;
		newPop.GetComponent<PopText> ().timer = t;
	}
}
