using UnityEngine;
using System.Collections;
using Photon;
using System;

public class NetworkPlayer : Photon.PunBehaviour {

	private Vector3 realPosition = Vector3.zero;
	private Vector3 positionAtLastPacket = Vector3.zero;
	private double currentTime = 0.0;
	private double currentPacketTime = 0.0;
	private double lastPacketTime = 0.0;
	private double timeToReachGoal = 0.0;
	private Animator ani;
	private Vector3 negScale;
	private Vector3 regScale;

	void Start()
	{
		ani = transform.GetChild (0).GetComponent<Animator> ();
		regScale = this.transform.GetChild (0).localScale;
		negScale = new Vector3 (-regScale.x, regScale.y, regScale.z); 
	}
	
	void Update ()
	{
		if (!photonView.isMine)
		{
			//position lag
			timeToReachGoal = currentPacketTime - lastPacketTime;
			currentTime += Time.deltaTime;
			transform.position = Vector3.Lerp(positionAtLastPacket, realPosition, (float)(currentTime / timeToReachGoal));
		}
	}
	
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			try{
			stream.SendNext((Vector3)transform.position);
			stream.SendNext((bool)ani.GetBool("Walking"));
			stream.SendNext((bool)ani.GetBool("InAir"));
			stream.SendNext((Vector3)transform.localScale);
			}
			catch(Exception e){}
		}
		else
		{
			try{
			//trying to fix position lag
			currentTime = 0.0;
			positionAtLastPacket = transform.position;
			realPosition = (Vector3)stream.ReceiveNext();
			lastPacketTime = currentPacketTime;
			currentPacketTime = info.timestamp;
			//animator syncing
			ani.SetBool("Walking", (bool)stream.ReceiveNext());
			ani.SetBool("InAir", (bool)stream.ReceiveNext());
			Vector3 recScale = (Vector3)stream.ReceiveNext();

			if(recScale.x > 0)
			{
				this.transform.GetChild(0).localScale = regScale;
			} 
			else
			{
				this.transform.GetChild(0).localScale = negScale;
			}
			}
			catch(Exception e){}
		}
	}
}
