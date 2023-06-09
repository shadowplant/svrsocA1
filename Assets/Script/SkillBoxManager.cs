using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;


public class SkillBoxManager:MonoBehaviour
{
    [SerializeField] SkillBox[] SkillBox;
    public void SendJson(){
        List<Skill> skillList = new List<Skill>();
        foreach (var item in SkillBox) skillList.Add(item.ReturnClass());
        string stringListAsJson = JsonUtility.ToJson(new JSListWrapper<Skill>(skillList));
        Debug.Log("Json data prepared:" +stringListAsJson);
        var req = new UpdateUserDataRequest{
            Data = new Dictionary<string, string>{
                {"Skills", stringListAsJson}
            }
        };
        PlayFabClientAPI.UpdateUserData(req,result =>Debug.Log("Data sent success!"), OnError);
    }
    public void LoadJson(){
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnJSONDataReceived, OnError);
    }

    void OnJSONDataReceived(GetUserDataResult r){
        Debug.Log("received JSON data");
        if (r.Data!=null&&r.Data.ContainsKey("Skills")){
            Debug.Log(r.Data["Skills"].Value);
            JSListWrapper<Skill> jlw = JsonUtility.FromJson<JSListWrapper<Skill>>(r.Data["Skills"].Value);
            for (int i = 0; i < SkillBox.Length; i++){
                SkillBox[i].SetUI(jlw.list[i]);
            }
        }
    }

    void OnError(PlayFabError e){
        Debug.Log("Error" + e.GenerateErrorReport());
    }

    public void BacktoMainScene(){
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
    }
}

[System.Serializable]
public class JSListWrapper<T>
{
    public List<T> list;
    public JSListWrapper(List<T> list) => this.list = list;
}

