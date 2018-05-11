using System;

public class InitialData : NetworkData {
    public string playerName;

    public InitialData() {
        type = OnlineGameManager.INITIAL_REQUEST;
    }
}
