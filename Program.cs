using System;
using System.Diagnostics;

namespace VariableFrameHeader
{
    class Program
    {
        static void Main(string[] args)
        {

            foreach (int size in new int[] { 0, 10, 120, 250, 0x1000, 0x10000 })
            {
                var frame = new Frame();
                frame.PrepareFrame();
                for (int i = 0; i < size; i++)
                {
                    frame.WriteByte((byte)i);
                }
                frame.FinishFrame();
                Console.WriteLine("frame body size:{0} total size:{1}", size, frame.Size);

                var body = frame.DecodeFrame();
                Debug.Assert(body.Length == size);

                var span = body.Span;
                for (int i = 0; i < size; i++)
                {
                    Debug.Assert(span[i] == (byte)i);
                }

            }
        }
    }
}
