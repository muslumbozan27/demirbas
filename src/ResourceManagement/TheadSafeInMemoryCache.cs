using System;
using System.Collections.Generic;
using System.Threading;

namespace AlmWitt.Web.ResourceManagement
{
	/// <summary>
	/// This class is intended to be used when you are accumulating a cache of stuff that can be accessed by multiple threads.
	/// It wraps a Dictionary, but is thread safe.  I didn't make it implement IDictionary because IDictionary's interface
	/// is pretty broad and that would be more work (YAGNI).
	/// </summary>
	/// <remarks>
	/// When we move to .NET 4, we can replace this with a ConcurrentDictionary.
	/// </remarks>
	public class ThreadSafeInMemoryCache<TKey, TValue>
	{
		public static ThreadSafeInMemoryCache<TKey, TValue> Create()
		{
			return new ThreadSafeInMemoryCache<TKey, TValue>();
		}

		private ThreadSafeInMemoryCache() { }

		private readonly IDictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();
		private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

		public int Count
		{
			get
			{
				using (_lock.ReadLock())
				{
					return _dictionary.Count;
				}
			}
		}

		public TValue Get(TKey key)
		{
			using (_lock.ReadLock())
			{
				return _dictionary[key];
			}
		}

		public TValue GetOrAdd(TKey key, Func<TValue> getValue)
		{
			//this double locking pattern was adapted from a blog post by Ayende
			//http://ayende.com/Blog/archive/2010/01/04/using-readerwriterlockslimrsquos-enterupgradeablereadlock.aspx
			using (_lock.ReadLock())
			{
				TValue value;
				if (_dictionary.TryGetValue(key, out value))
					return value;
			}
			using (_lock.WriteLock())
			{
				TValue value;
				if (_dictionary.TryGetValue(key, out value))
					return value;

				value = getValue();
				_dictionary[key] = value;

				return value;
			}
		}
	}

	internal static class ReaderWriterLockExtensions
	{
		public static IDisposable ReadLock(this ReaderWriterLockSlim @lock)
		{
			@lock.EnterReadLock();

			return new DisposableAction(@lock.ExitReadLock);
		}

		public static IDisposable WriteLock(this ReaderWriterLockSlim @lock)
		{
			@lock.EnterWriteLock();

			return new DisposableAction(@lock.ExitWriteLock);
		}

		private class DisposableAction : IDisposable
		{
			private readonly Action _onDispose;

			public DisposableAction(Action onDispose)
			{
				_onDispose = onDispose;
			}

			public void Dispose()
			{
				_onDispose();
			}
		}
	}
}