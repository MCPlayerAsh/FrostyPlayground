using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using NewEditor.Data.Randomization.FvxGen5;

namespace NewEditor.Forms
{
    public sealed class FvxCustomNamesEditorForm : Form
    {
        bool _dirty;
        readonly string _filePath;
        readonly TextBox _trainerNames = NewBox();
        readonly TextBox _trainerClasses = NewBox();
        readonly TextBox _doublesNames = NewBox();
        readonly TextBox _doublesClasses = NewBox();
        readonly TextBox _nicknames = NewBox();

        public FvxCustomNamesEditorForm()
        {
            _filePath = FvxCustomNamesSet.DefaultFilePath();
            Text = "Custom Names Editor";
            Size = new Size(720, 520);
            StartPosition = FormStartPosition.CenterParent;
            var tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add(NewTab("Trainer Names", _trainerNames));
            tabs.TabPages.Add(NewTab("Trainer Classes", _trainerClasses));
            tabs.TabPages.Add(NewTab("Doubles Trainer Names", _doublesNames));
            tabs.TabPages.Add(NewTab("Doubles Trainer Classes", _doublesClasses));
            tabs.TabPages.Add(NewTab("Pokémon Nicknames", _nicknames));

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 44,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(8)
            };
            var save = new Button { Text = "Save", AutoSize = true };
            var close = new Button { Text = "Close", AutoSize = true };
            save.Click += SaveClick;
            close.Click += (_, __) => Close();
            flow.Controls.Add(close);
            flow.Controls.Add(save);

            Controls.Add(tabs);
            Controls.Add(flow);

            LoadData();
            HookDirty(_trainerNames);
            HookDirty(_trainerClasses);
            HookDirty(_doublesNames);
            HookDirty(_doublesClasses);
            HookDirty(_nicknames);
        }

        static TextBox NewBox() =>
            new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                Dock = DockStyle.Fill,
                Font = new Font(FontFamily.GenericMonospace, 9f),
                AcceptsReturn = true,
                AcceptsTab = true
            };

        static TabPage NewTab(string title, Control content)
        {
            var p = new TabPage(title) { Padding = new Padding(6) };
            content.Dock = DockStyle.Fill;
            p.Controls.Add(content);
            return p;
        }

        void HookDirty(TextBox t)
        {
            t.TextChanged += (_, __) => _dirty = true;
        }

        void LoadData()
        {
            try
            {
                var set = FvxCustomNamesSet.ReadOrCreate(_filePath);
                _trainerNames.Text = JoinLines(set.TrainerNames);
                _trainerClasses.Text = JoinLines(set.TrainerClasses);
                _doublesNames.Text = JoinLines(set.DoublesTrainerNames);
                _doublesClasses.Text = JoinLines(set.DoublesTrainerClasses);
                _nicknames.Text = JoinLines(set.PokemonNicknames);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load custom names: " + ex.Message);
            }
            _dirty = false;
        }

        static string JoinLines(System.Collections.Generic.IReadOnlyList<string> lines)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < lines.Count; i++)
            {
                if (i > 0) sb.AppendLine();
                sb.Append(lines[i]);
            }
            return sb.ToString();
        }

        void SaveClick(object sender, EventArgs e)
        {
            try
            {
                var set = new FvxCustomNamesSet();
                AddLines(_trainerNames.Text, set.TrainerNames);
                AddLines(_trainerClasses.Text, set.TrainerClasses);
                AddLines(_doublesNames.Text, set.DoublesTrainerNames);
                AddLines(_doublesClasses.Text, set.DoublesTrainerClasses);
                AddLines(_nicknames.Text, set.PokemonNicknames);
                set.Save(_filePath);
                _dirty = false;
                MessageBox.Show("Saved to " + _filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save failed: " + ex.Message);
            }
        }

        static void AddLines(string text, System.Collections.Generic.List<string> target)
        {
            target.Clear();
            if (string.IsNullOrEmpty(text)) return;
            foreach (var line in text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
            {
                var t = line.Trim();
                if (t.Length > 0) target.Add(t);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_dirty)
            {
                var r = MessageBox.Show("Discard unsaved changes?", Text, MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (r == DialogResult.No)
                    e.Cancel = true;
            }
            base.OnFormClosing(e);
        }
    }
}
