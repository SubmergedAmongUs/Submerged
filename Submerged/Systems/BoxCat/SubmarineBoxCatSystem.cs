using Hazel;
using Il2CppInterop.Runtime.Injection;
using Reactor.Utilities.Attributes;
using Submerged.Extensions;
using UnityEngine;
using AU = Submerged.BaseGame.Interfaces.AU;

namespace Submerged.Systems.BoxCat;

[RegisterInIl2Cpp(typeof(ISystemType))]
public sealed class SubmarineBoxCatSystem(nint ptr) : CppObject(ptr), AU.ISystemType
{
    private byte _position = byte.MaxValue;

    public SubmarineBoxCatSystem() : this(ClassInjector.DerivedConstructorPointer<SubmarineBoxCatSystem>())
    {
        ClassInjector.DerivedConstructorBody(this);

        Instance = this;

        MoveCat();
    }

    public static SubmarineBoxCatSystem Instance { get; private set; }

    public bool IsDirty { get; private set; }

    public void Deteriorate(float deltaTime) { }

    public void Deserialize(MessageReader reader, bool initialState)
    {
        _position = reader.ReadByte();

        TriggerCatUpdate();
    }

    public void MarkClean()
    {
        IsDirty = false;
    }

    public void Serialize(MessageWriter writer, bool initialState)
    {
        writer.Write(_position);

        IsDirty = initialState;
    }

    public void UpdateSystem(PlayerControl player, MessageReader msgReader) { }

    public void MoveCat()
    {
        if (!AmongUsClient.Instance.AmHost) return;

        if (_position == byte.MaxValue)
            _position = (byte) UnityRandom.RandomRangeInt(0, 2);
        else
            _position = (byte) ((_position + 1) % 2);

        IsDirty = true;

        TriggerCatUpdate();
    }

    public void TriggerCatUpdate()
    {
        Transform cats = GameObject.Find("WillsCat").transform.Find("Cats");

        for (int i = 0; i < cats.childCount; i++)
        {
            Transform cat = cats.GetChild(i);
            cat.gameObject.SetActive(i == _position);
            cat.SetZPos(-0.0002f);
        }

        Transform boxes = GameObject.Find("WillsCat").transform.Find("Boxes");

        for (int i = 0; i < boxes.childCount; i++)
        {
            Transform box = boxes.GetChild(i);
            box.SetZPos(-0.0001f);
        }
    }
}
