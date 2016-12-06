using UnityEngine;
using System.Collections;

public class Xbox360Controller
{
	int id = -1;

	public Xbox360Controller(int playerID)
	{
		id = playerID;
		Debug.Log ("XBOX with id: " + id);
	}

	public bool PressedJump()
	{
		return Input.GetButtonDown ("Jump" + id.ToString ());
	}

	public float HorizontalAxis()
	{
		return Input.GetAxis ("Horiz" + id.ToString ());
	}

	public float VerticalAxis()
	{
		return Input.GetAxis ("Vert" + id.ToString ());
	}

	public bool PressedSpecial()
	{
		return Input.GetButtonDown ("Special" + id.ToString ());
	}

	public bool PressedThrow()
	{
		return Input.GetButtonDown ("Throw" + id.ToString ());
	}

	public bool StartPressed ()
	{
		return Input.GetButtonDown ("Start" + id.ToString ());
	}

	public bool SprintPressed()
	{
		Debug.Log ("TRIG: " + Input.GetAxis ("Sprint" + id.ToString ()));
		return Input.GetAxis ("Sprint" + id.ToString ()) < 0f;// + id.ToString ());
	}

	public bool CelebratePressed()
	{
		return Input.GetButtonDown ("Celebrate" + id.ToString ());
	}
}
