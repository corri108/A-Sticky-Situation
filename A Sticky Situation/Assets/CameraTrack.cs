using UnityEngine;
using System.Collections.Generic;

public class CameraTrack : MonoBehaviour {
	
	[SerializeField] 
	List<PlayerController> targets;
	
	[SerializeField] 
	float boundingBoxPadding = 2f;
	
	[SerializeField]
	float minimumOrthographicSize = 5f;
	
	[SerializeField]
	float zoomSpeed = 20f;
	
	Camera camera;
	
	void Awake () 
	{
		camera = GetComponent<Camera>();
		camera.orthographic = true;
	}

	public void SetTargets(List<PlayerController> targ)
	{
		targets = new List<PlayerController> ();
		targets = targ;
	}
	
	void LateUpdate()
	{
		Rect boundingBox = CalculateTargetsBoundingBox();
		transform.position = Vector3.Lerp(transform.position, CalculateCameraPosition(boundingBox), .1f);
		camera.orthographicSize = CalculateOrthographicSize(boundingBox);
		//camera.orthographicSize = Mathf.Lerp (camera.orthographicSize, CalculateOrthographicSize (boundingBox), Time.deltaTime * zoomSpeed); 
	}
	
	/// <summary>
	/// Calculates a bounding box that contains all the targets.
	/// </summary>
	/// <returns>A Rect containing all the targets.</returns>
	Rect CalculateTargetsBoundingBox()
	{
		float minX = Mathf.Infinity;
		float maxX = Mathf.NegativeInfinity;
		float minY = Mathf.Infinity;
		float maxY = Mathf.NegativeInfinity;
		
		foreach (PlayerController target in targets) {
			Vector3 position = target.transform.position;
			
			minX = Mathf.Min(minX, position.x);
			minY = Mathf.Min(minY, position.y);
			maxX = Mathf.Max(maxX, position.x);
			maxY = Mathf.Max(maxY, position.y);
		}
		
		return Rect.MinMaxRect(minX - boundingBoxPadding, maxY + boundingBoxPadding, maxX + boundingBoxPadding, minY - boundingBoxPadding);
	}
	
	/// <summary>
	/// Calculates a camera position given the a bounding box containing all the targets.
	/// </summary>
	/// <param name="boundingBox">A Rect bounding box containg all targets.</param>
	/// <returns>A Vector3 in the center of the bounding box.</returns>
	Vector3 CalculateCameraPosition(Rect boundingBox)
	{
		Vector2 boundingBoxCenter = boundingBox.center;
		
		return new Vector3(boundingBoxCenter.x, boundingBoxCenter.y, camera.transform.position.z);
	}
	
	/// <summary>
	/// Calculates a new orthographic size for the camera based on the target bounding box.
	/// </summary>
	/// <param name="boundingBox">A Rect bounding box containg all targets.</param>
	/// <returns>A float for the orthographic size.</returns>
	float CalculateOrthographicSize(Rect boundingBox)
	{
		float orthographicSize = camera.orthographicSize;
		Vector3 topRight = new Vector3(boundingBox.x + boundingBox.width, boundingBox.y, 0f);
		Vector3 topRightAsViewport = camera.WorldToViewportPoint(topRight);
		
		if (topRightAsViewport.x >= topRightAsViewport.y)
			orthographicSize = (Mathf.Abs(boundingBox.width) / camera.aspect / 2f) + 2;
		else
			orthographicSize = (Mathf.Abs(boundingBox.height) / 2f) + 2;
		
		return Mathf.Clamp(Mathf.Lerp(camera.orthographicSize, orthographicSize, Time.deltaTime * zoomSpeed), minimumOrthographicSize, Mathf.Infinity);
	}
}