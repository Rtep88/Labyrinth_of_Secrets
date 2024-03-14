using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labyrinth_of_Secrets
{
    public struct Color : IEquatable<Color>
    {
        private uint _packedValue;

        public byte B
        {
            get
            {
                return (byte)(_packedValue >> 16);
            }
            set
            {
                _packedValue = (_packedValue & 0xFF00FFFFu) | (uint)(value << 16);
            }
        }


        public byte G
        {
            get
            {
                return (byte)(_packedValue >> 8);
            }
            set
            {
                _packedValue = (_packedValue & 0xFFFF00FFu) | (uint)(value << 8);
            }
        }

        public byte R
        {
            get
            {
                return (byte)_packedValue;
            }
            set
            {
                _packedValue = (_packedValue & 0xFFFFFF00u) | value;
            }
        }

        public byte A
        {
            get
            {
                return (byte)(_packedValue >> 24);
            }
            set
            {
                _packedValue = (_packedValue & 0xFFFFFFu) | (uint)(value << 24);
            }
        }

        public static Color Transparent { get; private set; }

        public static Color AliceBlue { get; private set; }

        public static Color AntiqueWhite { get; private set; }

        public static Color Aqua { get; private set; }

        public static Color Aquamarine { get; private set; }

        public static Color Azure { get; private set; }

        public static Color Beige { get; private set; }

        public static Color Bisque { get; private set; }

        public static Color Black { get; private set; }

        public static Color BlanchedAlmond { get; private set; }

        public static Color Blue { get; private set; }

        public static Color BlueViolet { get; private set; }

        public static Color Brown { get; private set; }

        public static Color BurlyWood { get; private set; }

        public static Color CadetBlue { get; private set; }

        public static Color Chartreuse { get; private set; }

        public static Color Chocolate { get; private set; }

        public static Color Coral { get; private set; }

        public static Color CornflowerBlue { get; private set; }

        public static Color Cornsilk { get; private set; }

        public static Color Crimson { get; private set; }

        public static Color Cyan { get; private set; }

        public static Color DarkBlue { get; private set; }

        public static Color DarkCyan { get; private set; }

        public static Color DarkGoldenrod { get; private set; }

        public static Color DarkGray { get; private set; }

        public static Color DarkGreen { get; private set; }

        public static Color DarkKhaki { get; private set; }

        public static Color DarkMagenta { get; private set; }

        public static Color DarkOliveGreen { get; private set; }

        public static Color DarkOrange { get; private set; }

        public static Color DarkOrchid { get; private set; }

        public static Color DarkRed { get; private set; }

        public static Color DarkSalmon { get; private set; }

        public static Color DarkSeaGreen { get; private set; }

        public static Color DarkSlateBlue { get; private set; }

        public static Color DarkSlateGray { get; private set; }

        public static Color DarkTurquoise { get; private set; }

        public static Color DarkViolet { get; private set; }

        public static Color DeepPink { get; private set; }

        public static Color DeepSkyBlue { get; private set; }

        public static Color DimGray { get; private set; }

        public static Color DodgerBlue { get; private set; }

        public static Color Firebrick { get; private set; }

        public static Color FloralWhite { get; private set; }

        public static Color ForestGreen { get; private set; }

        public static Color Fuchsia { get; private set; }

        public static Color Gainsboro { get; private set; }

        public static Color GhostWhite { get; private set; }

        public static Color Gold { get; private set; }

        public static Color Goldenrod { get; private set; }

        public static Color Gray { get; private set; }

        public static Color Green { get; private set; }

        public static Color GreenYellow { get; private set; }

        public static Color Honeydew { get; private set; }

        public static Color HotPink { get; private set; }

        public static Color IndianRed { get; private set; }

        public static Color Indigo { get; private set; }

        public static Color Ivory { get; private set; }

        public static Color Khaki { get; private set; }

        public static Color Lavender { get; private set; }

        public static Color LavenderBlush { get; private set; }

        public static Color LawnGreen { get; private set; }

        public static Color LemonChiffon { get; private set; }

        public static Color LightBlue { get; private set; }

        public static Color LightCoral { get; private set; }

        public static Color LightCyan { get; private set; }

        public static Color LightGoldenrodYellow { get; private set; }

        public static Color LightGray { get; private set; }

        public static Color LightGreen { get; private set; }

        public static Color LightPink { get; private set; }

        public static Color LightSalmon { get; private set; }

        public static Color LightSeaGreen { get; private set; }

        public static Color LightSkyBlue { get; private set; }

        public static Color LightSlateGray { get; private set; }

        public static Color LightSteelBlue { get; private set; }

        public static Color LightYellow { get; private set; }

        public static Color Lime { get; private set; }

        public static Color LimeGreen { get; private set; }

        public static Color Linen { get; private set; }

        public static Color Magenta { get; private set; }

        public static Color Maroon { get; private set; }

        public static Color MediumAquamarine { get; private set; }

        public static Color MediumBlue { get; private set; }

        public static Color MediumOrchid { get; private set; }

        public static Color MediumPurple { get; private set; }

        public static Color MediumSeaGreen { get; private set; }

        public static Color MediumSlateBlue { get; private set; }

        public static Color MediumSpringGreen { get; private set; }

        public static Color MediumTurquoise { get; private set; }

        public static Color MediumVioletRed { get; private set; }

        public static Color MidnightBlue { get; private set; }

        public static Color MintCream { get; private set; }

        public static Color MistyRose { get; private set; }

        public static Color Moccasin { get; private set; }

        public static Color MonoGameOrange { get; private set; }

        public static Color NavajoWhite { get; private set; }

        public static Color Navy { get; private set; }

        public static Color OldLace { get; private set; }

        public static Color Olive { get; private set; }

        public static Color OliveDrab { get; private set; }

        public static Color Orange { get; private set; }

        public static Color OrangeRed { get; private set; }

        public static Color Orchid { get; private set; }

        public static Color PaleGoldenrod { get; private set; }

        public static Color PaleGreen { get; private set; }

        public static Color PaleTurquoise { get; private set; }

        public static Color PaleVioletRed { get; private set; }

        public static Color PapayaWhip { get; private set; }

        public static Color PeachPuff { get; private set; }

        public static Color Peru { get; private set; }

        public static Color Pink { get; private set; }

        public static Color Plum { get; private set; }

        public static Color PowderBlue { get; private set; }

        public static Color Purple { get; private set; }

        public static Color Red { get; private set; }

        public static Color RosyBrown { get; private set; }

        public static Color RoyalBlue { get; private set; }

        public static Color SaddleBrown { get; private set; }

        public static Color Salmon { get; private set; }

        public static Color SandyBrown { get; private set; }

        public static Color SeaGreen { get; private set; }

        public static Color SeaShell { get; private set; }

        public static Color Sienna { get; private set; }

        public static Color Silver { get; private set; }

        public static Color SkyBlue { get; private set; }

        public static Color SlateBlue { get; private set; }

        public static Color SlateGray { get; private set; }

        public static Color Snow { get; private set; }

        public static Color SpringGreen { get; private set; }

        public static Color SteelBlue { get; private set; }

        public static Color Tan { get; private set; }

        public static Color Teal { get; private set; }

        public static Color Thistle { get; private set; }

        public static Color Tomato { get; private set; }

        public static Color Turquoise { get; private set; }

        public static Color Violet { get; private set; }

        public static Color Wheat { get; private set; }

        public static Color White { get; private set; }

        public static Color WhiteSmoke { get; private set; }

        public static Color Yellow { get; private set; }

        public static Color YellowGreen { get; private set; }

        [CLSCompliant(false)]
        public uint PackedValue
        {
            get
            {
                return _packedValue;
            }
            set
            {
                _packedValue = value;
            }
        }

        internal string DebugDisplayString => R + "  " + G + "  " + B + "  " + A;

        static Color()
        {
            Transparent = new Color(0u);
            AliceBlue = new Color(4294965488u);
            AntiqueWhite = new Color(4292340730u);
            Aqua = new Color(4294967040u);
            Aquamarine = new Color(4292149119u);
            Azure = new Color(4294967280u);
            Beige = new Color(4292670965u);
            Bisque = new Color(4291093759u);
            Black = new Color(4278190080u);
            BlanchedAlmond = new Color(4291685375u);
            Blue = new Color(4294901760u);
            BlueViolet = new Color(4293012362u);
            Brown = new Color(4280953509u);
            BurlyWood = new Color(4287084766u);
            CadetBlue = new Color(4288716383u);
            Chartreuse = new Color(4278255487u);
            Chocolate = new Color(4280183250u);
            Coral = new Color(4283465727u);
            CornflowerBlue = new Color(4293760356u);
            Cornsilk = new Color(4292671743u);
            Crimson = new Color(4282127580u);
            Cyan = new Color(4294967040u);
            DarkBlue = new Color(4287299584u);
            DarkCyan = new Color(4287335168u);
            DarkGoldenrod = new Color(4278945464u);
            DarkGray = new Color(4289309097u);
            DarkGreen = new Color(4278215680u);
            DarkKhaki = new Color(4285249469u);
            DarkMagenta = new Color(4287299723u);
            DarkOliveGreen = new Color(4281297749u);
            DarkOrange = new Color(4278226175u);
            DarkOrchid = new Color(4291572377u);
            DarkRed = new Color(4278190219u);
            DarkSalmon = new Color(4286224105u);
            DarkSeaGreen = new Color(4287347855u);
            DarkSlateBlue = new Color(4287315272u);
            DarkSlateGray = new Color(4283387695u);
            DarkTurquoise = new Color(4291939840u);
            DarkViolet = new Color(4292018324u);
            DeepPink = new Color(4287829247u);
            DeepSkyBlue = new Color(4294950656u);
            DimGray = new Color(4285098345u);
            DodgerBlue = new Color(4294938654u);
            Firebrick = new Color(4280427186u);
            FloralWhite = new Color(4293982975u);
            ForestGreen = new Color(4280453922u);
            Fuchsia = new Color(4294902015u);
            Gainsboro = new Color(4292664540u);
            GhostWhite = new Color(4294965496u);
            Gold = new Color(4278245375u);
            Goldenrod = new Color(4280329690u);
            Gray = new Color(4286611584u);
            Green = new Color(4278222848u);
            GreenYellow = new Color(4281335725u);
            Honeydew = new Color(4293984240u);
            HotPink = new Color(4290013695u);
            IndianRed = new Color(4284243149u);
            Indigo = new Color(4286709835u);
            Ivory = new Color(4293984255u);
            Khaki = new Color(4287424240u);
            Lavender = new Color(4294633190u);
            LavenderBlush = new Color(4294308095u);
            LawnGreen = new Color(4278254716u);
            LemonChiffon = new Color(4291689215u);
            LightBlue = new Color(4293318829u);
            LightCoral = new Color(4286611696u);
            LightCyan = new Color(4294967264u);
            LightGoldenrodYellow = new Color(4292016890u);
            LightGray = new Color(4292072403u);
            LightGreen = new Color(4287688336u);
            LightPink = new Color(4290885375u);
            LightSalmon = new Color(4286226687u);
            LightSeaGreen = new Color(4289376800u);
            LightSkyBlue = new Color(4294626951u);
            LightSlateGray = new Color(4288252023u);
            LightSteelBlue = new Color(4292789424u);
            LightYellow = new Color(4292935679u);
            Lime = new Color(4278255360u);
            LimeGreen = new Color(4281519410u);
            Linen = new Color(4293325050u);
            Magenta = new Color(4294902015u);
            Maroon = new Color(4278190208u);
            MediumAquamarine = new Color(4289383782u);
            MediumBlue = new Color(4291624960u);
            MediumOrchid = new Color(4292040122u);
            MediumPurple = new Color(4292571283u);
            MediumSeaGreen = new Color(4285641532u);
            MediumSlateBlue = new Color(4293814395u);
            MediumSpringGreen = new Color(4288346624u);
            MediumTurquoise = new Color(4291613000u);
            MediumVioletRed = new Color(4286911943u);
            MidnightBlue = new Color(4285536537u);
            MintCream = new Color(4294639605u);
            MistyRose = new Color(4292994303u);
            Moccasin = new Color(4290110719u);
            MonoGameOrange = new Color(4278205671u);
            NavajoWhite = new Color(4289584895u);
            Navy = new Color(4286578688u);
            OldLace = new Color(4293326333u);
            Olive = new Color(4278222976u);
            OliveDrab = new Color(4280520299u);
            Orange = new Color(4278232575u);
            OrangeRed = new Color(4278207999u);
            Orchid = new Color(4292243674u);
            PaleGoldenrod = new Color(4289390830u);
            PaleGreen = new Color(4288215960u);
            PaleTurquoise = new Color(4293848751u);
            PaleVioletRed = new Color(4287852763u);
            PapayaWhip = new Color(4292210687u);
            PeachPuff = new Color(4290370303u);
            Peru = new Color(4282353101u);
            Pink = new Color(4291543295u);
            Plum = new Color(4292714717u);
            PowderBlue = new Color(4293320880u);
            Purple = new Color(4286578816u);
            Red = new Color(4278190335u);
            RosyBrown = new Color(4287598524u);
            RoyalBlue = new Color(4292962625u);
            SaddleBrown = new Color(4279453067u);
            Salmon = new Color(4285694202u);
            SandyBrown = new Color(4284523764u);
            SeaGreen = new Color(4283927342u);
            SeaShell = new Color(4293850623u);
            Sienna = new Color(4281160352u);
            Silver = new Color(4290822336u);
            SkyBlue = new Color(4293643911u);
            SlateBlue = new Color(4291648106u);
            SlateGray = new Color(4287660144u);
            Snow = new Color(4294638335u);
            SpringGreen = new Color(4286578432u);
            SteelBlue = new Color(4290019910u);
            Tan = new Color(4287411410u);
            Teal = new Color(4286611456u);
            Thistle = new Color(4292394968u);
            Tomato = new Color(4282868735u);
            Turquoise = new Color(4291878976u);
            Violet = new Color(4293821166u);
            Wheat = new Color(4289978101u);
            White = new Color(uint.MaxValue);
            WhiteSmoke = new Color(4294309365u);
            Yellow = new Color(4278255615u);
            YellowGreen = new Color(4281519514u);
        }

        [CLSCompliant(false)]
        public Color(uint packedValue)
        {
            _packedValue = packedValue;
        }

        public Color(Color color, int alpha)
        {
            if ((alpha & 0xFFFFFF00u) != 0L)
            {
                uint num = (uint)Math.Clamp(alpha, 0, 255);
                _packedValue = (color._packedValue & 0xFFFFFFu) | (num << 24);
            }
            else
            {
                _packedValue = (color._packedValue & 0xFFFFFFu) | (uint)(alpha << 24);
            }
        }

        public Color(Color color, float alpha)
            : this(color, (int)(alpha * 255f))
        {
        }

        public Color(float r, float g, float b)
            : this((int)(r * 255f), (int)(g * 255f), (int)(b * 255f))
        {
        }

        public Color(float r, float g, float b, float alpha)
            : this((int)(r * 255f), (int)(g * 255f), (int)(b * 255f), (int)(alpha * 255f))
        {
        }

        public Color(int r, int g, int b)
        {
            _packedValue = 4278190080u;
            if (((r | g | b) & 0xFFFFFF00u) != 0L)
            {
                uint num = (uint)Math.Clamp(r, 0, 255);
                uint num2 = (uint)Math.Clamp(g, 0, 255);
                uint num3 = (uint)Math.Clamp(b, 0, 255);
                _packedValue |= (num3 << 16) | (num2 << 8) | num;
            }
            else
            {
                _packedValue |= (uint)((b << 16) | (g << 8) | r);
            }
        }

        public Color(int r, int g, int b, int alpha)
        {
            if (((r | g | b | alpha) & 0xFFFFFF00u) != 0L)
            {
                uint num = (uint)Math.Clamp(r, 0, 255);
                uint num2 = (uint)Math.Clamp(g, 0, 255);
                uint num3 = (uint)Math.Clamp(b, 0, 255);
                uint num4 = (uint)Math.Clamp(alpha, 0, 255);
                _packedValue = (num4 << 24) | (num3 << 16) | (num2 << 8) | num;
            }
            else
            {
                _packedValue = (uint)((alpha << 24) | (b << 16) | (g << 8) | r);
            }
        }

        public Color(byte r, byte g, byte b, byte alpha)
        {
            _packedValue = (uint)((alpha << 24) | (b << 16) | (g << 8) | r);
        }

        public static bool operator ==(Color a, Color b)
        {
            return a._packedValue == b._packedValue;
        }

        public static bool operator !=(Color a, Color b)
        {
            return a._packedValue != b._packedValue;
        }

        public override int GetHashCode()
        {
            return _packedValue.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Color)
            {
                return Equals((Color)obj);
            }

            return false;
        }

        public static Color Multiply(Color value, float scale)
        {
            return new Color((int)((float)(int)value.R * scale), (int)((float)(int)value.G * scale), (int)((float)(int)value.B * scale), (int)((float)(int)value.A * scale));
        }

        public static Color operator *(Color value, float scale)
        {
            return new Color((int)((float)(int)value.R * scale), (int)((float)(int)value.G * scale), (int)((float)(int)value.B * scale), (int)((float)(int)value.A * scale));
        }

        public static Color operator *(float scale, Color value)
        {
            return new Color((int)((float)(int)value.R * scale), (int)((float)(int)value.G * scale), (int)((float)(int)value.B * scale), (int)((float)(int)value.A * scale));
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder(25);
            stringBuilder.Append("{R:");
            stringBuilder.Append(R);
            stringBuilder.Append(" G:");
            stringBuilder.Append(G);
            stringBuilder.Append(" B:");
            stringBuilder.Append(B);
            stringBuilder.Append(" A:");
            stringBuilder.Append(A);
            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }

        public static Color FromNonPremultiplied(int r, int g, int b, int a)
        {
            return new Color(r * a / 255, g * a / 255, b * a / 255, a);
        }

        public bool Equals(Color other)
        {
            return PackedValue == other.PackedValue;
        }

        public void Deconstruct(out byte r, out byte g, out byte b)
        {
            r = R;
            g = G;
            b = B;
        }

        public void Deconstruct(out float r, out float g, out float b)
        {
            r = (float)(int)R / 255f;
            g = (float)(int)G / 255f;
            b = (float)(int)B / 255f;
        }

        public void Deconstruct(out byte r, out byte g, out byte b, out byte a)
        {
            r = R;
            g = G;
            b = B;
            a = A;
        }

        public void Deconstruct(out float r, out float g, out float b, out float a)
        {
            r = (float)(int)R / 255f;
            g = (float)(int)G / 255f;
            b = (float)(int)B / 255f;
            a = (float)(int)A / 255f;
        }
    }
}
