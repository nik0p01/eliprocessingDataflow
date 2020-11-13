using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ELIProcessing
{
    static class Utilitis
    {
        public static void WriteFile(string path, int imageWidth, int imageHeight, ICollection<ushort> data)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.OpenOrCreate)))
            {
                writer.Write(4803653);
                writer.Write(32);
                writer.Write(512);
                writer.Write(0);
                writer.Write(imageWidth);
                writer.Write(imageHeight);
                writer.Write(16);
                writer.Write(1024);
                for (int i = 0; i < 120; i++)
                {
                    writer.Write(0);
                }

                foreach (var pixel in data)
                {
                    writer.Write(pixel);
                }
            }
        }
    }
}
