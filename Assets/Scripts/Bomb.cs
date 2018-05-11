using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : SpawnedObject {
	public GameObject ExplosionPrefab;
    public float TimeToExplode = 3;

	// Use this for initialization
    protected virtual void Start () {
        //x = (int)transform.position.x;
        //y = (int)transform.position.y;
        StartCoroutine(BombCoroutine());
	}

    protected virtual IEnumerator BombCoroutine() {
        iTween.PunchScale(gameObject, iTween.Hash("amount", Vector3.one * .8f, "looptype", iTween.LoopType.loop, "time", TimeToExplode / 5));
        iTween.ColorTo(gameObject, Color.red, TimeToExplode);
        yield return new WaitForSeconds(TimeToExplode);
        Explode();
        yield return null;
    }

    protected virtual void Explode() {
        Instantiate(ExplosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

}
