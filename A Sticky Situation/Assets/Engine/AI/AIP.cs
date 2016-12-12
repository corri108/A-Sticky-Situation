using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIP : MonoBehaviour {

	public AIPType type = AIPType.InheritVelocity;
	public bool jump = false;
	public bool move = false;
	public int FLOOR = 1;
	public bool ISMASTER = false;
	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	public static AIP FindMaster(int floor)
	{
		AIP[] aips = GameObject.FindObjectsOfType<AIP> ();

		foreach(var a in aips)
		{
			if(a.ISMASTER && a.FLOOR == floor)
				return a;
		}

		return null;
	}

	public static int MaxFloor ()
	{
		int highFloor = 0;
		AIP[] aips = GameObject.FindObjectsOfType<AIP> ();

		foreach(var a in aips)
		{
			if(a.FLOOR > highFloor)
			{
				highFloor = a.FLOOR;
			}
		}

		return highFloor;
	}

	public static List<AIP> FindMasters(int floor)
	{
		AIP[] aips = GameObject.FindObjectsOfType<AIP> ();
		List<AIP> ret = new List<AIP> ();
		
		foreach(var a in aips)
		{
			if(a.ISMASTER && a.FLOOR == floor)
			{
				ret.Add(a);
			}
		}
		
		return ret;
	}

	public static AIP PointNear (Vector3 pos)
	{
		AIP[] aips = GameObject.FindObjectsOfType<AIP> ();
		AIP closest = null;
		float closestD = float.MaxValue;
		
		foreach(var a in aips)
		{
			float d = Vector3.Distance(pos, a.transform.position);
			if(d < closestD)
			{
				closestD = d;
				closest = a;
			}
		}
		
		return closest;
	}

	public static AIP FindClosestMaster(int floor, Vector3 pos)
	{
		List<AIP> aips = FindMasters (floor);
		AIP closest = null;
		float closestD = float.MaxValue;

		foreach(var a in aips)
		{
			float d = Vector3.Distance(pos, a.transform.position);
			if(d < closestD)
			{
				closestD = d;
				closest = a;
			}
		}

		return closest;
	}
}
public enum AIPType
{
	AlwaysLeft,//will always trigger the point, always will go left
	AlwaysRight,//will always trigger the point, always will go right
	InheritVelocity,//will always trigger the point, will continue in direction was already traveling
	OnlyIfGoingRight,//will only trigger the point if you are going right
	OnlyIfGoingLeft//will only trigger the point if you are going left
}