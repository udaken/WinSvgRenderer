using System;
using System.Drawing;
using System.Windows.Forms;
using WinSvgRenderer;

namespace WinSvgRenderer
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            SvgRenderer.DisableAllFeatureOnProcess();
            using var stream = System.IO.File.OpenRead(args[0]);
            using SvgRenderer renderer = new();
            renderer.Render(stream, new Size(1000, 1000)).Save("result.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
        }
    }
}
