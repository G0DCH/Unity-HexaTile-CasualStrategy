using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    public class ObjectPool<T> where T : MonoBehaviour, IReusable
    {
        public readonly int maxCapacity;

        private readonly Queue<T> pool;
        private readonly T prefab;
        private readonly Transform objectHolder;

        public ObjectPool(T prefab, int maxCapacity, Transform objectHolder = null)
        {
            this.maxCapacity = maxCapacity;
            this.prefab = prefab;
            this.objectHolder = objectHolder;

            pool = new Queue<T>(maxCapacity);
        }

        public void Prepare(int targetCapacity)
        {
            targetCapacity = Mathf.Min(maxCapacity, targetCapacity);
            while (pool.Count < targetCapacity)
            {
                T poolObject = CreateNewObject();
                poolObject.gameObject.SetActive(false);
                pool.Enqueue(poolObject);
            }
        }

        public T Pop()
        {
            if (pool.Count > 0)
            {
                T poolObject = pool.Dequeue();
                poolObject.gameObject.SetActive(true);
                poolObject.OnReuse();
                return poolObject;
            }
            else
            {
                return CreateNewObject();
            }
        }

        public void Push(T poolObject)
        {
            if (pool.Count < maxCapacity)
            {
                poolObject.OnPooling();
                poolObject.gameObject.SetActive(false);
                pool.Enqueue(poolObject);
            }
            else
            {
                Object.Destroy(poolObject.gameObject);
            }
        }

        public void Clear()
        {
            foreach (var poolObject in pool)
            {
                Object.Destroy(poolObject.gameObject);
            }
        }

        private T CreateNewObject()
        {
            T newObject = Object.Instantiate(prefab, objectHolder);
            return newObject;
        }
    }
}
