﻿using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void newGame()
    {
        Application.LoadLevel("_scene_01");
    }

    public void loadGame()
    {

    }

    public void settings()
    {

    }
}
