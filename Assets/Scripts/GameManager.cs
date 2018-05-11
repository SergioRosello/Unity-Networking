using System.Collections.Generic;
using UnityEngine;

public abstract class GameManager : Singleton<GameManager> {
	public GameObject ObstaclePrefab, GroundPrefab, ChestPrefab, BombPrefab, PlayerPrefab, EnemyPrefab;
	public int MapWidth = 30, MapHeight = 12;
    public Dictionary<int, GameObject> Bombs, Chests, Obstacles;

	public int RemainingTime {
		get {
			return _remainingTime;
		} set {
			_remainingTime = value;
            if (_remainingTime <= 0) {
                _remainingTime = 0;
                // Se acaba la partida
                GameOver();
            }
			GUIManager.Instance.TimerText.text = "Time: " + RemainingTime;
		}
	}

	public Bounds MapBounds {
		get {
			return new Bounds (new Vector3(MapWidth/2f -.5f, MapHeight/2f -.5f, 0), new Vector3(MapWidth, MapHeight, 0));
		}
	}

	protected MapGenerator generator;
	protected int _remainingTime;
	protected Pathfinding.GridGraph _graph;
    protected List<Character> _characters;

	// Use this for initialization
	protected virtual void Awake () {
		Time.timeScale = 1;

        Chests = new Dictionary<int, GameObject>();
        Bombs = new Dictionary<int, GameObject>();
        Obstacles = new Dictionary<int, GameObject>();
        _characters = new List<Character>();
	}

    protected void GenerateBorders() {
        // Instanciar los 4 BoxCollider2D

        var leftWall = new GameObject("Left Wall").AddComponent<BoxCollider2D>();
        leftWall.size = new Vector2(1, MapHeight);
        leftWall.transform.position = new Vector3(-1, MapHeight / 2f - .5f, 0);

        var rightWall = new GameObject("Right Wall").AddComponent<BoxCollider2D>();
        rightWall.size = new Vector2(1, MapHeight);
        rightWall.transform.position = new Vector3(MapWidth, MapHeight / 2f - .5f, 0);

        var topWall = new GameObject("Top Wall").AddComponent<BoxCollider2D>();
        topWall.size = new Vector2(MapWidth, 1);
        topWall.transform.position = new Vector3(MapWidth / 2f - .5f, MapHeight);

        var bottomWall = new GameObject("Bottom Wall").AddComponent<BoxCollider2D>();
        bottomWall.size = new Vector2(MapWidth, 1);
        bottomWall.transform.position = new Vector3(MapWidth / 2f - .5f, -1);
    }
	
    /// <summary>
    /// Construye el mapa a partir de unos obstáculos
    /// </summary>
    /// <param name="obstacles">Obstáculos que tendrá el mapa, si el valor de la posición es 0, no tendrá obstáculo, en caso contrario, habrá un obstáculo con la id contenida en la posición.</param>
	protected void BuildMap(int[,] obstacles) {
		var mapParent = new GameObject ("Map").transform;
        var obstacleParent = new GameObject("Obstacles").transform;

		for (int row = 0; row < MapHeight; row++) {
			var rowTransform = new GameObject ("Row " + row).transform;
			rowTransform.parent = mapParent;
			for (int col = 0; col < MapWidth; col++) {
				var go = Instantiate(GroundPrefab, new Vector3(col, row, 0), Quaternion.identity);
				go.name = "Col " + col;
				go.transform.parent = rowTransform;
			}
		}

		for (int i = 0; i <obstacles.GetLength(0); i++) {
			for (int j = 0; j <obstacles.GetLength(1); j++) {
                if (obstacles [i, j] != 0) {
                    var obstacle = Instantiate(ObstaclePrefab, new Vector3(i, j, 0), Quaternion.identity).GetComponent<SpawnedObject>();
                    obstacle.transform.parent = obstacleParent;
                    Obstacles[obstacles[i, j]] = obstacle.gameObject;
                    SetSortingOrder (obstacle.gameObject);
				}
			}
		}
	}


	protected void SetSortingOrder(GameObject go) {
		var sr = go.GetComponent<SpriteRenderer> ();
		if (sr != null) {
			sr.sortingOrder = (int)(100 * -sr.transform.position.y);
		}
	}


	public void GameOver() {
		GUIManager.Instance.MainMenuButton.gameObject.SetActive(true);
		Time.timeScale = 0;
	}

    public abstract void PickedChest(Character character, Collider2D chestCol);
}
