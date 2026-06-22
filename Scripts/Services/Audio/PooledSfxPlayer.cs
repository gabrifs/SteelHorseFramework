using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SteelHorse.Framework.Services.Audio
{
    public class PooledSfxPlayer : MonoBehaviour, ISfxPlayer
    {
        [SerializeField] private int _poolSize = 24;

        private AudioSource[] _pool;
        private Coroutine[] _loopRoutines;
        private int[] _generations;
        private Transform[] _parents;
        private readonly List<int> _trackedIndices = new();
        private int _nextIndex;

        private void Awake()
        {
            BuildPool();
        }

        private void Start()
        {
            GameManagers.Instance.Services.AudioManagerService.RegisterWorldSfxPlayer(this);
        }

        private void OnDestroy()
        {
            GameManagers.Instance.Services.AudioManagerService.UnregisterWorldSfxPlayer(this);
        }

        private void Update()
        {
            for (int i = 0; i < _trackedIndices.Count; i++)
            {
                int index = _trackedIndices[i];
                if (_parents[index] != null)
                    _pool[index].transform.position = _parents[index].position;
            }
        }

        private void BuildPool()
        {
            _pool = new AudioSource[_poolSize];
            _loopRoutines = new Coroutine[_poolSize];
            _generations = new int[_poolSize];
            _parents = new Transform[_poolSize];

            for (int i = 0; i < _poolSize; i++)
            {
                GameObject sourceObject = new GameObject($"PooledSource_{i}");
                sourceObject.transform.SetParent(transform);

                AudioSource source = sourceObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 1f;

                _pool[i] = source;
            }
        }

        public SfxHandle Play(SfxCue cue, Transform parent = null, Vector3? position = null)
        {
            int index = _nextIndex;
            _nextIndex = (_nextIndex + 1) % _pool.Length;

            AudioSource source = _pool[index];
            StopLoop(index);
            _generations[index]++;

            PositionSource(index, parent, position);

            source.outputAudioMixerGroup = cue.OutputGroup;

            if (cue.Looped)
            {
                if (parent != null)
                    BeginTracking(index, parent);

                _loopRoutines[index] = StartCoroutine(PlayLooped(cue, source, index));
            }
            else
            {
                PlayClip(cue, source);
            }

            return new SfxHandle(this, index, _generations[index]);
        }

        public void Stop(SfxHandle handle)
        {
            if (_generations[handle.VoiceIndex] != handle.Generation)
                return;

            int index = handle.VoiceIndex;
            StopLoop(index);
            _pool[index].Stop();
        }

        private void PositionSource(int index, Transform parent, Vector3? position)
        {
            Transform sourceTransform = _pool[index].transform;
            if (position.HasValue)
                sourceTransform.position = position.Value;
            else if (parent != null)
                sourceTransform.position = parent.position;
        }

        private void BeginTracking(int index, Transform parent)
        {
            _parents[index] = parent;
            _trackedIndices.Add(index);
        }

        private void StopLoop(int index)
        {
            if (_loopRoutines[index] == null)
                return;

            StopCoroutine(_loopRoutines[index]);
            _loopRoutines[index] = null;

            if (_parents[index] != null)
            {
                _parents[index] = null;
                _trackedIndices.Remove(index);
            }
        }

        private void PlayClip(SfxCue cue, AudioSource source)
        {
            source.pitch = cue.GetPitch();
            source.volume = cue.GetVolume();
            source.clip = cue.GetNextClip();
            source.Play();
        }

        private IEnumerator PlayLooped(SfxCue cue, AudioSource source, int index)
        {
            while (true)
            {
                PlayClip(cue, source);
                yield return new WaitForSeconds(source.clip.length / source.pitch);
            }
        }
    }
}
