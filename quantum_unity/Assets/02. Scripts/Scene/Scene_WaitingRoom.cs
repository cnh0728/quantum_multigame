using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene_WaitingRoom : MonoBehaviour
{
    [SerializeField] SceneUI_WaitingRoom _sceneUI;

    private void Start()
    {
        // WaitingRoom 캐릭터 생성.
        //AddressableManager.LoadAssetAsync("Character_WaitingRoom", (prefab) =>
        //{
        //    NetworkObject no = Managers.Network.Runner.Spawn(prefab, Vector3.zero, Quaternion.identity);

        //    // 바인딩.
        //    Managers.Network.PlayerController.ControlledCharacter = no;
        //}).Forget();
    }
}
