#region License, Terms and Conditions
//
// ImageTarget.cs
//
// Authors: Kori Francis <twitter.com/djbyter>
// Copyright (C) 2013 Clinical Support Systems, Inc. All rights reserved.
// 
//  THIS FILE IS LICENSED UNDER THE MIT LICENSE AS OUTLINED IMMEDIATELY BELOW:
//
//  Permission is hereby granted, free of charge, to any person obtaining a
//  copy of this software and associated documentation files (the "Software"),
//  to deal in the Software without restriction, including without limitation
//  the rights to use, copy, modify, merge, publish, distribute, sublicense,
//  and/or sell copies of the Software, and to permit persons to whom the
//  Software is furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
//  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
//  DEALINGS IN THE SOFTWARE.
//
#endregion

#region Imports
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using NLog;
using NLog.Targets;
#endregion

namespace CSS.NLog.ImageExtension
{
    /// <summary>
    /// The whole point of ImageTarget is to allow someone to log a screenshot from their app to a known directory
    /// The target takes LogPath which will usually be set to the base path for logs, then you can filter down by using
    /// event parameters to help build the actual path. Event parameters are overlay, path and filename.
    /// </summary>
    [Target("ImageTarget")]
    public sealed class ImageTarget : Target
    {
        public ImageTarget()
        {
            // Let's set a default, just in case.
            LogPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        }

        public string LogPath { get; set; }

        protected override void Write(LogEventInfo logEvent)
        {
            try
            {
                using (var ms = new MemoryStream(Convert.FromBase64String(logEvent.FormattedMessage)))
                {
                    var bitmap = new Bitmap(ms);
                    if (logEvent.Parameters.FirstOrDefault(p => p.ToString().ToLowerInvariant().Contains("overlay")) != null)
                    {
                        // If there's an overlay parameter in the event, let's write it on the image as an overlay
                        var loginParameter = logEvent.Parameters.FirstOrDefault(p => p.ToString().ToLowerInvariant().Contains("overlay")).ToString();
                        var login = loginParameter.Split('=')[1]; // we've just seperated it by = for simplicity
                        if (!string.IsNullOrEmpty(login))
                        {
                            using (var graphics = Graphics.FromImage(bitmap))
                            {
                                using (var font = new Font("Arial", 30))
                                {
                                    graphics.DrawString(login, font, Brushes.Red, new PointF(0f, 0f));
                                }
                            }
                        }
                    }

                    var customPath = string.Empty;
                    if (logEvent.Parameters.FirstOrDefault(p => p.ToString().ToLowerInvariant().Contains("path")) != null)
                    {
                        var pathParameter = logEvent.Parameters.FirstOrDefault(p => p.ToString().ToLowerInvariant().Contains("path")).ToString();
                        customPath = pathParameter.Split('=')[1]; // we've just seperated it by = for simplicity
                    }

                    var imageFilename = string.Format("{0}.jpg", DateTime.Now.ToString("yyyyMMddHHmmssffff")); // default is timestamp.jpg
                    if (logEvent.Parameters.FirstOrDefault(p => p.ToString().ToLowerInvariant().Contains("filename")) != null)
                    {
                        var filenameParameter = logEvent.Parameters.FirstOrDefault(p => p.ToString().ToLowerInvariant().Contains("filename")).ToString();
                        imageFilename = filenameParameter.Split('=')[1]; // we've just seperated it by = for simplicity
                    }

                    // If a custom path was specified during the logging call - let's use it when saving the screenshot
                    var filePath = LogPath;
                    if (!string.IsNullOrWhiteSpace(customPath))
                    {
                        filePath = Path.Combine(LogPath, customPath);
                        if (!Directory.Exists(filePath))
                        {
                            Directory.CreateDirectory(filePath);
                        }
                    }
                    else
                    {
                        if (!Directory.Exists(filePath))
                        {
                            Directory.CreateDirectory(filePath);
                        }
                    }

                    // Let's make sure we don't overwrite an existing file, make it more unique
                    for (int fileNum = 0; fileNum < 100; fileNum++)
                    {
                        if (!File.Exists(Path.Combine(filePath, imageFilename)))
                        {
                            break;
                        }
                        imageFilename = string.Format("{0}-{1}.jpg", imageFilename, fileNum);
                    }

                    // Save the jpg screenshot
                    bitmap.Save(Path.Combine(filePath, imageFilename), ImageFormat.Jpeg);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
