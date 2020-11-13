using System;
using System.Collections.Generic;
using System.IO;

namespace MuTest.Cpp.CodeGenerator.Common
{
    public static class FileUtility
    {
        public static void WriteLines(this string path, IList<string> lines)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (lines == null)
            {
                throw new ArgumentNullException(nameof(lines));
            }

            using var writer = new StreamWriter(path);
            foreach (var fileLine in lines)
            {
                writer.WriteLine(fileLine);
            }
        }
    }
}
