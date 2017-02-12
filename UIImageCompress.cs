using System;
using System.Collections.Generic;
using System.Text;
using UIKit;
using Foundation;
using System.IO;
using CoreGraphics;
using System.Drawing;
using System.Threading.Tasks;

namespace CandyCaneClub.iOS.Extensions
{
    public static class UIImageExtensions
    {
        public async static Task<UIImage> compressedImage(this UIImage image, float scaleFactor)
        {
           UIImage compressedImage = await Task.Run(() =>
            {
                UIImage scaledImage = new UIImage(image.CGImage, scaleFactor, image.Orientation);
                NSData scaledData = scaledImage.AsJPEG(0.0f);
                UIImage cimage = new UIImage(scaledData);
                return cimage;
            });
            return compressedImage;
        }
        public static UIImage compressedImage(this UIImage image, float width, float height)
        {
                int actualHeight = (int)image.Size.Height;
                int actualWidth = (int)image.Size.Width;
                float maxHeight = height;
                float maxWidth = width;
                float imgRatio = (float)actualWidth / actualHeight;
                float maxRatio = maxWidth / maxHeight;

                if (actualHeight > maxHeight || actualWidth > maxWidth)
                {
                    if (imgRatio < maxRatio)
                    {
                        imgRatio = maxHeight / actualHeight;
                        actualWidth = (int)(imgRatio * actualWidth);
                        actualHeight = (int)maxHeight;
                    }
                    else if (imgRatio > maxRatio)
                    {
                        imgRatio = maxWidth / actualWidth;
                        actualHeight = (int)(imgRatio * actualHeight);
                        actualWidth = (int)maxWidth;
                    }
                    else
                    {
                        actualHeight = (int)maxHeight;
                        actualWidth = (int)maxWidth;

                    }
                }
                CGSize newSize = new CGSize(actualWidth, actualHeight);
                UIGraphics.BeginImageContext(newSize);
                image.Draw(new RectangleF(0, 0, (float)newSize.Width, (float)newSize.Height));
                UIImage img = UIGraphics.GetImageFromCurrentImageContext();
                UIGraphics.EndImageContext();
                NSData newImageData = img.AsJPEG(0.0f);
                UIImage newCompressedImage = new UIImage(newImageData);
                newImageData = null;
                return newCompressedImage;
        }
        static Double radians(double degrees) { return degrees * 3.1415 / 180; }

        public static UIImage rotateImage(this UIImage oldImage, int degrees)
        {
            UIView rotatedViewBox = new UIView(new CGRect(0, 0, oldImage.Size.Width, oldImage.Size.Height));
            rotatedViewBox.Transform = CGAffineTransform.MakeRotation((float)(degrees * 3.14) / 180); ;
            CGSize rotatedSize = rotatedViewBox.Frame.Size;

            //Create the bitmap context
            UIGraphics.BeginImageContext(rotatedSize);
            CGContext bitmap = UIGraphics.GetCurrentContext();

            //Move the origin to the middle of the image so we will rotate and scale around the center.
            bitmap.TranslateCTM(rotatedSize.Width / 2, rotatedSize.Height / 2);

            //Rotate the image context
            bitmap.RotateCTM((float)(degrees * 3.14 / 180));

            //Now, draw the rotated/scaled image into the context
            bitmap.ScaleCTM(1.0f, -1.0f);
            bitmap.DrawImage(new CGRect(-oldImage.Size.Width / 2, -oldImage.Size.Height / 2, oldImage.Size.Width, oldImage.Size.Height), oldImage.CGImage);

            UIImage newImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            /*
            Uncomment the following code for testing the Rotation effect
            You will see a new image named Rotated in the document directory*/
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            System.Console.WriteLine("Document :" + documents);
            var filename = Path.Combine(documents, "Rotated.jpg");
            NSData x1 = newImage.AsJPEG();
            x1.Save(filename, true);
            return newImage;
        }


    }
}
