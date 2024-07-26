using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager
{
    public SceneUI_Base CurrentSceneUI { get { return GameObject.FindObjectOfType<SceneUI_Base>(); } }
}