using System;
using System.Runtime.InteropServices;

namespace TotoBook.Spi
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PictureInfo
    {
        /// <summary>
        /// 画像を展開する位置
        /// </summary>
        public int Left;

        /// <summary>
        /// 画像を展開する位置
        /// </summary>
        public int Top;

        /// <summary>
        /// 画像の幅(pixel)
        /// </summary>
        public int Width;

        /// <summary>
        /// 画像の高さ(pixel)
        /// </summary>
        public int Height;

        /// <summary>
        /// 画素の水平方向密度
        /// </summary>
        public ushort XDensity;

        /// <summary>
        /// 画素の垂直方向密度
        /// </summary>
        public ushort YDensity;

        /// <summary>
        /// 画素当たりのbit数
        /// </summary>
        public short ColorDepth;

        /// <summary>
        /// 画像内のテキスト情報
        /// </summary>
        public IntPtr Text;
    }
}