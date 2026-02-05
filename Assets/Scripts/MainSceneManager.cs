using UnityEngine;

public class MainSceneManager : MonoBehaviour
{
    public AudioClip childhoodBGM;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // ¡Ú [Ãß°¡] ÆÛÁñ ¾À BGM Àç»ý
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM(childhoodBGM, 0.8f); // º¼·ý 0.8
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
