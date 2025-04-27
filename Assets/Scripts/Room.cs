using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Room
{
    RectInt area;
    public RectInt Area {get {return area; }}

    public Room(RectInt area)
    {
        this.area = area;
    }

    public List<Hallway> CalculateAllPossibleDoorways(int width, int length, int minDistanceFromEdge)
    {
        List<Hallway> hallwayCandidates = new List<Hallway>();

        int top = length - 1;
        int minX = minDistanceFromEdge;
        int maxX = width - minDistanceFromEdge;
        int minY = minDistanceFromEdge;
        int maxY = length - minDistanceFromEdge;
        int bottom = width - 1;

        for (int x = minX; x < maxX; x++)
        {
            hallwayCandidates.Add(new Hallway(HallwayDirection.Bottom, new Vector2Int(x, 0)));
            hallwayCandidates.Add(new Hallway(HallwayDirection.Top, new Vector2Int(x, top)));
        }
        for (int y = minY; y < maxY; y++)
        {
            hallwayCandidates.Add(new Hallway(HallwayDirection.Left, new Vector2Int(0, y)));
            hallwayCandidates.Add(new Hallway(HallwayDirection.Right, new Vector2Int(bottom, y)));
        }
        return hallwayCandidates;
    }
}
