using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Hallway
{
    Vector2Int startPosition;
    Vector2Int endPosition;

    HallwayDirection startDirection;
    HallwayDirection endDirection;

    Room startRoom;
    Room endRoom;

    public Room StartRoom {get =>startRoom; set => startRoom = value;}
    public Room EndRoom {get => endRoom; set => endRoom = value;}

    public Vector2Int StartPositionAbsolute {get => startPosition + startRoom.Area.position;}
    public Vector2Int EndPositionAbsolute {get => endPosition + endRoom.Area.position;}
    public HallwayDirection StartDirection {get => startDirection;}
    public HallwayDirection EndDirection {get => endDirection; set => endDirection = value;}
    public Vector2Int StartPosition {get => startPosition; set => startPosition = value;}
    public Vector2Int EndPosition {get => endPosition; set => endPosition = value;}
    public RectInt Area {
        get {
                int x = Mathf.Min(StartPositionAbsolute.x, EndPositionAbsolute.x);
                int y = Mathf.Min(StartPositionAbsolute.y, EndPositionAbsolute.y);
                int width = Mathf.Max(1, Mathf.Abs(StartPositionAbsolute.x - EndPositionAbsolute.x));
                int height = Mathf.Max(1, Mathf.Abs(StartPositionAbsolute.y - EndPositionAbsolute.y));
                if (StartPositionAbsolute.x == EndPositionAbsolute.x)
                {
                    y++;
                    height--;
                }
                if (StartPositionAbsolute.y == EndPositionAbsolute.y)
                {
                    x++;
                    width--;
                }
                return new RectInt(x, y, width, height);
            }
        }

    public Hallway(HallwayDirection startDirection, Vector2Int startPosition, Room startRoom = null)
    {
        this.startDirection = startDirection;
        this.startPosition = startPosition;
        this.startRoom = startRoom;
    }
}
