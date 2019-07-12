using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class luckyDiceScript : MonoBehaviour 
{
	public KMBombInfo bomb;
	public KMAudio Audio;

	//Logging
	static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

	void Awake()
	{
		moduleId = moduleIdCounter++;
		//btn.OnInteract += delegate () { PressButton(); return false; };
	}

	void Start () 
	{
		
	}
	
	void Update () 
	{
		
	}
}
