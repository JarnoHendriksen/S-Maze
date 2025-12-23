using JetBrains.Annotations;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class DataCollector : MonoBehaviour
{
    public static DataCollector Instance;

    public bool isLoggingEnabled = true;

    private float sessionStartTime;
    private int numberOfItems = 0;
    private int currentLevel = 1;
    private Dictionary<ItemData, string> itemsCollector = new();

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
        itemsCollector.Add(item, (Time.time - sessionStartTime).ToString("F2"));
    }

    public void LevelUp(int nextLevel)
    {
        if (!isLoggingEnabled) return;
        if (currentLevel < 3)
        {
            currentLevel = nextLevel;
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
            itemsCollected = new Dictionary<ItemData, string>(itemsCollector)
        };
        SessionData.Add(data);

        WriteToFile(data);
        ClearLogging();
    }

    private void ClearLogging()
    {
        SessionData.Clear();
        sessionStartTime = Time.time;
        numberOfItems = 0;
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

        string collectedItemsString = string.Join(", ", entry.itemsCollected.Select(kvp => $"[{kvp.Key.name}: {kvp.Value}]"));
        using (StreamWriter writer = new StreamWriter(filePath, true)) //Append to file
        {
            string LogEntry = $"[LEVEL COMPLETED] " +
                  $"Level: {entry.numberLevel} | " +
                  $"Time: {entry.timeLevel.ToString("F2")}s | " +
                  $"Items: {entry.itemsCollected.Count} | " +
                  $"Details: {(entry.itemsCollected.Count > 0 ? collectedItemsString : "None")}";
            writer.WriteLine(LogEntry);
        }
    }

    [Serializable]
    public class DataInfo
    {
        public float timeLevel;
        public int numberLevel;
        public Dictionary<ItemData, string> itemsCollected;
    }
}