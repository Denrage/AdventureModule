using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Denrage.AdventureModule.Helper
{
    public static class ClipboardHelper
    {
        public const string PngFormat = "PNG";

        private static uint GetPngFormatNumber()
        {
            uint lastRetrievedFormat = 0;
            while (0 != (lastRetrievedFormat = Native.EnumClipboardFormats(lastRetrievedFormat)))
            {
                var name = GetClipboardFormatName(lastRetrievedFormat);
                if (name.Equals(PngFormat, StringComparison.OrdinalIgnoreCase))
                {
                    return lastRetrievedFormat;
                }
            }

            // Return Bitmap format number
            return 2;
        }

        private static string GetClipboardFormatName(uint ClipboardFormat)
        {
            var sb = new StringBuilder(1000);
            _ = Native.GetClipboardFormatName(ClipboardFormat, sb, sb.Capacity);
            return sb.ToString();
        }

        public static bool TryGetPngData(out byte[] data)
        {
            _ = Native.OpenClipboard(IntPtr.Zero);
            var pngFormatNumber = GetPngFormatNumber();
            var pointer = Native.GetClipboardData(pngFormatNumber);

            if (pointer == IntPtr.Zero)
            {
                _ = Native.CloseClipboard();
                data = Array.Empty<byte>();
                return false;
            }

            var sizePtr = Native.GlobalSize(pointer);
            var buffer = new byte[sizePtr.ToUInt64()];
            Marshal.Copy(pointer, buffer, 0, buffer.Length);
            _ = Native.CloseClipboard();

            data = buffer;
            return true;
        }
    }
}
