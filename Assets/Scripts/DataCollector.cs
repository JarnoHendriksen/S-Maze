using JetBrains.Annotations;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DataCollector : MonoBehaviour
{
    public static DataCollector Instance;

    public bool isLoggingEnabled = true;

    private float sessionStartTime;
    private int currentLevel = 1;
    private int attempts = 0;
    private Dictionary<string, int> attemptsPerQ = new();
    private Dictionary<string, string> itemsCollector = new();

    private List<DataInfo> SessionData = new();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        sessionStartTime = Time.time;
    }

    public void LogItemCollected(ItemData item)
    {
        if (!isLoggingEnabled) return;
        itemsCollector.Add((Time.time - sessionStartTime).ToString("F2"), (item.type.ToString() + " " + item.value));
    }

    public void LogQuizAttempts(string question)
    {
        if (!isLoggingEnabled) return;
        attemptsPerQ.Add(question, attempts);
        attempts = 0;
    }

    public void AddAttempt()
    {
        if (!isLoggingEnabled) return;
        attempts++;
    }

    public void LevelUp(int levelCompleted)
    {
        if (!isLoggingEnabled) return;
        if (currentLevel < 3)
        {
            currentLevel = levelCompleted;
        } else
        {
            currentLevel = 1;
        }
            FinalizeData();
    }

    public void FinalizeData()
    {
        if (!isLoggingEnabled)
        {
            Debug.Log("Logging is disabled.");
            ClearLogging();
            return;
        }
        float TotalTime = Time.time - sessionStartTime;
        DataInfo data = new DataInfo
        {
            timeLevel = TotalTime,
            numberLevel = currentLevel,
            itemsCollected = new Dictionary<string, string>(itemsCollector),
            attemptsPerQ = new Dictionary<string, int>(attempts)
        };
        SessionData.Add(data);

        WriteToFile(data);
        ClearLogging();
    }

    private void ClearLogging()
    {
        SessionData.Clear();
        sessionStartTime = Time.time;
        itemsCollector.Clear();
    }

    public void LogData()
    {
        FinalizeData();
    }

    private void WriteToFile(DataInfo entry)
    {
        string customFolder = "PlayerLogs";
        if (!Directory.Exists("PlayerLogs"))
        {
            Directory.CreateDirectory("PlayerLogs");
        }
        string filePath = Path.Combine(customFolder, "DataLog.txt");

        string collectedItemsString = string.Join(", ", entry.itemsCollected.Select(kvp =>
            $"[{kvp.Value}: {kvp.Key:F2}]"));
        string attemptsString = string.Join(", ", entry.attemptsPerQ.Select(kvp =>
            $"[Question: {kvp.Key} - Attempts: {kvp.Value}]"));

        using (StreamWriter writer = new StreamWriter(filePath, true)) //Append to file
        {
            string LogEntry = $"[LEVEL COMPLETED] " +
                  $"Level: {entry.numberLevel} | " +
                  $"Time: {entry.timeLevel.ToString("F2")}s | " +
                  $"Items: {entry.itemsCollected.Count} | " +
                  $"Details: {(entry.itemsCollected.Count > 0 ? collectedItemsString : "None")} | " +
                  $"Attempts per Question: {(entry.attemptsPerQ.Count > 0 ? attemptsString : "None")}";
            writer.WriteLine(LogEntry);
        }
    }

    [Serializable]
    public class DataInfo
    {
        public float timeLevel;
        public int numberLevel;
        public Dictionary<string, int> attemptsPerQ;
        public Dictionary<string, string> itemsCollected;
    }
}