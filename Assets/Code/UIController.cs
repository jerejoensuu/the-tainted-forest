using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

    public bool paused = false;
    public GameObject pauseMenu;
    public GameObject[] pauseMenuPanels;
    public GameObject[] panelActiveButtons;
    public Slider[] volumeSliders;
    [Tooltip("Set selection to this button when level is won")] public GameObject winScreenActiveButton;
    [Tooltip("Set selection to this button when level is lost")] public GameObject loseScreenActiveButton;
    

    public GameObject winScreen;
    public GameObject loseScreen;
    public GameObject endOverlay;

    void Start() {
        //UnpauseGame();
    }

    void Update() {
        if (Input.GetButtonDown("Pause") && transform.parent.Find("Canvas").Find("LevelText").GetComponent<LevelStartTransition>().levelStarted) { // Esc or start button
            TogglePause();
        }
    }

    void TogglePause() {
        if (!paused) {
            PauseGame();
        }
        else {
            UnpauseGame();
        }
    }

    void PauseGame() {
        paused = true;
        pauseMenu.SetActive(true);
        ChangePanel(0);
        GameObject.Find("LevelManager").GetComponent<LevelManager>().ToggleMusic(false);
        Time.timeScale = 0;
    }

    public void UnpauseGame() {
        paused = false;
        pauseMenu.SetActive(false);
        GameObject.Find("LevelManager").GetComponent<LevelManager>().ToggleMusic(true);
        Time.timeScale = 1;
    }

    public void ResumeGame() {
        TogglePause();
    }

    void ChangePanel(int index) {
        for (int i = 0; i < pauseMenuPanels.Length; i++) {
            if (index == i) {
                pauseMenuPanels[i].SetActive(true);
                GetComponent<EventSystem>().SetSelectedGameObject(null);
                GetComponent<EventSystem>().SetSelectedGameObject(panelActiveButtons[i]);
            }
            else {
                pauseMenuPanels[i].SetActive(false);
            }
        }
    }

    public void OpenSettings() {
        ChangePanel(1);
        volumeSliders[0].value = ApplicationSettings.GetMasterVolume();
        volumeSliders[1].value = ApplicationSettings.GetSoundVolume();
        volumeSliders[2].value = ApplicationSettings.GetMusicVolume();
    }

    public void ApplySettings() {
        ApplicationSettings.ChangeVolumeSettings(volumeSliders[0].value, volumeSliders[1].value, volumeSliders[2].value);
    }

    public void ExitSettings() {
        ChangePanel(0);
    }

    public void ReturnToMenu() {
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void LevelWin() {
        paused = true;
        StartCoroutine(ShowEndScreen(true));
    }

    public void LevelLose() {
        paused = true;
        StartCoroutine(ShowEndScreen(false));
    }

    IEnumerator ShowEndScreen(bool hasWon) {
        float change = 0.02f;
        for (float alpha = 0f; alpha < 1; alpha += change) 
        {
            GameObject.Find("LevelManager").GetComponent<LevelManager>().SetMusicVolume(GameObject.Find("LevelManager").GetComponent<LevelManager>().GetMusicVolume() - change);
            Color overlayColor = endOverlay.GetComponent<Image>().color;
            overlayColor.a = alpha;
            endOverlay.GetComponent<Image>().color = overlayColor;
            yield return new WaitForSeconds(0.02f);
        }

        yield return new WaitForSeconds(0.25f);

        if (hasWon) {
            winScreen.SetActive(true);
            Time.timeScale = 0;
            GetComponent<EventSystem>().SetSelectedGameObject(null);
            GetComponent<EventSystem>().SetSelectedGameObject(winScreenActiveButton);
        }
        else {
            loseScreen.SetActive(true);
            Time.timeScale = 0;
            GetComponent<EventSystem>().SetSelectedGameObject(null);
            GetComponent<EventSystem>().SetSelectedGameObject(loseScreenActiveButton);
        }
        yield return null;
    }
}
