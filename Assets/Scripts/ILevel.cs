using UnityEngine;

public interface ILevel {
    int Length {get;}
    int Width {get;}

    bool IsBlocked(int x, int y);
    int FLoor(int x, int y);
}
