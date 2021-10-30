using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VariableFrameHeader
{

    /// <summary>
    /// 这是一个简化例子，主要演示算法。
    /// 为了不干扰展示核心实现，我们使用一个简化的var int实现：
    /// size小于255时，长度为1字节，超过255时，使用5字节。
    /// 实践中自己换成合适的compact算法。
    /// </summary>
    public class Frame
    {
        private readonly byte[] _data;

        private int _tail;

        public int Size => _tail;

        public Frame(byte[] data)
        {
            _data = data;
        }

        public Frame()
        {
            _data = new byte[1000000];
        }

        public void PrepareFrame()
        {
            _tail = 1;
        }

        public void WriteByte(byte x)
        {
            // 为了简化起见，我们不处理 _data 不够的细节，假设空间足够
            _data[_tail++] = x;
        }

        public unsafe void FinishFrame()
        {
            int length = _tail - 1;
            if (length < 255)
            {
                _data[0] = (byte)length;
            }
            else
            {
                _data[0] = 255;
                fixed (byte* p = &_data[1])
                {
                    // 将 [1,4] 字节移到 [_tail, _tail + 3]
                    *(int*)(p + length) = *(int*)p;

                    // 将 length 写入 [1,4]
                    *(int*)p = length;
                }
                _tail += 4;
            }
        }

        /// <summary>
        /// 注意，此函数只能被调用一次。可以自行调整设计。
        /// </summary>
        /// <returns></returns>
        public unsafe Memory<byte> DecodeFrame()
        {
            int length;
            if (_data[0] < 255)
            {
                length = _data[0];
            }
            else
            {
                fixed (byte* p = &_data[1])
                {
                    length = *(int*)p;
                    // 将 [length + 1, length + 4] 写入 [1,4]
                    *(int*)p = *(int*)(p + length);
                }
            }
            return new Memory<byte>(_data, 1, length);
        }
    }
}
