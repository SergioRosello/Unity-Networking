using System;
using UnityEngine;

public class UpdateData : NetworkData {
    public Vector2 position, velocity;
    public int mapVersion;
    public int playerId;
    public string clientTimeStamp;

    public UpdateData() {
        type = OnlineGameManager.UPDATE_REQUEST;
        clientTimeStamp = DateTime.Now.ToString("yyyy/MM/ddHH:mm:ss.fff");
    }

}
