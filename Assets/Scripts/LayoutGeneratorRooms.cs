using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Experimental.AI;
using System.Linq;
using Random = System.Random;

public class LayoutGeneratorRooms : MonoBehaviour
{
    [SerializeField] int seed = Environment.TickCount;
    [SerializeField] RoomLevelLayoutConfiguration levelConfig;
    
    
    [SerializeField] GameObject levelLayoutDisplay;

    Random random;
    Level level;
    [SerializeField] List<Hallway> openDoorways;

    [ContextMenu("Generate Level Layout")]
    public void GenerateLevel()
    {
        random = new Random(seed);
        openDoorways = new List<Hallway>();
        level = new Level(levelConfig.Width, levelConfig.Length);
        RectInt roomRect = GetStartRoomRect();
        Debug.Log(roomRect);
        Room room = new Room(roomRect);
        List<Hallway> hallways = room.CalculateAllPossibleDoorways(room.Area.width, room.Area.height, levelConfig.DoorDistanceFromEdge);
        hallways.ForEach (h => h.StartRoom = room);
        hallways.ForEach (h => openDoorways.Add(h));
        level.AddRoom(room);

        Hallway selectedEntryway = openDoorways[random.Next(openDoorways.Count)];
        AddRoom();

        DrawLayout(selectedEntryway, roomRect);
    }
    [ContextMenu("Generate New Seed")]
    public void GenerateNewSeed()
    {
        seed = Environment.TickCount;
    }

    [ContextMenu("Generate New Seed and Level")]
    public void GenerateNewSeedAndLevel()
    {
        seed = Environment.TickCount;
        GenerateLevel();
    }

    RectInt GetStartRoomRect()
    {
        int roomWidth = random.Next(levelConfig.RoomWidthMin, levelConfig.RoomWidthMax);
        int availableWidthX = levelConfig.Width/2 - roomWidth;
        int randomX = random.Next(0, availableWidthX);
        int roomX = randomX + (levelConfig.Width/4);

        int roomLength = random.Next(levelConfig.RoomLengthMin, levelConfig.RoomLengthMax);
        int avaialbleLengthX = levelConfig.Length/2 - roomLength;
        int randomY = random.Next(0, avaialbleLengthX);
        int roomY = randomY + (levelConfig.Length/4);

        return new RectInt(roomX, roomY, roomWidth, roomLength);
    }

    void DrawLayout(Hallway selectedEntryway = null, RectInt roomCandidateRect = new RectInt(), bool isDebug = false)
    {
        var renderer = levelLayoutDisplay.GetComponent<Renderer>();

        var layoutTexture = (Texture2D) renderer.sharedMaterial.mainTexture;

        layoutTexture.Reinitialize(levelConfig.Width, levelConfig.Length);
        levelLayoutDisplay.transform.localScale = new Vector3(levelConfig.Width, levelConfig.Length, 1);
        layoutTexture.FillWithColor(Color.black);
        System.Random colorRandom = new System.Random();

        Array.ForEach(level.Rooms, room => layoutTexture.DrawRectangle(room.Area, Color.white));
        Array.ForEach(level.Hallways, hallway => layoutTexture.DrawLine(hallway.StartPositionAbsolute, hallway.EndPositionAbsolute, Color.white));

        if (isDebug)
        {
            layoutTexture.DrawRectangle(roomCandidateRect, Color.cyan);
            openDoorways.ForEach( hallway => layoutTexture.SetPixel(hallway.StartPositionAbsolute.x, hallway.StartPositionAbsolute.y, hallway.StartDirection.GetColor()));
        }
        
        if (selectedEntryway != null && isDebug)
        {
            layoutTexture.SetPixel(selectedEntryway.StartPositionAbsolute.x, selectedEntryway.StartPositionAbsolute.y, Color.red);
        }

        layoutTexture.SaveAsset();
    }

    Hallway SelectHallwayCandidate(RectInt roomCandidateRect, Hallway entryway)
    {
        Room room = new Room(roomCandidateRect);
        List<Hallway> candidates = room.CalculateAllPossibleDoorways(room.Area.width, room.Area.height, levelConfig.DoorDistanceFromEdge);
        HallwayDirection requiredDirection = entryway.StartDirection.GetOppositeDirection();
        List<Hallway> filteredCandidates = candidates.Where(hallwayCandidate => hallwayCandidate.StartDirection == requiredDirection).ToList();
        return filteredCandidates.Count > 0 ? filteredCandidates[random.Next(0, filteredCandidates.Count)] : null;
    }

    Vector2Int CalculateRoomPosition(Hallway entryway, int roomWidth, int roomLength, int distance, Vector2Int endPosition)
    {
        Vector2Int roomPosition = entryway.StartPositionAbsolute;
        switch (entryway.StartDirection)
        {
            case HallwayDirection.Left:
                roomPosition.x -= distance + roomWidth;
                roomPosition.y -= endPosition.y;
                break;
            case HallwayDirection.Top:
                roomPosition.x -= endPosition.x;
                roomPosition.y += distance + 1;
                break;
            case HallwayDirection.Right:
                roomPosition.x += distance + 1;
                roomPosition.y -= endPosition.y;
                break;
            case HallwayDirection.Bottom:
                roomPosition.x -= endPosition.x;
                roomPosition.y -= distance + roomLength;
                break;
        }
        return roomPosition;
    }

    Room ConstructAdjacentRoom(Hallway selectedEntryway)
    {
        RectInt roomCandidateRect = new RectInt {
            width = random.Next(levelConfig.RoomWidthMin, levelConfig.RoomWidthMax),
            height = random.Next(levelConfig.RoomLengthMin, levelConfig.RoomLengthMax)
        };
        Hallway selectedExit = SelectHallwayCandidate(roomCandidateRect, selectedEntryway);
        if (selectedExit == null)
        {
            return null;
        }
        int distance = random.Next(levelConfig.MinHallwayLength, levelConfig.MaxHallwayLength) + 1;
        Vector2Int roomCandidatePosition = CalculateRoomPosition(selectedEntryway, roomCandidateRect.width, roomCandidateRect.height, distance, selectedExit.StartPosition);
        roomCandidateRect.position = roomCandidatePosition;

        if (!IsRoomCandidateValid(roomCandidateRect))
        {
            return null;
        }
        

        Room newRoom = new Room(roomCandidateRect);
        selectedEntryway.EndRoom = newRoom;
        selectedEntryway.EndPosition = selectedExit.StartPosition;
        return newRoom;
    }

    void AddRoom()
    {
        while (openDoorways.Count > 0 && level.Rooms.Length < levelConfig.MaxRoomCount)
        {
            Hallway selectedEntryway = openDoorways[random.Next(0, openDoorways.Count)];
            Room newRoom = ConstructAdjacentRoom(selectedEntryway);
            if (newRoom == null)
            {
                openDoorways.Remove(selectedEntryway);
                continue;
            }
                level.AddRoom(newRoom);
                level.AddHallway(selectedEntryway);
                selectedEntryway.EndRoom = newRoom;
                List<Hallway> hallways = newRoom.CalculateAllPossibleDoorways(newRoom.Area.width, newRoom.Area.height, levelConfig.DoorDistanceFromEdge);
                hallways.ForEach(h => h.StartRoom = newRoom);
                //hallways.ForEach(h => openDoorways.Add(h));

                openDoorways.Remove(selectedEntryway);
                openDoorways.AddRange(hallways);
                //DrawLayout(selectedEntryway, newRoom.Area);
            
            
        }
    }

    bool IsRoomCandidateValid(RectInt roomCandidateRect)
    {
        RectInt levelRect = new RectInt(1, 1, levelConfig.Width - 2, levelConfig.Length - 2);
        return levelRect.Contains(roomCandidateRect) && !CheckRoomOverlap(roomCandidateRect, level.Rooms, level.Hallways, levelConfig.MinRoomDistance);
    }

    bool CheckRoomOverlap(RectInt roomCandidateRect, Room[] rooms, Hallway[] hallways, int minRoomDistance)
    {
        RectInt paddedRoomRect = new RectInt {
            x = roomCandidateRect.x - minRoomDistance,
            y = roomCandidateRect.y - minRoomDistance,
            width = roomCandidateRect.width + 2 * minRoomDistance,
            height = roomCandidateRect.height + 2 * minRoomDistance
        }; 
        foreach (Room room in rooms)
        {
            if (paddedRoomRect.Overlaps(room.Area))
            {
                return true;
            }
        }
        foreach (Hallway hallway in hallways)
        {
            if (paddedRoomRect.Overlaps(hallway.Area))
            {
                return true;
            }
        }
        return false;
    }
}
