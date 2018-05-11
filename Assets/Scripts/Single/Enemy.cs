using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class Enemy : Character {
	protected Seeker _seeker;
	protected Path _path;

	protected float _nextPointDistance;
	protected int _nextPoint;
	protected float _lastRepath = float.NegativeInfinity;

	public float RepathRate = .3f;

	protected override void Awake () {
		base.Awake ();
		_seeker = GetComponent<Seeker> ();
	}

	protected override Vector2 GetMovement() {
		if (GameManager.Instance.Chests.Count == 0) {
			return Vector2.zero;
		}

		if (Time.time > _lastRepath + RepathRate) {
			// Localizar el cofre más cercano
			float minDist = Mathf.Infinity;
			GameObject closest = null;

			foreach (var chest in GameManager.Instance.Chests.Values) {
				if (chest && Vector3.Distance (transform.position, chest.transform.position) < minDist) {
					minDist = Vector3.Distance (transform.position, chest.transform.position);
					closest = chest;
				}
			}
			if (closest) {
				_lastRepath = Time.time;
				_path = _seeker.StartPath (transform.position, closest.transform.position, OnPathComplete);
			}
		}
			
		if (_path == null) {
			return Vector2.zero;
		}

		if (_nextPoint > _path.vectorPath.Count) {
			return Vector2.zero;
		}

		if (_nextPoint == _path.vectorPath.Count) {
			_nextPoint++;
			return Vector2.zero;
		}

		var nextNodePosition = _path.vectorPath [_nextPoint];
		Vector2 direction = new Vector2 (nextNodePosition.x - transform.position.x, nextNodePosition.y - transform.position.y).normalized;
		Direction = direction;

		if (Vector3.Distance (transform.position, _path.vectorPath [_nextPoint]) < .1f) {
			_nextPoint++;
		}

		// Nos movemos a él
		return direction;
	}

	public Vector2 Direction;

	public void OnPathComplete(Path p) {
		if (!p.error) {
			_nextPoint = 0;
		} else {
			Debug.Log ("Path error");
		}
	}

}
