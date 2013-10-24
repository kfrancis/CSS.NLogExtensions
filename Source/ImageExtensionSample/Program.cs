using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;

namespace ImageExtensionSample
{
    /// <summary>
    /// This is a simple program that generates something interesting to look at then logs the screenshot using the custom ImageTarget and NLog
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var edtLogin = "MOH12345";
            var sessionId = Guid.NewGuid().ToString();
            var logger = LogManager.GetLogger("screenshotLogger");

            // Let's put something random in the console
            Console.WriteLine();
            for (int i = 0; i < 50; i++)
            {
                Console.Write(Guid.NewGuid().ToString() + " ");
            }
            Console.WriteLine();
            
            // Capture the screen to use as the screenshot
            var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);
            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);
            gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);

            using (var ms = new MemoryStream())
            {
                bmpScreenshot.Save(ms, ImageFormat.Jpeg);
                logger.Trace(Convert.ToBase64String(ms.ToArray()),          // (Required) The image content, encoded in base64 (since we can only pass strings)
                    string.Format("overlay={0}", edtLogin),                 // (Optional) The string to overlay on the image
                    string.Format(@"path={0}\{1}\", edtLogin, sessionId),   // (Optional) The final path where the image should be saved within LogPath
                    string.Format("filename={0}.jpg", DateTime.Now.ToString("yyyyMMddHHmmssffff")));    // (Optional) The name of the image
            }
        }
    }
}
