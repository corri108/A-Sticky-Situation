using UnityEngine;
using System.Collections;

public class Scorecard 
{
	public int PlayerID = -1;
	public int Kills = 0;
	public int Deaths = 0;
	public int CratesPickedUp = 0;
	public int RoundsWon = 0;

	public Scorecard(int playerID)
	{
		this.PlayerID = playerID;
	}

	public Scorecard(int playerID, int kills, int deaths, int cratesPickedUp, int roundsWon)
	{
		this.PlayerID = playerID;
		this.Kills = kills;
		this.Deaths = deaths;
		this.CratesPickedUp = cratesPickedUp;
		this.RoundsWon = roundsWon;
	}

	public string ToString()
	{
		return string.Format ("P{0}: Kills:{1} Deaths: {2} Crates: {3} Wins: {4}", PlayerID, Kills, Deaths, CratesPickedUp, RoundsWon);  
	}
}
