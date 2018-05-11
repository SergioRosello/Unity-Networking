using System.Collections;
using UnityEngine;

public class SinglePlayerGameManager : GameManager {
    public float MinChestSpawnInterval = 3, MaxChestSpawnInterval = 7;
    public float MinBombSpawnInterval = 5, MaxBombSpawnInterval = 12;
    public float BombMinExplosionTimer = 2, BombMaxExplosionTimer = 5;
    public int RoundDuration = 90;

    float _bombSpawnMultiplier = 1, _chestSpawnMultiplier = 1;

    // Use this for initialization
    protected override void Awake() {
        base.Awake();

        RemainingTime = RoundDuration;
        // Generación del mapa
        generator = new MapGenerator(MapWidth, MapHeight, 60);
        BuildMap(generator.Map);

        // Configurar el A Star
        var aStar = FindObjectOfType<AstarPath>();
        _graph = (Pathfinding.GridGraph)aStar.graphs[0];
        _graph.SetDimensions(MapWidth, MapHeight, 1);
        _graph.center = new Vector3(MapWidth / 2f - .5f, MapHeight / 2f - .5f, 0);
        ScanMap();

        var player = Instantiate(PlayerPrefab, GetRandomSpawnPosition(), Quaternion.identity).GetComponent<Player>();
        player.Name = PlayerPrefs.GetString("playerName");
        _characters.Add(player);

        for (int i = 1; i < 4; i++) {
            var go = Instantiate(EnemyPrefab, GetRandomSpawnPosition(), Quaternion.identity);
            var character = go.GetComponent<Character>();
            character.Name = "Enemy " + i;
            _characters.Add(character);

            var color = Color.blue;
            switch (i) {
                case 1:
                    color = Color.red;
                    break;
                case 2:
                    color = Color.yellow;
                    break;
            }
            go.GetComponent<SpriteRenderer>().color = color;
        }

        GenerateBorders();

        StartCoroutine(ChestSpawnCoroutine());
        StartCoroutine(BombSpawnCoroutine());
        StartCoroutine(Countdown());

    }

    /// <summary>
    /// Instancia cofres por el mapa de forma periódica
    /// </summary>
    IEnumerator ChestSpawnCoroutine() {
        int chestId = 0;
        while (true) {
            yield return new WaitForSeconds(Random.Range(MinChestSpawnInterval, MaxChestSpawnInterval) * _chestSpawnMultiplier);
            var pos = GetRandomSpawnPosition();
            var chest = Instantiate(ChestPrefab, pos, Quaternion.identity).GetComponent<SpawnedObject>();
            Chests[chestId] = chest.gameObject;
            _chestSpawnMultiplier *= .995f;
            SetSortingOrder(chest.gameObject);
            chestId++;
        }
    }

    /// <summary>
    /// Instancia bombas por el mapa de forma periódica
    /// </summary>
    IEnumerator BombSpawnCoroutine() {
        int bombId = 0;
        while (true) {
            yield return new WaitForSeconds(Random.Range(MinBombSpawnInterval, MaxBombSpawnInterval) * _bombSpawnMultiplier);
            var pos = GetRandomSpawnPosition();
            var bomb = Instantiate(BombPrefab, pos, Quaternion.identity).GetComponent<Bomb>();
            Bombs[bombId] = bomb.gameObject;
            bomb.TimeToExplode = Random.Range(BombMinExplosionTimer, BombMaxExplosionTimer);
            bomb.Id = bombId;
            _bombSpawnMultiplier *= .99f;
            SetSortingOrder(BombPrefab.gameObject);
            bombId++;
        }
    }

    /// <summary>
    /// Cuenta atrás
    /// </summary>
    IEnumerator Countdown() {
        while (RemainingTime > -1) {
            yield return new WaitForSeconds(1);
            RemainingTime--;
        }
    }

    Vector3 GetRandomSpawnPosition() {
        int x = Random.Range(0, MapWidth);
        int y = Random.Range(0, MapHeight);
        while (!IsMapEmpty(x, y)) {
            x = Random.Range(0, MapWidth);
            y = Random.Range(0, MapHeight);
        }
        return new Vector3(x, y, 0);
    }

    protected bool IsMapEmpty(int x, int y) {
        return generator.Map[x, y] == 0;
    }

    public void ScanMap() {
        _graph.Scan();
    }

    public override void PickedChest(Character character, Collider2D chestCol) {
        chestCol.enabled = false;
        Debug.Log("Removing: " + chestCol.GetComponent<SpawnedObject>().Id);
        Chests.Remove(chestCol.GetComponent<SpawnedObject>().Id);
        iTween.ScaleTo(chestCol.gameObject, iTween.Hash("scale", Vector3.zero, "time", 1f));
        Destroy(chestCol.gameObject, 1f);
        character.Score++;
    }

}
