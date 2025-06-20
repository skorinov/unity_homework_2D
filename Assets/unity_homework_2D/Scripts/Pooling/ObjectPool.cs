using System.Collections.Generic;
using UnityEngine;

namespace Pooling
{
    public class ObjectPool<T> where T : Component
    {
        private readonly GameObject _prefab;
        private readonly Transform _parent;
        private readonly Queue<T> _availableObjects = new();
        private readonly HashSet<T> _activeObjects = new();

        public int ActiveCount => _activeObjects.Count;
        public int AvailableCount => _availableObjects.Count;

        public ObjectPool(GameObject prefab, Transform parent = null, int initialSize = 10)
        {
            _prefab = prefab;
            _parent = parent;

            for (int i = 0; i < initialSize; i++)
            {
                CreateNewObject();
            }
        }

        private T CreateNewObject()
        {
            var instance = Object.Instantiate(_prefab, _parent);
            var component = instance.GetComponent<T>();

            if (component is IPoolable poolable)
            {
                poolable.OnCreatedInPool();
            }
            
            instance.SetActive(false);
            _availableObjects.Enqueue(component);
            
            return component;
        }

        public T Get()
        {
            T obj = _availableObjects.Count > 0 ? _availableObjects.Dequeue() : CreateNewObject();

            if (!obj) return obj;
            
            _activeObjects.Add(obj);
            obj.gameObject.SetActive(true);

            if (obj is IPoolable poolable)
            {
                poolable.OnGetFromPool();
            }
            
            return obj;
        }

        public void Return(T obj)
        {
            if (!obj || !_activeObjects.Contains(obj)) return;

            if (obj is IPoolable poolable && !poolable.CanReturnToPool()) return;

            obj.gameObject.SetActive(false);

            if (obj is IPoolable poolableReturn)
            {
                poolableReturn.OnReturnToPool();
            }

            _activeObjects.Remove(obj);
            _availableObjects.Enqueue(obj);
        }

        public void ReturnAll()
        {
            var activeList = new List<T>(_activeObjects);
            foreach (var obj in activeList)
            {
                Return(obj);
            }
        }
    }
}