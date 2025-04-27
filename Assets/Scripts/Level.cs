using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class Level
{
    int width;
    int length;
    public int Width => width;
    public int Length  => length;

    public Room[] Rooms => rooms.ToArray();
    public Hallway[] Hallways => hallways.ToArray();

    List<Room> rooms;
    List<Hallway> hallways;

    public Level(int width, int length)
    {
        this.width = width;
        this.length = length;
        rooms = new List<Room>();
        hallways = new List<Hallway>();
    }

    public void AddRoom(Room newRoom) => rooms.Add(newRoom);
    public void AddHallway(Hallway newHallway) => hallways.Add(newHallway);
}
