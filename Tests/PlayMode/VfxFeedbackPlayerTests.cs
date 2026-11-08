using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Astra.Pulse.Tests
{
    public sealed class VfxFeedbackPlayerTests
    {
        private readonly System.Collections.Generic.List<Object> _toDestroy = new System.Collections.Generic.List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < _toDestroy.Count; i++)
            {
                if (_toDestroy[i] != null) Object.Destroy(_toDestroy[i]);
            }
            _toDestroy.Clear();
        }

        private GameObject CreatePrefab(string name, bool withParticles, bool loop)
        {
            GameObject prefab = new GameObject(name);
            prefab.SetActive(false);
            if (withParticles)
            {
                ParticleSystem system = prefab.AddComponent<ParticleSystem>();
                ParticleSystem.MainModule main = system.main;
                main.loop = loop;
                main.duration = 0.2f;
                main.startLifetime = 0.1f;
            }
            _toDestroy.Add(prefab);
            return prefab;
        }

        private GameObject FindInstance(string name, GameObject prefab)
        {
            GameObject[] all = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != prefab && all[i].name == name + "(Clone)") return all[i];
            }
            return null;
        }

        private static FeedbackResult PlayVfx(VfxFeedbackPlayer player, GameObject prefab, Vector3 position = default, Transform target = null)
        {
            VfxFeedback entry = new VfxFeedback(prefab);
            return player.Play(entry, new FeedbackRequest(entry) { Position = position, Target = target });
        }

        [UnityTest]
        public IEnumerator Play_InstantiatesAtTheRequestPosition()
        {
            VfxFeedbackPlayer player = new VfxFeedbackPlayer();
            GameObject prefab = CreatePrefab("VfxPosition", false, false);

            FeedbackResult result = PlayVfx(player, prefab, new Vector3(1f, 2f, 3f));
            yield return null;

            Assert.That(result.Success, Is.True, result.Message);
            GameObject instance = FindInstance("VfxPosition", prefab);
            Assert.That(instance, Is.Not.Null);
            Assert.That(instance.transform.position, Is.EqualTo(new Vector3(1f, 2f, 3f)));
            _toDestroy.Add(instance);
        }

        [UnityTest]
        public IEnumerator Play_TargetPositionWinsOverRequestPosition()
        {
            VfxFeedbackPlayer player = new VfxFeedbackPlayer();
            GameObject prefab = CreatePrefab("VfxTarget", false, false);
            GameObject target = new GameObject("VfxTargetAnchor");
            target.transform.position = new Vector3(9f, 8f, 7f);
            _toDestroy.Add(target);

            PlayVfx(player, prefab, Vector3.zero, target.transform);
            yield return null;

            GameObject instance = FindInstance("VfxTarget", prefab);
            Assert.That(instance, Is.Not.Null);
            Assert.That(instance.transform.position, Is.EqualTo(new Vector3(9f, 8f, 7f)));
            _toDestroy.Add(instance);
        }

        [UnityTest]
        public IEnumerator Play_NonLoopingParticleSystemIsDestroyedAfterItsDuration()
        {
            VfxFeedbackPlayer player = new VfxFeedbackPlayer();
            GameObject prefab = CreatePrefab("VfxFinite", true, false);

            PlayVfx(player, prefab);
            yield return null;
            GameObject instance = FindInstance("VfxFinite", prefab);
            Assert.That(instance, Is.Not.Null);

            yield return new WaitForSecondsRealtime(0.8f);

            Assert.That(instance == null, Is.True, "duration + start lifetime is ~0.3s, the instance must be gone");
        }

        [UnityTest]
        public IEnumerator Play_LoopingPrefabStillGetsAFiniteLifetime()
        {
            VfxFeedbackPlayer player = new VfxFeedbackPlayer();
            GameObject prefab = CreatePrefab("VfxLoop", true, true);

            PlayVfx(player, prefab);
            yield return null;
            GameObject instance = FindInstance("VfxLoop", prefab);

            Assert.That(instance, Is.Not.Null);
            Assert.That(VfxFeedbackPlayer.FallbackLifetimeSeconds, Is.GreaterThan(0f).And.LessThan(float.PositiveInfinity));
            // The destruction is scheduled (Destroy with a delay): the object is still alive now.
            Assert.That(instance != null, Is.True);
            _toDestroy.Add(instance);
        }

        [Test]
        public void Play_WithoutPrefabFailsWithoutSpawning()
        {
            RecordingLogger logger = new RecordingLogger();
            VfxFeedbackPlayer player = new VfxFeedbackPlayer(logger);

            FeedbackResult result = PlayVfx(player, null);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("VFX feedback has no prefab."));
            Assert.That(logger.Errors, Is.Empty);
        }

        [Test]
        public void UnityAudioPlayer_WithoutClipFails()
        {
            UnityAudioFeedbackPlayer player = new UnityAudioFeedbackPlayer();
            AudioFeedback entry = new AudioFeedback(null);

            FeedbackResult result = player.Play(entry, new FeedbackRequest(entry));

            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Audio feedback has no clip."));
        }

        [Test]
        public void AnchorBehaviour_FallsBackToItsOwnTransform()
        {
            GameObject go = new GameObject("Anchor");
            _toDestroy.Add(go);
            FeedbackAnchorBehaviour anchor = go.AddComponent<FeedbackAnchorBehaviour>();

            Assert.That(anchor.FeedbackAnchor, Is.SameAs(go.transform));

            GameObject child = new GameObject("AnchorChild");
            _toDestroy.Add(child);
            child.transform.SetParent(go.transform, false);
            typeof(FeedbackAnchorBehaviour)
                .GetField("_anchor", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .SetValue(anchor, child.transform);

            Assert.That(anchor.FeedbackAnchor, Is.SameAs(child.transform));
        }

        [UnityTest]
        public IEnumerator MinimalProjectSmoke_ImpactProfileWithUnityAudioAndVfxNeedsNoFramework()
        {
            PulseService service = new PulseService();
            service.RegisterPlayer(new VfxFeedbackPlayer());
            service.RegisterPlayer(new UnityAudioFeedbackPlayer());

            GameObject prefab = CreatePrefab("VfxImpact", true, false);
            AudioClip clip = AudioClip.Create("PulseImpact", 44100, 1, 44100, false);
            _toDestroy.Add(clip);

            FeedbackResult result = service.Play(new FeedbackRequest(new VfxFeedback(prefab), new AudioFeedback(clip, 0.5f))
            {
                Position = Vector3.up,
            });
            yield return null;

            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(result.Message, Is.EqualTo("2 of 2 feedback entries played."));
            GameObject vfxInstance = FindInstance("VfxImpact", prefab);
            Assert.That(vfxInstance, Is.Not.Null);
            _toDestroy.Add(vfxInstance);
            // PlayClipAtPoint creates a temporary "One shot audio" object.
            GameObject oneShotAudio = GameObject.Find("One shot audio");
            Assert.That(oneShotAudio, Is.Not.Null);
            _toDestroy.Add(oneShotAudio);
        }
    }
}
