﻿/*
 * Copyright (C) Rei HOBARA 2007
 * 
 * Name:
 *     SFMT.cs
 * Class:
 *     Rei.Random.SFMT
 *     Rei.Random.MTPeriodType
 * Purpose:
 *     A random number generator using SIMD-oriented Fast Mersenne Twister(SFMT).
 * Remark:
 *     This code is C# implementation of SFMT.
 *     SFMT was introduced by Mutsuo Saito and Makoto Matsumoto.
 *     See http://www.math.sci.hiroshima-u.ac.jp/~m-mat/MT/SFMT/index.html for detail of SFMT.
 * History:
 *     2007/10/6 initial release.
 * 
 */

namespace TerrainGenerator {
	using System;

	public class SFMT {
		public virtual Int32 NextInt32() {
			return (Int32)NextUInt32();
		}

		public virtual UInt64 NextUInt64() {
			return ((UInt64)NextUInt32() << 32) | NextUInt32();
		}

		public virtual Int64 NextInt64() {
			return ((Int64)NextUInt32() << 32) | NextUInt32();
		}

		public Double Random() {
			Double x = NextDouble();
			return x - Math.Truncate(x);
		}

		public virtual void NextBytes(byte[] buffer) {
			int i = 0;
			UInt32 r;
			while (i + 4 <= buffer.Length) {
				r = NextUInt32();
				buffer[i++] = (byte)r;
				buffer[i++] = (byte)(r >> 8);
				buffer[i++] = (byte)(r >> 16);
				buffer[i++] = (byte)(r >> 24);
			}
			if (i >= buffer.Length) return;
			r = NextUInt32();
			buffer[i++] = (byte)r;
			if (i >= buffer.Length) return;
			buffer[i++] = (byte)(r >> 8);
			if (i >= buffer.Length) return;
			buffer[i++] = (byte)(r >> 16);
		}

		public virtual double NextDouble() {
			UInt32 r1, r2;
			r1 = NextUInt32();
			r2 = NextUInt32();
			return (r1 * (double)(2 << 11) + r2) / (double)(2 << 53);
		}

		#region Fields

		protected int MEXP;
		protected int POS1;
		protected int SL1;
		protected int SL2;
		protected int SR1;
		protected int SR2;
		protected UInt32 MSK1;
		protected UInt32 MSK2;
		protected UInt32 MSK3;
		protected UInt32 MSK4;
		protected UInt32 PARITY1;
		protected UInt32 PARITY2;
		protected UInt32 PARITY3;
		protected UInt32 PARITY4;

		protected int N;
		protected int N32;
		protected int SL2_x8;
		protected int SR2_x8;
		protected int SL2_ix8;
		protected int SR2_ix8;

		protected UInt32[] sfmt;
		protected int idx;

		#endregion

		public SFMT() : this(Environment.TickCount, 19937) { }

		public SFMT(int seed) : this(seed, 19937) { }

		public SFMT(int seed, MTPeriodType period) : this(seed, (int)period) { }

		public SFMT(int seed, int mexp) {
			this.MEXP = mexp;
			if (mexp == 607) {
				POS1 = 2;
				SL1 = 15;
				SL2 = 3;
				SR1 = 13;
				SR2 = 3;
				MSK1 = 0xfdff37ffU;
				MSK2 = 0xef7f3f7dU;
				MSK3 = 0xff777b7dU;
				MSK4 = 0x7ff7fb2fU;
				PARITY1 = 0x00000001U;
				PARITY2 = 0x00000000U;
				PARITY3 = 0x00000000U;
				PARITY4 = 0x5986f054U;
			} else if (mexp == 1279) {
				POS1 = 7;
				SL1 = 14;
				SL2 = 3;
				SR1 = 5;
				SR2 = 1;
				MSK1 = 0xf7fefffdU;
				MSK2 = 0x7fefcfffU;
				MSK3 = 0xaff3ef3fU;
				MSK4 = 0xb5ffff7fU;
				PARITY1 = 0x00000001U;
				PARITY2 = 0x00000000U;
				PARITY3 = 0x00000000U;
				PARITY4 = 0x20000000U;
			} else if (mexp == 2281) {
				POS1 = 12;
				SL1 = 19;
				SL2 = 1;
				SR1 = 5;
				SR2 = 1;
				MSK1 = 0xbff7ffbfU;
				MSK2 = 0xfdfffffeU;
				MSK3 = 0xf7ffef7fU;
				MSK4 = 0xf2f7cbbfU;
				PARITY1 = 0x00000001U;
				PARITY2 = 0x00000000U;
				PARITY3 = 0x00000000U;
				PARITY4 = 0x41dfa600U;
			} else if (mexp == 4253) {
				POS1 = 17;
				SL1 = 20;
				SL2 = 1;
				SR1 = 7;
				SR2 = 1;
				MSK1 = 0x9f7bffffU;
				MSK2 = 0x9fffff5fU;
				MSK3 = 0x3efffffbU;
				MSK4 = 0xfffff7bbU;
				PARITY1 = 0xa8000001U;
				PARITY2 = 0xaf5390a3U;
				PARITY3 = 0xb740b3f8U;
				PARITY4 = 0x6c11486dU;
			} else if (mexp == 11213) {
				POS1 = 68;
				SL1 = 14;
				SL2 = 3;
				SR1 = 7;
				SR2 = 3;
				MSK1 = 0xeffff7fbU;
				MSK2 = 0xffffffefU;
				MSK3 = 0xdfdfbfffU;
				MSK4 = 0x7fffdbfdU;
				PARITY1 = 0x00000001U;
				PARITY2 = 0x00000000U;
				PARITY3 = 0xe8148000U;
				PARITY4 = 0xd0c7afa3U;
			} else if (mexp == 19937) {
				POS1 = 122;
				SL1 = 18;
				SL2 = 1;
				SR1 = 11;
				SR2 = 1;
				MSK1 = 0xdfffffefU;
				MSK2 = 0xddfecb7fU;
				MSK3 = 0xbffaffffU;
				MSK4 = 0xbffffff6U;
				PARITY1 = 0x00000001U;
				PARITY2 = 0x00000000U;
				PARITY3 = 0x00000000U;
				PARITY4 = 0x13c9e684U;
				PARITY4 = 0x20000000U;
			} else if (mexp == 44497) {
				POS1 = 330;
				SL1 = 5;
				SL2 = 3;
				SR1 = 9;
				SR2 = 3;
				MSK1 = 0xeffffffbU;
				MSK2 = 0xdfbebfffU;
				MSK3 = 0xbfbf7befU;
				MSK4 = 0x9ffd7bffU;
				PARITY1 = 0x00000001U;
				PARITY2 = 0x00000000U;
				PARITY3 = 0xa3ac4000U;
				PARITY4 = 0xecc1327aU;
			} else if (mexp == 86243) {
				POS1 = 366;
				SL1 = 6;
				SL2 = 7;
				SR1 = 19;
				SR2 = 1;
				MSK1 = 0xfdbffbffU;
				MSK2 = 0xbff7ff3fU;
				MSK3 = 0xfd77efffU;
				MSK4 = 0xbf9ff3ffU;
				PARITY1 = 0x00000001U;
				PARITY2 = 0x00000000U;
				PARITY3 = 0x00000000U;
				PARITY4 = 0xe9528d85U;
			} else if (mexp == 132049) {
				POS1 = 110;
				SL1 = 19;
				SL2 = 1;
				SR1 = 21;
				SR2 = 1;
				MSK1 = 0xffffbb5fU;
				MSK2 = 0xfb6ebf95U;
				MSK3 = 0xfffefffaU;
				MSK4 = 0xcff77fffU;
				PARITY1 = 0x00000001U;
				PARITY2 = 0x00000000U;
				PARITY3 = 0xcb520000U;
				PARITY4 = 0xc7e91c7dU;
			} else if (mexp == 216091) {
				POS1 = 627;
				SL1 = 11;
				SL2 = 3;
				SR1 = 10;
				SR2 = 1;
				MSK1 = 0xbff7bff7U;
				MSK2 = 0xbfffffffU;
				MSK3 = 0xbffffa7fU;
				MSK4 = 0xffddfbfbU;
				PARITY1 = 0xf8000001U;
				PARITY2 = 0x89e80709U;
				PARITY3 = 0x3bd2b64bU;
				PARITY4 = 0x0c64b1e4U;
			} else {
				throw new ArgumentException();
			}
			init_gen_rand(seed);
		}

		public UInt32 NextUInt32() {
			if (idx >= N32) {
				gen_rand_all();
				idx = 0;
			}
			return sfmt[idx++];
		}

		protected void init_gen_rand(int seed) {
			int i;
			N = MEXP / 128 + 1;
			N32 = N * 4;
			SL2_x8 = SL2 * 8;
			SR2_x8 = SR2 * 8;
			SL2_ix8 = 64 - SL2 * 8;
			SR2_ix8 = 64 - SR2 * 8;
			sfmt = new UInt32[N32];
			sfmt[0] = (UInt32)seed;
			for (i = 1; i < N32; i++)
				sfmt[i] = (UInt32)(1812433253 * (sfmt[i - 1] ^ (sfmt[i - 1] >> 30)) + i);
			period_certification();
			idx = N32;
		}

		protected void period_certification() {
			UInt32[] PARITY = new UInt32[] { PARITY1, PARITY2, PARITY3, PARITY4 };
			UInt32 inner = 0;
			int i, j;
			UInt32 work;

			for (i = 0; i < 4; i++) inner ^= sfmt[i] & PARITY[i];
			for (i = 16; i > 0; i >>= 1) inner ^= inner >> i;
			inner &= 1;
			// check OK
			if (inner == 1) return;
			// check NG, and modification
			for (i = 0; i < 4; i++) {
				work = 1;
				for (j = 0; j < 32; j++) {
					if ((work & PARITY[i]) != 0) {
						sfmt[i] ^= work;
						return;
					}
					work = work << 1;
				}
			}
		}

		protected virtual void gen_rand_all() {
			if (MEXP == 19937) { gen_rand_all_19937(); return; }
			int a, b, c, d;
			UInt64 xh, xl, yh, yl;

			a = 0;
			b = POS1 * 4;
			c = (N - 2) * 4;
			d = (N - 1) * 4;
			do {
				xh = ((UInt64)sfmt[a + 3] << 32) | sfmt[a + 2];
				xl = ((UInt64)sfmt[a + 1] << 32) | sfmt[a + 0];
				yh = xh << (SL2_x8) | xl >> (SL2_ix8);
				yl = xl << (SL2_x8);
				xh = ((UInt64)sfmt[c + 3] << 32) | sfmt[c + 2];
				xl = ((UInt64)sfmt[c + 1] << 32) | sfmt[c + 0];
				yh ^= xh >> (SR2_x8);
				yl ^= xl >> (SR2_x8) | xh << (SR2_ix8);

				sfmt[a + 3] = sfmt[a + 3] ^ ((sfmt[b + 3] >> SR1) & MSK4) ^ (sfmt[d + 3] << SL1) ^ ((UInt32)(yh >> 32));
				sfmt[a + 2] = sfmt[a + 2] ^ ((sfmt[b + 2] >> SR1) & MSK3) ^ (sfmt[d + 2] << SL1) ^ ((UInt32)yh);
				sfmt[a + 1] = sfmt[a + 1] ^ ((sfmt[b + 1] >> SR1) & MSK2) ^ (sfmt[d + 1] << SL1) ^ ((UInt32)(yl >> 32));
				sfmt[a + 0] = sfmt[a + 0] ^ ((sfmt[b + 0] >> SR1) & MSK1) ^ (sfmt[d + 0] << SL1) ^ ((UInt32)yl);

				c = d; d = a; a += 4; b += 4;
				if (b >= N32) b = 0;
			} while (a < N32);
		}

		private void gen_rand_all_19937() {
			int a, b, c, d;
			UInt32[] p = this.sfmt;

			const int cMEXP = 19937;
			const int cPOS1 = 122;
			const uint cMSK1 = 0xdfffffefU;
			const uint cMSK2 = 0xddfecb7fU;
			const uint cMSK3 = 0xbffaffffU;
			const uint cMSK4 = 0xbffffff6U;
			const int cSL1 = 18;
			const int cSR1 = 11;
			const int cN = cMEXP / 128 + 1;
			const int cN32 = cN * 4;

			a = 0;
			b = cPOS1 * 4;
			c = (cN - 2) * 4;
			d = (cN - 1) * 4;
			do {
				p[a + 3] = p[a + 3] ^ (p[a + 3] << 8) ^ (p[a + 2] >> 24) ^ (p[c + 3] >> 8) ^ ((p[b + 3] >> cSR1) & cMSK4) ^ (p[d + 3] << cSL1);
				p[a + 2] = p[a + 2] ^ (p[a + 2] << 8) ^ (p[a + 1] >> 24) ^ (p[c + 3] << 24) ^ (p[c + 2] >> 8) ^ ((p[b + 2] >> cSR1) & cMSK3) ^ (p[d + 2] << cSL1);
				p[a + 1] = p[a + 1] ^ (p[a + 1] << 8) ^ (p[a + 0] >> 24) ^ (p[c + 2] << 24) ^ (p[c + 1] >> 8) ^ ((p[b + 1] >> cSR1) & cMSK2) ^ (p[d + 1] << cSL1);
				p[a + 0] = p[a + 0] ^ (p[a + 0] << 8) ^ (p[c + 1] << 24) ^ (p[c + 0] >> 8) ^ ((p[b + 0] >> cSR1) & cMSK1) ^ (p[d + 0] << cSL1);
				c = d; d = a; a += 4; b += 4;
				if (b >= cN32) b = 0;
			} while (a < cN32);
		}
	}
	public enum MTPeriodType {
		MT607 = 607,
		MT1279 = 1279,
		MT2281 = 2281,
		MT4253 = 4253,
		MT11213 = 11213,
		MT19937 = 19937,
		MT44497 = 44497,
		MT86243 = 86243,
		MT132049 = 132049,
		MT216091 = 216091
	}
}