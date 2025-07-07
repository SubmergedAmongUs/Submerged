using System.Collections.Generic;
using System.Linq;
using Hazel;
using Il2CppInterop.Runtime.Injection;
using Reactor.Utilities.Attributes;
using AU = Submerged.BaseGame.Interfaces.AU;

namespace Submerged.Systems.SecuritySabotage;

[RegisterInIl2Cpp(typeof(ISystemType))]
public sealed class SubmarineSecuritySabotageSystem(nint ptr) : CppObject(ptr), AU.ISystemType
{
    public List<byte> fixedCams =
    [
        0,
        1,
        2,
        3,
        4,
        5,
        6
    ];

    public SubmarineSecuritySabotageSystem() : this(ClassInjector.DerivedConstructorPointer<SubmarineSecuritySabotageSystem>())
    {
        ClassInjector.DerivedConstructorBody(this);
        Instance = this;
    }

    public static SubmarineSecuritySabotageSystem Instance { get; private set; }

    public bool IsDirty { get; set; }

    public void Deteriorate(float deltaTime) { }

    public void Deserialize(MessageReader reader, bool initialState)
    {
        fixedCams = reader.ReadBytesAndSize().ToList();
    }

    public void MarkClean()
    {
        IsDirty = false;
    }

    public void Serialize(MessageWriter writer, bool initialState)
    {
        writer.WriteBytesAndSize(fixedCams.ToArray());
        IsDirty = initialState;
    }

    public void UpdateSystem(PlayerControl player, MessageReader msgReader)
    {
        int amountInt = msgReader.ReadByte();
        byte camId = (byte) (amountInt / 10);
        int state = amountInt % 10;

        switch (state)
        {
            case 0:
                fixedCams.RemoveAll(c => c == camId);

                break;
            case 1:
                if (!fixedCams.Contains(camId)) fixedCams.Add(camId);

                break;
        }

        IsDirty = true;
    }

    public bool IsSabotaged(byte camId) => !fixedCams.Contains(camId);
}
