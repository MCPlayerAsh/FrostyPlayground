using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>UPR-FVX <c>customnames.rncn</c> v1: version byte + 5 big-endian length-prefixed UTF-8 blocks.</summary>
    public sealed class FvxCustomNamesSet
    {
        public const byte FileVersion = 1;
        public const string DefaultFileName = "customnames.rncn";

        public List<string> TrainerNames { get; } = new List<string>();
        public List<string> TrainerClasses { get; } = new List<string>();
        public List<string> DoublesTrainerNames { get; } = new List<string>();
        public List<string> DoublesTrainerClasses { get; } = new List<string>();
        public List<string> PokemonNicknames { get; } = new List<string>();

        public static string DefaultFilePath()
            => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultFileName);

        public static FvxCustomNamesSet ReadOrCreate(string path)
        {
            if (!File.Exists(path))
            {
                var seeded = new FvxCustomNamesSet();
                FvxCustomNamesDefaults.ApplyTo(seeded);
                try { seeded.Save(path); } catch { /* still return in-memory defaults */ }
                return seeded;
            }
            using (var fs = File.OpenRead(path))
                return Read(fs);
        }

        public static FvxCustomNamesSet Read(Stream stream)
        {
            var set = new FvxCustomNamesSet();
            int ver = stream.ReadByte();
            if (ver != FileVersion)
                throw new InvalidDataException("Invalid custom names file (expected version " + FileVersion + ").");

            ReadBlock(stream, set.TrainerNames);
            ReadBlock(stream, set.TrainerClasses);
            ReadBlock(stream, set.DoublesTrainerNames);
            ReadBlock(stream, set.DoublesTrainerClasses);
            ReadBlock(stream, set.PokemonNicknames);
            return set;
        }

        static void ReadBlock(Stream stream, List<string> target)
        {
            var lenBuf = new byte[4];
            if (stream.Read(lenBuf, 0, 4) != 4)
                throw new EndOfStreamException();
            int size = (lenBuf[0] << 24) | (lenBuf[1] << 16) | (lenBuf[2] << 8) | lenBuf[3];
            if (size < 0 || size > 10_000_000)
                throw new InvalidDataException("Invalid custom names block size.");
            var body = new byte[size];
            if (size > 0 && stream.Read(body, 0, size) != size)
                throw new EndOfStreamException();
            var text = Encoding.UTF8.GetString(body);
            foreach (var line in text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
            {
                var t = line.Trim();
                if (t.Length > 0) target.Add(t);
            }
        }

        public void Save(string path)
        {
            using (var fs = File.Create(path))
                Write(fs);
        }

        public void Write(Stream stream)
        {
            stream.WriteByte(FileVersion);
            WriteBlock(stream, TrainerNames);
            WriteBlock(stream, TrainerClasses);
            WriteBlock(stream, DoublesTrainerNames);
            WriteBlock(stream, DoublesTrainerClasses);
            WriteBlock(stream, PokemonNicknames);
        }

        static void WriteBlock(Stream stream, IReadOnlyList<string> lines)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < lines.Count; i++)
            {
                if (i > 0) sb.AppendLine();
                sb.Append(lines[i] ?? "");
            }
            var body = Encoding.UTF8.GetBytes(sb.ToString());
            int size = body.Length;
            stream.WriteByte((byte)(size >> 24));
            stream.WriteByte((byte)(size >> 16));
            stream.WriteByte((byte)(size >> 8));
            stream.WriteByte((byte)size);
            if (size > 0)
                stream.Write(body, 0, size);
        }
    }
}
