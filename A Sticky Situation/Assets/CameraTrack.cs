﻿using UnityEngine;
using System.Collections.Generic;

public class CameraTrack : MonoBehaviour 
{
	public float m_DampTime = 0.2f;                 
	public float m_ScreenEdgeBuffer = 4f;           
	public float m_MinSize = 6.5f;                  
	[HideInInspector]
	public List<PlayerController> targets; 
	
	private Camera m_Camera;                        
	private float m_ZoomSpeed;                      
	private Vector3 m_MoveVelocity;                 
	private Vector3 m_DesiredPosition;              
	
	private void Awake()
	{
		m_Camera = GetComponentInChildren<Camera>();
	}	

	public void SetTargets( List<PlayerController> targ)
	{
		targets = new List<PlayerController> ();
		targets = targ;
	}
	
	private void FixedUpdate()
	{
		Move();
		Zoom();
	}
	
	private void Move()
	{
		FindAveragePosition();
		
		transform.position = Vector3.SmoothDamp(transform.position, m_DesiredPosition, ref m_MoveVelocity, m_DampTime);
	}

	private void FindAveragePosition()
	{
		Vector3 averagePos = new Vector3();
		int numTargets = 0;
		
		for (int i = 0; i < targets.Count; i++)
		{
			if (!targets[i].gameObject.activeSelf)
				continue;
			
			averagePos += targets[i].transform.position;
			numTargets++;
		}
		
		if (numTargets > 0)
			averagePos /= numTargets;
		
		averagePos.y = transform.position.y;
		
		m_DesiredPosition = new Vector3(averagePos.x, averagePos.y, -200);
	}
	
	
	private void Zoom()
	{
		float requiredSize = FindRequiredSize();
		m_Camera.orthographicSize = Mathf.SmoothDamp(m_Camera.orthographicSize, requiredSize, ref m_ZoomSpeed, m_DampTime);
	}
	
	
	private float FindRequiredSize()
	{
		Vector3 desiredLocalPos = transform.InverseTransformPoint(m_DesiredPosition);
		
		float size = 0f;
		
		for (int i = 0; i < targets.Count; i++)
		{
			if (!targets[i].gameObject.activeSelf)
				continue;
			
			Vector3 targetLocalPos = transform.InverseTransformPoint(targets[i].transform.position);
			
			Vector3 desiredPosToTarget = targetLocalPos - desiredLocalPos;
			
			size = Mathf.Max (size, Mathf.Abs (desiredPosToTarget.y));
			
			size = Mathf.Max (size, Mathf.Abs (desiredPosToTarget.x) / m_Camera.aspect);
		}
		
		size += m_ScreenEdgeBuffer;
		
		size = Mathf.Max(size, m_MinSize);
		
		return size;
	}
	
	
	public void SetStartPositionAndSize()
	{
		FindAveragePosition();
		
		transform.position = m_DesiredPosition;
		
		m_Camera.orthographicSize = FindRequiredSize();
	}
}