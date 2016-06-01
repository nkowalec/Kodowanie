using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ArythmeticCode
{
    public class ArithmeticCompressor
    {
        static int bitlen = 62;
        readonly ulong maxRange, half, quarter, quarter3;
        DataModel model;
        ulong currentDecodeValue, rangeHigh, rangeLow;
        BitWriter writer;
        BitReader reader;
        long underflow;

        //Konstruktor klasy kompresora
        public ArithmeticCompressor()
        {
            maxRange = 1UL << bitlen;
            half = maxRange >> 1;
            quarter = half >> 1;
            quarter3 = 3 * quarter;
        }

        /// <summary>
        /// Metoda kompresująca przekazany ciąg bajtów
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] Compress(IEnumerable<byte> data)
        {
            ResetEncoder(new DataModel(256));
            foreach (byte b in data)
                EncodeSymbol(b);
            EncodeSymbol(model.EOF);
            FlushEncoder();
            return writer.Data;
        }

        /// <summary>
        /// Metoda dekompresująca przekazany ciąg bajtów
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] Decompress(IEnumerable<byte> data)
        {
            ResetDecoder(data, new DataModel(256));
            List<byte> output = new List<byte>();
            ulong symbol = 0;
            while ((symbol = DecodeSymbol()) != model.EOF)
                output.Add((byte)symbol);
            return output.ToArray();
        }

        public ulong OptimalLength
        {
            get
            {
                return model.OptimalLength;
            }
        }


        void EncodeSymbol(ulong symbol)
        {
            Debug.Assert(rangeLow < rangeHigh);
            ulong range = rangeHigh - rangeLow + 1, left, right;
            model.GetRangeFromSymbol(symbol, out left, out right);

            ulong step = range / model.Total;
            Debug.Assert(step > 0);
            rangeHigh = rangeLow + step * right - 1;
            rangeLow = rangeLow + step * left;

            model.AddSymbol(symbol);

            while ((rangeHigh < half) || (half <= rangeLow))
            {
                if (rangeHigh < half)
                {
                    writer.Write(0);
                    while (underflow > 0) { --underflow; writer.Write(1); }
                    rangeHigh = (rangeHigh << 1) + 1;
                    rangeLow <<= 1;
                }
                else
                {
                    writer.Write(1);
                    while (underflow > 0) { --underflow; writer.Write(0); }
                    rangeHigh = ((rangeHigh - half) << 1) + 1;
                    rangeLow = (rangeLow - half) << 1;
                }
            }
            while ((quarter <= rangeLow) && (rangeHigh < quarter3))
            {
                underflow++;
                rangeLow = (rangeLow - quarter) << 1;
                rangeHigh = ((rangeHigh - quarter) << 1) + 1;
            }

            Debug.Assert(rangeHigh - rangeLow >= quarter);
        }

        void FlushEncoder()
        {

            if (rangeLow < quarter)
            {
                writer.Write(0);
                for (int i = 0; i < underflow + 1; ++i)
                    writer.Write(1);
            }
            else
            {
                writer.Write(1);
            }
            writer.Flush();
        }

        void ResetEncoder(DataModel model)
        {
            this.model = model;

            rangeHigh = half + half - 1;
            rangeLow = 0;
            underflow = 0;
            writer = new BitWriter();
        }

        void ResetDecoder(IEnumerable<byte> data, DataModel model)
        {
            this.model = model;

            rangeHigh = half + half - 1;
            rangeLow = 0;
            underflow = 0;

            currentDecodeValue = 0;
            reader = new BitReader(data);

            for (int i = 0; i < bitlen; ++i)
                currentDecodeValue = (currentDecodeValue << 1) | reader.Read();
        }

        ulong DecodeSymbol()
        {
            {
                ulong symbol = 0;
                Debug.Assert(rangeLow < rangeHigh);
                ulong range = rangeHigh - rangeLow + 1, left, right;

                ulong step = range / model.Total;
                Debug.Assert(step > 0);
                Debug.Assert(currentDecodeValue >= rangeLow);
                Debug.Assert(rangeHigh >= currentDecodeValue);
                ulong value = (currentDecodeValue - rangeLow) / step;
                symbol = model.GetSymbolAndRange(value, out left, out right);
                rangeHigh = rangeLow + step * right - 1;
                rangeLow = rangeLow + step * left;

                model.AddSymbol(symbol);

                while ((rangeHigh < half) || (half <= rangeLow))
                {
                    if (rangeHigh < half)
                    {
                        rangeHigh = (rangeHigh << 1) + 1;
                        rangeLow <<= 1;
                        currentDecodeValue = (currentDecodeValue << 1) | reader.Read();
                    }
                    else
                    {
                        rangeHigh = ((rangeHigh - half) << 1) + 1;
                        rangeLow = (rangeLow - half) << 1;
                        currentDecodeValue = ((currentDecodeValue - half) << 1) | reader.Read();
                    }
                }
                while ((quarter <= rangeLow) && (rangeHigh < quarter3))
                {
                    rangeLow = (rangeLow - quarter) << 1;
                    rangeHigh = ((rangeHigh - quarter) << 1) + 1;
                    currentDecodeValue = ((currentDecodeValue - quarter) << 1) | reader.Read();
                }
                return symbol;
            }
        }
    }

    class DataModel
    {
        public ulong EOF { get { return eof; } }

        public ulong Total { get; set; }

        public DataModel(ulong size)
        {
            eof = size;
            tree = new ulong[EOF + 2];

            for (int i = 0; i < tree.Length - 1; ++i)
                AddSymbol((ulong)i);
        }

        public void AddSymbol(ulong symbol)
        {
            long k = (long)symbol;
            for (; k < tree.Length; k |= k + 1)
                tree[k]++;
            ++Total;
        }

        public void GetRangeFromSymbol(ulong symbol, out ulong low, out ulong high)
        {

            ulong diff = tree[symbol];
            long b = (long)(symbol);
            long target = (b & (b + 1)) - 1;

            ulong sum = 0;
            b = (long)(symbol - 1);
            for (; b >= 0; b = (b & (b + 1)) - 1)
            {
                if (b == target) diff -= sum;
                sum += tree[b];
            }
            if (b == target) diff -= sum;
            low = sum;
            high = sum + diff;
        }

        public ulong GetSymbolAndRange(ulong value, out ulong low, out ulong high)
        {
            ulong N = (ulong)tree.Length;
            ulong bottom = 0;
            ulong top = N;
            while (bottom < top)
            {
                ulong mid = bottom + ((top - bottom) / 2);
                if (Query(0, (long)mid) <= value)
                    bottom = mid + 1;
                else
                    top = mid;
            }
            if (bottom < N)
            {
                GetRangeFromSymbol(bottom, out low, out high);
                Debug.Assert((low <= value) && (value < high));
                return bottom;
            }
            Debug.Assert(false, "Illegal lookup overflow!");
            low = high = UInt64.MaxValue;
            return UInt64.MaxValue;
        }

        public ulong OptimalLength
        {
            get
            {
                double sum = 0;
                double total = (Total - (ulong)tree.Length + 1);
                for (int i = 0; i < tree.Length - 1; ++i)
                {
                    double freq = Query(i, i) - 1;
                    if (freq > 0)
                    {
                        double p = freq / total;
                        sum += -Math.Log(p, 2) * freq;
                    }
                }
                return (ulong)(sum);
            }
        }

        ulong Query(long a, long b)
        {
            if (a == 0)
            {
                ulong sum = 0;
                for (; b >= 0; b = (b & (b + 1)) - 1)
                    sum += tree[b];
                return sum;
            }
            else
            {
                return Query(0, b) - Query(0, a - 1);
            }
        }

        ulong[] tree;


        ulong eof;
    }

    #region Bit Writer/Reader                                                                                                                                                                          

    class BitWriter
    {
        public BitWriter()
        {
            bitPos = 0;
            encodeData = new List<byte>();
            datum = 0;
        }

        public void Write(byte bit)
        {
            datum <<= 1;
            datum = (byte)(datum | (bit & 1));
            ++bitPos;
            if (bitPos == 8)
            {
                encodeData.Add(datum);
                bitPos = 0;
            }
        }

        public void Flush()
        {
            while (bitPos != 0)
                Write(0);
        }

        public byte[] Data { get { return encodeData.ToArray(); } }
                                                                                                                                                                  
        int bitPos = 0;
        byte datum;
        List<byte> encodeData;
    }

    class BitReader
    {                                                                                                                                                                    

        public BitReader(IEnumerable<byte> data)
        {
            bitPos = 0;
            datum = 0;
            decodeByteIndex = 0;
            decodeData = data.GetEnumerator();
            decodeData.MoveNext();
            datum = decodeData.Current;
        }

        public byte Read()
        {
            byte bit = (byte)(datum >> 7);
            datum <<= 1;
            ++bitPos;
            if (bitPos == 8)
            {
                decodeByteIndex++;
                if (decodeData.MoveNext())
                    datum = decodeData.Current;
                bitPos = 0;
            }
            return bit;
        }
                                                                                                                                                                
        byte datum;
        int bitPos;
        IEnumerator<byte> decodeData;
        long decodeByteIndex;
    }

    #endregion
}