using UnityEngine;

public class PlayerManager
{
    public string Nickname
    {
        get => PlayerPrefs.GetString("nickname", string.Empty);
        set => PlayerPrefs.SetString("nickname", value);
    }
}
