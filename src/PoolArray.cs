﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace JsonSpanParser
{
    internal class PoolArray<T> : IMemoryHolder<T>
        where T : unmanaged
    {
        private T[] buffer;
        public int Length { get; private set; }
        public int Count { get; private set; } = 2;
        private const int MAX_SIZE = (int.MaxValue - 1) / 2;

        public PoolArray()
        {
            buffer = new T[Count];
        }
        public PoolArray(int initLength) : this()
        {
            SetSize(initLength);
        }
        private void SetSize(int newLength, bool copy = false)
        {
            if (newLength < 0) throw new ArgumentException($"{newLength} cant be less the zero");

            if (Count < newLength)
            {
                if (newLength > MAX_SIZE)
                {
                    Count = int.MaxValue;
                }
                else
                {
                    while (Count < newLength)
                    {
                        Count <<= 1;
                    }
                }

                if (buffer.Length != Count)
                {
                    var buff = new T[Count];
                    if (copy)
                    {
                        buffer.CopyTo(buff, 0);
                    }
                    buffer = buff;
                }
            }

            Length = newLength;
        }

        public void Compress()
        {
            int length = 2;
            while (length < Length)
            {
                length <<= 1;
            }

            Count = length;
            if (buffer.Length != length)
            {
                buffer = new T[length];
            }
        }

        public void Clear()
        {
            Length = 0;
            Count = 2;
            buffer = new T[Count];
        }

        public T this[int indx]
        {
            get
            {
                IndexOutOfRangeExceptionCheck(indx);
                return buffer[indx];
            }
            set
            {
                IndexOutOfRangeExceptionCheck(indx);
                buffer[indx] = value;
            }
        }

        private void IndexOutOfRangeExceptionCheck(int indx)
        {
            if (indx < 0 || indx > Length)
            {
                var excp = indx < 0 ?
                new IndexOutOfRangeException($"{indx} less then zero") :
                new IndexOutOfRangeException($"{indx} bigger then length({Length})");
                throw excp;
            }
        }
        public T[] Write(int length)
        {
            SetSize(length);
            return buffer;
        }
        public ReadOnlySpan<T> Read()
        {
            return Read(0, Length);
        }
        public ReadOnlySpan<T> Read(int startIndx, int length)
        {
            if (startIndx + length > Length) throw new ArgumentOutOfRangeException();
            return buffer.AsSpan().Slice(startIndx, length);
        }
        public ReadOnlySpan<T> Read(int startIndx)
        {
            return Read(startIndx, Length - startIndx);
        }

        public T GetByRef(int indx)
        {
            try
            {
                return this[indx];
            }
            catch
            {
                return default;
            }
        }

        public int Add(T value)
        {
            var indx = Length;
            SetSize(++Length, true);
            this[indx] = value;
            return indx;
        }

        public void Release()
        {
            Length = 0;
        }
    }
}
