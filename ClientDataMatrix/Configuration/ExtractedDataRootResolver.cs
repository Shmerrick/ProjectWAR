using System;
using System.Collections.Generic;
using System.IO;

namespace ClientDataMatrix.Configuration
{
    public static class ExtractedDataRootResolver
    {
        private const string DefaultExtractedRoot = @"C:\Users\Admin\Downloads\myps";

        // Ordering matters and used to be wrong. Pictures\WAR_extracted was tried first, and it is
        // the older, partial extraction -- 701 usable files against 8,499 -- from before more myp
        // hashes were solved. Whenever it exists it silently shadows the current one and every
        // answer the tool gives is drawn from an incomplete client, which is indistinguishable from
        // the client simply not containing the thing you asked about. Newest extraction first.
        private const string LegacyExtractedRoot = @"C:\Users\Admin\Pictures\WAR_extracted";

        public static string Resolve(string explicitPath)
        {
            List<string> candidates = new List<string>();
            if (!string.IsNullOrWhiteSpace(explicitPath))
                candidates.Add(explicitPath);

            candidates.Add(DefaultExtractedRoot);
            candidates.Add(LegacyExtractedRoot);
            candidates.Add(Path.Combine("data", "WAR_extracted"));
            candidates.Add(Path.Combine("..", "WAR_extracted"));

            foreach (string candidate in candidates)
            {
                if (string.IsNullOrWhiteSpace(candidate))
                    continue;

                string fullPath = Path.GetFullPath(candidate);
                if (Directory.Exists(fullPath))
                    return fullPath;
            }

            throw new DirectoryNotFoundException("Unable to locate the extracted WAR client root. Pass --root explicitly or place the files at " + DefaultExtractedRoot + ".");
        }
    }
}
