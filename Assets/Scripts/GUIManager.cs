using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GUIManager : Singleton<GUIManager> {
	public Text TimerText, TopScoreText;
	public Button MainMenuButton;

    public void GoToMenu() {
        SceneManager.LoadScene(0);
    }

	/*public void UpdateScore (Character source) {
		int index = 0;

		if (source.gameObject.name.Contains ("1")) {
			index = 1;
		} else if (source.gameObject.name.Contains ("2")) {
			index = 2;
		} if (source.gameObject.name.Contains ("3")) {
			index = 3;
		}
		ScoreTexts [index].text = source.gameObject.name + ": " + source.Score;
	}*/
}
