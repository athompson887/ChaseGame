using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameTimer : MonoBehaviour
{
	public int minutes;
	public int seconds;
	public int fraction;

	public float startTime;
	float FadeStartTime;
	float FadeTime;

	public float playTime;
	public float FadeSpeed;

	public Color TimerColor = new Color(1, 1, 1, 0);


	public bool go = false;
	public bool fadeIn = false;
	public bool fadeOut = false;
	private Text guiText = null;


	void Start()
	{
	    guiText = transform.Find("/UIOverlay/TimerText").GetComponent<Text>();
		if (!guiText)
		{
			Debug.Log("This timer requires a GUIText component");
			enabled = false;
			return;
		}

		if (FadeSpeed == 0)
		{
			FadeSpeed = 1;
		}
		guiText.material.color = TimerColor;
	}


	void Update()
	{
		if (go)
		{
			playTime = Time.time - startTime;
			minutes = (int)(playTime / 60f);
			seconds = (int)(playTime % 60f);
			fraction = (int)((playTime * 10) % 10);
			guiText.text = string.Format("ELAPSED TIME {0}\'{1}\"{2}", minutes, seconds, fraction);
		}

		if (fadeIn)
		{
			FadeIn();
		}

		if (fadeOut)
		{
			FadeOut();
		}

	}

	public void StartTimer()
	{
		startTime = Time.time;
		go = true;
		FadeStartTime = Time.time;
		fadeIn = true;
	}

	public void StopTimer()
	{
		go = false;
	}

	public void Fade()
	{
		if (!fadeIn && !fadeOut)
		{
			FadeStartTime = Time.time;
			if (TimerColor.a == 1)
			{
				Debug.Log(TimerColor.a);
				fadeOut = true;
			}
			else
			{
				guiText.enabled = true;
				fadeIn = true;
			}
		}
		else
		{

			FadeStartTime = Time.time - ((1 - FadeTime) / FadeSpeed);
			fadeIn = !fadeIn;
			fadeOut = !fadeOut;

		}
	}

	void FadeIn()
	{
		FadeTime = (Time.time - FadeStartTime) * FadeSpeed;
		TimerColor.a = Mathf.SmoothStep(0, 1, FadeTime);
		guiText.material.color = TimerColor;
		if (TimerColor.a == 1)
		{
			fadeIn = false;
		}
	}

	void FadeOut()
	{
		FadeTime = (Time.time - FadeStartTime) * FadeSpeed;
		TimerColor.a = Mathf.SmoothStep(1, 0, FadeTime);
		guiText.material.color = TimerColor;
		if (TimerColor.a == 0)
		{
			fadeOut = false;
		}
	}
}