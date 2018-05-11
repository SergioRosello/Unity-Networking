using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestData : NetworkData {
    public int chestId;
    public int playerId;

    public ChestData() {
        type = OnlineGameManager.CHEST_REQUEST;
    }
}
