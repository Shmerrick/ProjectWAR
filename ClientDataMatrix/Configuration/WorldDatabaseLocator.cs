using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Linq;

namespace ClientDataMatrix.Configuration
{
    /// <summary>
    /// Finds the world database the running server actually uses, by reading its own config.
    ///
    /// WHY NOT A FLAG. A crosswalk that checks a different database than the server loads is worse
    /// than no crosswalk: it reports clean while the server serves broken rows, or reports damage
    /// that is not there. `bin/Release/Configs/World.xml` is generated from `WorldConfigs.cs` and is
    /// the same file WorldServer reads at startup, so taking the credentials from it means the two
    /// cannot drift apart. `--connection` still overrides for the case of pointing at a copy.
    /// </summary>
    public static class WorldDatabaseLocator
    {
        public static string Resolve(string explicitConnectionString)
        {
            if (!string.IsNullOrWhiteSpace(explicitConnectionString))
                return explicitConnectionString;

            foreach (string path in CandidatePaths())
            {
                if (!File.Exists(path))
                    continue;

                string built = TryBuild(path);
                if (built != null)
                    return built;
            }

            throw new FileNotFoundException(
                "Could not find Configs/World.xml to read the world database settings from. Looked next "
                + "to the executable and in bin/Release and bin/Debug from here up to four directories. "
                + "Build the solution once, or pass --connection.");
        }

        /// <summary>
        /// Everywhere World.xml plausibly is, most specific first.
        ///
        /// This used to be one relative path, `bin/Release/Configs/World.xml`, resolved against the
        /// current directory -- which is right only when the tool is launched from the repository
        /// root. Launched from `bin/Release`, where the exe actually lives and where a double-click
        /// puts the working directory, it looked for `bin/Release/bin/Release/Configs/World.xml` and
        /// the whole crosswalk failed with a file-not-found. So: start beside the executable, then
        /// walk up looking for the build folders, and only then fall back to the current directory.
        /// </summary>
        private static IEnumerable<string> CandidatePaths()
        {
            string[] configurations = { "Release", "Debug" };

            string executableDirectory = null;
            try
            {
                executableDirectory = Path.GetDirectoryName(
                    System.Reflection.Assembly.GetEntryAssembly() != null
                        ? System.Reflection.Assembly.GetEntryAssembly().Location
                        : System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
            catch (NotSupportedException)
            {
            }

            // The common case: Configs sits beside the exe in bin/<configuration>.
            if (!string.IsNullOrEmpty(executableDirectory))
                yield return Path.Combine(executableDirectory, "Configs", "World.xml");

            yield return Path.GetFullPath(Path.Combine("Configs", "World.xml"));

            // Then bin/<configuration>/Configs from here, and from each ancestor, so it works from
            // the repository root, from a subdirectory, and from beside the exe alike.
            var roots = new List<string> { Path.GetFullPath(".") };
            if (!string.IsNullOrEmpty(executableDirectory))
                roots.Add(executableDirectory);

            foreach (string start in roots)
            {
                string directory = start;
                for (int depth = 0; depth < 5 && directory != null; ++depth)
                {
                    foreach (string configuration in configurations)
                        yield return Path.Combine(directory, "bin", configuration, "Configs", "World.xml");

                    DirectoryInfo parent = Directory.GetParent(directory);
                    directory = parent == null ? null : parent.FullName;
                }
            }
        }

        private static string TryBuild(string configPath)
        {
            XDocument document;
            try
            {
                document = XDocument.Load(configPath);
            }
            catch (System.Xml.XmlException)
            {
                return null;
            }

            XElement world = document.Root == null ? null : document.Root.Element("WorldDatabase");
            if (world == null)
                return null;

            string server = Value(world, "Server");
            string database = Value(world, "Database");
            string user = Value(world, "Username");
            if (server.Length == 0 || database.Length == 0 || user.Length == 0)
                return null;

            string port = Value(world, "Port");
            if (port.Length == 0)
                port = "3306";

            var text = new System.Text.StringBuilder();
            text.Append("Server=").Append(server)
                .Append(";Port=").Append(port)
                .Append(";Database=").Append(database)
                .Append(";Uid=").Append(user)
                .Append(";Pwd=").Append(Value(world, "Password")).Append(';');

            // The server's own Custom string carries SslMode and AllowPublicKeyRetrieval, without
            // which MySQL 8 refuses a root connection over loopback. Reuse it rather than guessing.
            string custom = Value(world, "Custom");
            if (custom.Length > 0)
            {
                text.Append(custom);
                if (!custom.EndsWith(";", StringComparison.Ordinal))
                    text.Append(';');
            }

            return text.ToString();
        }

        private static string Value(XElement parent, string name)
        {
            XElement element = parent.Element(name);
            return element == null ? string.Empty : element.Value.Trim();
        }

        /// <summary>
        /// The repository root, found by walking up for `ProjectWAR.sln`, or null if not inside one.
        ///
        /// Reports default to `docs/data-matrix`, and a plain relative path puts them wherever the
        /// tool happened to be launched from -- running the exe in place scattered them into
        /// `bin/Release/docs/data-matrix`, away from the ones already in the repository and away
        /// from the .gitignore entry that covers them.
        /// </summary>
        public static string FindRepositoryRoot()
        {
            var starts = new List<string>();

            try
            {
                System.Reflection.Assembly entry = System.Reflection.Assembly.GetEntryAssembly()
                    ?? System.Reflection.Assembly.GetExecutingAssembly();
                string directory = Path.GetDirectoryName(entry.Location);
                if (!string.IsNullOrEmpty(directory))
                    starts.Add(directory);
            }
            catch (NotSupportedException)
            {
            }

            starts.Add(Path.GetFullPath("."));

            foreach (string start in starts)
            {
                string directory = start;
                for (int depth = 0; depth < 6 && directory != null; ++depth)
                {
                    if (File.Exists(Path.Combine(directory, "ProjectWAR.sln")))
                        return directory;

                    DirectoryInfo parent = Directory.GetParent(directory);
                    directory = parent == null ? null : parent.FullName;
                }
            }

            return null;
        }

        /// <summary>The database name inside a connection string, for reporting.</summary>
        public static string DescribeTarget(string connectionString)
        {
            foreach (string part in connectionString.Split(';'))
            {
                string[] pair = part.Split(new[] { '=' }, 2);
                if (pair.Length == 2 && pair[0].Trim().Equals("Database", StringComparison.OrdinalIgnoreCase))
                    return pair[1].Trim();
            }

            return "(unknown)";
        }
    }
}
