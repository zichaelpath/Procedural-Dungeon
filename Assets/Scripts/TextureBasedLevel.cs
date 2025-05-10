using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class TextureBasedLevel : ILevel
{
    Texture2D levelTexture;
    public TextureBasedLevel(Texture2D levelTexture)
    {
        this.levelTexture = levelTexture;
    }
    public int Length => levelTexture.height;

    public int Width => levelTexture.width;

    public int FLoor(int x, int y)
    {
        return 0;
    }

    public bool IsBlocked(int x, int y)
    {
        if (x < 0 || y < 0 || x >= levelTexture.width || y >= levelTexture.height)
        {
            return true;
        }
        Color pixel = levelTexture.GetPixel(x, y);
        return Color.black == pixel ? true : false;
    }
}
