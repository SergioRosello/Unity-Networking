using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherPlayer : Character {
    public Vector2 Velocity;
	protected override Vector2 GetMovement() {
        return Velocity;
	}


    protected override void ApplyMovement() {
        _rb.velocity = GetMovement();
    }

    override protected void OnTriggerEnter2D(Collider2D other) {
    }


}