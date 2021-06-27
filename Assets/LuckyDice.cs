using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;
using rnd = UnityEngine.Random;

public class LuckyDice : MonoBehaviour
{
	public KMAudio Audio;
	public KMBombModule modSelf;
	public KMColorblindMode colorblindMode;

	//Logging
	static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

	public KMSelectable rollBtn, selfSelectable;
	public KMSelectable[] diceBtns;
	public GameObject[] dice;
	public GameObject[] rotators;
	public GameObject[] hl;
	public Material[] colors;
	public TextMesh colorblindText;
	int[] diceVal = new int[3];
	int[] diceColor = new int[3];
	int lucky = -1;
	int lastLuckyRoll;
    private List<int[]> allDiceRolls = new List<int[]>();


	Coroutine diceRoll;
	bool animating = false, hasStarted = false, colorblindDetected;

	void Awake()
	{

		/*
		diceBtns[0].OnInteract += delegate () { SelectDice(0); return false; };
		diceBtns[1].OnInteract += delegate () { SelectDice(1); return false; };
		diceBtns[2].OnInteract += delegate () { SelectDice(2); return false; };
		*/
		try
        {
			colorblindDetected = colorblindMode.ColorblindModeActive;
        }
		catch
        {
			colorblindDetected = false;
        }
	}

	void Activate()
	{
		Roll();
		for (var x = 0; x < dice.Length; x++)
		{
			dice[x].SetActive(true);
		}
		selfSelectable.Children = new[] { diceBtns[0], null, diceBtns[1], diceBtns[2], null, rollBtn };
		selfSelectable.UpdateChildren();
		hasStarted = true;
	}

	void Start ()
	{
		moduleId = moduleIdCounter++;
		SetupDice();
		for (var x = 0; x < diceBtns.Length; x++)
        {
			var y = x;
            diceBtns[x].OnInteract += () => { SelectDice(y); return false; };
			diceBtns[x].OnHighlight += () => { if (colorblindDetected && !moduleSolved) colorblindText.text = GetColorName(diceColor[y]) + " " + diceVal[y]; };
			diceBtns[x].OnHighlightEnded += () => { colorblindText.text = ""; };
		}
		modSelf.OnActivate += Activate;
		rollBtn.OnInteract += delegate () { if (hasStarted) Roll(); return false; };
		for (var x = 0; x < dice.Length; x++)
		{
			dice[x].SetActive(false);
		}
		selfSelectable.Children = new[] { rollBtn };
		selfSelectable.UpdateChildren();
		colorblindText.text = "";
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
		diceRoll = StartCoroutine(RollAnim());
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

	bool IsBlacklisted()
    {
		Dictionary<int, int[]> blacklistLuckyDice = new Dictionary<int, int[]>()
		{
            { 3, new[] { 7 } },
			{ 7, new[] { 3 } }
		};
		// Check if there is a key for that specific lucky dice, and prohibit any other dice being colored as such.
		return blacklistLuckyDice.ContainsKey(diceColor[lucky]) &&
			diceColor.Any(a => blacklistLuckyDice[diceColor[lucky]].Contains(a));
    }


	void SetupDice()
	{
		lastLuckyRoll = -1;
		do
		{
			List<int> used = new List<int>();
			for (int i = 0; i < dice.Length; i++)
			{
				int color;
				do
				{
					color = rnd.Range(0, colors.Length);
				} while (used.Contains(color));
				diceColor[i] = color;
				used.Add(color);
				dice[i].transform.Find("cube").GetComponentInChildren<Renderer>().material = colors[diceColor[i]];
				dice[i].transform.Find("pips").GetComponentInChildren<Renderer>().material = diceColor[i] == 7 ? colors[9] : colors[7];
            }
			lucky = rnd.Range(0, 3);
		}
		while (IsBlacklisted());
        Debug.LogFormat("[Lucky Dice #{0}] Dice colors (In this order; Top, Bottom Right, Bottom Left): {1}", moduleId, diceColor.Select(a => GetColorName(a)).Join(", "));

        Debug.LogFormat("[Lucky Dice #{0}] Determined lucky die: {1} ({2}).", moduleId, GetPosition(lucky), GetColorName(diceColor[lucky]));
	}

	void SetDiceValues()
	{
		switch(diceColor[lucky])
		{
			case 0: // Red
			{
				diceVal[lucky] = rnd.Range(2, 7);
				for(int i = 0; i < diceVal.Length; i++)
				{
					if(i != lucky)
						diceVal[i] = rnd.Range(1, diceVal[lucky]);
				}
				break;
			}
			case 1: // Pink
			{
				diceVal[lucky] = rnd.Range(1, 4);
				if(diceVal[lucky] == 2)
					diceVal[lucky] = 5;
				for(int i = 0; i < diceVal.Length; i++)
				{
					if(i != lucky)
						diceVal[i] = rnd.Range(1, 7);
				}
				break;
			}
			case 2: // Purple
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
			case 3: // Orange
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
			case 4: // Yellow
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
			case 5: // Cyan
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
			case 6: // Blue
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
			case 7: // White
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
			case 8: // Gray
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
			case 9: // Black
			{
				diceVal[lucky] = rnd.Range(1, 4);
				if(diceVal[lucky] == 1)
					diceVal[lucky] = 5;
				for(int i = 0; i < diceVal.Length; i++)
				{
					if(i != lucky)
						diceVal[i] = rnd.Range(1, 7);
				}
				break;
			}
		}

		lastLuckyRoll = diceVal[lucky];
		allDiceRolls.Add(diceVal.ToArray());
        Debug.LogFormat("[Lucky Dice #{0}] New Dice Roll (In this order; Top, Bottom Right, Bottom Left): {1}", moduleId, diceVal.Join(", "));
	}

	void SelectDice(int n)
	{
		if(moduleSolved || animating || !hasStarted)
			return;
		colorblindText.text = "";
		Debug.LogFormat("<Lucky Dice #{0}> All rolls up to this point: ({1})", moduleId, allDiceRolls.Select(a => a.Join(", ")).Join("), ("));
		if (n == lucky)
		{
			Audio.PlaySoundAtTransform("correct", transform);
			Debug.LogFormat("[Lucky Dice #{0}] The lucky die ({1} - {2}) was correctly selected. Module solved.", moduleId, GetPosition(lucky), GetColorName(diceColor[lucky]));
			moduleSolved = true;
			modSelf.HandlePass();
		}
		else
		{
			allDiceRolls.Clear();
        	Debug.LogFormat("[Lucky Dice #{0}] Strike! The wrong die ({1} - {2}) was selected! Expected this die ({3} - {4}).", moduleId,
				GetPosition(n), GetColorName(diceColor[n]),
				GetPosition(lucky), GetColorName(diceColor[lucky]));
			modSelf.HandleStrike();
			SetupDice();
			Roll();
		}
	}

	string GetColorName(int color)
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
			case 8: return "Gray";
			case 9: return "Black";
		}

		return "";
	}
	string GetPosition(int pos)
	{
		switch (pos)
		{
			case 0: return "Top";
			case 1: return "Bottom Right";
			case 2: return "Bottom Left";
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

    //Twitch Plays Handling (Modified from original author)
    private bool isValid1(string s)
    {
        char[] valids = { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };
        foreach(char c in s)
        {
            if (!valids.Contains(c))
            {
                return false;
            }
        }
        return true;
    }

    private bool isValid2(string s)
    {
        string[] valids = { "T", "BL", "BR", "t", "bl", "br" };
        if (!valids.Contains(s))
        {
            return false;
        }
        return true;
    }

    #pragma warning disable 414
    private readonly string TwitchHelpMessage = "\"!{0} press <die>\" [Presses the specificed die] | \"!{0} roll <#>\" [Rolls the dice <#> times] | \"!{0} colorblind <die>\" [Obtain the color of the specified die] | Valid dice are T(top), BL(bottomleft), and BR(bottomright)";
	bool TwitchShouldCancelCommand;
	#pragma warning restore 414

	IEnumerator TwitchHandleForcedSolve()
    {
		while (!hasStarted || animating)
			yield return true;
		diceBtns[lucky].OnInteract();
		yield return true;
    }

    IEnumerator ProcessTwitchCommand(string command)
    {
		if (!hasStarted)
		{
			yield return "sendtochaterror The module is not ready yet. Wait a bit until the module has started.";
			yield break;
		}
        if (Regex.IsMatch(command, @"^\s*roll\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            rollBtn.OnInteract();
            yield break;
        }
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*roll\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if(parameters.Length == 2)
            {
                if (isValid1(parameters[1]))
                {
                    yield return null;
                    int temp;
                    if  (!int.TryParse(parameters[1], out temp))
                    {
						yield return "sendtochaterror I do not know how to roll \"" + temp + "\" times.";
						yield break;
					}
                    int counter = 0;
                    while(counter != temp && !TwitchShouldCancelCommand)
                    {
                        rollBtn.OnInteract();
                        yield return new WaitWhile(() => !TwitchShouldCancelCommand && animating);
                        counter++;
                    }
					if (TwitchShouldCancelCommand)
					{
						yield return "sendtochat The dice rolling has been cancelled after " + counter + " roll(s) due to a request to cancel!";
						TwitchShouldCancelCommand = false;
					}
				}
            }
            yield break;
        }
        else if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if (parameters.Length == 2)
            {
                if (isValid2(parameters[1]))
                {
                    yield return null;
                    if (parameters[1].EqualsIgnoreCase("T"))
                    {
                        diceBtns[0].OnInteract();
                    }
                    else if (parameters[1].EqualsIgnoreCase("BL"))
                    {
                        diceBtns[2].OnInteract();
                    }
                    else if (parameters[1].EqualsIgnoreCase("BR"))
                    {
                        diceBtns[1].OnInteract();
                    }
                }
            }
            yield break;
        }
		else if (Regex.IsMatch(parameters[0], @"^\s*colorblind\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			if (parameters.Length == 2)
			{
				if (isValid2(parameters[1]))
				{
					yield return null;
					colorblindDetected = true;
					if (parameters[1].EqualsIgnoreCase("T"))
					{
						var highlight = diceBtns[0].Highlight.transform.Find("Highlight(Clone)");
						highlight = highlight ?? diceBtns[0].Highlight.transform;
						highlight.gameObject.SetActive(true);
						diceBtns[0].OnHighlight();
						yield return string.Format("sendtochat The color of the {1} die is {0}", GetColorName(diceColor[0]), GetPosition(0));
						yield return new WaitForSeconds(1f);
						diceBtns[0].OnHighlightEnded();
						highlight.gameObject.SetActive(false);
					}
					else if (parameters[1].EqualsIgnoreCase("BL"))
					{
						var highlight = diceBtns[2].Highlight.transform.Find("Highlight(Clone)");
						highlight = highlight ?? diceBtns[0].Highlight.transform;
						highlight.gameObject.SetActive(true);
						diceBtns[2].OnHighlight();
						yield return string.Format("sendtochat The color of the {1} die is {0}", GetColorName(diceColor[2]), GetPosition(2));
						yield return new WaitForSeconds(1f);
						diceBtns[2].OnHighlightEnded();
						highlight.gameObject.SetActive(false);
					}
					else if (parameters[1].EqualsIgnoreCase("BR"))
					{
						var highlight = diceBtns[1].Highlight.transform.Find("Highlight(Clone)");
						highlight = highlight ?? diceBtns[0].Highlight.transform;
						highlight.gameObject.SetActive(true);
						diceBtns[1].OnHighlight();
						yield return string.Format("sendtochat The color of the {1} die is {0}", GetColorName(diceColor[1]), GetPosition(1));
						yield return new WaitForSeconds(1f);
						diceBtns[1].OnHighlightEnded();
						highlight.gameObject.SetActive(false);
					}
					colorblindDetected = false;
				}
			}
			yield break;
		}
	}
}
