using UnityEngine;
using System.Collections;
using Photon;

public class NetworkStickyBomb : Photon.PunBehaviour {
	
	private Vector3 correctBombPos;
	private Quaternion correctBombRot;
	private StickyBomb sb;

	void Start()
	{
		sb = GetComponent<StickyBomb> ();
	}
	
	// Update is called once per frame
	void Update()
	{
		if (!photonView.isMine)
		{
			transform.position = Vector3.Lerp(transform.position, this.correctBombPos, Time.deltaTime * 20);
			transform.rotation = Quaternion.Lerp(transform.rotation, this.correctBombRot, Time.deltaTime * 20);
		}
	}
	
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			// We own this player: send the others our data
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
		}
		else
		{
			// Network player, receive data
			this.correctBombPos = (Vector3)stream.ReceiveNext();
			this.correctBombRot = (Quaternion)stream.ReceiveNext();
		}
	}
}
