using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class LuckyDice : MonoBehaviour 
{
	public KMBombInfo bomb;
	public KMAudio Audio;

	//Logging
	static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

	public KMSelectable btn;
	public KMSelectable[] diceBtns;
	public GameObject[] dice;
	public GameObject[] rotators;
	public GameObject[] hl;
	public Material[] colors;

	int[] diceVal = new int[3];
	int[] diceColor = new int[3];
	int lucky;
	int lastLuckyRoll;

	Coroutine diceRoll;
	bool animating = false;

	void Awake()
	{
		moduleId = moduleIdCounter++;
		GetComponent<KMBombModule>().OnActivate += Activate;

		btn.OnInteract += delegate () { Roll(); return false; };
		diceBtns[0].OnInteract += delegate () { SelectDice(0); return false; };
		diceBtns[1].OnInteract += delegate () { SelectDice(1); return false; };
		diceBtns[2].OnInteract += delegate () { SelectDice(2); return false; };
	}

	void Activate()
	{
		Roll();
	}

	void Start () 
	{
		SetupDice();
	}
	
	void Roll()
	{
		if(moduleSolved)
			return;

		Audio.PlaySoundAtTransform("roll", transform);

		SetDiceValues();
		
		SetDiceRotation(0, diceVal[0]);
		SetDiceRotation(1, diceVal[1]);
		SetDiceRotation(2, diceVal[2]);

		if(diceRoll != null)
			StopCoroutine(diceRoll);
		diceRoll = StartCoroutine("RollAnim");
	}

	void SetDiceRotation(int diceIndex, int val)
	{
		float diceX = 0;
		float diceY = dice[diceIndex].transform.localEulerAngles.y;
		float diceZ = 0;

		switch(val)
		{
			case 2: diceX = 270f; break;
			case 3: diceZ = 270f; break;
			case 4: diceZ = 90f; break;
			case 5: diceX = 90f; break;
			case 6: diceX = 180f; break;
		}

		dice[diceIndex].transform.localEulerAngles = new Vector3(diceX, diceY, diceZ);
	}

	void SetupDice()
	{
		lastLuckyRoll = -1;

		List<int> used = new List<int>();

		for(int i = 0; i < dice.Length; i++)
		{
			int color;
			do {
				color = rnd.Range(0, colors.Length);
			} while(used.Contains(color));
			diceColor[i] = color;
			used.Add(color);
			dice[i].transform.Find("cube").GetComponentInChildren<Renderer>().material = colors[diceColor[i]];
			if(diceColor[i] == 7)
				dice[i].transform.Find("pips").GetComponentInChildren<Renderer>().material = colors[9];
			else
				dice[i].transform.Find("pips").GetComponentInChildren<Renderer>().material = colors[7];
		}

        Debug.LogFormat("[Lucky Dice #{0}] Dice colors: {1}, {2}, {3}", moduleId, GetColorName(diceColor[0]), GetColorName(diceColor[1]), GetColorName(diceColor[2]));
	
		lucky = rnd.Range(0, 3);

        Debug.LogFormat("[Lucky Dice #{0}] Lucky die is die {1} ({2}).", moduleId, lucky + 1, GetColorName(diceColor[lucky]));
	}

	void SetDiceValues()
	{
		switch(diceColor[lucky])
		{
			case 0:
			{
				diceVal[lucky] = rnd.Range(2, 7);
				for(int i = 0; i < diceVal.Length; i++)
				{
					if(i != lucky)
						diceVal[i] = rnd.Range(1, diceVal[lucky]);
				}
				break;
			}
			case 1:
			{
				do {
					diceVal[lucky] = rnd.Range(1, 7);
				} while (diceVal[lucky] == 3 || diceVal[lucky] == 6);
				for(int i = 0; i < diceVal.Length; i++)
				{
					if(i != lucky)
						diceVal[i] = rnd.Range(1, 7);
				}
				break;
			}
			case 2:
			{
				if(lastLuckyRoll == -1)
				{
					diceVal[lucky] = rnd.Range(1, 7);
				}
				else if(lastLuckyRoll % 2 == 0)
				{
					diceVal[lucky] = rnd.Range(1, 4);
					if(diceVal[lucky] == 2)
						diceVal[lucky] = 5;
				}
				else
				{
					diceVal[lucky] = rnd.Range(4, 7);
					if(diceVal[lucky] == 5)
						diceVal[lucky] = 2;
				}
				for(int i = 0; i < diceVal.Length; i++)
				{
					if(i != lucky)
						diceVal[i] = rnd.Range(1, 7);
				}
				break;
			}
			case 3:
			{
				int min;
				do
				{
					min = 7;
					for(int i = 0; i < diceVal.Length; i++)
					{
						if(i != lucky)
						{
							diceVal[i] = rnd.Range(1, 7);
							if(diceVal[i] < min)
								min = diceVal[i];
						}
					} 
				} while(min == 6);

				diceVal[lucky] = rnd.Range(min + 1, 7);
				
				break;
			}
			case 4:
			{
				if(lastLuckyRoll == -1)
				{
					diceVal[lucky] = rnd.Range(1, 7);
				}
				else
				{
					diceVal[lucky] = 7 - lastLuckyRoll;
				}
				for(int i = 0; i < diceVal.Length; i++)
				{
					if(i != lucky)
						diceVal[i] = rnd.Range(1, 7);
				}
				break;
			}
			case 5:
			{
				int min;
				int max;
				do
				{
					min = 7;
					max = -1;
					for(int i = 0; i < diceVal.Length; i++)
					{
						if(i != lucky)
						{
							diceVal[i] = rnd.Range(1, 7);
							if(diceVal[i] < min)
								min = diceVal[i];
							if(diceVal[i] > max)
								max = diceVal[i];
						}
					} 
				} while(min == 6 || min == max);

				diceVal[lucky] = rnd.Range(min + 1, max + 1);
				
				break;
			}
			case 6:
			{
				diceVal[lucky] = rnd.Range(4, 7);
				if(diceVal[lucky] == 5)
					diceVal[lucky] = 2;
				for(int i = 0; i < diceVal.Length; i++)
				{
					if(i != lucky)
						diceVal[i] = rnd.Range(1, 7);
				}
				break;
			}
			case 7:
			{
				int min;
				do
				{
					min = 7;
					for(int i = 0; i < diceVal.Length; i++)
					{
						if(i != lucky)
						{
							diceVal[i] = rnd.Range(1, 7);
							if(diceVal[i] < min)
								min = diceVal[i];
						}
					} 
				} while(min == 1);

				diceVal[lucky] = rnd.Range(1, min);
				
				break;
			}
			case 8:
			{
				int max = -1;
				for(int i = 0; i < diceVal.Length; i++)
				{
					if(i != lucky)
					{
						diceVal[i] = rnd.Range(1, 7);
						if(diceVal[i] > max)
							max = diceVal[i];
					}
				} 

				diceVal[lucky] = rnd.Range(1, 7);

				if(diceVal[lucky] <= max)
					diceVal[lucky] = 1;
				
				break;
			}
			case 9:
			{
				do {
					diceVal[lucky] = rnd.Range(1, 7);
				} while (diceVal[lucky] == 1 || diceVal[lucky] == 4);
				for(int i = 0; i < diceVal.Length; i++)
				{
					if(i != lucky)
						diceVal[i] = rnd.Range(1, 7);
				}
				break;
			}
		}

		lastLuckyRoll = diceVal[lucky];
        Debug.LogFormat("[Lucky Dice #{0}] Dice roll: {1}, {2}, {3}", moduleId, diceVal[0], diceVal[1], diceVal[2]);
	}

	void SelectDice(int n)
	{
		if(moduleSolved || animating)
			return;
			
		if(n == lucky)
		{
			Audio.PlaySoundAtTransform("correct", transform);
        	Debug.LogFormat("[Lucky Dice #{0}] Selected the lucky die ({1} - {2}). Module solved.", moduleId, lucky + 1, GetColorName(diceColor[lucky]));
			moduleSolved = true;
            GetComponent<KMBombModule>().HandlePass();
		}
		else
		{
        	Debug.LogFormat("[Lucky Dice #{0}] Strike! Selected wrong die ({1} - {2}).", moduleId, lucky + 1, GetColorName(diceColor[lucky]));
            GetComponent<KMBombModule>().HandleStrike();
			SetupDice();
			Roll();
		}
	}

	String GetColorName(int color)
	{
		switch(color)
		{
			case 0: return "Red";
			case 1: return "Pink";
			case 2: return "Purple";
			case 3: return "Orange";
			case 4: return "Yellow";
			case 5: return "Cyan";
			case 6: return "Blue";
			case 7: return "White";
			case 8: return "Grey";
			case 9: return "Black";
		}

		return "";
	}

	IEnumerator RollAnim()
	{
		animating = true;

		foreach(GameObject d in hl)
			d.gameObject.SetActive(false);		

		rotators[0].transform.localPosition = new Vector3(-0.0261f + 0.15f, 0.0346f + 0.04f, 0.0355f + 0.015f);
		rotators[0].transform.localEulerAngles = new Vector3(0, 0, 270f);
		rotators[1].transform.localPosition = new Vector3(0.0396f + 0.1f, 0.0346f + 0.04f, 0.0052f + 0.015f);
		rotators[1].transform.localEulerAngles = new Vector3(0, 0, 270f);
		rotators[2].transform.localPosition = new Vector3(-0.0208f + 0.15f, 0.0346f + 0.04f, -0.034f + 0.015f);
		rotators[2].transform.localEulerAngles = new Vector3(0, 0, 270f);

		float[] xIncrement = {0.0075f, 0.005f, 0.0075f};

		for(int i = 0; i < 20; i++)
		{
			for(int j = 0; j < 3; j++)
			{
				Vector3 pos = rotators[j].transform.localPosition; 
				rotators[j].transform.localEulerAngles = rotators[j].transform.localEulerAngles - new Vector3(0, 0, 13.5f);

				if(i < 10)
					rotators[j].transform.localPosition = pos - new Vector3(xIncrement[j], 0.004f, 0.0015f); 
				else if(i < 15)
					rotators[j].transform.localPosition = pos - new Vector3(xIncrement[j], -0.002f, -0.0015f); 
				else
					rotators[j].transform.localPosition = pos - new Vector3(xIncrement[j], 0.002f, 0.0015f); 

			}
			yield return new WaitForSeconds(0.017f);
		}

		foreach(GameObject d in hl)
			d.gameObject.SetActive(true);
		
		animating = false;
	}
}
