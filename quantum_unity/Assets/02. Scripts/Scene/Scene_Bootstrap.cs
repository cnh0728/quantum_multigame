using UnityEngine;

public class Scene_Bootstrap : MonoBehaviour
{
    private void Start()
    {
        Managers.Scene.LoadScene(Define.Scene.Login);
    }
}
