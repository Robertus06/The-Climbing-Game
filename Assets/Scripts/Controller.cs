using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Controller : MonoBehaviour
{
    private GameObject soporte;
    private GameObject pelota;
    private GameObject enemigos;
    private GameObject interfazInGame;
    private GameObject interfazMuerte;
    private GameObject interfazInicio;
    private GameObject interfazFirstTime;
    private GameObject decoracion;
    [SerializeField] private GameObject Enemigo;

    private AudioPeer audioMicro;

    private Rigidbody rbPelota;
    private string _selectedDevice;

    public bool playing;
    private int score;
    public int highScore;
    private string scoreText;
    private string highScoreText;

    private SaveAndLoad saveGame;

    private Vector3 SpawnPosition;
    private bool generando;
    private bool permiteGenerar;
    private bool dontTouch;
    
    // Start is called before the first frame update
    void Start()
    {
        pelota = GameObject.Find("Pelota");
        soporte = GameObject.Find("Soporte");
        enemigos = GameObject.Find("Enemigos");

        interfazInGame = GameObject.Find("Interfaz in-Game");
        interfazMuerte = GameObject.Find("Interfaz Muerte");
        interfazInicio = GameObject.Find("Interfaz Inicial");
        interfazFirstTime = GameObject.Find("Interfaz FirstTime");
        decoracion = GameObject.Find("Decoracion");

        rbPelota = pelota.GetComponent<Rigidbody>();
        
        interfazMuerte.SetActive(false);
        interfazInGame.SetActive(false);
        interfazFirstTime.SetActive(false);
        decoracion.SetActive(false);
        soporte.SetActive(false);
        pelota.SetActive(false);
        enemigos.SetActive(false);
        
        playing = false;
        score = 0;
        scoreText = "";
        highScoreText = "";

        saveGame = FindObjectOfType<SaveAndLoad>();
        audioMicro = FindObjectOfType<AudioPeer>();

        generando = false;
        permiteGenerar = false;
        dontTouch = false;
        
        FormatearHighScore();
        interfazInicio.transform.GetChild(7).GetComponent<TextMeshProUGUI>().text = highScoreText;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (playing)
        {
            /**
            if (interfazInGame.GetComponentInChildren<Slider>().value < 100 && Input.touches.Length == 0)
            {
                interfazInGame.GetComponentInChildren<Slider>().value += 0.5f;
                dontTouch = true;
            }
            /**/

            /**/
            if (audioMicro._AmplitudeBuffer > 0.5f && interfazInGame.GetComponentInChildren<Slider>().value < 100 && Input.touches.Length == 0)
            {
                interfazInGame.GetComponentInChildren<Slider>().value += 0.5f;
                dontTouch = true;
            }
            /**/
            
            if (Input.touches.Length > 0 && interfazInGame.GetComponentInChildren<Slider>().value > 0)
            {
                if (dontTouch)
                {
                    dontTouch = false;
                    StartCoroutine(EsperaGenerarCoroutine());
                }
                interfazInGame.GetComponentInChildren<Slider>().value -= 0.1f;
                score++;
                FormatearScore();
                interfazInGame.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = scoreText;

                if (!generando && permiteGenerar)
                    StartCoroutine(GenerarCoroutine());

                enemigos.transform.position -= new Vector3(0, 0.01f, 0);
            }
        }
    }

    IEnumerator EsperaGenerarCoroutine()
    {
        permiteGenerar = false;
        yield return new WaitForSeconds(3);
        permiteGenerar = true;
    }
    
    void GenerarEnemigo()
    {
        SpawnPosition = this.transform.position + Random.onUnitSphere * 2;
        SpawnPosition = new Vector3(SpawnPosition.x, SpawnPosition.y, -0.032f);
        GameObject agujero = Instantiate(Enemigo, SpawnPosition, Quaternion.Euler(90,0,0));
        agujero.transform.SetParent(enemigos.transform);
    }

    IEnumerator GenerarCoroutine()
    {
        generando = true;
        GenerarEnemigo();
        yield return new WaitForSeconds(3);
        generando = false;
    }

    public void GameOverScene()
    {
        foreach (Transform child in enemigos.transform)
        {
            Destroy(child.gameObject);
        }
        soporte.SetActive(false);
        pelota.SetActive(false);
        enemigos.SetActive(false);
        interfazInGame.SetActive(false);
        interfazMuerte.SetActive(true);
        playing = false;
        if (score > highScore)
            highScore = score;
        FormatearScore();
        FormatearHighScore();
        interfazMuerte.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = scoreText;
        interfazMuerte.transform.GetChild(7).GetComponent<TextMeshProUGUI>().text = highScoreText;
        saveGame.SaveData(highScore);
    }

    public void InicioScene()
    {
        interfazMuerte.SetActive(false);
        interfazInicio.SetActive(true);
        FormatearHighScore();
        interfazInicio.transform.GetChild(7).GetComponent<TextMeshProUGUI>().text = highScoreText;
    }

    public void GameScene()
    {
        if (saveGame.firstTime == 0)
        {
            saveGame.SaveFirstTime();
            interfazInicio.SetActive(false);
            interfazFirstTime.SetActive(true);
            decoracion.SetActive(true);
            StartCoroutine(SalirDelJuegoCoroutine());
        }
        else
        {
            interfazInicio.SetActive(false);
            interfazMuerte.SetActive(false);
            interfazInGame.SetActive(true);
            interfazInGame.GetComponentInChildren<Slider>().value = 0;
            soporte.transform.rotation = Quaternion.Euler(0, 0, 0);
            soporte.SetActive(true);
            pelota.transform.rotation = Quaternion.Euler(0, 0, 0);
            pelota.transform.position = new Vector3(0, -3.2f, -0.287f);
            rbPelota.velocity = new Vector3(0, 0, 0);
            rbPelota.angularVelocity = new Vector3(0, 0, 0);
            pelota.SetActive(true);
            enemigos.SetActive(true);
            GenerarEnemigo();
            playing = true;
            score = 0;
            interfazInGame.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "0000";
            FormatearHighScore();
            interfazInGame.transform.GetChild(6).GetComponent<TextMeshProUGUI>().text = highScoreText;
        }
    }

    IEnumerator SalirDelJuegoCoroutine()
    {
        yield return new WaitForSeconds(5);
        Application.Quit();
    }

    void FormatearScore()
    {
        if (score >= 0 && score < 10)
        {
            scoreText = "000" + score;
        }
        else if (score >= 10 && score < 100)
        {
            scoreText = "00" + score;
        }
        else if (score >= 100 && score < 1000)
        {
            scoreText = "0" + score;
        }
        else if (score >= 1000)
        {
            scoreText = "" + score;
        }
    }
    
    void FormatearHighScore()
    {
        if (highScore >= 0 && highScore < 10)
        {
            highScoreText = "000" + highScore;
        }
        else if (highScore >= 10 && highScore < 100)
        {
            highScoreText = "00" + highScore;
        }
        else if (highScore >= 100 && highScore < 1000)
        {
            highScoreText = "0" + highScore;
        }
        else if (highScore >= 1000)
        {
            highScoreText = "" + highScore;
        }
    }
}
