namespace Wipe
{
    internal static class Program
    {
        /// <summary>
        /// Whether to delete all root folders of all files wiped.
        /// </summary>
        private static bool DeleteFolders { get; set; }

        /// <summary>
        /// Files to wipe.
        /// </summary>
        private static List<FileInfo> Files { get; } = new();

        /// <summary>
        /// All folders.
        /// </summary>
        private static List<string> Folders { get; set; } = new();

        /// <summary>
        /// How many passes to do on each file.
        /// </summary>
        private static int Passes { get; set; } = 20;

        /// <summary>
        /// Delete all files gathered from all the files.
        /// </summary>
        private static void DeleteAllFolders()
        {
            if (!DeleteFolders ||
                Folders == null)
            {
                return;
            }

            Write(
                "Deleting ",
                ConsoleColor.Blue,
                Folders.Count,
                (byte)0x00,
                " folder",
                Folders.Count == 1 ? string.Empty : "s",
                Environment.NewLine);

            for (var i = 0; i < Folders.Count; i++)
            {
                DeleteFolder(i + 1, Folders[i]);
            }

            Write(Environment.NewLine);
        }

        /// <summary>
        /// Delete a single folder.
        /// </summary>
        /// <param name="index">Index in queue.</param>
        /// <param name="path">Full path.</param>
        private static void DeleteFolder(
            int index,
            string path)
        {
            var total = Folders.Count.ToString();
            var position =
                new string(' ', total.Length - index.ToString().Length) +
                index +
                "/" +
                total;

            var name = path;
            var maxNameLen =
                position.Length +
                1;

            maxNameLen = Console.WindowWidth - maxNameLen;

            if (name.Length > maxNameLen)
            {
                name = name.Substring(name.Length - maxNameLen);
            }

            Write(
                ConsoleColor.Green,
                position,
                " ",
                (byte)0x00,
                name);

            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            catch (Exception ex)
            {
                Write(
                    Environment.NewLine,
                    ConsoleColor.Red,
                    "Error: ",
                    (byte)0x00,
                    ex.Message);
            }

            Write(Environment.NewLine);
        }

        /// <summary>
        /// Get the application name.
        /// </summary>
        /// <returns>Name.</returns>
        private static string GetAppName()
        {
            // TODO: Get actual name from program via reflection.
            return "Wipe";
        }

        /// <summary>
        /// Get the application version.
        /// </summary>
        /// <returns>Version.</returns>
        private static string GetAppVersion()
        {
            // TODO: Get actual version from program via reflection.
            return "0.1-alpha";
        }

        /// <summary>
        /// Create a new exception with console objects to write.
        /// </summary>
        /// <param name="objects">Console objects.</param>
        /// <returns>Exception.</returns>
        private static Exception GetException(params object[] objects)
        {
            var message = string.Empty;

            foreach (var obj in objects)
            {
                if (obj is ConsoleColor cc)
                {
                    //
                }
                else if (obj is byte b &&
                         b == 0x00)
                {
                    //
                }
                else
                {
                    message += obj;
                }
            }

            var ex = new Exception(message);

            ex.Data.Add(
                "ConsoleObjects",
                objects);

            return ex;
        }

        /// <summary>
        /// Format bytes into a more human readable string.
        /// </summary>
        /// <param name="bytes">Total bytes.</param>
        /// <returns>Formatted string.</returns>
        private static string GetReadableFileSize(long bytes)
        {
            var sizes = new[]
            {
                "B",
                "KB",
                "MB",
                "GB",
                "TB"
            };

            var order = 0;

            while (bytes >= 1024 &&
                   order < sizes.Length - 1)
            {
                order++;
                bytes /= 1024;
            }

            return $"{bytes:0.##} {sizes[order]}";
        }

        /// <summary>
        /// Check if input is folder or a folder with pattern.
        /// </summary>
        /// <param name="input">Input.</param>
        /// <param name="path">Output path, if found.</param>
        /// <param name="pattern">Output pattern, if found.</param>
        /// <returns>Success.</returns>
        private static bool ParseFolderPattern(
            string input,
            out string path,
            out string pattern)
        {
            path = null!;
            pattern = null!;

            if (Directory.Exists(input))
            {
                path = input;
                pattern = "*.*";

                return true;
            }

            var separators = new[]
            {
                "\\",
                "/"
            };

            foreach (var separator in separators)
            {
                var index = input.LastIndexOf(separator);

                if (index == -1)
                {
                    continue;
                }

                var pth = input.Substring(0, index);
                var ptn = input.Substring(index + 1);

                if (!Directory.Exists(pth) ||
                    string.IsNullOrWhiteSpace(ptn))
                {
                    continue;
                }

                path = pth;
                pattern = ptn;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Init all the things..
        /// </summary>
        /// <param name="args">Program arguments.</param>
        private static void Main(string[] args)
        {
            Write(
                GetAppName(),
                " v",
                GetAppVersion(),
                Environment.NewLine,
                Environment.NewLine);

            // Parse the program arguments into actionable data.
            try
            {
                Console.CursorVisible = false;

                if (!ParseProgramArguments(args))
                {
                    ShowProgramUsage();
                    return;
                }

                Write(
                    "Passes: ",
                    ConsoleColor.Blue,
                    Passes,
                    Environment.NewLine,
                    (byte)0x00,
                    "Delete Folders: ",
                    ConsoleColor.Blue,
                    DeleteFolders ? "True" : "False",
                    Environment.NewLine,
                    Environment.NewLine);
            }
            catch (Exception ex)
            {
                if (ex.Data.Contains("ConsoleObjects") &&
                    ex.Data["ConsoleObjects"] is object[] objects &&
                    objects.Length > 0)
                {
                    Write(objects);
                    return;
                }

                Write(
                    ConsoleColor.Red,
                    "Error: ",
                    (byte)0x00,
                    ex.Message,
                    Environment.NewLine);

                return;
            }

            // Started.
            var started = DateTimeOffset.Now;

            Write(
                "Started: ",
                ConsoleColor.Blue,
                started.ToString("yyyy-MM-dd HH:mm:ss"),
                Environment.NewLine,
                (byte)0x00,
                "Press ",
                ConsoleColor.Blue,
                "CTRL+C",
                (byte)0x00,
                " to abort!",
                Environment.NewLine,
                Environment.NewLine);

            // Wipe all files gathered.
            WipeFiles();

            // Delete all files gathered from all the files.
            DeleteAllFolders();

            // Finished
            var finished = DateTimeOffset.Now;
            var duration = finished - started;

            Write(
                "Finished: ",
                ConsoleColor.Blue,
                finished.ToString("yyyy-MM-dd HH:mm:ss"),
                Environment.NewLine,
                (byte)0x00,
                "Duration: ",
                ConsoleColor.Blue,
                duration,
                Environment.NewLine);
        }

        /// <summary>
        /// Parse the program arguments into actionable data.
        /// </summary>
        /// <param name="args">Program arguments.</param>
        /// <returns>Success.</returns>
        private static bool ParseProgramArguments(string[] args)
        {
            if (args.Length == 0 ||
                args.Any(n => n == "-h"))
            {
                return false;
            }

            var skip = false;

            for (var i = 0; i < args.Length; i++)
            {
                if (skip)
                {
                    skip = false;
                    continue;
                }

                // Is file?
                if (File.Exists(args[i]))
                {
                    var fi = new FileInfo(args[i]);

                    if (!Files.Contains(fi))
                    {
                        Files.Add(fi);
                        
                        if (fi.Directory != null)
                        {
                            Folders.Add(fi.Directory.FullName);
                        }
                    }

                    continue;
                }

                // Is folder?
                if (ParseFolderPattern(args[i], out var path, out var pattern))
                {
                    Folders.Add(path);

                    foreach (var file in Directory.GetFiles(path, pattern, SearchOption.AllDirectories))
                    {
                        var fi = new FileInfo(file);

                        if (!Files.Contains(fi))
                        {
                            Files.Add(fi);
                        }
                    }

                    continue;
                }

                // Is option?
                switch (args[i])
                {
                    // Delete the folders after wiping the files.
                    case "-d":
                        DeleteFolders = true;
                        continue;

                    // Set the number of passes to do on each file.
                    case "-p":
                        if (i == args.Length - 1 ||
                            !int.TryParse(args[i + 1], out var passes))
                        {
                            throw GetException(
                                ConsoleColor.Blue,
                                "-p ",
                                (byte)0x00,
                                "must be followed by a number.");
                        }

                        if (passes < 1 ||
                            passes > 9999)
                        {
                            throw GetException(
                                "Invalid value for passes: ",
                                ConsoleColor.Blue,
                                passes,
                                (byte)0x00,
                                ". Valid value must be a positive integer and less than 10000.");
                        }

                        Passes = passes;
                        skip = true;

                        continue;
                }

                // Invalid option.
                throw GetException(
                    "Argument not valid: ",
                    ConsoleColor.Blue,
                    args[i]);
            }

            // Remove duplicate folders.
            Folders = Folders
                .Distinct()
                .ToList();

            return true;
        }

        /// <summary>
        /// Show program usage and arguments.
        /// </summary>
        private static void ShowProgramUsage()
        {
            Write(
                GetAppName(),
                " writes random bytes to each files found acording to the number of passes specified, then creates and deletes files with the same filenames, also acording to the number of passes specified.",
                Environment.NewLine,
                Environment.NewLine);

            Write(
                "Usage:",
                Environment.NewLine,
                "  wipe ",
                ConsoleColor.Blue,
                "<file/folder/pattern> ",
                ConsoleColor.Green,
                "[<options>]",
                Environment.NewLine,
                Environment.NewLine);

            Write(
                "Examples:",
                Environment.NewLine,
                "  /some/folder",
                Environment.NewLine,
                "  /some/folder/some-file.txt",
                Environment.NewLine,
                "  /some/folder/*.txt",
                Environment.NewLine,
                "  *.mp3 ",
                ConsoleColor.DarkGray,
                "(without leading path working dir is assumed)",
                Environment.NewLine,
                Environment.NewLine);

            Write(
                "Options:",
                Environment.NewLine,
                ConsoleColor.Blue,
                "  -d           ",
                (byte)0x00,
                "Delete the folders after wiping the files.",
                Environment.NewLine,
                ConsoleColor.Blue,
                "  -p ",
                ConsoleColor.Green,
                "<number>  ",
                (byte)0x00,
                $"Set the number of passes to do on each file. Defaults to {Passes}. Must be between (and including) 0 and 9999.",
                Environment.NewLine,
                Environment.NewLine);
        }

        /// <summary>
        /// Wipe a single file with passes.
        /// </summary>
        /// <param name="index">Index in queue.</param>
        /// <param name="file">File.</param>
        private static void WipeFile(
            int index,
            FileInfo file)
        {
            var path = file.FullName;
            var name = file.FullName;
            var hrfs = GetReadableFileSize(file.Length);
            var bytes = new byte[file.Length];
            var rnd = new Random();

            var total = Files.Count.ToString();
            var position =
                new string(' ', total.Length - index.ToString().Length) +
                index +
                "/" +
                total;

            var size =
                new string(' ', 8 - hrfs.Length) +
                hrfs;

            var maxNameLen =
                position.Length +
                size.Length +
                7;

            maxNameLen = Console.WindowWidth - maxNameLen;

            if (name.Length > maxNameLen)
            {
                name = name.Substring(name.Length - maxNameLen);
            }

            Write(
                ConsoleColor.Green,
                position,
                " ",
                ConsoleColor.Blue,
                size,
                " ",
                ConsoleColor.Yellow,
                "0000 ",
                (byte)0x00,
                name);

            try
            {
                // Overwrite file with random bytes.
                for (var i = 0; i < Passes; i++)
                {
                    // Update pass number.
                    var pass = (i + 1).ToString();
                    
                    pass =
                        new string('0', 4 - pass.Length) +
                        pass;

                    Console.CursorLeft =
                        position.Length +
                        size.Length +
                        2;

                    Write(
                        ConsoleColor.Yellow,
                        pass);

                    // Write random bytes to the file.
                    rnd.NextBytes(bytes);

                    using var stream = File.OpenWrite(path);

                    stream.Write(bytes, 0, bytes.Length);
                    stream.Close();
                }

                // Delete the file.
                File.Delete(path);

                // Create n number of fake files with random bytes, then delete them.
                for (var i = 0; i < Passes; i++)
                {
                    // Update pass number.
                    var pass = (i + 1).ToString();

                    pass =
                        new string('0', 4 - pass.Length) +
                        pass;

                    Console.CursorLeft =
                        position.Length +
                        size.Length +
                        2;

                    Write(
                        ConsoleColor.Yellow,
                        pass);

                    // Write random bytes and delete the same file.
                    rnd.NextBytes(bytes);

                    File.WriteAllBytes(path, bytes);
                    File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                Write(
                    Environment.NewLine,
                    ConsoleColor.Red,
                    "Error: ",
                    (byte)0x00,
                    ex.Message);
            }

            Write(Environment.NewLine);
        }

        /// <summary>
        /// Wipe all files gathered.
        /// </summary>
        private static void WipeFiles()
        {
            Write(
                "Wiping ",
                ConsoleColor.Blue,
                Files.Count,
                (byte)0x00,
                " file",
                Files.Count == 1 ? string.Empty : "s",
                Environment.NewLine);

            for (var i = 0; i < Files.Count; i++)
            {
                WipeFile(i + 1, Files[i]);
            }

            Write(Environment.NewLine);
        }

        /// <summary>
        /// Write objects to console with color control.
        /// </summary>
        /// <param name="objects">Objects to write.</param>
        private static void Write(params object[] objects)
        {
            Console.ResetColor();

            foreach (var obj in objects)
            {
                if (obj is ConsoleColor cc)
                {
                    Console.ForegroundColor = cc;
                }
                else if (obj is byte b &&
                         b == 0x00)
                {
                    Console.ResetColor();
                }
                else
                {
                    Console.Write(obj);
                }
            }

            Console.ResetColor();
        }
    }
}