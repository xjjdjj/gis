using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AYKJ.GISDevelop.Platform
{
    public class clsMapExportToImage
    {
        public bool ExportPNG(UIElement uiElement)
        {
            SaveFileDialog fsd = new SaveFileDialog();
            fsd.Filter = "PNG (*.png)|*.png";

            if (fsd.ShowDialog() == true)
            {
                try
                {
                    WriteableBitmap bitmap = new WriteableBitmap(uiElement, new ScaleTransform { ScaleX = 1, ScaleY = 1 });

                    int height = bitmap.PixelHeight;
                    int width = bitmap.PixelWidth;
                    bitmap.Invalidate();

                    System.IO.Stream pngStream = EncodePNG(bitmap);
                    Byte[] printBuffer = new Byte[pngStream.Length];
                    pngStream.Read(printBuffer, 0, printBuffer.Length);
                    System.IO.Stream fs = fsd.OpenFile();
                    fs.Write(printBuffer, 0, printBuffer.Length);
                    fs.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("导出PNG图片失败：\r\n" + ex.ToString());
                }
            }
            return false;
        }

        public static Stream EncodePNG(WriteableBitmap wb)
        {
            int width = wb.PixelWidth;
            int height = wb.PixelHeight;
            int adr;

            int rowLength = width * 4 + 1;

            byte[] buffer = new byte[rowLength * height];

            for (int y = 0; y < height; y++)
            {
                buffer[y * rowLength] = 0;

                for (int x = 0; x < width; x++)
                {
                    adr = y * rowLength + x * 4 + 3;

                    int pixel = wb.Pixels[x + y * width];

                    buffer[adr--] = (byte)(pixel & 0xff); pixel >>= 8;
                    buffer[adr--] = (byte)(pixel & 0xff); pixel >>= 8;
                    buffer[adr--] = (byte)(pixel & 0xff); pixel >>= 8;
                    buffer[adr] = (byte)(pixel & 0xff);
                }
            }

            return clsPngEncoder.Encode(buffer, width, height);
        }
    }
}
