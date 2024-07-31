using UnityEngine;

namespace Submerged.Extensions;

public static class TextureExtensions
{
    public static Texture2D GetReadable(this Texture2D texture)
    {
        if (texture.isReadable) return texture;

        Texture2D copy = new(texture.width, texture.height);
        //Graphics.CopyTexture(tex, copy); // copies GPU to GPU, we can't read that here in CPU land

        RenderTexture tmp = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
        Graphics.Blit(texture, tmp);

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = tmp;

        copy.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        copy.Apply();

        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(tmp);

        return copy;
    }
}
