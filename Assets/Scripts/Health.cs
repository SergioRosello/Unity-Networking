using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour {
	public int MaxHealth = 1;

	protected int _currentHealth;
	protected Animator _anim;

	public int CurrentHealth {
		get {
			return _currentHealth;
		} set {
			if (IsAlive) {
				_currentHealth = value;

				if (_currentHealth <= 0) {
					if (_anim) {
						_anim.SetTrigger ("die");
					}
					if (gameObject.tag == "Player") {
						GameManager.Instance.GameOver ();
					}
				}
			}
		}
	}

	public bool IsAlive {
		get {
			return _currentHealth > 0;
		}
	}

	// Use this for initialization
	void Awake () {
		_currentHealth = MaxHealth;
		_anim = GetComponentInChildren<Animator> ();
	}
}


// https://pastebin.com/nKJXSxyg