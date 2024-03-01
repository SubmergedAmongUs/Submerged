using System.Collections.Generic;
using Submerged.IL2CPP;
using I = Il2CppSystem.Collections.Generic;

namespace Submerged.Extensions;

public static class ListExtensions
{
    public static Il2CppListEnumerable<T> GetFastEnumerator<T>(this I.List<T> list) where T : CppObject => new(list);

    public static T RemoveAndGet<T>(this List<T> list, int index)
    {
        T result = list[index];
        list.RemoveAt(index);

        return result;
    }

    public static IList<T> Shuffle<T>(this IList<T> self, int startAt = 0)
    {
        for (int i = startAt; i < self.Count - 1; i++)
        {
            T t = self[i];
            int num = UnityRandom.Range(i, self.Count);
            self[i] = self[num];
            self[num] = t;
        }

        return self;
    }

    public static T Random<T>(this IList<T> self)
    {
        if (self.Count > 0)
        {
            return self[UnityRandom.Range(0, self.Count)];
        }

        return default;
    }

    public static T[] ShuffleCopy<T>(this T[] list)
    {
        int count = list.Length;
        T[] result = new T[count];
        result[0] = list[0];

        for (int i = 1; i < count; ++i)
        {
            int j = UnityRandom.Range(0, i + 1);

            if (j != i)
            {
                result[i] = result[j];
            }

            result[j] = list[i];
        }

        return result;
    }
}
