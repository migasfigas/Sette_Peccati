﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class Main : MonoBehaviour
{

    #region GUI
    [SerializeField]
    private GameObject canvas;
    private GameObject HUD, pauseUI;
    private GameObject questUI;
    private RectTransform temptationUI;

    private GameObject loadingBackground;
    private GameObject loadingText;
    private GameObject loadingImage;
    private int loadProgress = 0;

    private GameObject diedUI;

    private GameObject optionsMenu;
    private GameObject pauseMenu;

    private GameObject inventoryIcon;
    private GameObject inventoryDropdown;

    private float sizeX = 0;
    #endregion

    #region Quests
    public enum QuestType
    {
        none,
        lamp,
        hallway,
        platforms
    };

    public enum NPCs
    {
        none,
        meMyselfAndI,
        oldMan
    };

    [SerializeField]
    private QuestType activeQuest;
    private Quest lampQuest, hallwayQuest, platformQuest;

    [SerializeField]
    private bool chatting;
    #endregion

    #region Player stats
    [SerializeField]
    private int temptation;
    public bool pause = false;
    public bool playerAttacking = false;
    private bool died = false;
    #endregion

    [SerializeField]
    private int currentLevel;

    void Awake()
    {
        //o main nunca é destruido, passa de cena para cena
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        Time.timeScale = 1;

        canvas = transform.FindChild("Canvas").gameObject;

        HUD = canvas.transform.FindChild("HUD").gameObject;
        inventoryIcon = HUD.transform.FindChild("inventory").gameObject;
        inventoryDropdown = inventoryIcon.transform.FindChild("Dropdown").gameObject;
        diedUI = canvas.transform.FindChild("death").gameObject;

        pauseUI = canvas.transform.FindChild("pause").gameObject;
        optionsMenu = pauseUI.transform.FindChild("options screen").gameObject;
        pauseMenu = pauseUI.transform.FindChild("menu screen").gameObject;

        questUI = HUD.transform.FindChild("quest").gameObject;
        temptationUI = HUD.transform.FindChild("temptation").GetComponent<Image>().transform.FindChild("bar").GetComponent<RectTransform>();

        loadingBackground = canvas.transform.Find("loading screen/background").gameObject;
        loadingText = canvas.transform.Find("loading screen/loading text").gameObject;
        loadingImage = canvas.transform.Find("loading screen/loading image").gameObject;

        canvas.transform.FindChild("interact text").gameObject.SetActive(false);
        inventoryDropdown.SetActive(false);


        lampQuest = new Quest(this, QuestType.lamp, questUI);

        sizeX = temptationUI.sizeDelta.x;
        activeQuest = QuestType.none;
        pause = false;
        playerAttacking = false;

        Cursor.visible = false;

        if (currentLevel == 2)
        {
            lampQuest.Done = true;
            hallwayQuest = new Quest(this, QuestType.hallway, questUI);
        }
        else if (currentLevel == 3)
        {
            lampQuest.Done = true;
            platformQuest = new Quest(this, QuestType.platforms, questUI);
        }

        else if(currentLevel == 4)
        {
            lampQuest.Done = true;
        }
    }

    //é chamado quando um novo nivel é carregado (!= start)
    void OnLevelWasLoaded(int level)
    {
        Time.timeScale = 1;

        if (loadingBackground != null)
        {
            loadingBackground.SetActive(false);
            loadingText.SetActive(false);
            loadingImage.SetActive(false);
        }

        SaveGame();

        currentLevel = level;

        chatting = false;

        switch (level)
        {
            case 0:
                loadingBackground.SetActive(false);
                loadingText.SetActive(false);
                DestroyObject(gameObject);
                break;

            case 1:
                Start();
                break;

            case 2:
                lampQuest.Done = true;
                hallwayQuest = new Quest(this, QuestType.hallway, questUI);
                gameObject.AddComponent<StatuePuzzle>();
                break;

            case 3:
                DestroyObject(gameObject.GetComponent<StatuePuzzle>());
                hallwayQuest = new Quest(this, QuestType.hallway, questUI);
                GameObject.Find("player/FirstPersonCharacter").GetComponent<ShakingPlatform>().enabled = true;
                break;

            case 4:
                break;

            default:
                break;
        }
    }

    void Update()
    {
        setUI();
        getInput();

        if (temptation >= 100)
            PriestDie();

        if (Input.GetKey(KeyCode.X))
            temptation++;
    }

    private void setUI()
    {
        temptationUI.sizeDelta = new Vector2(sizeX * temptation / 100, temptationUI.sizeDelta.y);

        switch (activeQuest)
        {
            case QuestType.lamp:
                lampQuest.setGUI();
                break;

            case QuestType.hallway:
                break;

            default:
                break;
        }
    }

    //for fast debugging
    private void getInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && 
            ((pause && pauseUI.GetComponent<CanvasGroup>().alpha == 1) || (!pause && pauseUI.GetComponent<CanvasGroup>().alpha == 0)))
        {
            pause = !pause;

            if (pause) onPauseGame();
            else onResumeGame();
        }

        if(Input.GetKeyDown(KeyCode.I))
        {
            inventoryDropdown.SetActive(!inventoryDropdown.activeSelf);

            if (lampQuest != null && lampQuest.Done)
                inventoryDropdown.GetComponent<Text>().text = "lamp\n";

            if (hallwayQuest != null && hallwayQuest.Done)
                inventoryDropdown.GetComponent<Text>().text += "key\n";
        }

        if(died && Input.GetKeyDown(KeyCode.Return))
        {
            StartCoroutine(DisplayLoadingScreen(currentLevel));
            DestroyObject(gameObject);
        }

        #region For Debugging Purposes

        //if (Input.GetKeyDown(KeyCode.PageUp))
        //{
        //    if (currentLevel + 1 < 4)
        //    {
        //        StartCoroutine(DisplayLoadingScreen(currentLevel + 1));
        //        DestroyObject(gameObject);

        //        if (currentLevel + 1 != 1) Instantiate(Resources.Load("/_prefabs/_for all levels/main"));
        //    }
        //}
        
        //if (Input.GetKeyDown(KeyCode.PageDown))
        //{
        //    if (currentLevel - 1 > -1)
        //    {
        //        StartCoroutine(DisplayLoadingScreen(currentLevel - 1));
        //        DestroyObject(gameObject);

        //        if(currentLevel - 1 != 1) Instantiate(Resources.Load("/_prefabs/_for all levels/main"));
        //    }
        //}

        //if (Input.GetKeyDown(KeyCode.Delete))
        //{
        //    StartCoroutine(DisplayLoadingScreen(0));
        //    DestroyObject(gameObject);
        //}

        //if (Input.GetKeyDown(KeyCode.PageUp))
        //{
        //    StartCoroutine(DisplayLoadingScreen(currentLevel + 1));
        //}

        #endregion
    }

    IEnumerator DisplayLoadingScreen(int level)
    {
        loadingBackground.SetActive(true);
        loadingText.SetActive(true);

        loadingText.GetComponent<Text>().text = "Loading " + loadProgress + "%";

        AsyncOperation async = SceneManager.LoadSceneAsync(level);

        while (!async.isDone)
        {
            loadProgress = (int)(async.progress * 100);
            loadingText.GetComponent<Text>().text = "Loading " + loadProgress + "%";

            yield return null;
        }

        loadingBackground.SetActive(false);
        loadingText.SetActive(false);
        loadProgress = 0;

        currentLevel = level;
    }

    private void onPauseGame()
    {
        Time.timeScale = 0;

        AudioListener.pause = true;

        StartCoroutine(Fade(HUD, -0.05f, false));
        StartCoroutine(Fade(pauseUI, +0.05f, false));
        StartCoroutine(Fade(pauseMenu, +0.05f, true));
        StartCoroutine(Fade(optionsMenu, -0.05f, true));

        Cursor.visible = true;
    }

    public void onResumeGame()
    {
        Time.timeScale = 1;
        pause = false;

        AudioListener.pause = false;

        StartCoroutine(Fade(pauseUI, -0.05f, false));
        StartCoroutine(Fade(pauseMenu, -0.05f, true));
        StartCoroutine(Fade(optionsMenu, -0.05f, true));
        StartCoroutine(Fade(HUD, +0.05f, false));
        
        Cursor.visible = false;
    }

    public void onOptionsGame()
    {
        StartCoroutine(Fade(pauseMenu, -0.05f, true));
        StartCoroutine(Fade(optionsMenu, +0.05f, true));
    }

    public void onOptionsBack()
    {
        StartCoroutine(Fade(pauseMenu, 0.05f, true));
        StartCoroutine(Fade(optionsMenu, -0.05f, true));
    }

    public void exit()
    {
        StartCoroutine(DisplayLoadingScreen(0));
        DestroyObject(gameObject);
    }

    private void PriestDie()
    {
        Time.timeScale = 0;
        died = true;
        StartCoroutine(Fade(diedUI, 0.05f, false));
    }

    IEnumerator Fade(GameObject group, float incrementation, bool disable)
    {
        bool fade = true;

        if (disable)
        {
            foreach (Transform t in group.transform)
            {
                if (incrementation > 0)
                    t.gameObject.SetActive(true);
                else
                    t.gameObject.SetActive(false);
            }
        }

        while (fade)
        {
            group.GetComponent<CanvasGroup>().alpha += incrementation;

            if (group.GetComponent<CanvasGroup>().alpha <= 0 || group.GetComponent<CanvasGroup>().alpha >= 1)
            {
                fade = false;
            }

            yield return null;
        }
    }

    private void SaveGame()
    {
        StreamWriter sw = new StreamWriter("savegame.txt");
        sw.WriteLine(currentLevel);

        sw.Close();
    }

    #region Getters & Setters

    public int Temptation
    {
        get { return temptation; }
        set { temptation = value; }
    }

    public GameObject Canvas
    {
        get { return canvas; }
    }

    public bool Chatting
    {
        get { return chatting; }
        set { chatting = value; }
    }

    public QuestType ActiveQuest
    {
        get { return activeQuest; }
        set { activeQuest = value; }
    }

    public Quest LampQuest { get { return lampQuest; } }
    public Quest HallwayQuest { get { return hallwayQuest; } }

    public int CurrentLevel { get { return currentLevel; } }
    #endregion
}
