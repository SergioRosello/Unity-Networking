using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;

public class OnlineGameManager : GameManager
{
    public GameObject OtherPlayerPrefab;
    public int MapVersion;
    public string HostIp;
    public int HostPort;
    public Text highscoreText;


    public const string INITIAL_REQUEST = "initial";
    public const string UPDATE_REQUEST = "update";
    public const string CHEST_REQUEST = "picked_chest";

    protected Player _player;
    protected int _playerId;
    protected Dictionary<int, OtherPlayer> _otherPlayers;
    protected Dictionary<int, OtherPlayer> otherPlayersControl;
    protected TAPNet _client;
    protected int highscore = -1;

    // Use this for initialization
    protected override void Awake()
    {
        base.Awake();
        _client = new TAPNet(HostIp, HostPort)
        {
            onResponseReceived = OnServerResponse
        };
        _otherPlayers = new Dictionary<int, OtherPlayer>();

        var initialData = new InitialData
        {
            playerName = PlayerPrefs.GetString("playerName")
        };
        _client.Send(initialData.ToJson(), TAPNet.DATAGRAM_RELIABLE);
    }

    /// <summary>
    /// Método a ejecutar cuando el servidor nos responda a una petición
    /// </summary>
    /// <param name="json">JSON de respuesta que llega desde el servidor.</param>
    public void OnServerResponse(JSONNode json)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(ProcessJSON(json));
    }

    /// <summary>
    /// Lo utilizamos para procesar el JSON que nos llega desde el servidor.
    /// </summary>
    /// <param name="json">JSON que nos envía el servidor.</param>
    IEnumerator ProcessJSON(JSONNode json)
    {
        if (json["type"].Value == INITIAL_REQUEST)
        {
            MapWidth = json["width"].AsInt;
            MapHeight = json["height"].AsInt;
            MapVersion = json["map_version"].AsInt;
            var playerSpawnPosition = json["spawn"].ReadVector3();
            _playerId = json["playerId"].AsInt;

            var obstacleParent = new GameObject("Obstacles").transform;

            var mapNode = json["map"];
            var obstacles = new int[mapNode.AsArray.Count, mapNode.AsArray[0].AsArray.Count];

            for (int i = 0; i < mapNode.AsArray.Count; i++)
            {
                for (int j = 0; j < mapNode.AsArray[i].AsArray.Count; j++)
                {
                    obstacles[i, j] = mapNode.AsArray[i].AsArray[j].AsInt;
                }
            }

            BuildMap(obstacles);
            var go = Instantiate(PlayerPrefab, playerSpawnPosition, Quaternion.identity);
            _player = go.GetComponent<Player>();
            _player.Name = PlayerPrefs.GetString("playerName");
            _characters.Add(_player);
            GenerateBorders();
            StartCoroutine(NetworkUpdateLoop());
        }
        else if (json["type"].Value == UPDATE_REQUEST)
        {
            var gameState = json["state"];
            RemainingTime = gameState["timer"].AsInt;
            // Lista de cambios que tenemos que hacer en el mapa
            foreach (var version in gameState["map_changes"].AsArray)
            {
                MapVersion++;
                foreach (var change in version.Value.AsArray)
                {
                    var k = change.Value.AsInt;
                    if (Obstacles.ContainsKey(k))
                    {
                        iTween.ScaleTo(Obstacles[k], iTween.Hash("scale", Vector3.zero, "time", .5f));
                        Destroy(Obstacles[k], 0.5f);
                        Obstacles.Remove(k);
                    }
                }
            }

            foreach (var bomb in gameState["bombs"].AsArray)
            {
                var timer = bomb.Value["timer"].AsFloat;
                var bombId = bomb.Value["id"].AsInt;

                if (!Bombs.ContainsKey(bombId))
                {
                    Bombs[bombId] = Instantiate(BombPrefab, bomb.Value.ReadVector3(), Quaternion.identity);
                    Bombs[bombId].GetComponent<Bomb>().TimeToExplode = timer;
                    Bombs[bombId].GetComponent<Bomb>().Id = bombId;
                    SetSortingOrder(Bombs[bombId]);
                }
            }

            foreach (var chest in gameState["chests"].AsArray)
            {
                var chestId = chest.Value["id"].AsInt;

                if (!Chests.ContainsKey(chestId))
                {
                    Chests[chestId] = Instantiate(ChestPrefab, chest.Value.ReadVector3(), Quaternion.identity);
                    Chests[chestId].GetComponent<SpawnedObject>().Id = chestId;
                    SetSortingOrder(Chests[chestId].gameObject);
                }
            }

            // Hacemos una copia de los players que tenemos controlados
            otherPlayersControl = new Dictionary<int, OtherPlayer>(_otherPlayers);

            foreach (var pair in gameState["players"]){
                var key = int.Parse(pair.Key);
                var value = pair.Value;
                // Si estamos actualizando los otros personajes
                if (key != _playerId) {
                    if (!_otherPlayers.ContainsKey(key)) {
                        // Jugador que no tenemos instanciado. lo instanciamos
                        var otherPlayer = Instantiate(OtherPlayerPrefab).GetComponent<OtherPlayer>();
                        otherPlayer.Name = value["playerName"];
                        _otherPlayers[key] = otherPlayer;
                    }

                    // Elimino personaje si existe en la partida actual y me quedo con los que ya no esten
                    // para mantener un control de que ha cambiado
                    if (otherPlayersControl.ContainsKey(key)){
                        otherPlayersControl.Remove(key);
                    }
                    // Actualizamos su posicion, velocidad y puntuacion
                    _otherPlayers[key].GetComponent<Character>().SetPosition(new Vector2(pair.Value["position"]["x"], pair.Value["position"]["y"]));
                    _otherPlayers[key].GetComponent<OtherPlayer>().Velocity = pair.Value["velocity"];
                    _otherPlayers[key].GetComponent<OtherPlayer>().Score = pair.Value["score"];
                    
                    // Si el servidor dice que siguen vivos, actualizar su vida
                    if (_otherPlayers[key].GetComponent<Health>().CurrentHealth > 0) {
                        _otherPlayers[key].GetComponent<Health>().CurrentHealth = pair.Value["health"];
                    }
                    else{ // Si dice que estan muertos, destruirlos
                        _otherPlayers[key].GetComponent<Health>().CurrentHealth = 0;
                    }

                }
                // Si estamos actualizando nuestro personaje
                else {
                    // Actualizamos su vida y puntuacion
                    _player.GetComponent<Health>().CurrentHealth = pair.Value["health"];
                    _player.GetComponent<Character>().Score = pair.Value["score"];
                }

                // Actualizamos la puntuacion, de forma que siempre mantenga la
                // Mas alta
                if (pair.Value["score"].AsInt >= highscore) {
                    highscore = pair.Value["score"].AsInt;
                    highscoreText.text = "Top: " + pair.Value["playerName"] + " " + pair.Value["score"];
                }
            }

            // Los que quedan en el control son los que se han desconectado
            // Han muerto o han sufrido algun problema en red
            // Los eliminamos de otherPlayers
            if (otherPlayersControl.Count > 0) {
                foreach (var pair in otherPlayersControl) {
                    _otherPlayers[pair.Key].GetComponent<Health>().CurrentHealth = 0;
                }
                // Si hemos usado el diccionario de control, lo reestablecemos
                otherPlayersControl.Clear();
            }

        }
        yield return null;
    }

    /// <summary>
    /// Se actualiza periódicamente para mantener el estado de la partida actualizado
    /// </summary>
    IEnumerator NetworkUpdateLoop()
    {
        while (true)
        {
            var playerRb = _player.GetComponent<Rigidbody2D>();
            var updateData = new UpdateData
            {
                position = playerRb.position,
                velocity = playerRb.velocity,
                mapVersion = MapVersion,
                playerId = _playerId
            };
            _client.Send(updateData.ToJson());
            yield return new WaitForSeconds(.1f);
        }
    }

    /// <summary>
    /// Se ejecuta cuando un personaje coge un cofre
    /// </summary>
    /// <param name="character">Character que ha cogido el cofre.</param>
    /// <param name="chestCol">Collider del cofre.</param>
    public override void PickedChest(Character character, Collider2D chestCol)
    {
        chestCol.enabled = false;
        var chestId = chestCol.GetComponent<SpawnedObject>().Id;
        iTween.ScaleTo(chestCol.gameObject, iTween.Hash("scale", Vector3.zero, "time", 1f));
        Chests.Remove(chestId);
        Destroy(chestCol.gameObject, 1f);
        var chestRequest = new ChestData
        {
            chestId = chestId,
            playerId = _playerId
        };

        _client.Send(chestRequest.ToJson(), TAPNet.DATAGRAM_RELIABLE);
    }

    void OnDestroy()
    {
        _client.Cleanup();
    }
}