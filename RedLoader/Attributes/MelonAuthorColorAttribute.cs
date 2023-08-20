using System;
using System.Drawing;
using RedLoader.Utils;

namespace RedLoader
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class MelonAuthorColorAttribute : Attribute
    {
        /// <summary>
        /// Color of the Author Log.
        /// </summary>
        [Obsolete("Color is obsolete. Use DrawingColor for full Color support.")]
        public ConsoleColor Color
        {
            get => LoggerUtils.DrawingColorToConsoleColor(DrawingColor);
            set => DrawingColor = LoggerUtils.ConsoleColorToDrawingColor(value);
        }

        /// <summary>
        /// Color of the Author Log.
        /// </summary>
        public Color DrawingColor { get; internal set; }

        public MelonAuthorColorAttribute() 
            => DrawingColor = RLog.DefaultTextColor;

        public MelonAuthorColorAttribute(Color drawingColor) 
            => DrawingColor = drawingColor;

        [Obsolete("ConsoleColor is obsolete, use the (int, int, int, int) or (Color) constructor instead.")]
        public MelonAuthorColorAttribute(ConsoleColor color) 
            => Color = ((color == ConsoleColor.Black) ? LoggerUtils.DrawingColorToConsoleColor(RLog.DefaultMelonColor) : color);

        public MelonAuthorColorAttribute(int alpha, int red, int green, int blue) 
            => DrawingColor =  System.Drawing.Color.FromArgb(alpha, red, green, blue);
    }
}