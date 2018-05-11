using System;

public class NetworkRequest {
    public byte[] data;
    public DateTime timeStamp;
    public int id;

    public NetworkRequest(int id, byte[] data) {
        this.id = id;
        this.data = data;
        timeStamp = DateTime.Now;
    }
}
