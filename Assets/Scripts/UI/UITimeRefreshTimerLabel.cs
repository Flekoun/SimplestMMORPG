using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UITimeRefreshTimerLabel : MonoBehaviour
{
    private TextMeshProUGUI countdownText;
    private DateTime nextHour;

    private void Awake()
    {
        countdownText = GetComponent<TextMeshProUGUI>();
        SetNextHour();
    }

    private void SetNextHour()
    {
        DateTime now = DateTime.Now;
        nextHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddHours(1);
    }

    private void Update()
    {
        TimeSpan timeRemaining = nextHour - DateTime.Now;

        // Reset timer if an hour has passed
        if (timeRemaining.TotalSeconds <= 0)
        {
            SetNextHour();
            timeRemaining = nextHour - DateTime.Now;
        }

        countdownText.text = $"{timeRemaining.Minutes:D2}:{timeRemaining.Seconds:D2}";
    }
}
