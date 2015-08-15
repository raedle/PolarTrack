using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Emgu.CV.External.Structure;
using Emgu.CV.Structure;

namespace Emgu.CV.External.Extensions
{
    public static class EmguExtensions
    {
        #region static members

        public const float DefaultLowConfidence = 32001.0f;
        public const float DefaultSaturation = 32002.0f;

        public static float LowConfidence = DefaultLowConfidence;
        public static float Saturation = DefaultSaturation;

        #endregion

        private static readonly Rgb[] Gradient = CreateGradient();

        private static Rgb[] CreateGradient()
        {
            var gradient = new Rgb[4 * 256];
            int i = 0;

            /* Blue 0,0,255 --> Cyan 0,255,255 */
            for (int j = 0; j < 256; j++) gradient[i++] = new Rgb(0, j, 255);

            /* Cyan 0,255,255 --> Green 0,255,0  */
            for (int j = 0; j < 256; j++) gradient[i++] = new Rgb(0, 255, 255 - j);

            /* Green 0,255,0 --> Yellow 255,255,0 */
            for (int j = 0; j < 256; j++) gradient[i++] = new Rgb(j, 255, 0);

            /* Yellow 255,255,0 --> Red 255,0,0 */
            for (int j = 0; j < 256; j++) gradient[i++] = new Rgb(255, 255 - j, 0);

            return gradient;
        }

        public static BitmapSource ToBitmapSource(this IImage image, bool freeze = false)
        {
            using (var bitmap = image.Bitmap)
            {
                using (var ms = new MemoryStream())
                {
                    bitmap.Save(ms, ImageFormat.Bmp);

                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = ms;
                    bitmapImage.EndInit();

                    if (freeze)
                        bitmapImage.Freeze();

                    return bitmapImage;
                }
            }
        }
        public static BitmapSource ToGradientBitmapSourceForHandTracking(this Image<Gray, float> image, bool freeze = true, float lowConfidence = DefaultLowConfidence, float saturation = DefaultSaturation)
        {
            var width = image.Width;
            var height = image.Height;

            var gradientImage = new Image<Rgb, byte>(width, height, Rgbs.White);
            var imageData = image.Data;


            Parallel.For(0, height, y =>
            {
                Rgb color;
                for (var x = 0; x < width; x++)
                {
                    var depth = imageData[y, x, 0];

                    if (depth > 10000)
                    {
                        color = Rgbs.Black;
                    }
                    else
                    {
                        var index = (int)((Gradient.Length - 1) * (255.0 - depth) / 255.0);

                        if (index < 0 || index > Gradient.Length - 1)
                            color = Rgbs.White;
                        else
                            color = Gradient[index];
                    }

                    gradientImage[y, x] = color;
                }
            });

            return gradientImage.ToBitmapSource();
        }

        public static BitmapSource ToGradientBitmapSource(this Image<Gray, float> image, bool freeze = false, float lowConfidence = DefaultLowConfidence, float saturation = DefaultSaturation)
        {
            var width = image.Width;
            var height = image.Height;

            var imageData = image.Data;
            var gradientImage = new Image<Rgb, byte>(width, height, Rgbs.White);

            Parallel.For(0, height, y =>
            {
                Rgb color;
                for (int x = 0; x < width; x++)
                {
                    var depth = imageData[y, x, 0];

                    if (depth == LowConfidence)
                    {
                        color = Rgbs.Black;
                    }
                    else if (depth == Saturation)
                    {
                        color = Rgbs.White;
                    }
                    else
                    {
                        var index = (int)((Gradient.Length - 1) * (255.0 - depth) / 255.0);
                        if (index < 0) index = 0;
                        if (index > Gradient.Length) index = Gradient.Length - 1;
                        color = Gradient[index];
                    }
                    gradientImage[y, x] = color;
                }
            });

            return gradientImage.ToBitmapSource(freeze);
        }

        public static IImage ToImage(this UMat mat)
        {
            return UMatToImage(mat);
        }

        public static UMat DeepClone(this UMat umat)
        {
            return new UMat(umat, new System.Drawing.Rectangle(0, 0, umat.Cols, umat.Rows));
        }

        public static void CopyToMask(this UMat src, ref UMat dst, UMat mask)
        {
            int nc = src.NumberOfChannels;
            int width = src.Cols;
            int height = src.Rows;
            CvEnum.DepthType depth = src.Depth;

            // <Rgb, byte>
            IImage img = null;
            var _d = dst.ToImage();
            var _m = mask.ToImage();
            if (nc == 3 && depth == CvEnum.DepthType.Cv8U)
            {
                img = src.ToImage<Rgb, byte>();
                (img as Image<Rgb,byte>).Copy(_d as Image<Rgb, byte>, _m as Image<Gray, byte>);
                dst = (_d as Image<Rgb, byte>).ToUMat();
                mask = (_m as Image<Gray, byte>).ToUMat();
            }
            // <Gray, float>
            else if(nc == 1 && depth == CvEnum.DepthType.Cv32F)
            {
                img = src.ToImage<Gray, float>();
                (img as Image<Gray, float>).Copy(_d as Image<Gray, float>, _m as Image<Gray, byte>);
                dst = (_d as Image<Gray, float>).ToUMat();
                mask = (_m as Image<Gray, byte>).ToUMat();
            }
            // <Gray, byte>
            else if (nc == 1 && depth == CvEnum.DepthType.Cv8U)
            {
                img = src.ToImage<Gray, byte>();
                (img as Image<Gray, byte>).Copy(_d as Image<Gray, byte>, _m as Image<Gray, byte>);
                dst = (_d as Image<Gray, byte>).ToUMat();
                mask = (_m as Image<Gray, byte>).ToUMat();
            }
        }

        //TODO remove me when changed
        public static IImage UMatToImage(this UMat umat)
        {
            int nc = umat.NumberOfChannels;
            int width = umat.Cols;
            int height = umat.Rows;
            CvEnum.DepthType depth = umat.Depth;

            // TODO fix me, this is more or less guesing nc and depth needs to be transformed
            // <Rgb, byte>
            IImage img = null;
            if (nc == 3 && depth == CvEnum.DepthType.Cv8U)
            {
                img = umat.ToImage<Rgb, byte>();
            }
            // <Gray, float>
            else if(nc == 1 && depth == CvEnum.DepthType.Cv32F)
            {
                img = umat.ToImage<Gray, float>();
            }
            // <Gray, byte>
            else if (nc == 1 && depth == CvEnum.DepthType.Cv8U)
            {
                img = umat.ToImage<Gray, byte>();
            }

            return img;

        }

        public static IImage Copy(this IImage image)
        {
            return image.CallInternalMethod<IImage>("Copy");
        }

        public static int Width(this IImage image)
        {
            return image.CallInternalProperty<int>("Width");
        }

        public static int Height(this IImage image)
        {
            return image.CallInternalProperty<int>("Height");
        }

        public static void Draw(this IImage image, string message, Emgu.CV.CvEnum.FontFace font, double fontScale, Point bottomLeft, IColor color)
        {
            image.CallInternalMethod("Draw", new[] { false, true, false, false }, new object[] { message, font, fontScale, bottomLeft, color });
        }

        public static Image<TColor, TDepth> Convert<TColor, TDepth>(this IImage image)
            where TColor : struct, IColor
            where TDepth : new()
        {
            return image.CallInternalMethod<Image<TColor, TDepth>>("Convert");
        }

        internal static void CallInternalMethod(this IImage image, string methodName, bool[] refTypes, params object[] parameters)
        {
            var types = parameters.Select((t, i) => !refTypes[i] ? t.GetType() : t.GetType().MakeByRefType()).ToArray();

            var type = image.GetType();

            var methodInfo = type.GetMethod(methodName, types);

            methodInfo.Invoke(image, parameters);
        }

        internal static void CallInternalMethod(this IImage image, string methodName, params object[] parameters)
        {
            var types = parameters.Select(parameter => parameter.GetType()).ToArray();

            var type = image.GetType();

            var methodInfo = type.GetMethod(methodName, types);

            methodInfo.Invoke(image, parameters);
        }

        internal static TResult CallInternalMethod<TResult>(this IImage image, string methodName, params object[] parameters)
        {
            var types = parameters.Select(parameter => parameter.GetType()).ToArray();

            var type = image.GetType();

            var methodInfo = type.GetMethod(methodName, types);

            return (TResult)methodInfo.Invoke(image, parameters);
        }

        internal static TResult CallInternalProperty<TResult>(this IImage image, string propertyName)
        {
            var type = image.GetType();

            var propertyInfo = type.GetProperty(propertyName, typeof(TResult));

            return (TResult)propertyInfo.GetValue(image);
        }

        //public static TResult CallInternal<TResult, TColor, TDepth>(this IImage image, Func<Image<TColor, TDepth>, TResult> func)
        //    where TColor : struct, IColor
        //    where TDepth : new()
        //{
        //    var castImage = image as Image<TColor, TDepth>;

        //    if (castImage == null)
        //        throw new InvalidCastException();

        //    return func.Invoke(castImage);
        //}

        //public static Image<TColor, TDepth> Cast<TColor, TDepth>(this IImage image)
        //    where TColor : struct, IColor
        //    where TDepth : new()
        //{
        //    return image as Image<TColor, TDepth>;
        //}

        public static double Length(this PointF self, PointF point)
        {
            return Math.Sqrt(Math.Pow(self.X - point.X, 2) + Math.Pow(self.Y - point.Y, 2));
        }

        public static bool IsRectangle(Point[] contour, double toleranceAngle = 10.0)
        {
            if (contour.Length != 4) return false;

            // determine if all the angles in the contour are within [80, 100] degree
            var pts = contour.ToArray();
            var edges = PointCollection.PolyLine(pts, true);

            var isRectangle = true;
            for (var i = 0; i < edges.Length; i++)
            {
                var edge = edges[(i + 1) % edges.Length];

                var angle = Math.Abs(edge.GetExteriorAngleDegree(edges[i]));

                var absAngle = Math.Abs(90.0 - angle);
                if (absAngle > toleranceAngle)
                {
                    isRectangle = false;
                    break;
                }
            }

            return isRectangle;
        }

        // TODO check if resize with small/negatiev values will work
        // TODO check if width + x / height + y can exceed maxRectange size
        /// <summary>
        /// Inflates the given rectangle by <see cref="increaseByFactor"/>. If the resulting rectangle is larger 
        /// than <see cref="maxRectangle"/> it will be croped to the maximum size.
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="increaseByFactor"></param>
        /// <param name="maxRectangle"></param>
        /// <returns></returns>
        public static Rectangle GetInflatedBy(this Rectangle rectangle, float increaseByFactor, Rectangle maxRectangle)
        {
            var x = rectangle.X;
            var y = rectangle.Y;
            var width = rectangle.Width;
            var height = rectangle.Height;
            var factoredMarginX = width * (increaseByFactor);
            var factoredMarginY = height * (increaseByFactor);

            var roiX = Math.Min(Math.Max(0, x + width / 2 - factoredMarginX / 2), maxRectangle.Width);
            var roiY = Math.Min(Math.Max(0, y + height / 2 - factoredMarginY / 2), maxRectangle.Height);
            var roiWidth = Math.Min(maxRectangle.Width - roiX, factoredMarginX);
            var roiHeight = Math.Min(maxRectangle.Height - roiY, factoredMarginY);

                return new Rectangle(
                (int)roiX,
                (int)roiY,
                (int)roiWidth,
                (int)roiHeight
                );
        }
    }
}
