using Il2CppInterop.Runtime.Runtime;
using Il2CppSystem.Collections.Generic;

namespace Submerged.IL2CPP;

public unsafe struct Il2CppListEnumerable<T> where T : CppObject
{
    private struct Il2CppListStruct
    {
#pragma warning disable CS0169
        private IntPtr _ptr1;
        private IntPtr _ptr2;
#pragma warning restore CS0169

#pragma warning disable CS0649
        public IntPtr items;
        public int size;
#pragma warning restore CS0649
    }

    private static readonly int _elemSize;
    private static readonly int _offset;

    static Il2CppListEnumerable()
    {
        _elemSize = IntPtr.Size;
        _offset = 4 * IntPtr.Size;
    }

    private readonly IntPtr _arrayPointer;
    private readonly int _count;
    private int _index = -1;

    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    public Il2CppListEnumerable(List<T> list)
    {
        Il2CppListStruct* listStruct = (Il2CppListStruct*) list.Pointer;
        _count = listStruct->size;
        _arrayPointer = listStruct->items;

        Current = default;
    }

    public T Current { get; private set; }

    public bool MoveNext()
    {
        if (++_index >= _count) return false;
        IntPtr refPtr = *(IntPtr*) IntPtr.Add(IntPtr.Add(_arrayPointer, _offset), _index * _elemSize);
        Current = Il2CppObjectPool.Get<T>(refPtr);

        return true;
    }

    public Il2CppListEnumerable<T> GetEnumerator() => this;
}
