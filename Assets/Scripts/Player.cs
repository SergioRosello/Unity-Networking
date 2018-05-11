using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character {
	protected override void Awake() {
		base.Awake();
	}

	protected override Vector2 GetMovement() {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
	}
}