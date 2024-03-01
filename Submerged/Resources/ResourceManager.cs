using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Submerged.Resources;

public static class ResourceManager
{
    public static readonly Dictionary<string, Sprite> spriteCache = new();

    private static readonly Assembly _resourceManagerAssembly = typeof(ResourceManager).Assembly;

    public static unsafe Il2CppStructArray<byte> GetEmbeddedBytes(string name)
    {
        string path = _resourceManagerAssembly.GetManifestResourceNames().FirstOrDefault(n => n.Contains(name));

        if (path == default) return null;

        Stream manifestResourceStream = _resourceManagerAssembly.GetManifestResourceStream(path)!;
        long length = manifestResourceStream.Length;
        Il2CppStructArray<byte> array = new(manifestResourceStream.Length);

        Span<byte> span = new(nint.Add(array.Pointer, nint.Size * 4).ToPointer(), (int) length);
        // ReSharper disable once MustUseReturnValue
        manifestResourceStream.Read(span);

        return array;
    }

    public static AssetBundle GetAssetBundle(string name)
    {
        Il2CppStructArray<byte> buffer = GetEmbeddedBytes(name);

        if (buffer == null) return null;

        AssetBundle assetBundle = AssetBundle.LoadFromMemory(buffer);

        return assetBundle;
    }

    public static Texture2D GetTexture(string name)
    {
        Il2CppStructArray<byte> buffer = GetEmbeddedBytes(name);

        if (buffer == null) return null;

        Texture2D tex = new(2, 2, TextureFormat.ARGB32, false);
        ImageConversion.LoadImage(tex, buffer, false);

        return tex;
    }

    private static Sprite GetSprite(string name, float ppu = 100)
    {
        Texture2D tex = GetTexture(name);
        return tex == null ? null : Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), ppu);
    }

    public static void CacheSprite(string name, float ppu = 100, string cacheName = null)
    {
        cacheName ??= name;
        Sprite sprite = GetSprite(name, ppu);
        sprite.DontUnload();
        spriteCache[cacheName] = sprite;
    }
}
