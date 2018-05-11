using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour {
    protected InputField _inputField;

    void Awake() {
        _inputField = GetComponentInChildren<InputField>();
        _inputField.text = PlayerPrefs.GetString("playerName");
    }

	public void LoadScene(int index) {
        if (_inputField.text.Length > 0) {
            PlayerPrefs.SetString("playerName", _inputField.text);
            SceneManager.LoadScene(index);
        }
	}
}