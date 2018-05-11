using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class Extensions {
	public static Bounds OrthographicBounds(this Camera camera) {
		var t = camera.transform;
		var size = camera.orthographicSize * 2;

		var w = size * (float)Screen.width / Screen.height;
		var h = size;

		return new Bounds (t.position, new Vector3(w, h, 0));
	}

	public static bool Contains(this Bounds b1, Bounds b2) {
		return b1.Contains (b2.min) && b1.Contains (b2.max);
	}

    public static T[] Sub<T>(this T[] source, int pos, int length) {
        var auxArray = new T[length];
        System.Array.ConstrainedCopy(source, pos, auxArray, 0, length);
        return auxArray;
    }

    public static string sha256(this string randomString) {
        var crypt = new System.Security.Cryptography.SHA256Managed();
        var hash = new System.Text.StringBuilder();
        byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(randomString));
        foreach (byte theByte in crypto) {
            hash.Append(theByte.ToString("x2"));
        }
        return hash.ToString();
    }
}
