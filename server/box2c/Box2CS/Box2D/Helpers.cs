using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.ComponentModel;
using System.Collections.Generic;

namespace Box2CS
{
	public interface ICompare<T>
	{
		bool CompareWith(T v);
	}

	internal interface IFixedSize
	{
		int FixedSize();
		void Lock();
		void Unlock();
	}

	internal class StructToPtrMarshaller : IDisposable
	{
		IntPtr _ptr;
		IFixedSize _storedVal;

		public IntPtr Pointer
		{
			get { return _ptr; }
		}

		[EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted=true)]
		public StructToPtrMarshaller(IFixedSize val)
		{
			_storedVal = val;
			_storedVal.Lock();
			_ptr = Marshal.AllocHGlobal(val.FixedSize());
			Marshal.StructureToPtr(val, _ptr, false);
		}

		[EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted=true)]
		public object GetValue(Type type)
		{
			return Marshal.PtrToStructure(_ptr, type);
		}

		[EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted=true)]
		public void Free()
		{
			disposed = true;
			Marshal.FreeHGlobal(_ptr);
			_storedVal.Unlock();
		}

		#region IDisposable
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		bool disposed;

		private void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
				}

				Free();

				disposed = true;
			}
		}

		~StructToPtrMarshaller()
		{
			Dispose(false);
		}
		#endregion
	}

	/// <summary>
	/// A simple parser that parses data in "x=vv y=vv" format.
	/// </summary>
	public class SimpleParser
	{
		Dictionary<string, string> _split = new Dictionary<string, string>();
		bool _ignoreCase = false;

		public static string[] Split(string value)
		{
			List<string> splitValues = new List<string>();

			int startKey = -1, endKey = -1;
			int startValue = -1, endValue = -1;
			int i = 0;

			while (true)
			{
				if (endValue != -1)
				{
					string key = value.Substring(startKey, endKey - startKey);
					string val = value.Substring(startValue, endValue - startValue);

					splitValues.Add(key);
					splitValues.Add(val);

					startKey = endKey = startValue = endValue = -1;
					continue;
				}

				if (i == value.Length)
					break;

				char c = value[i];

				// finding key
				if (startKey == -1 && endKey == -1 && startValue == -1 && endValue == -1)
				{
					if (!char.IsWhiteSpace(c))
					{
						startKey = i;
						i++;
						continue;
					}
				}
				// finding end key
				else if (endKey == -1 && startValue == -1 && endValue == -1)
				{
					if (c == '=')
					{
						endKey = i;
						startValue = i + 1;
						i++;
						continue;
					}

					i++;
					continue;
				}
				else if (endValue == -1)
				{
					if (c == ' ')
					{
						endValue = i;
						i++;
						continue;
					}
					else if (c == '{')
					{
						int depth = 1;
						while (true)
						{
							i++;
							c = value[i];

							switch (c)
							{
							case '{':
								depth++;
								break;
							case '}':
								depth--;
								break;
							}

							if (depth == 0)
								break;
						};
					}

					i++;

					if (i == value.Length)
						endValue = i;

					continue;
				}
			}

			return splitValues.ToArray();
		}

		public SimpleParser(string value, bool ignoreCase)
		{
			var split = Split(value);
			_ignoreCase = ignoreCase;

			for (int i = 0; i < split.Length; i += 2)
				_split.Add((ignoreCase) ? split[i].ToLowerInvariant() : split[i], split[i + 1]);
		}

		public string ValueFromKey(string key)
		{
			if (_ignoreCase)
				key = key.ToLowerInvariant();

			if (!_split.ContainsKey(key))
				throw new KeyNotFoundException();
			return _split[key];
		}
	}

	/// <summary>
	/// Provides a way to parse arrays that are stored as {{value} {value}}
	/// </summary>
	public class SimpleArrayParser
	{
		string[] _strings;

		public static int NumOfCharInString(string input, char c)
		{
			int count = 0;

			foreach (var ch in input)
				if (ch == c)
					count++;

			return count;
		}

		public SimpleArrayParser(string input)
		{
			if (!(input.StartsWith("{{") && input.EndsWith("}}")))
				throw new FormatException();

			input = input.Substring(1, input.Length - 2);

			_strings = new string[NumOfCharInString(input, '{')];

			int i = 0;
			int c = 0;

			while (true)
			{
				i = input.IndexOf('{', i);

				if (i == -1)
					break;

				int end = input.IndexOf('}', i);

				_strings[c] = input.Substring(i + 1, end - (i + 1));
				i++;
				c++;
			}
		}

		public delegate T ConvertDelegate<T>(string value);
		public T[] Convert<T>(ConvertDelegate<T> converter)
		{
			T[] array = new T[_strings.Length];

			for (int i = 0; i < _strings.Length; ++i)
				array[i] = converter(_strings[i]);

			return array;
		}

		public IEnumerator<string> Values
		{
			get { foreach (var x in _strings) yield return x; }
		}

		public static T[] GenerateArray<T>(string input, ConvertDelegate<T> converter)
		{
			return new SimpleArrayParser(input).Convert<T>(converter);
		}
	}

	public static class Extensions
	{
		public static T PopFront<T>(this IList<T> list)
		{
			if (list.Count == 0)
				throw new IndexOutOfRangeException();

			var val = list[0];
			list.RemoveAt(0);
			return val;
		}

		public static T PopBack<T>(this IList<T> list)
		{
			if (list.Count == 0)
				throw new IndexOutOfRangeException();

			var val = list[list.Count - 1];
			list.RemoveAt(list.Count - 1);
			return val;
		}
	}

	public struct ValueTuple<T1, T2>
	{
		public T1 Value1
		{
			get;
			set;
		}

		public T2 Value2
		{
			get;
			set;
		}

		public ValueTuple(T1 value1, T2 value2) :
			this()
		{
			Value1 = value1;
			Value2 = value2;
		}
	}
}
