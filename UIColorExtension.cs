using System;
using System.Collections.Generic;
using System.Text;
using UIKit;
using Foundation;
namespace CandyCaneClub.iOS.Extensions
{
    public static class UIColorExtensions
    {
        public static UIColor fromHex(this UIColor color, string hex)
        {
            string cString = hex.Trim().ToUpper();
             
            if (cString.Length < 6)
                return UIColor.Gray;
 
            if (cString.StartsWith("0X"))
                cString = cString.Substring(2);

            if (cString.Length != 6)
                return  UIColor.Gray;

            // Separate into r, g, b substrings  
            string rString = cString.Substring(0,2);
            string gString = cString.Substring(2,2);
            string bString = cString.Substring(4,2);

            // Scan values  
            uint r, g, b;
            r = (uint)Int32.Parse(rString);
            g = (uint)Int32.Parse(gString);
            b = (uint)Int32.Parse(bString);

            return new UIColor(((float)r / 255.0f), ((float)g / 255.0f), ((float)b / 255.0f), 1.0f );
        }
    }
}
