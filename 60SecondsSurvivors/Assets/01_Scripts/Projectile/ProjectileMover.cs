using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace _60SecondsSurvivors.Projectile
{
    [DefaultExecutionOrder(-50)]
    public class ProjectileMover : MonoBehaviour
    {
        public static ProjectileMover Instance { get; private set; }

        private TransformAccessArray _transforms;
        private List<ProjectileBase> _entries = new List<ProjectileBase>(128);

        private NativeArray<Vector3> _velocitiesNative;
        private int _capacity = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            EnsureEntriesInitialized();
            EnsureTransformInitialized();
        }

        private void OnDestroy()
        {
            if (_velocitiesNative.IsCreated)
                _velocitiesNative.Dispose();

            if (_transforms.isCreated)
                _transforms.Dispose();

            if (Instance == this) Instance = null;

            _entries?.Clear();
            _entries = null;
        }

        private void EnsureEntriesInitialized()
        {
            if (_entries == null)
                _entries = new List<ProjectileBase>(128);
        }

        private void EnsureTransformInitialized()
        {
            if (!_transforms.isCreated)
            {
                _transforms = new TransformAccessArray(0);
            }
        }

        private void RebuildTransformArrayFromEntries()
        {
            if (_transforms.isCreated)
            {
                _transforms.Dispose();
            }

            int count = _entries != null ? _entries.Count : 0;
            if (count == 0)
            {
                _transforms = new TransformAccessArray(0);
                return;
            }

            var transforms = new Transform[count];
            for (int i = 0; i < count; i++)
            {
                var e = _entries[i];
                transforms[i] = e != null ? e.transform : null;
            }

            _transforms = new TransformAccessArray(transforms);
        }

        public void Register(ProjectileBase p)
        {
            if (p == null) return;

            if (_entries.Contains(p)) return;

            _entries.Add(p);

            EnsureTransformInitialized();

            if (_transforms.isCreated && _transforms.length == _entries.Count - 1)
            {
                _transforms.Add(p.transform);
            }
            else
            {
                RebuildTransformArrayFromEntries();
            }

            EnsureNativeCapacity(_entries.Count);
        }

        public void Unregister(ProjectileBase p)
        {
            if (p == null) return;

            int idx = _entries.IndexOf(p);
            if (idx < 0) return;

            int last = _entries.Count - 1;

            if (idx != last)
            {
                _entries[idx] = _entries[last];
            }
            _entries.RemoveAt(last);

            if (!_transforms.isCreated || _transforms.length != last + 1)
            {
                RebuildTransformArrayFromEntries();
                return;
            }

            _transforms.RemoveAtSwapBack(idx);

            if (_transforms.length != _entries.Count)
            {
                RebuildTransformArrayFromEntries();
            }
        }

        private void EnsureNativeCapacity(int required)
        {
            if (required <= _capacity) return;

            if (_velocitiesNative.IsCreated)
                _velocitiesNative.Dispose();

            _capacity = required + 16;
            _velocitiesNative = new NativeArray<Vector3>(_capacity, Allocator.Persistent);
        }

        private void Update()
        {
            int count = _entries.Count;
            if (count == 0) return;

            EnsureNativeCapacity(count);

            if (!_transforms.isCreated || _transforms.length != count)
            {
                RebuildTransformArrayFromEntries();
            }

            for (int i = 0; i < count; i++)
            {
                var e = _entries[i];
                _velocitiesNative[i] = e != null ? e.GetVelocity() : Vector3.zero;
            }

            var job = new MoveJob
            {
                velocities = _velocitiesNative,
                deltaTime = Time.deltaTime
            };

            var handle = job.Schedule(_transforms);
            handle.Complete();
        }

        [BurstCompile]
        private struct MoveJob : IJobParallelForTransform
        {
            [ReadOnly] public NativeArray<Vector3> velocities;
            public float deltaTime;

            public void Execute(int index, TransformAccess transform)
            {
                Vector3 pos = transform.position;
                pos += velocities[index] * deltaTime;
                transform.position = pos;
            }
        }
    }
}