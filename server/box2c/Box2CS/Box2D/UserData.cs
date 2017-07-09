using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Box2CS
{
	internal class UserDataPin
	{
		public object Object
		{
			get;
			set;
		}

		public uint ReferenceCount
		{
			get;
			set;
		}

		public UserDataPin(object obj)
		{
			Object = obj;
		}
	}

	internal class UserDataStorage
	{
		Dictionary<uint, UserDataPin> _resolve = new Dictionary<uint, UserDataPin>();
		List<uint> _freeHandles = new List<uint>();
		uint _highHandle = 1;

		uint GetFreeHandle()
		{
			uint handle;

			if (_freeHandles.Count == 0)
			{
				handle =_highHandle;
				_highHandle++;
				return handle;
			}

			handle = _freeHandles[0];
			_freeHandles.RemoveAt(0);
			return handle;
		}

		public uint PinDataToHandle(object obj)
		{
			foreach (var x in _resolve)
			{
				if (x.Value.Object == obj)
				{
					x.Value.ReferenceCount++;
					return x.Key;
				}
			}

			var handle = GetFreeHandle();
			_resolve[handle] = new UserDataPin(obj);
			_resolve[handle].ReferenceCount++;

			return handle;
		}

		public uint HandleFromPinnedData(object obj)
		{
			foreach (var x in _resolve)
			{
				if (x.Value == obj)
					return x.Key;
			}

			return 0;
		}

		public object ObjectFromHandle(uint handle)
		{
			return _resolve.ContainsKey(handle) ? _resolve[handle].Object : null;
		}

		public void UnpinObject(uint handle)
		{
			_resolve[handle].ReferenceCount--;

			if (_resolve[handle].ReferenceCount == 0)
			{
				_resolve.Remove(handle);
				_freeHandles.Add(handle);
			}
		}

		public static IntPtr HandleToIntPtr(uint handle)
		{
			return new IntPtr((int)handle);
		}

		public static uint IntPtrToHandle(IntPtr ptr)
		{
			return (uint)ptr.ToInt32();
		}

		public static UserDataStorage
			BodyStorage = new UserDataStorage(),
			FixtureStorage = new UserDataStorage(),
			JointStorage = new UserDataStorage();
	}
}
