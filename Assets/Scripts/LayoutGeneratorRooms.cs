using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Random = System.Random;

public class LayoutGeneratorRooms : MonoBehaviour
{
    [SerializeField] int seed = Environment.TickCount;
    [SerializeField] RoomLevelLayoutConfiguration levelConfig;

    [SerializeField] GameObject levelLayoutDisplay;
    [SerializeField] List<Hallway> openDoorways;

    Random random;
    Level level;

    Dictionary<RoomTemplate, int> availableRooms;

    [ContextMenu("Generate Level Layout")]
    public Level GenerateLevel() {
        SharedLevelData.Instance.ResetRandom();
        random = SharedLevelData.Instance.Rand;
        availableRooms = levelConfig.GetAvailableRooms();
        openDoorways = new List<Hallway>();
        level = new Level(levelConfig.Width, levelConfig.Length);
        
        RoomTemplate startRoomTemplate = availableRooms.Keys.ElementAt(random.Next(0, availableRooms.Count));
        RectInt roomRect = GetStartRoomRect(startRoomTemplate);
        Room room = CreateNewRoom(roomRect, startRoomTemplate);
        List<Hallway> hallways = room.CalculateAllPossibleDoorways(room.Area.width, room.Area.height, levelConfig.DoorDistanceFromEdge);
        hallways.ForEach (h => h.StartRoom = room);
        hallways.ForEach (h => openDoorways.Add(h));
        level.AddRoom(room);

        Hallway selectedEntryway = openDoorways[random.Next(0, openDoorways.Count)];
        AddRooms();
        DrawLayout(selectedEntryway, roomRect);

        int startRoomIndex = random.Next(0, level.Rooms.Length);
        Room randomStartRoom = level.Rooms[startRoomIndex];
        level.playerStartRoom = randomStartRoom;

        return level;
    }

    [ContextMenu("Generate new Seed")]
    public void GenerateNewSeed()
    {
        SharedLevelData.Instance.GenerateSeed();
    }

    [ContextMenu("Generate new Seed and Level")]
    public void GenerateNewSeedAndLevel()
    {
        GenerateNewSeed();
        GenerateLevel();
    }

    RectInt GetStartRoomRect(RoomTemplate roomTemplate) {
        RectInt roomSize = roomTemplate.GenerateRoomCandidateRect(random);

        int roomWidth = roomSize.width;
        int availableWidthX = level.Width / 2 - roomWidth;
        int randomX = random.Next(0, availableWidthX);
        int roomX = randomX + level.Width/4;

        int roomLength = roomSize.height;
        int availableLengthY = level.Length / 2 - roomLength;
        int randomY = random.Next(0, availableLengthY);
        int roomY = randomY + level.Length / 4;

        return new RectInt(roomX, roomY, roomWidth, roomLength);
    }

    
    void DrawLayout(Hallway selectedEntryway = null, RectInt roomCandidateRect = new RectInt(), bool isDebug = false) {
        Renderer renderer = levelLayoutDisplay.GetComponent<Renderer>();

        Texture2D layoutTexture = (Texture2D) renderer.sharedMaterial.mainTexture;

        layoutTexture.Reinitialize(level.Width, level.Length);
        levelLayoutDisplay.transform.localScale = new Vector3(level.Width, level.Length, 1);
        layoutTexture.FillWithColor(Color.black);

        foreach (Room room in level.Rooms) {
            if (room.LayoutTexture != null)
            {
                layoutTexture.DrawTexture(room.LayoutTexture, room.Area);
            } else {
                layoutTexture.DrawRectangle(room.Area, Color.white);
            }
        }
        Array.ForEach(level.Hallways, hallway => layoutTexture.DrawLine(hallway.StartPositionAbsolute, hallway.EndPositionAbsolute, Color.white));

        layoutTexture.ConvertToBlackAndWhite();

        if (isDebug) {
            layoutTexture.DrawRectangle(roomCandidateRect, Color.blue);
            openDoorways.ForEach(hallway => layoutTexture.SetPixel(hallway.StartPositionAbsolute.x, hallway.StartPositionAbsolute.y, hallway.StartDirection.GetColor()));
        }

        if (isDebug && selectedEntryway != null)
        {
            layoutTexture.SetPixel(selectedEntryway.StartPositionAbsolute.x, selectedEntryway.StartPositionAbsolute.y, Color.red);
        }

        layoutTexture.SaveAsset();
    }

    Hallway SelectHallwayCandidate(RectInt roomCandidateRect, RoomTemplate roomTemplate, Hallway entryway)
    {
        Room room = CreateNewRoom(roomCandidateRect, roomTemplate, false);
        List<Hallway> candidates = room.CalculateAllPossibleDoorways(room.Area.width, room.Area.height, levelConfig.DoorDistanceFromEdge);
        HallwayDirection requiredDirection = entryway.StartDirection.GetOppositeDirection();
        List<Hallway> filteredHallwayCandidates = candidates.Where(hc => hc.StartDirection == requiredDirection).ToList();
        return filteredHallwayCandidates.Count > 0 ? filteredHallwayCandidates[random.Next(filteredHallwayCandidates.Count)] : null;
    }

    Vector2Int CalculateRoomPosition(Hallway entryway, int roomWidth, int roomLength, int distance, Vector2Int endPosition) {
        Vector2Int roomPosition = entryway.StartPositionAbsolute;
        switch (entryway.StartDirection) {
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
        RoomTemplate roomTemplate = availableRooms.Keys.ElementAt(random.Next(0, availableRooms.Count));
        RectInt roomCandidateRect = roomTemplate.GenerateRoomCandidateRect(random);

        Hallway selectedExit = SelectHallwayCandidate(roomCandidateRect, roomTemplate, selectedEntryway);
        if (selectedExit == null) { return null; }
        int distance = random.Next(levelConfig.MinHallwayLength, levelConfig.MaxHallwayLength + 1);
        Vector2Int roomCandidatePosition = CalculateRoomPosition(selectedEntryway, roomCandidateRect.width, roomCandidateRect.height, distance, selectedExit.StartPosition);
        roomCandidateRect.position = roomCandidatePosition;

        if (!IsRoomCandidateValid(roomCandidateRect))
        {
            return null;
        }

        Room newRoom = CreateNewRoom(roomCandidateRect, roomTemplate);
        selectedEntryway.EndRoom = newRoom;
        selectedEntryway.EndPosition = selectedExit.StartPosition;
        return newRoom;
    }

    bool IsRoomCandidateValid(RectInt roomCandidateRect)
    {
        RectInt levelRect = new RectInt(1, 1, levelConfig.Width - 2, levelConfig.Length - 2);
        return levelRect.Contains(roomCandidateRect) && 
               !CheckRoomOverlap(roomCandidateRect, level.Rooms, level.Hallways, levelConfig.MinRoomDistance);
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

    void AddRooms()
    {
        while (openDoorways.Count > 0 && level.Rooms.Length < levelConfig.MaxRoomCount && availableRooms.Count > 0)
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
            List<Hallway> newOpenHallways = newRoom.CalculateAllPossibleDoorways(newRoom.Area.width, newRoom.Area.height, levelConfig.DoorDistanceFromEdge);
            newOpenHallways.ForEach(hallway => hallway.StartRoom = newRoom);

            openDoorways.Remove(selectedEntryway);
            openDoorways.AddRange(newOpenHallways);
        }
    }

    private void UseUpRoomTemplate(RoomTemplate roomTemplate)
    {
        availableRooms[roomTemplate] -= 1;
        if (availableRooms[roomTemplate] == 0)
        {
            availableRooms.Remove(roomTemplate);
        }
    }

    Room CreateNewRoom(RectInt roomCandidateRect, RoomTemplate roomTemplate, bool useUp = true) {
        if (useUp)
        {
            UseUpRoomTemplate(roomTemplate);
        }
        if (roomTemplate.LayoutTexture == null)
        {
            return new Room(roomCandidateRect);
        }
        else
        {
            return new Room(roomCandidateRect.x, roomCandidateRect.y, roomTemplate.LayoutTexture);
        }
    }

}
