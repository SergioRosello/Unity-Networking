using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinglePlayerBomb : Bomb {
    public float ExplosionRadius = 4;
    	
    protected override IEnumerator BombCoroutine() {
        base.BombCoroutine();
        yield return new WaitForSeconds(TimeToExplode);
        Destroy(gameObject);
        Instantiate(ExplosionPrefab, transform.position, Quaternion.identity);
        var cols = Physics2D.OverlapCircleAll(new Vector2(transform.position.x, transform.position.y), ExplosionRadius);
        foreach (var col in cols) {
            if (col.gameObject.layer == Layers.Obstacle) {
                iTween.ScaleTo(col.gameObject, iTween.Hash("scale", Vector3.zero, "time", .5f));
                Destroy(col.gameObject, 0.5f);
            }

            var targetHealth = col.GetComponent<Health>();
            if (targetHealth) {
                targetHealth.CurrentHealth--;
            }
        }
        GameManager.Instance.Bombs.Remove(Id);

        //SinglePlayerGameManager.Instance.ScanMap ();
        // Dañar al jugador etc etc
        yield return null;
    }

    protected override void Explode() {
        base.Explode();
        var cols = Physics2D.OverlapCircleAll(new Vector2(transform.position.x, transform.position.y), ExplosionRadius);
        foreach (var col in cols) {
            if (col.gameObject.layer == Layers.Obstacle) {
                //GameManager.Instance.Obstacles.Remove(col.GetComponent<SpawnedObject>().Id);
                iTween.ScaleTo(col.gameObject, iTween.Hash("scale", Vector3.zero, "time", .5f));
                Destroy(col.gameObject, 0.5f);
            }

            var targetHealth = col.GetComponent<Health>();
            if (targetHealth) {
                targetHealth.CurrentHealth--;
            }
        }
    }

}
