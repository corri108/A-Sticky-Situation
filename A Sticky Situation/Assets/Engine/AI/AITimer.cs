using UnityEngine;
using System.Collections;

public class AITimer 
{
	private int timerCount;
	private int timerReset;

	public AITimer(int count)
	{
		timerCount = count;
		timerReset = count;
	}

	public void Decrement()
	{
		timerCount--;
	}

	public int Count ()
	{
		return timerCount;
	}

	public void Reset()
	{
		timerCount = timerReset;
	}
}
