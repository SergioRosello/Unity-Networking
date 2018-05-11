using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {
	private Transform target;
	public Vector3 Offset = new Vector3(0, 0, -10);
	private Camera _cam;
	// Use this for initialization
	void Awake () {
		_cam = GetComponent<Camera> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (!target) {
			var go = GameObject.FindGameObjectWithTag ("Player");
			if (go) {
				target = go.transform;
			}
		}

		if (target) {
			var targetPosition = target.transform.position + Offset;
			var targetPositionBounds = new Bounds (targetPosition, _cam.OrthographicBounds ().size);
			var mapBounds = GameManager.Instance.MapBounds;

			if (targetPositionBounds.min.x < mapBounds.min.x) {
				targetPosition = new Vector3 (mapBounds.min.x + targetPositionBounds.size.x / 2, targetPosition.y, targetPosition.z);
			} else if (targetPositionBounds.max.x > mapBounds.max.x) {
				targetPosition = new Vector3 (mapBounds.max.x - targetPositionBounds.size.x / 2, targetPosition.y, targetPosition.z);
			}

			if (targetPositionBounds.min.y < mapBounds.min.y) {
				targetPosition = new Vector3 (targetPosition.x, mapBounds.min.y + targetPositionBounds.size.y / 2, targetPosition.z);
			} else if (targetPositionBounds.max.y > mapBounds.max.y) {
				targetPosition = new Vector3 (targetPosition.x, mapBounds.max.y - targetPositionBounds.size.y / 2, targetPosition.z);
			}
			transform.position = targetPosition;
		}
	}
}
