using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "RoomLevelLayout", menuName = "Custom/Procedural Generation/Room Level Layout Configuration", order = 1)]
public class RoomLevelLayoutConfiguration : ScriptableObject
{
    [SerializeField] int width = 64;
    [SerializeField] int length = 64;

    [SerializeField] int roomWidthMin = 3;
    [SerializeField] int roomWidthMax = 8;
    [SerializeField] int roomLengthMin = 5;
    [SerializeField] int roomLengthMax = 12;
    [SerializeField] int doorDistanceFromEdge = 1;
    [SerializeField] int minHallwayLength = 3;
    [SerializeField] int maxHallwayLength = 8;
    [SerializeField] int maxRoomCount = 10;
    [SerializeField] int minRoomDistance = 1;

    public int Width {get => width;}
    public int Length {get => length;}
    public int RoomWidthMin {get => roomWidthMin;}
    public int RoomWidthMax {get => roomWidthMax;}
    public int RoomLengthMin {get => roomLengthMin;}
    public int RoomLengthMax {get => roomLengthMax;}
    public int DoorDistanceFromEdge {get => doorDistanceFromEdge;}
    public int MinHallwayLength {get => minHallwayLength;}
    public int MaxHallwayLength {get => maxHallwayLength;}
    public int MaxRoomCount {get => maxRoomCount;}
    public int MinRoomDistance {get => minRoomDistance;}
}
