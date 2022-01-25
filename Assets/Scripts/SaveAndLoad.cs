using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveAndLoad : MonoBehaviour
{
    private Controller controller;
    public int firstTime;

    // Start is called before the first frame update
    void Awake()
    {
        controller = FindObjectOfType<Controller>();
        
        /* Resetear las stats del juego a 0 */
        /**
        SaveData(0);
        SaveFirstTime();
        /**/
        
        LoadData();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadData()
    {
        /**
        if (PlayerPrefs.HasKey("highScore"))
            controller.highScore = PlayerPrefs.GetInt("highScore");
        else
            controller.highScore = 0;
        /**/

        controller.highScore = PlayerPrefs.GetInt("highScore", 0);
        firstTime = PlayerPrefs.GetInt("firstTime", 0);
    }

    public void SaveData(int highScore)
    {
        PlayerPrefs.SetInt("highScore", highScore);
    }

    public void SaveFirstTime()
    {
        PlayerPrefs.SetInt("firstTime", 1);
    }
}
