using System;
using System.Collections.Generic;

namespace TrueSync
{
	public abstract class ResourcePool
	{
		protected bool fresh = true;

		protected static List<ResourcePool> resourcePoolReferences = new List<ResourcePool>();

        /// <summary>
        /// 为了方便清除引用 以便系统gc
        /// </summary>
		public static void CleanUpAll()
		{
			int i = 0;
			int count = ResourcePool.resourcePoolReferences.Count;
			while (i < count)
			{
				ResourcePool.resourcePoolReferences[i].ResetResourcePool();
				i++;
			}
			ResourcePool.resourcePoolReferences.Clear();
		}

		public abstract void ResetResourcePool();
	}
    /// <summary>
    /// 就是对stack的存于取
    /// </summary>
    /// <typeparam name="T"></typeparam>
	public class ResourcePool<T> : ResourcePool
	{
		protected Stack<T> stack = new Stack<T>(10);

		public int Count
		{
			get
			{
				return this.stack.Count;
			}
		}

		public override void ResetResourcePool()
		{
			this.stack.Clear();
			this.fresh = true;
		}

		public void GiveBack(T obj)
		{
			this.stack.Push(obj);
		}

		public T GetNew()
		{
			if (this.fresh)
			{
				ResourcePool.resourcePoolReferences.Add(this);
				this.fresh = false;
			}
			if (this.stack.Count == 0)
			{
				this.stack.Push(this.NewInstance());
			}
			T t = this.stack.Pop();
			if (t is ResourcePoolItem)
			{
				((ResourcePoolItem)t).CleanUp();
			}
			return t;
		}

		protected virtual T NewInstance()
		{
			return Activator.CreateInstance<T>();
		}
	}
}
