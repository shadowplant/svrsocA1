using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro; //for text mesh pro UI elements

public class PlayFabUserMgtTMP : MonoBehaviour
{
    [SerializeField] TMP_InputField userEmail, userPassword, userName, currentScore, displayName;
    [SerializeField] TextMeshProUGUI Msg, Display, XP, level;
    void UpdateMsg(string msg)
    {
        Debug.Log(msg);
        Msg.text = msg.ToString();
    }

    void UpdateDisplay(string display)
    {
        Debug.Log(display);
        Display.text = display.ToString();
    }


    void UpdateXP(string xp)
    {
        Debug.Log(xp);
        XP.text = xp.ToString();
    }


    void UpdateLevel(string lvl)
    {
        Debug.Log(lvl);
        level.text = lvl.ToString();
    }

    void OnError(PlayFabError e) //report any errors here!
    {
        UpdateMsg("Error" + e.GenerateErrorReport());
    }

    public void OnButtonRegUser()
    { //for button click
        var registerRequest = new RegisterPlayFabUserRequest
        {
            Email = userEmail.text,
            Password = userPassword.text,
            Username = userName.text
        };
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegSuccess, OnError);
    }

    void OnRegSuccess(RegisterPlayFabUserResult r)
    {
        UpdateMsg("Registration success!");

        //To create a player display name 
        var req = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = displayName.text,
        };
        // update to profile
        PlayFabClientAPI.UpdateUserTitleDisplayName(req, OnDisplayNameUpdate, OnError);
    }

    void OnDisplayNameUpdate(UpdateUserTitleDisplayNameResult r)
    {
        UpdateMsg("display name updated!" + r.DisplayName);
    }

    private void OnGetPlayerProfileSuccess(GetPlayerProfileResult r)
    {
        // Retrieve the player's display name from the result
        string displayName = r.PlayerProfile.DisplayName;

        // Update the UI text with the player's display name
        Msg.text = displayName;

    }

    void OnLoginSuccess(LoginResult r)
    {
        UpdateMsg("Login Success!"+r.PlayFabId+r.InfoResultPayload.PlayerProfile.DisplayName);
        ClientGetTitleData(); //MOTD
        //OnButtonSetUserData();
        GetUserData(); //Player Data
        UnityEngine.SceneManagement.SceneManager.LoadScene("Landing");
    }
    public void OnButtonLoginEmail() //login using email + password

    {
        string checkemail = userEmail.text;
        var loginRequest = new LoginWithEmailAddressRequest
        {
            Email = userEmail.text,
            Password = userPassword.text,
        //to get player profile, to get displayname
        InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };
        PlayFabClientAPI.LoginWithEmailAddress(loginRequest, OnLoginSuccess, OnError);
    }
    public void OnButtonLoginUserName() //login using username + password
    {
        var loginRequest = new LoginWithPlayFabRequest
        {
            Username = userName.text,
            Password = userPassword.text,
            //to get player profile, including displayname
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };
        PlayFabClientAPI.LoginWithPlayFab(loginRequest, OnLoginSuccess, OnError);
    }


    public void OnButtonLogout()
    {
        PlayFabClientAPI.ForgetAllCredentials();
        UpdateMsg("logged out");
        GotoLoginScene();
    }

    public void PasswordResetRequest()
    {
        var req = new SendAccountRecoveryEmailRequest
        {
            Email = userEmail.text,
            TitleId = "C7000"
        };
        PlayFabClientAPI.SendAccountRecoveryEmail(req, OnPasswordReset, OnError);
    }

    void OnPasswordReset(SendAccountRecoveryEmailResult r)
    {
        Msg.text = "Password reset email sent.";
    }

    public void OnButtonGetLeaderboard()
    {
        var lbreq = new GetLeaderboardRequest
        {
            StatisticName = "highscore", //playfab leaderboard statistic name
            StartPosition = 0,
            MaxResultsCount = 10
        };
        PlayFabClientAPI.GetLeaderboard(lbreq, OnLeaderboardGet, OnError);
    }
    void OnLeaderboardGet(GetLeaderboardResult r)
    {
        string LeaderboardStr = "Leaderboard\n";
        //foreach (Transform item in rowsParent)
        //{
        //    Destroy(item.gameObject);
        //}
        foreach (var item in r.Leaderboard)
        {
            //GameObject newGO = Instantiate(rowPrefab, rowsParent);
            //Text[] texts = newGO.GetComponentsInChildren<Text>();
            //texts[0].text = (item.Position + 1).ToString();
            //texts[1].text = item.DisplayName;
            //texts[2].text = item.StatValue.ToString();
            string onerow = (item.Position + 1) + "/       " + item.DisplayName + "/        " + item.StatValue + "\n";
            LeaderboardStr += onerow; //combine all display into one string 1.
            //Debug.Log(string.Format("Place: {0} | ID: {1} | Score: {2}", item.Position, item.DisplayName, item.StatValue));
        }
        UpdateMsg(LeaderboardStr);
    }
    public void OnButtonSendLeaderboard()
    {
        var req = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>{ //playfab leaderboard statistic name
                new StatisticUpdate{
                    StatisticName="highscore",
                    Value=int.Parse(currentScore.text)
                }
            }
        };
        UpdateMsg("Submitting score:" + currentScore.text);
        PlayFabClientAPI.UpdatePlayerStatistics(req, OnLeaderboardUpdate, OnError);
    }
    void OnLeaderboardUpdate(UpdatePlayerStatisticsResult r)
    {
        UpdateMsg("Successful leaderboard sent:" + r.ToString());
    }
    public void ClientGetTitleData() { //Slide 33: MOTD
        PlayFabClientAPI.GetTitleData(new GetTitleDataRequest(),
            result => {
                if(result.Data == null || !result.Data.ContainsKey("MOTD")) UpdateMsg("No MOTD");
                else UpdateMsg("MOTD: "+result.Data["MOTD"]);
            },
            error => {
                UpdateMsg("Got error getting titleData:");
                UpdateMsg(error.GenerateErrorReport());
            }
        );
    }
    public void OnButtonSetUserData()
    { //Player Data
        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>() {
                {"XP", XP.text.ToString()},
                {"Level", level.text.ToString()}
            }
        },
        result => UpdateMsg("Successfully updated user data"),
        error => {
            UpdateMsg("Error setting user data");
            UpdateMsg(error.GenerateErrorReport());
        });
    }
    public void GetUserData()
    { //Player Data
        PlayFabClientAPI.GetUserData(new GetUserDataRequest()
        , result => {
            UpdateMsg("Got user data:");
            if (result.Data == null || !result.Data.ContainsKey("XP")) UpdateXP("No XP");
            else UpdateXP(result.Data["XP"].Value);
            if (result.Data == null || !result.Data.ContainsKey("Level")) UpdateLevel("No Level");
            else UpdateLevel(result.Data["Level"].Value);
        }, (error) => {
            UpdateMsg("Got error retrieving user data:");
            UpdateMsg(error.GenerateErrorReport());
        });
    }

    public void GoToSkillScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("SkillScene");
    }

    public void GotoGameScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Landing");
    }

    public void GotoInventoryScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Inventory");
    }

    public void GotoLoginScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
    }

    public void GotoRegisterScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("RegisterScene");
    }
}

