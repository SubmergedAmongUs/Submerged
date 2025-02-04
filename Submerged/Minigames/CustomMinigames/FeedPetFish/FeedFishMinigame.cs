using System.Collections.Generic;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using Submerged.BaseGame.Extensions;
using Submerged.Extensions;
using Submerged.Minigames.CustomMinigames.FeedPetFish.MonoBehaviours;
using Submerged.Minigames.MonoBehaviours;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.FeedPetFish;

[RegisterInIl2Cpp]
public sealed class FeedFishMinigame(nint ptr) : Minigame(ptr)
{
    private readonly bool[] _completedSpecies = new bool[2];
    private readonly List<DraggableFeeder> _fishFood = [];
    private AudioSource _audio;
    private List<Transform> _fishGroup;

    private MinigameProperties _minigameProperties;

    private int[] _selectedSpecies = new int[2];

    private void Start()
    {
        _minigameProperties = GetComponent<MinigameProperties>();
        _audio = SoundManager.Instance.PlayNamedSound("Pump", _minigameProperties.audioClips[0], true, SoundManager.Instance.SfxChannel);
        _audio.volume = 0.25f;

        _fishGroup = transform.Find("Background/Fish").GetChildren().ToList();
        Transform rotationLookAtPoint = transform.Find("UI/RotationLookAtPoint");
        ParticleSystem shakerParticles = transform.Find("UI/ShakerParticleHome/ShakerParticles").GetComponent<ParticleSystem>();
        BoxCollider2D activatorArea = transform.Find("UI/ActivatorArea").GetComponent<BoxCollider2D>();

        foreach (Transform t in transform.Find("UI/Feed").GetChildren())
        {
            DraggableFeeder draggableFeeder = t.GetChild(0).gameObject.AddComponent<DraggableFeeder>();
            draggableFeeder.owner = this;
            draggableFeeder.rotationTarget = rotationLookAtPoint;
            draggableFeeder.fishFood = shakerParticles;
            // draggableFeeder.fishFoodParent = shakerParticles.transform.parent;
            draggableFeeder.activatedArea = activatorArea;
            // draggableFeeder.shakeDuration = 3f;
            _fishFood.Add(draggableFeeder);
        }

        _selectedSpecies = Enumerable.Range(0, 5).ToList().Shuffle().Take(2).ToArray();

        _completedSpecies[0] = false;
        _completedSpecies[1] = false;

        for (int i = 0; i < _fishGroup.Count; i++)
        {
            if (i == _selectedSpecies[0] || i == _selectedSpecies[1])
            {
                int index = i == _selectedSpecies[0] ? 0 : 1;
                _fishGroup[i].gameObject.SetActive(true);
                this.StartCoroutine(RandomBob(_fishGroup[i]));
                _fishFood[i].SetCorrectFoodStatus(true, index);
            }
            else
            {
                _fishGroup[i].gameObject.SetActive(false);
                _fishFood[i].SetCorrectFoodStatus(false);
            }
        }
    }

    public void UpdateCompletedStep(int index)
    {
        _completedSpecies[index] = true;

        if (_completedSpecies[0] && _completedSpecies[1] && amClosing == CloseState.None)
        {
            if (MyNormTask != null)
            {
                MyNormTask.NextStep();
            }
            StartCoroutine(CoStartClose());
        }
    }

    [HideFromIl2Cpp]
    public IEnumerator RandomBob(Transform trans)
    {
        Vector3 initialPosition = trans.localPosition;
        float rng = UnityRandom.Range(-0.15f, 0.15f);
        float duration = UnityRandom.Range(1f, 3f);

        while (amClosing != CloseState.Closing)
        {
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                trans.localPosition = Vector3.Lerp(initialPosition, new Vector3(initialPosition.x, initialPosition.y + rng, initialPosition.z), t / duration);

                yield return null;
            }

            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                trans.localPosition = Vector3.Lerp(new Vector3(initialPosition.x, initialPosition.y + rng, initialPosition.z), initialPosition, t / duration);

                yield return null;
            }

            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                trans.localPosition = Vector3.Lerp(initialPosition, new Vector3(initialPosition.x, initialPosition.y - rng, initialPosition.z), t / duration);

                yield return null;
            }

            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                trans.localPosition = Vector3.Lerp(new Vector3(initialPosition.x, initialPosition.y - rng, initialPosition.z), initialPosition, t / duration);

                yield return null;
            }
        }
    }

    public override void Close()
    {
        _audio.Stop();
        this.BaseClose();
    }
}
