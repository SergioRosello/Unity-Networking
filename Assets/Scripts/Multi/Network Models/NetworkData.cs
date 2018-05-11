using System;
using UnityEngine;

[Serializable]
public abstract class NetworkData {
    public string type;

    public string ToJson() {
        return JsonUtility.ToJson(this);
    }
}
