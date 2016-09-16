using UnityEngine;
using System.Collections;
using Photon;

public class NetworkPlayer : Photon.PunBehaviour {

	private Vector3 realPosition = Vector3.zero;
	private Vector3 positionAtLastPacket = Vector3.zero;
	private double currentTime = 0.0;
	private double currentPacketTime = 0.0;
	private double lastPacketTime = 0.0;
	private double timeToReachGoal = 0.0;
	private Animator ani;

	void Start()
	{
		ani = transform.GetChild (0).GetComponent<Animator> ();
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
			stream.SendNext((Vector3)transform.position);
			stream.SendNext((bool)ani.GetBool("Walking"));
			stream.SendNext((bool)ani.GetBool("InAir"));
		}
		else
		{
			//trying to fix position lag
			currentTime = 0.0;
			positionAtLastPacket = transform.position;
			realPosition = (Vector3)stream.ReceiveNext();
			lastPacketTime = currentPacketTime;
			currentPacketTime = info.timestamp;
			//animator syncing
			ani.SetBool("Walking", (bool)stream.ReceiveNext());
			ani.SetBool("InAir", (bool)stream.ReceiveNext());
		}
	}
}
