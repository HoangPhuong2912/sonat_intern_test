using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelController : MonoBehaviour
{
    [Header("Level Data")]
    [SerializeField] List<LevelCreateController> levels;
    [SerializeField] List<LevelCreateController> randomLevelTemplates;
    [Header("Prefabs & Spawn Points")]
    [SerializeField] GameObject bottle;
    [SerializeField] List<Transform> bottleCreate; // 3 transforms: [0]=top row, [1]=bottom row, [2]=single row

    [Header("UI")]
    [SerializeField] Text levelText;

    [Header("Debug")]
    [SerializeField] LevelCreateController testLevel; // For testing specific levels

    private LevelCreateController activeLevel;
    public static int levelWinPoint = 0;
    private float bottleSpace;
    private int firstBlock, secondBlock;
    private List<Color> availableColors;

    void Start()
    {
        InitializeColors();
        LoadLevel(MenuController.activeLevel);
    }

    private void LoadLevel(int levelIndex)
    {
        // Load designed level (0-29) or generate random level (30+)
        if (levelIndex < 30 && levelIndex < levels.Count)
        {
            // Use designed level
            activeLevel = levels[levelIndex];
        }
        else
        {
            // Generate random level
            if (randomLevelTemplates.Count == 0)
            {
                Debug.LogError("No random level templates assigned!");
                return;
            }

            int templateIndex = UnityEngine.Random.Range(0, randomLevelTemplates.Count);
            activeLevel = CreateRandomLevel(randomLevelTemplates[templateIndex]);
        }

        // Set bottle spacing based on count
        bottleSpace = activeLevel.bottles.Count < 11 ? 0.5f : 0.4f;

        // Create bottles in scene
        CreateBottlesLayout();

        // Update UI
        levelWinPoint = activeLevel.winBottleCount;
        levelText.text = "LEVEL " + (levelIndex + 1).ToString();
    }

    private void CreateBottlesLayout()
    {
        if (activeLevel.bottles.Count > 5)
        {
            // Two rows
            firstBlock = activeLevel.bottles.Count / 2 + activeLevel.bottles.Count % 2; // Top row (rounded up)
            secondBlock = activeLevel.bottles.Count / 2; // Bottom row

            CreateBlocks(firstBlock, 0); // Top row
            CreateBlocks(secondBlock, 1); // Bottom row
        }
        else
        {
            // Single row
            firstBlock = activeLevel.bottles.Count;
            CreateBlocks(firstBlock, 2); // Center row
        }
    }

    private void CreateBlocks(int blockCount, int rowIndex)
    {
        int leftCount = 1, rightCount = 1;

        for (int i = 0; i < blockCount; i++)
        {
            Vector3 spawnPos;

            // Calculate spawn position (center, left, right alternating)
            if (i == 0)
            {
                spawnPos = bottleCreate[rowIndex].position;
            }
            else if (i % 2 == 1)
            {
                spawnPos = new Vector3(
                    bottleCreate[rowIndex].position.x - leftCount * bottleSpace,
                    bottleCreate[rowIndex].position.y,
                    bottleCreate[rowIndex].position.z
                );
                leftCount++;
            }
            else
            {
                spawnPos = new Vector3(
                    bottleCreate[rowIndex].position.x + rightCount * bottleSpace,
                    bottleCreate[rowIndex].position.y,
                    bottleCreate[rowIndex].position.z
                );
                rightCount++;
            }

            // Instantiate bottle
            GameObject obj = Instantiate(bottle, spawnPos, Quaternion.identity, transform);

            // Calculate bottle index in level data
            int bottleIndex = rowIndex >= 2 ? i : i + rowIndex * firstBlock;

            // Set bottle colors
            BottleController bottleController = obj.GetComponent<BottleController>();
            for (int j = 0; j < 4; j++)
            {
                bottleController.bottleColors[j] = activeLevel.bottles[bottleIndex].colors[j];
            }
            bottleController.numberOfColorInBottle = activeLevel.bottles[bottleIndex].numberBottle;
            bottleController.lineRenderer = GameObject.Find("LineRenderer").GetComponent<LineRenderer>();
        }
    }

    private LevelCreateController CreateRandomLevel(LevelCreateController template)
    {
        // Create a new instance (don't modify template!)
        LevelCreateController randomLevel = ScriptableObject.CreateInstance<LevelCreateController>();
        randomLevel.bottles = new List<LevelCreateController.LevelProperty>();

        int totalBottles = template.bottles.Count;
        int emptyBottles = 2; // Last 2 bottles are empty
        int filledBottles = totalBottles - emptyBottles;

        // Create color pool: each color appears 4 times
        List<Color> colorPool = new List<Color>();
        for (int i = 0; i < filledBottles; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                colorPool.Add(availableColors[i % availableColors.Count]);
            }
        }

        // Shuffle color pool
        for (int i = colorPool.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            Color temp = colorPool[i];
            colorPool[i] = colorPool[randomIndex];
            colorPool[randomIndex] = temp;
        }

        // Create bottles
        int poolIndex = 0;
        for (int i = 0; i < totalBottles; i++)
        {
            LevelCreateController.LevelProperty bottle = new LevelCreateController.LevelProperty();
            bottle.colors = new List<Color>();

            if (i < filledBottles)
            {
                // Filled bottle
                for (int j = 0; j < 4; j++)
                {
                    bottle.colors.Add(colorPool[poolIndex]);
                    poolIndex++;
                }
                bottle.numberBottle = 4;
            }
            else
            {
                // Empty bottle
                for (int j = 0; j < 4; j++)
                {
                    bottle.colors.Add(Color.clear);
                }
                bottle.numberBottle = 0;
            }

            randomLevel.bottles.Add(bottle);
        }

        randomLevel.winBottleCount = filledBottles; // Number of unique colors to sort
        return randomLevel;
    }

    private void InitializeColors()
    {
        availableColors = new List<Color>
        {
            Color.red,
            Color.blue,
            Color.green,
            Color.yellow,
            Color.cyan,
            Color.magenta,
            new Color(1f, 0.5f, 0f),       // Orange
            new Color(0.5f, 0f, 0.5f),      // Purple
            new Color(1f, 0.75f, 0.8f),     // Pink
            new Color(0.65f, 0.16f, 0.16f), // Brown
            new Color(0.5f, 0.5f, 0.5f),    // Gray
            new Color(0f, 0.5f, 0f)         // Dark Green
        };
    }
}