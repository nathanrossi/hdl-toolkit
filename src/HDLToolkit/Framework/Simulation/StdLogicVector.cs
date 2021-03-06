﻿// Copyright 2011 Nathan Rossi - http://nathanrossi.com
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace HDLToolkit.Framework.Simulation
{
	[Serializable]
	public class StdLogicVector : IEnumerable, ICloneable
	{
		public enum StdLogic : byte
		{
			_0 = 0,
			_1 = 1,
			X,
			Z,
			U
		}

		private StdLogic[] vector;

		#region Constructors

		public StdLogicVector(StdLogicVector vector)
		{
			this.vector = new StdLogic[vector.Count];
			Array.Copy(vector.vector, this.vector, this.vector.Length);
		}

		public StdLogicVector(bool[] values)
		{
			this.vector = new StdLogic[values.Length];
			for (int i = 0; i < values.Length; i++)
			{
				this.vector[i] = (values[i] ? StdLogic._1 : StdLogic._0);
			}
		}

		public StdLogicVector(byte[] bytes)
		{
			this.vector = new StdLogic[bytes.Length * 8];
			int k = 0;
			for (int i = 0; i < bytes.Length; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					this.vector[k++] = ((bytes[i] & (1 << j)) != 0 ? StdLogic._1 : StdLogic._0);
				}
			}
		}

		public StdLogicVector(int length)
		{
			this.vector = new StdLogic[length];
		}

		public StdLogicVector(int[] values)
		{
			this.vector = new StdLogic[values.Length * 32];
			int k = 0;
			for (int i = 0; i < values.Length; i++)
			{
				for (int j = 0; j < 32; j++)
				{
					this.vector[k++] = ((values[i] & (1 << j)) != 0 ? StdLogic._1 : StdLogic._0);
				}
			}
		}

		public StdLogicVector(StdLogic[] values)
		{
			this.vector = new StdLogic[values.Length];
			for (int i = 0; i < values.Length; i++)
			{
				this.vector[i] = values[i];
			}
		}

		public StdLogicVector(int length, bool defaultValue)
		{
			this.vector = new StdLogic[length];
			for (int i = 0; i < length; i++)
			{
				this.vector[i] = (defaultValue ? StdLogic._1 : StdLogic._0);
			}
		}

		public StdLogicVector(int length, StdLogic defaultValue)
		{
			this.vector = new StdLogic[length];
			for (int i = 0; i < length; i++)
			{
				this.vector[i] = defaultValue;
			}
		}

		#endregion

		public int Count
		{
			get { return this.vector.Length; }
		}

		public StdLogic this[int index]
		{
			get { return this.vector[index]; }
			set { this.vector[index] = value; }
		}

		public object Clone()
		{
			return new StdLogicVector(this);
		}

		public IEnumerator GetEnumerator()
		{
			return vector.GetEnumerator();
		}

		public void SetAll(StdLogic value)
		{
			for (int i = 0; i < vector.Length; i++)
			{
				this.vector[i] = value;
			}
		}

		public void SetAll(bool value)
		{
			for (int i = 0; i < vector.Length; i++)
			{
				this.vector[i] = (value ? StdLogic._1 : StdLogic._0);
			}
		}

		public StdLogicVector Flip()
		{
			StdLogic[] vectorFlipped = new StdLogic[vector.Length];
			for (int i = 0; i < vector.Length; i++)
			{
				vectorFlipped[i] = vector[vector.Length-i-1];
			}
			return new StdLogicVector(vectorFlipped);
		}

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();

			// Append MSB to LSB
			for (int i = vector.Length - 1; i >= 0; i--)
			{
				builder.Append(StdLogicToString(vector[i]));
			}

			return builder.ToString();
		}

		public long ToLong()
		{
			long value = 0;

			if (this.Count > 64)
			{
				throw new OverflowException("Larger the 64 bits");
			}

			for (int i = 0; i < this.Count; i++)
			{
				if (this[i] == StdLogic._1)
				{
					long shift = 1L << i;
					value |= shift;
				}
			}

			return value;
		}

		#region Bitwise Operations
		public StdLogicVector Or(StdLogicVector value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value is null.");
			}
			if (value.Count != this.Count)
			{
				throw new ArgumentException("value and the current System.Collections.BitArray do not have the same number of elements.");
			}

			StdLogicVector result = new StdLogicVector(value.Count);
			for (int i = 0; i < value.Count; i++)
			{
				result[i] = StdLogicOr(this[i], value[i]);
			}
			return result;
		}

		internal static StdLogic StdLogicOr(StdLogic a, StdLogic b)
		{
			if (a == StdLogic.X || b == StdLogic.X)
			{
				return StdLogic.X;
			}
			else if (a == StdLogic._1 || b == StdLogic._1)
			{
				return StdLogic._1;
			}
			else if (a == StdLogic._0 && b == StdLogic._0)
			{
				return StdLogic._0;
			}
			else
			{
				// both are Z.
				return StdLogic.Z;
			}
		}

		public StdLogicVector And(StdLogicVector value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value is null.");
			}
			if (value.Count != this.Count)
			{
				throw new ArgumentException("value and the current System.Collections.BitArray do not have the same number of elements.");
			}

			StdLogicVector result = new StdLogicVector(value.Count);
			for (int i = 0; i < value.Count; i++)
			{
				result[i] = StdLogicAnd(this[i], value[i]);
			}
			return result;
		}

		internal static StdLogic StdLogicAnd(StdLogic a, StdLogic b)
		{
			if (a == StdLogic.X || b == StdLogic.X)
			{
				return StdLogic.X;
			}
			else if (a == StdLogic._0 || b == StdLogic._0)
			{
				return StdLogic._0;
			}
			else if (a == StdLogic._1 && b == StdLogic._1)
			{
				return StdLogic._1;
			}
			else
			{
				// both are Z.
				return StdLogic.Z;
			}
		}

		public StdLogicVector Not()
		{
			StdLogicVector result = new StdLogicVector(this.Count);
			for (int i = 0; i < this.Count; i++)
			{
				result[i] = StdLogicNot(this[i]);
			}
			return result;
		}

		internal static StdLogic StdLogicNot(StdLogic a)
		{
			if (a == StdLogic._0)
			{
				return StdLogic._1;
			}
			else if (a == StdLogic._1)
			{
				return StdLogic._0;
			}
			else
			{
				return a;
			}
		}

		public StdLogicVector Xor(StdLogicVector value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value is null.");
			}
			if (value.Count != this.Count)
			{
				throw new ArgumentException("value and the current System.Collections.BitArray do not have the same number of elements.");
			}

			StdLogicVector result = new StdLogicVector(value.Count);
			for (int i = 0; i < value.Count; i++)
			{
				result[i] = StdLogicXor(this[i], value[i]);
			}
			return result;
		}

		internal static StdLogic StdLogicXor(StdLogic a, StdLogic b)
		{
			if (a == StdLogic.X || b == StdLogic.X)
			{
				return StdLogic.X;
			}
			else if (a == StdLogic._0 && b == StdLogic._0)
			{
				return StdLogic._0;
			}
			else if (a == StdLogic._1 && b == StdLogic._1)
			{
				return StdLogic._1;
			}
			else if (a == StdLogic._1 && b == StdLogic._0)
			{
				return StdLogic._1;
			}
			else if (a == StdLogic._0 && b == StdLogic._1)
			{
				return StdLogic._1;
			}
			else
			{
				// both are Z.
				return StdLogic.Z;
			}
		}
		#endregion

		#region Parser

		public static StdLogicVector Parse(string parse)
		{
			List<StdLogic> bits = new List<StdLogic>();

			// MSB is always first in a string
			for (int i = 0; i < parse.Length; i++)
			{
				StdLogic? value = ParseStdLogic(parse[i]);

				if (!value.HasValue)
				{
					return null;
				}

				bits.Add(value.Value);
			}

			if (bits.Count > 0)
			{
				return new StdLogicVector(bits.ToArray());
			}
			return null;
		}

		public static StdLogic? ParseStdLogic(char parse)
		{
			switch (parse)
			{
				case '0':
					return StdLogic._0;
				case '1':
					return StdLogic._1;
				case 'Z':
					return StdLogic.Z;
				case 'X':
					return StdLogic.X;
				case 'U':
					return StdLogic.U;
				default:
					return null;
			}
		}

		public static String StdLogicToString(StdLogic value)
		{
			switch (value)
			{
				case StdLogic._0:
					return "0";
				case StdLogic._1:
					return "1";
				case StdLogic.Z:
					return "Z";
				case StdLogic.X:
					return "X";
				case StdLogic.U:
					return "U";
				default:
					return null;
			}
		}

		#endregion
	}
}
