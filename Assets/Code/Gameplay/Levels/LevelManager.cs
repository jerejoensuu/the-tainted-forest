using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TaintedForest;

public class LevelManager : MonoBehaviour {

    public List<GameObject> bubblesRemaining = new List<GameObject>();

    private bool levelWon = false;
    private bool levelLost = false;
    [Range(1, 2)] [SerializeField] public int theme = 1;
    [Range(1, 3)] [SerializeField] int taintLevel = 1;
    [Range(10, 180)] [SerializeField] [Tooltip("In seconds")] public int time = 90;
    public GameObject blackScreen;
    
    void Awake() {

        StartCoroutine(Transition());
        int bubbleCount = 0;
        foreach (Transform child in transform) {

            if (child.tag == "Ball") {
                bubbleCount += (int)Mathf.Pow(child.GetComponent<BallController>().size, 2);
            }
            
        }

        transform.Find("PlatformAndDropManager").GetComponent<DropManager>().ApplyTheme(theme);
        ApplyBackground();

        transform.Find("Player").GetComponent<PlayerController>().ammoCount = (int)(bubbleCount * 1.2f);


    }

    public void CheckRemainingBubbles() {
        StartCoroutine(ICheckRemainingBubbles());
    }

    IEnumerator ICheckRemainingBubbles() { // Delay the check by small amount because spawning seems to be slow!
        float delay = 0.5f;

        yield return new WaitForSeconds(delay);

        if (bubblesRemaining.Count == 0) {
            LevelWin();
        }
    }

    public int CountVines() { 
        int bubbleCount = 0;
        foreach (Transform child in transform) {
            if (child.tag == "Vine") {
                bubbleCount++;
            }
        }
        return bubbleCount;
    }

    public bool SubmitScore() {
        Score scores = new Score(GameData.GetFilePath());
        if (scores.Add(int.Parse(SceneManager.GetActiveScene().name) - 1, GameObject.Find("PlayerUI").GetComponent<PlayerUI>().GetScore())) {
            scores.Save();
            return true;
        }
        return false;
    }

    public bool FindDrops(GameObject drop) {
        foreach (Transform child in transform) {
            if (child.tag == drop.tag) {
                return true;
            }
        }
        return false;
    }

    public void LevelWin() {
        if (!levelWon && !levelLost) {
            levelWon = true;
            SubmitScore();
            GameObject.Find("UIController").GetComponent<UIController>().LevelWin();
        }
    }

    public void LevelLose() {
        if (!levelWon && !levelLost) {
            levelLost = true;
            GameObject.Find("UIController").GetComponent<UIController>().LevelLose();
        }
    }

    public IEnumerator FreezeBubbles() {

        for (float i = 0; i < 5; i += 0.01f) {
            foreach (Transform child in transform) {
                if (child.tag == "Ball") {
                    child.GetComponent<BallController>().Freeze();
                }
            }
            yield return new WaitForSeconds(0.01f);
        }

        foreach (Transform child in transform) {
            if (child.tag == "Ball") {
                child.GetComponent<BallController>().UnFreeze();
            }
        }
    }

    public void DamageAllBubbles() {
        List<GameObject> bubbles = new List<GameObject>();
        foreach (Transform child in transform) {
            if (child.tag == "Ball") {
                bubbles.Add(child.gameObject);
            }
        }
        foreach (GameObject bubble in bubbles) {
            bubble.GetComponent<BallController>().DestroyBall();
        }
    }

    public void DestroyUnmovingVines() {
        foreach (Transform child in transform) {
            if (child.tag == "Vine" && !child.GetComponent<Grapple>().moving) {
                Destroy(child.gameObject);
            }
        }
    }

    private void ApplyBackground() {
        transform.Find("Backgrounds").GetChild(theme-1).gameObject.SetActive(false);
        transform.Find("Backgrounds").GetChild(0).GetChild(0).gameObject.SetActive(false);

        transform.Find("Backgrounds").GetChild(theme-1).gameObject.SetActive(true);
        transform.Find("Backgrounds").GetChild(theme-1).GetChild(taintLevel-1).gameObject.SetActive(true);
        transform.Find("Backgrounds").GetChild(theme-1).GetChild(taintLevel-1).GetComponent<AudioSource>().volume = ApplicationSettings.MusicVolume() * 0.5f;
    }

    public void ToggleMusic(bool musicOn) {
        if (musicOn) {
            transform.Find("Backgrounds").GetChild(theme-1).GetChild(taintLevel-1).GetComponent<AudioSource>().UnPause();
        } else {
            transform.Find("Backgrounds").GetChild(theme-1).GetChild(taintLevel-1).GetComponent<AudioSource>().Pause();
        }
    }

    public void SetMusicVolume(float volume) {
        transform.Find("Backgrounds").GetChild(theme-1).GetChild(taintLevel-1).GetComponent<AudioSource>().volume = volume;
    }

    public float GetMusicVolume() {
        return transform.Find("Backgrounds").GetChild(theme-1).GetChild(taintLevel-1).GetComponent<AudioSource>().volume;
    }

    IEnumerator Transition() {
        GameObject transitionScreen = Instantiate(blackScreen, Vector3.zero, Quaternion.identity) as GameObject;

        Destroy(transitionScreen.transform.GetChild(0).gameObject);
        float opacity = 1f;
        while (true) {
            if (opacity <= 0) {
                yield return null;
            } else {
                opacity -= Time.unscaledDeltaTime;
                Color c = transitionScreen.GetComponent<SpriteRenderer>().color;
                transitionScreen.GetComponent<SpriteRenderer>().color = new Color(c.r, c.g, c.b, opacity);
                
            }
            yield return null;
        }
    }
}
