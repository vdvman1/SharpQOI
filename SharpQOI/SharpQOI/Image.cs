using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Buffers.Binary;

namespace SharpQOI
{
    public class Image
    {
        public static readonly ReadOnlyMemory<byte> HeaderMagic = new(new[] {
            (byte)'q',
            (byte)'o',
            (byte)'i',
            (byte)'f'
        });

        private const int OpBits = 2;
        private const int DataBits = 8 - OpBits;
        private const byte OpMask = 0b11 << DataBits;
        private const byte DataMask = unchecked((byte)~OpMask);

        /// <summary>
        /// Width*Height pixels stored in row-major order
        /// </summary>
        private readonly Color[,] Pixels;

        public readonly ColorSpace ColorSpace;

        public Color this[int x, int y]
        {
            get => Pixels[y, x];
            set => Pixels[y, x] = value;
        }

        public int Width => Pixels.GetLength(1);

        public int Height => Pixels.GetLength(0);

        public Image(uint width, uint height, ColorSpace colorSpace)
        {
            Pixels = new Color[height, width];
            ColorSpace = colorSpace;
        }

        public static async Task<Image> LoadAsync(string path) => ParseFromBytes(await File.ReadAllBytesAsync(path), path);

        public static Image ParseFromBytes(ReadOnlySpan<byte> bytes, string name = "")
        {
            if (bytes.Length < 14)
            {
                ParseError("Invalid QOI image, missing header");
            }

            if (!bytes.StartsWith(HeaderMagic.Span))
            {
                ParseError("Not a QOI image, incorrect magic header bytes");
            }
            bytes = bytes[HeaderMagic.Length..];

            var width = BinaryPrimitives.ReadUInt32BigEndian(bytes);
            bytes = bytes[sizeof(UInt32)..];

            var height = BinaryPrimitives.ReadUInt32BigEndian(bytes);
            bytes = bytes[sizeof(UInt32)..];

            var channels = bytes[0];
            bytes = bytes[1..];
            if (channels is not (3 or 4))
            {
                ParseError($"Unsupported channel count {channels}");
            }

            var colorSpace = (ColorSpace)bytes[0];
            bytes = bytes[1..];
            if (!Enum.IsDefined(colorSpace))
            {
                ParseError($"Unsupported color space {colorSpace}");
            }

            var image = new Image(width, height, colorSpace);
            var color = new Color(0, 0, 0, 255);
            const int IndexCount = 64;
            Span<Color> seenColors = stackalloc Color[IndexCount];
            seenColors.Clear();
            int runLength = 0;

            for (uint y = 0; y < height; y++)
            {
                for (uint x = 0; x < width; x++)
                {
                    if (x == (uint)393 && y == (uint)287)
                    {
                        Console.WriteLine();
                    }

                    if (runLength > 0)
                    {
                        runLength--;
                        image.Pixels[y, x] = color;
                        continue;
                    }

                    if (bytes.Length == 0)
                    {
                        ParseError("Size mismatch, not enough data for specified size");
                    }


                    var op = bytes[0];
                    TrueByte trueOp = op;
                    switch ((op & OpMask) >> DataBits)
                    {
                        case 0b00:
                            // Index
                            color = seenColors[op & DataMask];
                            bytes = bytes[1..];
                            break;
                        case 0b01:
                            // Diff
                            {
                                var dRed = ((trueOp >> 4) & 0b11) - 2;
                                color.Red += dRed;

                                var dGreen = ((trueOp >> 2) & 0b11) - 2;
                                color.Green += dGreen;

                                var dBlue = (trueOp & 0b11) - 2;
                                color.Blue += dBlue;
                            }

                            seenColors[color.GetHashCode() % IndexCount] = color;
                            bytes = bytes[1..];
                            break;
                        case 0b10:
                            // Luma
                            if (bytes.Length < 2)
                            {
                                ParseError("Incomplete QOI_OP_LUMA");
                            }

                            {
                                var dGreen = (trueOp & DataMask) - 32;
                                color.Green += dGreen;

                                const int Mask = 0b1111;
                                const int MaskBits = 4;
                                TrueByte data = bytes[1];
                                // dr_dg = (r - pr) - (g - dg)
                                // dr_dg = dr - dg
                                // dr_dg + dg = dr
                                var dRed = ((data >> MaskBits) & Mask) + dGreen - 8;
                                color.Red += dRed;

                                var dBlue = (data & Mask) + dGreen - 8;
                                color.Blue += dBlue;
                            }
                            seenColors[color.GetHashCode() % IndexCount] = color;
                            bytes = bytes[2..];
                            break;
                        case 0b11:
                            // RGB, RGBA, or run
                            switch (op)
                            {
                                case 0b11111110:
                                    // RGB
                                    if (bytes.Length < 4)
                                    {
                                        ParseError("Incomplete QOI_OP_RGB");
                                    }

                                    color.Red = bytes[1];
                                    color.Green = bytes[2];
                                    color.Blue = bytes[3];
                                    seenColors[color.GetHashCode() % IndexCount] = color;
                                    bytes = bytes[4..];
                                    break;
                                case 0b11111111:
                                    // RGBA
                                    if (bytes.Length < 5)
                                    {
                                        ParseError("Incomplete QOI_OP_RGBA");
                                    }

                                    color = new(bytes[1], bytes[2], bytes[3], bytes[4]);
                                    seenColors[color.GetHashCode() % IndexCount] = color;
                                    bytes = bytes[5..];
                                    break;
                                default:
                                    // run
                                    runLength = (op & DataMask)/* + 1*/;
                                    bytes = bytes[1..];
                                    break;
                            }
                            break;
                    }

                    image.Pixels[y, x] = color;
                }
            }

            return image;

            [DoesNotReturn]
            void ParseError(string msg)
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    msg += $": {name}";
                }
                throw new InvalidOperationException(msg);
            }
        }
    }
}
