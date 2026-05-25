using NewEditor.Data;
using NewEditor.Data.NARCTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NewEditor.Forms
{
    public partial class NewScriptEditor : Form
    {
        public List<object> sequenceClipboard;

        public NewScriptEditor()
        {
            InitializeComponent();

            for (int i = 0; i < MainEditor.scriptNarc.scriptFiles.Count; i++) scriptFileDropdown.Items.Add(i);

            if (MainEditor.RomType == RomType.BW1)
            {
                commandNameSelection2.Checked = true;
                commandNameSelection1.Enabled = false;
            }

            List<byte> names = FileFunctions.ReadFileSection("Preferences.txt", "CommandNames");
            if (names != null)
            {
                if (Encoding.ASCII.GetString(names.ToArray()) == "Beaterscript")
                {
                    commandNameSelection2.Checked = true;
                }
            }

            loading = false;
        }

        private void LoadScriptFile(object sender, EventArgs e)
        {
            if (scriptFileDropdown.SelectedIndex >= 0 && scriptFileDropdown.SelectedIndex < scriptFileDropdown.Items.Count)
            {
                ScriptFile sf = MainEditor.scriptNarc.scriptFiles[scriptFileDropdown.SelectedIndex];

                if (sf.valid && sf.sequences != null && sf.sequences.Count > 0)
                {
                    rawDataTextBox.Enabled = true;
                    applyRawDataButton.Enabled = true;
                    readScriptFileButton.Enabled = true;
                    exportScriptFileButton.Enabled = true;
                    importRawDataButton.Enabled = true;
                    exportRawDataButton.Enabled = true;
                    quickBuildButton.Enabled = quickBuildRom != "";
                }
                else
                {
                    rawDataTextBox.Enabled = true;
                    applyRawDataButton.Enabled = true;
                    readScriptFileButton.Enabled = false;
                    exportScriptFileButton.Enabled = false;
                    importRawDataButton.Enabled = true;
                    exportRawDataButton.Enabled = true;
                    quickBuildButton.Enabled = false;
                }
                string text = "";
                foreach (byte b in sf.bytes) text += b.ToString("X2") + " ";
                rawDataTextBox.Text = text;
            }
        }

        private void ApplyRawData(object sender, EventArgs e)
        {
            if (scriptFileDropdown.SelectedIndex >= 0 && scriptFileDropdown.SelectedIndex < scriptFileDropdown.Items.Count)
            {
                rawDataTextBox.Text = rawDataTextBox.Text.Replace("\n", " ");

                ScriptFile sf = MainEditor.scriptNarc.scriptFiles[scriptFileDropdown.SelectedIndex];

                //Test for improper text length
                if (rawDataTextBox.Text.Length % 3 == 2 && rawDataTextBox.Text[rawDataTextBox.Text.Length - 1] != ' ') rawDataTextBox.Text += ' ';
                if (rawDataTextBox.Text.Length < 3 || rawDataTextBox.Text.Length % 3 != 0)
                {
                    MessageBox.Show("Raw Data detected an incorrect format");
                    return;
                }

                //Test for improper text values
                for (int i = 2; i < rawDataTextBox.Text.Length; i += 3) if (rawDataTextBox.Text[i] != ' ' ||
                        (!char.IsDigit(rawDataTextBox.Text[i - 1]) && !(rawDataTextBox.Text[i - 1] >= 'A' && rawDataTextBox.Text[i - 1] <= 'F')) ||
                        (!char.IsDigit(rawDataTextBox.Text[i - 2]) && !(rawDataTextBox.Text[i - 2] >= 'A' && rawDataTextBox.Text[i - 2] <= 'F')))
                    {
                        MessageBox.Show("Raw Data detected an incorrect format");
                        return;
                    }

                //Convert data to file
                sf.bytes = new RefByte[rawDataTextBox.Text.Length / 3];
                for (int i = 0; i < rawDataTextBox.Text.Length; i += 3)
                {
                    sf.bytes[i / 3] = byte.Parse(rawDataTextBox.Text.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
                }
                sf.ReadData();

                statusText.Text = "Saved script file from binary data - " + DateTime.Now.StatusText();
            }
        }

        bool updateByte = true;
        private void byteNumberBox_ValueChanged(object sender, EventArgs e)
        {
            if (updateByte)
            {
                rawDataTextBox.SelectionStart = (int)byteNumberBox.Value * 3;
                rawDataTextBox.ScrollToCaret();
            }
        }

        private void rawDataTextBox_Click(object sender, EventArgs e)
        {
            updateByte = false;
            byteNumberBox.Value = rawDataTextBox.SelectionStart / 3;
            updateByte = true;
        }

        private void ConfigureCommandList()
        {
            CommandReference.commandList = new Dictionary<int, Data.NARCTypes.CommandType>(commandNameSelection3.Checked && CommandReference.customCommandList.Count > 0 ? CommandReference.customCommandList : MainEditor.RomType == RomType.BW1 ? CommandReference.bw1CommandList :
                commandNameSelection2.Checked ? CommandReference.bw2BeaterScriptCommandList : CommandReference.bw2CommandList);
        }

        private void ApplyManualOverlay()
        {
            if (loadedOverlayDropdown.SelectedIndex > 0 && int.TryParse((string)loadedOverlayDropdown.SelectedItem, out int ov))
            {
                foreach (var cmd in CommandReference.bw2OverlayCommands[ov])
                    CommandReference.commandList.Add(cmd.Key, cmd.Value);
            }
        }

        private int? GetZoneOverlayForScriptIndex(int index)
        {
            if (MainEditor.zoneDataNarc?.zones == null || MainEditor.RomType != RomType.BW2) return null;
            int ow = MainEditor.zoneDataNarc.zones.FindIndex(z => z.scriptFile == index);
            if (ow >= 0 && OverworldEditor.overlayZones.ContainsKey(ow))
                return OverworldEditor.overlayZones[ow];
            return null;
        }

        private void ConfigureCommandListForScriptIndex(int index)
        {
            ConfigureCommandList();
            int? overlay = GetZoneOverlayForScriptIndex(index);
            if (overlay.HasValue)
            {
                foreach (var cmd in CommandReference.bw2OverlayCommands[overlay.Value])
                    CommandReference.commandList.Add(cmd.Key, cmd.Value);
            }
        }

        private List<string> BuildExportHeaders(int? overlayId)
        {
            List<string> headers = new List<string>()
            {
                MainEditor.RomType == RomType.BW1 ? "ScriptHeaders/ScriptCommandsBW1.h" :
                commandNameSelection2.Checked ? "ScriptHeaders/BeaterScriptCommandsBW2.h" : "ScriptHeaders/FrostScriptCommandsBW2.h",
                "ScriptHeaders/MovementCommands.h"
            };
            if (overlayId.HasValue)
                headers.Add("ScriptHeaders/CommandOverlay" + overlayId.Value + ".h");
            return headers;
        }

        private static bool TryParseScriptFileIndex(string filePath, out int index)
        {
            index = -1;
            string name = Path.GetFileNameWithoutExtension(filePath);
            if (string.IsNullOrEmpty(name)) return false;

            if (int.TryParse(name, out index) && index >= 0) return true;

            int separator = name.IndexOf('_');
            if (separator > 0 && int.TryParse(name.Substring(0, separator), out index) && index >= 0)
                return true;

            return false;
        }

        private static string SanitizeFileNamePart(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "";

            StringBuilder sanitized = new StringBuilder(value.Length);
            foreach (char c in value.Trim())
            {
                if (Array.IndexOf(Path.GetInvalidFileNameChars(), c) >= 0 || c == '.')
                    sanitized.Append('_');
                else
                    sanitized.Append(c);
            }

            string result = sanitized.ToString();
            while (result.Contains("__"))
                result = result.Replace("__", "_");
            return result.Trim('_');
        }

        private string GetZoneDisplayName(ZoneDataEntry zone)
        {
            if (MainEditor.textNarc?.textFiles == null) return null;
            int nameFileId = VersionConstants.ZoneNameTextFileID;
            if (nameFileId < 0 || nameFileId >= MainEditor.textNarc.textFiles.Count) return null;

            var names = MainEditor.textNarc.textFiles[nameFileId].text;
            if (zone.nameId < 0 || zone.nameId >= names.Count) return null;
            return names[zone.nameId];
        }

        private string GetScriptExportDescriptor(int scriptIndex)
        {
            if (MainEditor.zoneDataNarc?.zones == null) return null;

            List<string> zoneNames = new List<string>();
            for (int zoneIndex = 0; zoneIndex < MainEditor.zoneDataNarc.zones.Count; zoneIndex++)
            {
                ZoneDataEntry zone = MainEditor.zoneDataNarc.zones[zoneIndex];
                if (zone.scriptFile != scriptIndex) continue;

                string zoneName = GetZoneDisplayName(zone);
                if (string.IsNullOrWhiteSpace(zoneName)) continue;

                zoneName = SanitizeFileNamePart(zoneName);
                if (zoneName.Length == 0 || zoneNames.Contains(zoneName)) continue;
                zoneNames.Add(zoneName);
            }

            if (zoneNames.Count == 0) return null;
            zoneNames.Sort(StringComparer.OrdinalIgnoreCase);

            if (zoneNames.Count == 1) return zoneNames[0];
            if (zoneNames.Count == 2) return zoneNames[0] + "+" + zoneNames[1];
            return zoneNames[0] + "+" + zoneNames[1] + "+" + (zoneNames.Count - 2) + "more";
        }

        private string GetScriptExportFileName(int index, string extension)
        {
            string descriptor = GetScriptExportDescriptor(index);
            string baseName = index.ToString();
            if (!string.IsNullOrEmpty(descriptor))
                baseName += "_" + descriptor;

            const int maxBaseLength = 200;
            if (baseName.Length > maxBaseLength)
                baseName = baseName.Substring(0, maxBaseLength).TrimEnd('_');

            return baseName + extension;
        }

        private void CopyScriptHeadersToFolder(string folder)
        {
            string source = Directory.GetCurrentDirectory() + "\\ScriptHeaders";
            if (!Directory.Exists(source)) return;
            string dest = folder + "\\ScriptHeaders";
            if (!Directory.Exists(dest)) Directory.CreateDirectory(dest);
            foreach (string file in Directory.GetFiles(source))
                File.Copy(file, dest + "\\" + Path.GetFileName(file), true);
        }

        private void ReadScriptFile(object sender, EventArgs e)
        {
            ConfigureCommandList();
            ApplyManualOverlay();

            OpenFileDialog prompt = new OpenFileDialog();
            prompt.Filter = "c file|*.c";

            if (prompt.ShowDialog() == DialogResult.OK)
            {
                StreamReader reader = null;
                try
                {
                    reader = File.OpenText(prompt.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to open file");
                    return;
                }

                if (reader != null)
                {
                    ScriptFile newFile = null;
                    try
                    {
                        newFile = ScriptFile.FromFile(reader);
                    }
                    catch (Exception ex)
                    {
                        reader.Close();
                        MessageBox.Show(ex.Message);
                        return;
                    }

                    reader.Close();
                    MainEditor.scriptNarc.scriptFiles[scriptFileDropdown.SelectedIndex] = newFile;
                    LoadScriptFile(sender, e);
                }

                statusText.Text = "Imported script from script file - " + DateTime.Now.StatusText();
            }
        }

        private void ExportScriptFile(object sender, EventArgs e)
        {
            ConfigureCommandList();
            ApplyManualOverlay();

            MainEditor.scriptNarc.scriptFiles[scriptFileDropdown.SelectedIndex].ReadData();

            SaveFileDialog prompt = new SaveFileDialog();
            prompt.Filter = "c file|*.c";
            prompt.FileName = GetScriptExportFileName(scriptFileDropdown.SelectedIndex, ".c");

            if (prompt.ShowDialog() == DialogResult.OK)
            {
                int? manualOverlay = null;
                if (loadedOverlayDropdown.SelectedIndex > 0 && int.TryParse((string)loadedOverlayDropdown.SelectedItem, out int ov2))
                    manualOverlay = ov2;
                string[] headers = BuildExportHeaders(manualOverlay).ToArray();

                FileStream writer = null;
                try
                {
                    writer = File.OpenWrite(prompt.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to open file");
                    return;
                }

                if (writer != null)
                {
                    writer.SetLength(0);
                    try
                    {
                        MainEditor.scriptNarc.scriptFiles[scriptFileDropdown.SelectedIndex].Export(writer, headers);
                    }
                    catch
                    {
                        MessageBox.Show("An error has occured while exporting the file.\nThis may be caused by the required overlay commands not being loaded.");
                        writer.Close();
                        return;
                    }
                    writer.Close();
                }

                string root = Path.GetDirectoryName(prompt.FileName);
                CopyScriptHeadersToFolder(root);

                statusText.Text = "Exported script file to " + prompt.FileName + " - " + DateTime.Now.StatusText();
                var result = MessageBox.Show("Script file saved to " + prompt.FileName + "\n\nWould you like to open the file in a text editor?", "Script Saved", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    ProcessStartInfo start = new ProcessStartInfo("explorer", prompt.FileName);
                    Process.Start(start);
                }
            }
        }

        string quickBuildScript = "";
        string quickBuildRom = "";
        int quickBuildID = 0;

        private void setupQuickBuildButton_Click(object sender, EventArgs e)
        {
            if (scriptFileDropdown.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a script file before setting up a quick build");
                return;
            }

            OpenFileDialog script = new OpenFileDialog();
            script.Title = "Select a script file to import";
            script.Filter = "C file|*.c";
            if (script.ShowDialog() != DialogResult.OK) return;

            SaveFileDialog rom = new SaveFileDialog();
            rom.Title = "Select where to save your rom";
            rom.Filter = "Nds file|*.nds";
            if (rom.ShowDialog() != DialogResult.OK) return;

            quickBuildScript = script.FileName;
            quickBuildRom = rom.FileName;
            quickBuildID = scriptFileDropdown.SelectedIndex;

            quickBuildLabel.Text = "Import " + quickBuildScript.Substring(quickBuildScript.LastIndexOf("\\") + 1) + " as file " + quickBuildID + ",\nSave rom to " + quickBuildRom.Substring(quickBuildRom.LastIndexOf("\\") + 1);
            quickBuildButton.Enabled = exportScriptFileButton.Enabled;
        }

        private void quickBuildButton_Click(object sender, EventArgs e)
        {
            ConfigureCommandList();
            ApplyManualOverlay();

            StreamReader reader = null;
            try
            {
                reader = File.OpenText(quickBuildScript);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open file");
                return;
            }

            if (reader != null)
            {
                ScriptFile newFile = null;
                try
                {
                    newFile = ScriptFile.FromFile(reader);
                }
                catch (Exception ex)
                {
                    reader.Close();
                    MessageBox.Show(ex.Message);
                    return;
                }

                reader.Close();
                MainEditor.scriptNarc.scriptFiles[quickBuildID] = newFile;
                LoadScriptFile(sender, e);
            }

            FileStream fileStream = null;
            try
            {
                fileStream = File.OpenWrite(quickBuildRom);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to save rom");
                return;
            }
            fileStream.SetLength(0);
            byte[] data = MainEditor.fileSystem.BuildRom();
            fileStream.Write(data, 0, data.Length);
            fileStream.Close();
            MessageBox.Show("Rom saved to " + quickBuildRom);

            statusText.Text = "Applied quick build - " + DateTime.Now.StatusText();
        }

        bool loading = true;

        private void commandNameSelection1_CheckedChanged(object sender, EventArgs e)
        {
            if (!commandNameSelection1.Checked) return;

            if (!loading)
                FileFunctions.WriteFileSection("Preferences.txt", "CommandNames", ASCIIEncoding.ASCII.GetBytes("Frosts"));
        }

        private void commandNameSelection2_CheckedChanged(object sender, EventArgs e)
        {
            if (!commandNameSelection2.Checked) return;

            if (!loading)
                FileFunctions.WriteFileSection("Preferences.txt", "CommandNames", ASCIIEncoding.ASCII.GetBytes("Beaterscript"));
        }

        private void commandNameSelection3_CheckedChanged(object sender, EventArgs e)
        {
            if (!commandNameSelection3.Checked) return;

            OpenFileDialog open = new OpenFileDialog();
            open.FileName = "ScriptCommands.json";

            if (open.ShowDialog() == DialogResult.OK)
            {
                Dictionary<int, Data.NARCTypes.CommandType> comDefs = new Dictionary<int, Data.NARCTypes.CommandType>();

                JsonDocument json = JsonDocument.Parse(File.ReadAllText(open.FileName));
                foreach (JsonProperty com in json.RootElement.EnumerateObject())
                {
                    if (int.TryParse(com.Name.Substring(com.Name.IndexOf("x") + 1), System.Globalization.NumberStyles.HexNumber, null, out int id) &&
                        com.Value.TryGetProperty("name", out JsonElement name) && com.Value.TryGetProperty("parameters", out JsonElement pars))
                    {
                        int[] paramInts = pars.EnumerateArray().Count() == 0 ? new int[0] : JsonSerializer.Deserialize<int[]>(pars);
                        comDefs.Add(id, new Data.NARCTypes.CommandType(name.GetString(), paramInts.Length, paramInts));
                    }
                    else
                    {
                        commandNameSelection3.Checked = false;
                        commandNameSelection1.Checked = true;
                        MessageBox.Show("Error reading definition for command: \"" + com.Name + "\"");
                        return;
                    }
                }

                CommandReference.customCommandList = comDefs;

                if (!loading)
                    FileFunctions.WriteFileSection("Preferences.txt", "CommandNames", ASCIIEncoding.ASCII.GetBytes("Frosts"));
            }
            else
            {
                commandNameSelection3.Checked = false;
                commandNameSelection1.Checked = true;
            }
        }

        private void importRawDataButton_Click(object sender, EventArgs e)
        {
            if (scriptFileDropdown.SelectedIndex == -1) return;

            ScriptFile sf = MainEditor.scriptNarc.scriptFiles[scriptFileDropdown.SelectedIndex];
            OpenFileDialog open = new OpenFileDialog();
            open.FileName = GetScriptExportFileName(scriptFileDropdown.SelectedIndex, ".bin");

            if (open.ShowDialog() == DialogResult.OK)
            {
                byte[] b = File.ReadAllBytes(open.FileName);
                sf.bytes = new RefByte[b.Length];
                for (int i = 0; i < b.Length; i++) sf.bytes[i] = b[i];

                sf.ReadData();

                statusText.Text = "Imported script from binary file - " + DateTime.Now.StatusText();
                LoadScriptFile(null, null);
            }
        }

        private void exportRawDataButton_Click(object sender, EventArgs e)
        {
            if (scriptFileDropdown.SelectedIndex == -1) return;

            ScriptFile sf = MainEditor.scriptNarc.scriptFiles[scriptFileDropdown.SelectedIndex];
            SaveFileDialog save = new SaveFileDialog();
            save.FileName = GetScriptExportFileName(scriptFileDropdown.SelectedIndex, ".bin");

            if (save.ShowDialog() == DialogResult.OK)
            {
                byte[] b = new byte[sf.bytes.Length];
                for (int i = 0; i < b.Length; i++) b[i] = sf.bytes[i];
                File.WriteAllBytes(save.FileName, b);

                statusText.Text = "Exported binary file to " + save.FileName + " - " + DateTime.Now.StatusText();
            }
        }

        private void exportAllScriptsButton_Click(object sender, EventArgs e)
        {
            if (MainEditor.scriptNarc?.scriptFiles == null || MainEditor.scriptNarc.scriptFiles.Count == 0)
            {
                MessageBox.Show("No script files are available to export.");
                return;
            }

            FolderBrowserDialog prompt = new FolderBrowserDialog();
            if (prompt.ShowDialog() != DialogResult.OK) return;

            string folder = prompt.SelectedPath;
            int exported = 0, skipped = 0, failed = 0;

            for (int i = 0; i < MainEditor.scriptNarc.scriptFiles.Count; i++)
            {
                ScriptFile sf = MainEditor.scriptNarc.scriptFiles[i];
                if (!sf.valid)
                {
                    skipped++;
                    continue;
                }

                ConfigureCommandListForScriptIndex(i);
                sf.ReadData();
                string path = Path.Combine(folder, GetScriptExportFileName(i, ".c"));

                try
                {
                    using (FileStream writer = File.OpenWrite(path))
                    {
                        writer.SetLength(0);
                        sf.Export(writer, BuildExportHeaders(GetZoneOverlayForScriptIndex(i)).ToArray());
                    }
                    exported++;
                }
                catch
                {
                    failed++;
                }
            }

            CopyScriptHeadersToFolder(folder);
            statusText.Text = "Exported all scripts: " + exported + " exported, " + skipped + " skipped, " + failed + " failed - " + DateTime.Now.StatusText();
        }

        private void importAllScriptsButton_Click(object sender, EventArgs e)
        {
            if (MainEditor.scriptNarc?.scriptFiles == null || MainEditor.scriptNarc.scriptFiles.Count == 0)
            {
                MessageBox.Show("No script files are available to import.");
                return;
            }

            FolderBrowserDialog prompt = new FolderBrowserDialog();
            if (prompt.ShowDialog() != DialogResult.OK) return;

            List<string> fileNames = Directory.GetFiles(prompt.SelectedPath, "*.c").ToList();
            foreach (string path in fileNames)
            {
                if (!TryParseScriptFileIndex(path, out _))
                {
                    MessageBox.Show("Unable to process files in the provided folder.\nScript files must start with a numeric index (e.g. 42.c or 42_Route1.c).");
                    return;
                }
            }

            int applied = 0, failed = 0, skipped = 0;
            List<string> failures = new List<string>();

            foreach (string path in fileNames)
            {
                if (!TryParseScriptFileIndex(path, out int index))
                    continue;

                if (index >= MainEditor.scriptNarc.scriptFiles.Count)
                {
                    skipped++;
                    continue;
                }

                ConfigureCommandListForScriptIndex(index);
                try
                {
                    using (StreamReader reader = File.OpenText(path))
                    {
                        MainEditor.scriptNarc.scriptFiles[index] = ScriptFile.FromFile(reader);
                    }
                    applied++;
                }
                catch (Exception ex)
                {
                    failed++;
                    if (failures.Count < 8)
                        failures.Add("Script " + index + ": " + ex.Message);
                }
            }

            LoadScriptFile(null, null);

            StringBuilder report = new StringBuilder();
            report.AppendLine("Imported script files.");
            report.AppendLine("Applied: " + applied);
            report.AppendLine("Failed: " + failed);
            report.AppendLine("Skipped (out of range): " + skipped);
            if (failures.Count > 0)
            {
                report.AppendLine();
                report.AppendLine("First failures:");
                foreach (string f in failures)
                    report.AppendLine("- " + f);
            }
            MessageBox.Show(report.ToString());
            statusText.Text = "Imported all scripts: " + applied + " applied, " + failed + " failed - " + DateTime.Now.StatusText();
        }

        private void exportAllRawDataButton_Click(object sender, EventArgs e)
        {
            if (MainEditor.scriptNarc?.scriptFiles == null || MainEditor.scriptNarc.scriptFiles.Count == 0)
            {
                MessageBox.Show("No script files are available to export.");
                return;
            }

            FolderBrowserDialog prompt = new FolderBrowserDialog();
            if (prompt.ShowDialog() != DialogResult.OK) return;

            string folder = prompt.SelectedPath;
            for (int i = 0; i < MainEditor.scriptNarc.scriptFiles.Count; i++)
            {
                ScriptFile sf = MainEditor.scriptNarc.scriptFiles[i];
                byte[] b = new byte[sf.bytes.Length];
                for (int j = 0; j < b.Length; j++) b[j] = sf.bytes[j];
                File.WriteAllBytes(Path.Combine(folder, GetScriptExportFileName(i, ".bin")), b);
            }

            statusText.Text = "Exported all raw script data (" + MainEditor.scriptNarc.scriptFiles.Count + " files) - " + DateTime.Now.StatusText();
        }

        private void importAllRawDataButton_Click(object sender, EventArgs e)
        {
            if (MainEditor.scriptNarc?.scriptFiles == null || MainEditor.scriptNarc.scriptFiles.Count == 0)
            {
                MessageBox.Show("No script files are available to import.");
                return;
            }

            FolderBrowserDialog prompt = new FolderBrowserDialog();
            if (prompt.ShowDialog() != DialogResult.OK) return;

            List<string> fileNames = Directory.GetFiles(prompt.SelectedPath, "*.bin").ToList();
            foreach (string path in fileNames)
            {
                if (!TryParseScriptFileIndex(path, out _))
                {
                    MessageBox.Show("Unable to process files in the provided folder.\nRaw files must start with a numeric index (e.g. 42.bin or 42_Route1.bin).");
                    return;
                }
            }

            int applied = 0, skipped = 0;

            foreach (string path in fileNames)
            {
                if (!TryParseScriptFileIndex(path, out int index))
                    continue;

                if (index >= MainEditor.scriptNarc.scriptFiles.Count)
                {
                    skipped++;
                    continue;
                }

                byte[] b = File.ReadAllBytes(path);
                ScriptFile sf = MainEditor.scriptNarc.scriptFiles[index];
                sf.bytes = new RefByte[b.Length];
                for (int j = 0; j < b.Length; j++) sf.bytes[j] = b[j];
                sf.ReadData();
                applied++;
            }

            LoadScriptFile(null, null);

            StringBuilder report = new StringBuilder();
            report.AppendLine("Imported raw script data.");
            report.AppendLine("Applied: " + applied);
            report.AppendLine("Skipped (out of range): " + skipped);
            MessageBox.Show(report.ToString());
            statusText.Text = "Imported all raw script data: " + applied + " applied - " + DateTime.Now.StatusText();
        }

        private void genCommandDatabaseButton_Click(object sender, EventArgs e)
        {
            ConfigureCommandList();

            JsonObject json = new JsonObject();
            foreach (var com in CommandReference.commandList)
            {
                JsonArray array = new JsonArray();
                foreach (int p in com.Value.parameterBytes) array.Add(p);

                JsonObject entry = new JsonObject
                {
                    { "name", com.Value.name },
                    { "parameters", array },
                };
                json.Add("0x" + com.Key.ToString("X"), entry);
            }

            SaveFileDialog save = new SaveFileDialog();
            save.FileName = "ScriptCommands.json";

            if (save.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(save.FileName, json.ToString());
                statusText.Text = "Exported command database to " + save.FileName + " - " + DateTime.Now.StatusText();
            }
        }
    }
}
