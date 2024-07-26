using UnityEngine;

public class Scene_Base : MonoBehaviour
{
    public Define.Scene SceneType { get; protected set; } = Define.Scene.Unknown;
}
