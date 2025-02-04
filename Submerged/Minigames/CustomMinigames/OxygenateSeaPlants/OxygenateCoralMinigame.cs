using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using Submerged.BaseGame.Extensions;
using Submerged.Extensions;
using Submerged.Minigames.CustomMinigames.OxygenateSeaPlants.DataStructures;
using Submerged.Minigames.CustomMinigames.OxygenateSeaPlants.MonoBehaviours;
using Submerged.Minigames.MonoBehaviours;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.OxygenateSeaPlants;

[RegisterInIl2Cpp]
public sealed class OxygenateCoralMinigame(nint ptr) : Minigame(ptr)
{
    private const float PROBABILITY = 0.25f;

    private const float BUBBLE_FORCE_MODIFIER = 1f;
    private static readonly int _hsvaAdjust = Shader.PropertyToID("_HSVAAdjust");

    public Transform[] colorSpriteSets = new Transform[5];
    public Transform mazeCells;
    public Transform endCoral;
    private Rigidbody2D _bubbleBody;
    private Vector3 _bubblePos;

    private Transform _bubbleTransform;

    private int[,] _colorMap;
    private Vector2 _inputDelta;

    private Camera _mainCam;

    public CoralCell[,] cells;

    public (Sprite sprite, PolygonCollider2D collider)[] slugs;

    private void Start()
    {
        UnityRandom.seed = new SystemRandom().Next();

        _mainCam = Camera.main;

        colorSpriteSets = transform.Find("ColorSpriteSets").GetChildren();
        mazeCells = transform.Find("MazeCells");

        _bubbleTransform = transform.Find("Bubble");
        _bubbleBody = _bubbleTransform.GetComponent<Rigidbody2D>();

        slugs = transform.Find("Slugs/1x1")
                         .GetChildren()
                         .Select(t =>
                                     (t.GetComponent<SpriteRenderer>().sprite, t.GetComponent<PolygonCollider2D>()))
                         .ToArray();

        SetupColors();
        cells = GetDefaultCells();
        GenerateMazeDfsNonRecursive();

        int randomStartX = UnityRandom.Range(0, 9);
        int randomEndX = UnityRandom.Range(0, 9);
        List<(int xPos, int yPos)> path = PathfindBfs(randomEndX, 1, randomStartX, 11);

        int[,] active = new int[9, 12];

        for (int y = 0; y < 12; y++)
            for (int x = 0; x < 9; x++)
            {
                GameObject cell = mazeCells.Find($"Row {y}/Column {x}").gameObject;
                bool shouldBeActive = UnityRandom.Range(0f, 1f) < 0.90f;
                cell.SetActive(shouldBeActive);
                active[x, y] = shouldBeActive ? 1 : 0;

                if (path.Contains((x, y)))
                {
                    cell.SetActive(false);
                }
                else if (UnityRandom.Range(0f, 1f) < 0.25f && y < 7)
                {
                    (Sprite sprite, PolygonCollider2D collider) = slugs.Random();
                    cell.GetComponent<SpriteRenderer>().sprite = sprite;
                    DestroyImmediate(cell.GetComponent<PolygonCollider2D>());
                    CopyComponent(cell.gameObject, collider);
                    cell.AddComponent<SlugBehaviour>().minigame = this;
                }
            }

        foreach ((int xPos, int yPos) cell in path)
        {
            int around = 0;
            DoAroundPoint(active,
                          cell,
                          (x, y) =>
                          {
                              if (!path.Contains((x, y)))
                              {
                                  if (UnityRandom.Range(0f, 1f) < 0.05f && y < 7)
                                  {
                                      GameObject mazecell = mazeCells.Find($"Row {y}/Column {x}").gameObject;

                                      if (mazecell.GetComponent<SlugBehaviour>()) return;

                                      (Sprite sprite, PolygonCollider2D collider) = slugs.Random();
                                      mazecell.GetComponent<SpriteRenderer>().sprite = sprite;
                                      DestroyImmediate(mazecell.GetComponent<PolygonCollider2D>());
                                      CopyComponent(mazecell.gameObject, collider);
                                      mazecell.AddComponent<SlugBehaviour>().minigame = this;
                                  }
                              }
                              else
                              {
                                  around++;
                              }
                          });

            if (around > 7 && cell.yPos < 7)
            {
                GameObject middle = mazeCells.Find($"Row {cell.yPos}/Column {cell.xPos}").gameObject;
                middle.SetActive(true);

                if (UnityRandom.Range(0f, 1f) < 0.33f && !middle.GetComponent<SlugBehaviour>())
                {
                    (Sprite sprite, PolygonCollider2D collider) = slugs.Random();
                    middle.GetComponent<SpriteRenderer>().sprite = sprite;
                    DestroyImmediate(middle.GetComponent<PolygonCollider2D>());
                    CopyComponent(middle.gameObject, collider);
                    middle.AddComponent<SlugBehaviour>().minigame = this;
                }
            }
        }

        endCoral = transform.Find("EndCoral");
        Transform endLower = mazeCells.Find($"Row {1}/Column {randomEndX}");
        Transform endUpper = mazeCells.Find($"Row {0}/Column {randomEndX}");
        endCoral.position = (endLower.position + endUpper.position) / 2;
        endLower.gameObject.SetActive(false);
        endUpper.gameObject.SetActive(false);
        endCoral.gameObject.AddComponent<CoralBehaviour>().minigame = this;

        // MazeCells.GetComponentsInChildren<SpriteRenderer>().ForEach(r => r.gameObject.AddComponent<PolygonCollider2D>());

        _bubbleTransform.position = mazeCells.Find($"Row {path.First().yPos}/Column {path.First().xPos}").position;
        _bubbleTransform.SetWorldZ(_bubbleTransform.position.z - 5f);
        _bubblePos = _bubbleTransform.localPosition;
    }

    private void Update()
    {
        _inputDelta = Vector2.zero;
        // if (Input.GetKey(KeyCode.W)) InputDelta.y += 1;
        // if (Input.GetKey(KeyCode.A)) InputDelta.x -= 1;
        // if (Input.GetKey(KeyCode.S)) InputDelta.y -= 1;
        // if (Input.GetKey(KeyCode.D)) InputDelta.x += 1;

        Vector2 bubblePoint = _bubbleTransform.position;
        Vector2 worldPoint = _mainCam.ScreenToWorldPoint(Input.mousePosition);
        _inputDelta = worldPoint - bubblePoint;
        _inputDelta.Normalize();
    }

    private void FixedUpdate()
    {
        Vector2 force = new(_inputDelta.x * BUBBLE_FORCE_MODIFIER, _inputDelta.y * BUBBLE_FORCE_MODIFIER);
        _bubbleBody.AddForce(force);
    }

    public void SetupColors()
    {
        _colorMap = new int[9, 12];

        for (int x = 0; x < _colorMap.GetLength(0); x++)
            for (int y = 0; y < _colorMap.GetLength(1); y++)
            {
                _colorMap[x, y] = 8;
            }

        int[] randomXs = Enumerable.Range(0, 9).ToArray().ShuffleCopy().Take(colorSpriteSets.Length).ToArray();
        int[] randomYs = Enumerable.Range(0, 12).ToArray().ShuffleCopy().Take(colorSpriteSets.Length).ToArray();

        List<(int x, int y)> pointsToVisit = [];

        for (int i = 0; i < randomXs.Length; i++)
        {
            int x = randomXs[i];
            int y = randomYs[i];

            _colorMap[x, y] = i;
            pointsToVisit.Add((x, y));
        }

        int visitedPoints = pointsToVisit.Count;

        while (visitedPoints < _colorMap.Length)
        {
            if (pointsToVisit.Count == 0)
            {
                int randomSpot;

                do
                {
                    int randomX = UnityRandom.Range(0, 9);
                    int randomY = UnityRandom.Range(0, 12);
                    randomSpot = _colorMap[randomX, randomY];

                    if (randomSpot != 8)
                    {
                        pointsToVisit.Add((randomX, randomY));
                    }
                }
                while (randomSpot == 8);
            }

            (int x, int y) currentPoint = pointsToVisit.RemoveAndGet(0);
            int currentColor = _colorMap[currentPoint.x, currentPoint.y];

            DoAroundPoint(_colorMap,
                          currentPoint,
                          (x, y) =>
                          {
                              if (_colorMap[x, y] != 8) return;
                              if (UnityRandom.Range(0f, 1f) >= PROBABILITY) return;
                              pointsToVisit.Add((x, y));
                              _colorMap[x, y] = currentColor;
                              visitedPoints++;
                          });
        }

        int[] amounts = new int[5];

        for (int y = 0; y < 12; y++)
            for (int x = 0; x < 9; x++)
            {
                amounts[_colorMap[x, y]]++;
            }

        List<int> sortedAmounts = amounts.ToList();
        sortedAmounts.Sort();

        List<Transform>[] spriteChildren = new List<Transform>[5];

        for (int y = 0; y < 12; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                int color = _colorMap[x, y];
                SpriteRenderer cellRenderer = mazeCells.Find($"Row {y}/Column {x}").GetComponent<SpriteRenderer>();
                int index = sortedAmounts.IndexOf(amounts[color]);
                spriteChildren[index] ??= colorSpriteSets[index].Find("1x1").GetChildren().ToList();
                SpriteRenderer randomSpriteRenderer = spriteChildren[index].Random().GetComponent<SpriteRenderer>();
                cellRenderer.sprite = randomSpriteRenderer.sprite;

                CopyComponent(cellRenderer.gameObject, randomSpriteRenderer.GetComponent<PolygonCollider2D>());
            }
        }
    }

    public void ResetBubble()
    {
        this.StartCoroutine(PopBubble());
    }

    [HideFromIl2Cpp]
    private IEnumerator PopBubble()
    {
        _bubbleBody.simulated = false;
        Vector3 originalScale = _bubbleTransform.localScale;
        const float DURATION = 0.3f;

        for (float t = 0; t < DURATION; t += Time.deltaTime)
        {
            _bubbleTransform.localScale = Vector3.Lerp(originalScale, new Vector3(0, 0, 1), t / DURATION);

            yield return null;
        }

        _bubbleTransform.localPosition = _bubblePos;

        for (float t = 0; t < DURATION; t += Time.deltaTime)
        {
            _bubbleTransform.localScale = Vector3.Lerp(new Vector3(0, 0, 1), originalScale, t / DURATION);

            yield return null;
        }

        _bubbleBody.simulated = true;
        _bubbleBody.velocity = Vector2.zero;
        _bubbleTransform.localScale = originalScale;
    }

    [HideFromIl2Cpp]
    public IEnumerator Oxygenate()
    {
        if (amClosing != CloseState.None) yield break;
        if (MyNormTask != null)
        {
            MyNormTask.NextStep();
        }
        StartCoroutine(CoStartClose());

        _bubbleTransform.gameObject.SetActive(false);
        SpriteRenderer endRend = GameObject.Find("EndCoral").GetComponent<SpriteRenderer>();
        Material mat = endRend.material;
        Vector4 ogVec = mat.GetVector(_hsvaAdjust);

        const float DURATION = 0.5f;

        for (float t = 0; t < DURATION; t += Time.deltaTime)
        {
            mat.SetVector(_hsvaAdjust, Vector4.Lerp(ogVec, Vector4.zero, t / DURATION));

            yield return null;
        }
    }

    public static T CopyFrom<T>(Component comp, T other) where T : Component
    {
        const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;

        Type type = comp.GetType();

        if (type != other.GetType()) return null; // type mis-match
        PropertyInfo[] pinfos = type.GetProperties(FLAGS);

        foreach (PropertyInfo pinfo in pinfos)
        {
            if (pinfo.CanWrite)
            {
                try
                {
                    pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                }
                catch
                {
                    // ignore
                } // In case of Not Implemented Exception being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
            }
        }

        FieldInfo[] finfos = type.GetFields(FLAGS);

        foreach (FieldInfo finfo in finfos)
        {
            finfo.SetValue(comp, finfo.GetValue(other));
        }

        return comp as T;
    }

    public static T CopyComponent<T>(GameObject go, T copyFrom) where T : Component => CopyFrom(go.AddComponent<T>(), copyFrom);

    private static bool ValidIndex<T>(T[,] arr, (int xPos, int yPos) pos)
    {
        bool xValid = arr.GetUpperBound(0) >= pos.xPos && pos.xPos >= arr.GetLowerBound(0);
        bool yValid = arr.GetUpperBound(1) >= pos.yPos && pos.yPos >= arr.GetLowerBound(1);

        return xValid && yValid;
    }

    private static void DoAroundPoint<T>(T[,] arr, (int initialX, int initialY) point, Action<int, int> action)
    {
        for (int x = -1; x < 2; x++)
        {
            for (int y = -1; y < 2; y++)
            {
                if (x == 0 && y == 0) continue;
                int newX = point.initialX + x;
                int newY = point.initialY + y;

                if (!ValidIndex(arr, (newX, newY))) continue;
                action(newX, newY);
            }
        }
    }

    public override void Close()
    {
        CooldownConsole cc = Console.Cast<CooldownConsole>();
        cc.CoolDown = cc.MaxCoolDown;
        this.BaseClose();
    }

    #region Maze Generation

    [HideFromIl2Cpp]
    public CoralCell[,] GetDefaultCells()
    {
        CoralCell[,] def = new CoralCell[9, 12];

        for (int x = 0; x < 9; x++)
        {
            for (int y = 0; y < 12; y++)
            {
                def[x, y] = new CoralCell
                {
                    leftWall = x == 0 ? new CoralWall() : def[x - 1, y].rightWall,
                    rightWall = new CoralWall(),
                    topWall = y == 0 ? new CoralWall() : def[x, y - 1].bottomWall,
                    bottomWall = new CoralWall()
                };
            }
        }

        return def;
    }

    public void GenerateMazeDfsNonRecursive()
    {
        List<(int xPos, int yPos)> stack = [];

        [HideFromIl2Cpp]
        List<(int xPos, int yPos)> getNeighbours((int xPos, int yPos) position)
        {
            List<(int xPos, int yPos)> neighbours = [];
            CoralCell northCell = position.yPos == 0 ? null : cells[position.xPos, position.yPos - 1];
            CoralCell eastCell = position.xPos == 8 ? null : cells[position.xPos + 1, position.yPos];
            CoralCell southCell = position.yPos == 11 ? null : cells[position.xPos, position.yPos + 1];
            CoralCell westCell = position.xPos == 0 ? null : cells[position.xPos - 1, position.yPos];

            if (northCell is { visited: false }) neighbours.Add((position.xPos, position.yPos - 1));
            if (eastCell is { visited: false }) neighbours.Add((position.xPos + 1, position.yPos));
            if (southCell is { visited: false }) neighbours.Add((position.xPos, position.yPos + 1));
            if (westCell is { visited: false }) neighbours.Add((position.xPos - 1, position.yPos));

            foreach ((int xPos, int yPos) location in neighbours)
            {
                if (stack.Contains(location)) stack.Remove(location);
            }

            return neighbours;
        }

        (int xPos, int yPos) currentPosition = (0, 0); // Starting Position
        cells[currentPosition.xPos, currentPosition.yPos].visited = true;
        stack.Add(currentPosition);

        while (stack.Count != 0)
        {
            currentPosition = stack[^1];
            List<(int xPos, int yPos)> neighbours = getNeighbours(currentPosition);

            if (neighbours.Count == 0)
            {
                stack.RemoveAt(stack.Count - 1);

                continue;
            }

            neighbours.Shuffle();

            (int xPos, int yPos) selectedPos = neighbours[0];
            stack.Add(selectedPos);
            CoralCell selectedCell = cells[selectedPos.xPos, selectedPos.yPos];
            selectedCell.visited = true;

            if (selectedPos.xPos + 1 == currentPosition.xPos) selectedCell.rightWall.exists = false;
            if (selectedPos.xPos - 1 == currentPosition.xPos) selectedCell.leftWall.exists = false;
            if (selectedPos.yPos + 1 == currentPosition.yPos) selectedCell.bottomWall.exists = false;
            if (selectedPos.yPos - 1 == currentPosition.yPos) selectedCell.topWall.exists = false;
        }
    }

    [HideFromIl2Cpp]
    public List<(int xPos, int yPos)> PathfindBfs(int xStart, int yStart, int xEnd, int yEnd)
    {
        List<(int xPos, int yPos)> stack = [];
        Dictionary<(int xPos, int yPos), (int xPos, int yPos)> visitedCells = new();

        (int xStart, int yStart) startCell = (xStart, yStart);
        (int xEnd, int yEnd) endCell = (xEnd, yEnd);

        stack.Add(startCell);

        visitedCells[startCell] = startCell;

        while (!visitedCells.ContainsKey(endCell))
        {
            (int xPos, int yPos) currentCell = stack.RemoveAndGet(0);

            (int xPos, int) northCell = (currentCell.xPos, currentCell.yPos - 1);
            (int, int yPos) eastCell = (currentCell.xPos + 1, currentCell.yPos);
            (int xPos, int) southCell = (currentCell.xPos, currentCell.yPos + 1);
            (int, int yPos) westCell = (currentCell.xPos - 1, currentCell.yPos);

            void checkNorth()
            {
                if (ValidIndex(cells, northCell) &&
                    !visitedCells.ContainsKey(northCell) &&
                    !cells[currentCell.xPos, currentCell.yPos].topWall.exists)
                {
                    stack.Add(northCell);
                    visitedCells[northCell] = currentCell;
                }
            }

            void checkEast()
            {
                if (ValidIndex(cells, eastCell) &&
                    !visitedCells.ContainsKey(eastCell) &&
                    !cells[currentCell.xPos, currentCell.yPos].rightWall.exists)
                {
                    stack.Add(eastCell);
                    visitedCells[eastCell] = currentCell;
                }
            }

            void checkSouth()
            {
                if (ValidIndex(cells, southCell) &&
                    !visitedCells.ContainsKey(southCell) &&
                    !cells[currentCell.xPos, currentCell.yPos].bottomWall.exists)
                {
                    stack.Add(southCell);
                    visitedCells[southCell] = currentCell;
                }
            }

            void checkWest()
            {
                if (ValidIndex(cells, westCell) &&
                    !visitedCells.ContainsKey(westCell) &&
                    !cells[currentCell.xPos, currentCell.yPos].leftWall.exists)
                {
                    stack.Add(westCell);
                    visitedCells[westCell] = currentCell;
                }
            }

            List<Action> wallChecks =
            [
                checkNorth,
                checkEast,
                checkSouth,
                checkWest
            ];

            wallChecks.Shuffle();

            foreach (Action check in wallChecks)
            {
                check.Invoke();
            }
        }

        (int xEnd, int yEnd) tracingCell = endCell;
        List<(int xPos, int yPos)> pathPoints = [];

        while (visitedCells[tracingCell] != tracingCell)
        {
            tracingCell = visitedCells[tracingCell];
            pathPoints.Add(tracingCell);
        }

        pathPoints.Insert(0, endCell);

        return pathPoints;
    }

    #endregion
}
