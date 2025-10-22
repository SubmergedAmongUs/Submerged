using System;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using Submerged.Extensions;
using Submerged.Minigames.CustomMinigames.ReconnectPiping.DataStructures;
using Submerged.Minigames.CustomMinigames.ReconnectPiping.Enums;
using Submerged.Minigames.CustomMinigames.ReconnectPiping.MonoBehaviours;
using Submerged.Minigames.MonoBehaviours;
using Submerged.Localization.Strings;
using TMPro;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.ReconnectPiping;

[RegisterInIl2Cpp]
public sealed class ReconnectPipingMinigame(nint ptr) : Minigame(ptr)
{
    public static int LastSeed { get; private set; }

    public int[] seeds = [326907259, 407357077, 2001864847, 20885327, 2118582405, 1545346156, 276236684, 436702103, 1613700476, 1151508687, 1908408660, 782524822, 38597829, 1823140859, 285183977, 116038448, 963051378, 2133468912, 60144147, 2081408537, 127617096, 1520984973, 64894018, 737729816, 1490420376, 1964503931, 663335080, 1055545808, 2048850280, 1834039874, 425877024, 1020089911, 203988300, 904052500, 2097071838, 1815037601, 44206942, 418787165, 2037043467, 402392094, 1095217546, 1769336404, 1911757930, 574371127, 1095048023, 563977301, 1943992207, 1057236064, 1819439226, 2104842910, 568151366, 509610580, 1816391224, 615074661, 845740689, 600989049, 262237723, 33807084, 201414864, 625200086, 2033357123, 770133455, 885000920, 2030125499, 1095048023, 563977301, 1943992207, 1057236064, 1819439226, 2104842910, 568151366, 509610580, 1816391224, 615074661, 845740689, 600989049, 262237723, 33807084, 201414864, 625200086, 2033357123, 770133455, 885000920, 2030125499];

    private Cell[,] _cells;
    private List<(int xPos, int yPos)> _path;

    private void Awake()
    {
        transform.Find("Background/Text").GetComponent<TextMeshPro>().text = Tasks.ReconnectPiping_HighPressure;
    }

    private void Start()
    {
        AssignDefaultCells();

        Dictionary<int, int> dict = new();

        while (_path == null)
        {
            LastSeed = seeds.Random();
            UnityRandom.seed = LastSeed;
            _path?.Clear();
            AssignDefaultCells();

            GenerateMazeDfsNonRecursive();
            _path = PathfindBfs(0, 5, 3, 0);
            int pathLength = _path.Count;
            dict.TryAdd(pathLength, 0);
            dict[pathLength] += 1;
        }

        List<Cell> randomCells = [];
        foreach (Cell cell in _cells) randomCells.Add(cell);

        foreach ((int xPos, int yPos) pos in _path!) randomCells.Remove(_cells[pos.xPos, pos.yPos]);

        foreach (Cell cell in randomCells) cell.pipe.SetRandom();

        _cells[_path[0].xPos, _path[0].yPos].pipe.SetPiece(Direction.North, Direction.South, false);
        _cells[_path[^1].xPos, _path[^1].yPos].pipe.SetPiece(Direction.East, Direction.West, false);

        for (int i = 1; i < _path.Count - 1; i++)
        {
            (int xPos, int yPos) pos = _path[i];
            (int xPos, int yPos) previousPos = _path[i - 1];
            (int xPos, int yPos) nextPos = _path[i + 1];

            Direction firstDirection = FindDirection(pos, previousPos);
            Direction secondDirection = FindDirection(pos, nextPos);
            _cells[pos.xPos, pos.yPos].pipe.SetPiece(firstDirection, secondDirection);
        }
    }

    public void CheckComplete()
    {
        List<(int xPos, int yPos)> visitedPositions = [];

        (int xPos, int yPos) currentPosition = (0, 5);
        Direction lastDirection = Direction.East;

        while (!visitedPositions.Contains(currentPosition) && ValidIndex(_cells, currentPosition))
        {
            if (currentPosition == (3, 0) && lastDirection == Direction.North)
            {
                Complete();

                return;
            }

            visitedPositions.Add(currentPosition);

            Direction oppositeLastDirection = (Direction) (((int) lastDirection + 2) % 4);
            Cell currentCell = _cells[currentPosition.xPos, currentPosition.yPos];
            lastDirection = currentCell.pipe.GetNextDirection(oppositeLastDirection);

            switch (lastDirection)
            {
                case Direction.North:
                    currentPosition.yPos -= 1;

                    break;
                case Direction.East:
                    currentPosition.xPos += 1;

                    break;
                case Direction.South:
                    currentPosition.yPos += 1;

                    break;
                case Direction.West:
                    currentPosition.xPos -= 1;

                    break;
            }
        }
    }

    public void Complete()
    {
        NeedleBehaviour needle = transform.Find("needle").gameObject.AddComponent<NeedleBehaviour>();
        needle.duration = 0.1f;
        needle.initialAngle = 21.14f;
        needle.amount = 20f;
        needle.movementType = NeedleBehaviour.Movement.RandomBounce;

        Transform valve = transform.Find("Valve");
        if (MyNormTask != null)
        {
            MyNormTask.NextStep();
        }
        StartCoroutine(CoStartClose());
        this.StartCoroutine(SpinItem(valve));
    }

    public Direction FindDirection((int xPos, int yPos) start, (int xPos, int yPos) next)
    {
        switch (start.xPos - next.xPos)
        {
            case 1:
                return Direction.West;
            case -1:
                return Direction.East;
            case 0:
                break;
        }

        switch (start.yPos - next.yPos)
        {
            case 1:
                return Direction.North;
            case -1:
                return Direction.South;
            case 0:
                break;
        }

        throw new ArgumentException();
    }

    public void AssignDefaultCells()
    {
        Cell[,] cells = new Cell[4, 6];

        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 6; y++)
            {
                cells[x, y] = new Cell
                {
                    leftWall = x == 0 ? new Wall() : cells[x - 1, y].rightWall,
                    rightWall = new Wall(),
                    topWall = y == 0 ? new Wall() : cells[x, y - 1].bottomWall,
                    bottomWall = new Wall(),
                    pipe = transform.Find($"Pipes (yx)/Pipe {y}{x}").gameObject.AddComponent<PipeCell>()
                };
            }
        }

        cells[0, 5].rightWall.exists = false;
        cells[0, 5].visited = true;

        cells[3, 0].bottomWall.exists = false;
        cells[3, 0].visited = true;

        _cells = cells;
    }

    public void GenerateMazeDfsNonRecursive()
    {
        List<(int xPos, int yPos)> stack = [];

        [HideFromIl2Cpp]
        List<(int xPos, int yPos)> getNeighbours((int xPos, int yPos) position)
        {
            List<(int xPos, int yPos)> neighbours = [];
            Cell northCell = position.yPos == 0 ? null : _cells[position.xPos, position.yPos - 1];
            Cell eastCell = position.xPos == 3 ? null : _cells[position.xPos + 1, position.yPos];
            Cell southCell = position.yPos == 5 ? null : _cells[position.xPos, position.yPos + 1];
            Cell westCell = position.xPos == 0 ? null : _cells[position.xPos - 1, position.yPos];

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
        _cells[currentPosition.xPos, currentPosition.yPos].visited = true;
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
            Cell selectedCell = _cells[selectedPos.xPos, selectedPos.yPos];
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

            // North
            if (ValidIndex(_cells, northCell) &&
                !visitedCells.ContainsKey(northCell) &&
                !_cells[currentCell.xPos, currentCell.yPos].topWall.exists)
            {
                stack.Add(northCell);
                visitedCells[northCell] = currentCell;
            }

            // East
            if (ValidIndex(_cells, eastCell) &&
                !visitedCells.ContainsKey(eastCell) &&
                !_cells[currentCell.xPos, currentCell.yPos].rightWall.exists)
            {
                stack.Add(eastCell);
                visitedCells[eastCell] = currentCell;
            }

            // South
            if (ValidIndex(_cells, southCell) &&
                !visitedCells.ContainsKey(southCell) &&
                !_cells[currentCell.xPos, currentCell.yPos].bottomWall.exists)
            {
                stack.Add(southCell);
                visitedCells[southCell] = currentCell;
            }

            // West
            if (ValidIndex(_cells, westCell) &&
                !visitedCells.ContainsKey(westCell) &&
                !_cells[currentCell.xPos, currentCell.yPos].leftWall.exists)
            {
                stack.Add(westCell);
                visitedCells[westCell] = currentCell;
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

    [HideFromIl2Cpp]
    public IEnumerator SpinItem(Transform item)
    {
        while (item != null)
        {
            Vector3 newRotation = item.localEulerAngles;
            newRotation.z -= 720f * 4 * Time.deltaTime;
            item.localEulerAngles = newRotation;

            yield return null;
        }
    }

    private static bool ValidIndex<T>(T[,] arr, (int xPos, int yPos) pos)
    {
        bool xValid = arr.GetUpperBound(0) >= pos.xPos && pos.xPos >= arr.GetLowerBound(0);
        bool yValid = arr.GetUpperBound(1) >= pos.yPos && pos.yPos >= arr.GetLowerBound(1);

        return xValid && yValid;
    }
}
