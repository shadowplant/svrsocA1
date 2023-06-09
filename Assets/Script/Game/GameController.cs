using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameController : MonoBehaviour
{
    public static GameController instance = null;
    public GameObject gameOverObject;
    
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI finalscoreText;
    public TextMeshProUGUI Msg;

    public int difficultyMax = 5;

    [HideInInspector]
    public bool isGameOver = false;
    public float scrollSpeed = -2.5f;

    public int columnScore = 1;
    private int score = 0;
    private int highestScore = 0;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }

        LoadHighScore();
        scoreText.text = "0";
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void UpdateMsg(string msg)
    {
        Debug.Log(msg);
        Msg.text = msg.ToString();
    }

    void OnError(PlayFabError e) //report any errors here!
    {
        UpdateMsg("Error" + e.GenerateErrorReport());
    }

    public void ResetGame()
    {
        //Reset the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToLanding()
    {
        SceneManager.LoadScene("Landing");
    }

    public void GameOver()
    {
        gameOverObject.SetActive(true);
        scoreText.gameObject.SetActive(false);
        highScoreText.gameObject.SetActive(false);
        isGameOver = true;
    }

    public void Scored(int value)
    {
        //Check if it is game over
        if (isGameOver)
        return;

        score += value;
        scoreText.text = score.ToString();
        finalscoreText.text = score.ToString();
        var request = new AddUserVirtualCurrencyRequest
        {
            VirtualCurrency = "GD",
            Amount = value

        };

        var req = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            { //playfab leaderboard statistic name
                new StatisticUpdate
                {
                    StatisticName="highscore",
                    Value = score
                }
            }
        };
        UpdateMsg("Submitting score:" + highestScore);
        PlayFabClientAPI.UpdatePlayerStatistics(req, OnLeaderboardUpdate, OnError);

        PlayFabClientAPI.AddUserVirtualCurrency(request, OnCurrencyAdded, OnError);
        if (score >= highestScore)
        {
            SaveHighScore(score);
        }
    }

    private void OnCurrencyAdded(ModifyUserVirtualCurrencyResult result)
    {
        // Currency added successfully, handle the result
        Debug.Log("Currency added successfully");
    }

    private void SaveHighScore(int score)
    {
        highestScore = score;
        PlayerPrefs.SetInt("highestScore", highestScore);
        highScoreText.text = highestScore.ToString();
    }

    void OnLeaderboardUpdate(UpdatePlayerStatisticsResult r)
    {
        UpdateMsg("Successful leaderboard sent:" + r.ToString());
    }

    private void LoadHighScore()
    {
        if(PlayerPrefs.HasKey("highestScore"))
        {
            highestScore = PlayerPrefs.GetInt("highestScore");
            highScoreText.text = highestScore.ToString();
        }
    }
}
