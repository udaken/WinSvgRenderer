// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PreviewHandlerCommon;

namespace WinSvgRenderer
{
    public sealed class SvgRenderer : IDisposable
    {
        public uint MaxOutputSize { get; set; } = 10000;

        private readonly WebBrowserExt _browser = new()
        {
            Dock = DockStyle.Fill,
            IsWebBrowserContextMenuEnabled = false,
            ScriptErrorsSuppressed = true,
            ScrollBarsEnabled = false,
            AllowNavigation = false,
        };

        public static void DisableAllFeatureOnProcess()
        {
            DisableAllFeature(NativeMethods.SetFeatureFlag.OnProcess);
        }

        public static void DisableAllFeatureOnThread()
        {
            DisableAllFeature(NativeMethods.SetFeatureFlag.OnThread);
        }

        private static void DisableAllFeature(NativeMethods.SetFeatureFlag flag)
        {
            for (int i = 0; i < (int)NativeMethods.INTERNETFEATURELIST.FEATURE_ENTRY_COUNT; i++)
            {
                int hresult = NativeMethods.CoInternetSetFeatureEnabled((NativeMethods.INTERNETFEATURELIST)i, flag, false);
                if (hresult < 0)
                    throw new System.ComponentModel.Win32Exception(hresult);
            }
        }

        public Bitmap Render(Stream stream, Size box, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            using var reader = new StreamReader(stream, encoding);
            string svgData = reader.ReadToEnd();

            var lang = System.Threading.Thread.CurrentThread.CurrentUICulture.ThreeLetterISOLanguageName;

            // Wrap the SVG content in HTML in IE Edge mode so we can ensure
            // we render properly.
            string wrappedContent = WrapSvgInHtml(svgData, Color.White, lang);
            return RenderContent(wrappedContent, box);
        }

        // Wait for the browser to render the content.
        private void WaitForBrowser()
        {
            while (_browser.IsBusy || _browser.ReadyState != WebBrowserReadyState.Complete)
            {
                Application.DoEvents();
            }
        }

        private Bitmap RenderContent(string content, Size box)
        {
            if (box.Height > MaxOutputSize || box.Height <= 0 ||
                box.Width > MaxOutputSize || box.Width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(box));
            }
            _browser.ClientSize = box;
            _browser.DocumentText = content;

            WaitForBrowser();

            // Check size of the rendered SVG.
            var svg = _browser.Document.GetElementsByTagName("svg").Cast<HtmlElement>().FirstOrDefault();
            if (svg == null)
                throw new InvalidDataException();

            var viewBox = svg.GetAttribute("viewbox");
            if (viewBox != null)
            {
                // Update the svg style to override any width or height explicit settings
                // Setting to 100% width and height will allow to scale to our intended size
                // Otherwise, we would end up with a scaled up blurry image.
                svg.Style = "max-width:100%;max-height:100%";

                WaitForBrowser();
            }

            // Update the size of the browser control to fit the SVG
            // in the visible viewport.
            _browser.Width = svg.OffsetRectangle.Width;
            _browser.Height = svg.OffsetRectangle.Height;

            WaitForBrowser();

            var outRectangle = svg.OffsetRectangle;
            Bitmap bitmap = new(outRectangle.Width, outRectangle.Height);
            _browser.DrawToBitmap(bitmap, outRectangle);
            if (bitmap.Width != box.Width && bitmap.Height != box.Height)
            {
                // We are not the appropriate size for caller.  Resize now while
                // respecting the aspect ratio.
                float scale = Math.Min((float)box.Width / bitmap.Width, (float)box.Height / bitmap.Height);
                int scaleWidth = (int)(bitmap.Width * scale);
                int scaleHeight = (int)(bitmap.Height * scale);
                bitmap = ResizeImage(bitmap, scaleWidth, scaleHeight);
            }
            return bitmap;
        }

        /// <summary>
        /// Wrap the SVG markup in HTML with a meta tag to ensure the
        /// WebBrowser control is in Edge mode to enable SVG rendering.
        /// We also set the padding and margin for the body to zero as
        /// there is a default margin of 8.
        /// </summary>
        /// <param name="svg">The original SVG markup.</param>
        /// <returns>The SVG content wrapped in HTML.</returns>
        private static string WrapSvgInHtml(string svg, Color backgroundColor, string? lang = null)
        {
            var langAttr = string.IsNullOrWhiteSpace(lang) ? "" : $"lang={lang}";
            string html = @"
                <!DOCTYPE html>
                <html " + langAttr + @">
                <head>
                  <meta http-equiv='X-UA-Compatible' content='IE=Edge'>
                </head>
                <body style='padding:0px;margin:0px;background-color:" + ColorTranslator.ToHtml(backgroundColor) + ";' scroll='no'>" +
                svg + @"
                </body>
                </html>";

            return html;
        }

        /// <summary>
        /// Resize the image with high quality to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        private Bitmap ResizeImage(Image image, int width, int height)
        {
            if (width <= 0 || width > MaxOutputSize)
                throw new ArgumentOutOfRangeException(nameof(width));

            if (height <= 0 || height > MaxOutputSize)
                throw new ArgumentOutOfRangeException(nameof(height));

            if (image == null)
                throw new ArgumentNullException(nameof(image));

            Bitmap destImage = new(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                graphics.Clear(Color.Transparent);
                graphics.DrawImage(image, 0, 0, width, height);
            }

            return destImage;
        }

        public void Dispose()
        {
            _browser.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
