using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx
{
    public Scene_Base CurrentScene { get { return GameObject.FindObjectOfType<Scene_Base>(); } }

    public int GetBuildIndex(Define.Scene type)
    {
        return (int)type;
    }

    public void LoadScene(Define.Scene type)
    {
        SceneManager.LoadScene(GetBuildIndex(type));
    }
}
