using System;
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

            foreach (string configuration in new[] { "Release", "Debug" })
            {
                string path = Path.GetFullPath(Path.Combine("bin", configuration, "Configs", "World.xml"));
                if (!File.Exists(path))
                    continue;

                string built = TryBuild(path);
                if (built != null)
                    return built;
            }

            throw new FileNotFoundException(
                "Could not find bin/Release/Configs/World.xml to read the world database settings from. "
                + "Build the solution once, or pass --connection.");
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
