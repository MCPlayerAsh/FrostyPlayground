using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NewEditor.Data;
using NewEditor.Forms;

namespace NewEditor.Forms
{
    public sealed class FvxLimitPokemonForm : Form
    {
        readonly CheckedListBox _list;
        HashSet<int> _selected;

        public IReadOnlyCollection<int> SelectedSpeciesIndices => _selected?.ToList() ?? new List<int>();

        public FvxLimitPokemonForm(HashSet<int> initial)
        {
            _selected = initial != null ? new HashSet<int>(initial) : new HashSet<int>();
            Text = "Limit Pokémon — select allowed species";
            Size = new Size(420, 520);
            StartPosition = FormStartPosition.CenterParent;
            _list = new CheckedListBox { Dock = DockStyle.Fill, CheckOnClick = true };
            var names = MainEditor.textNarc?.textFiles?[VersionConstants.PokemonNameTextFileID]?.text;
            if (names != null && names.Count > 1)
            {
                for (int i = 1; i < names.Count; i++)
                {
                    string label = i + ": " + (names[i] ?? "?");
                    _list.Items.Add(label, _selected.Contains(i));
                }
            }

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                FlowDirection = FlowDirection.RightToLeft
            };
            var ok = new Button { Text = "OK", DialogResult = DialogResult.OK };
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };
            flow.Controls.Add(cancel);
            flow.Controls.Add(ok);
            AcceptButton = ok;
            CancelButton = cancel;

            Controls.Add(flow);
            if (_list.Items.Count == 0)
            {
                var hint = new Label
                {
                    Text = "Pokémon names are not loaded. Open a ROM (or reload the text NARC) and try again.",
                    Dock = DockStyle.Top,
                    AutoSize = false,
                    Height = 48,
                    Padding = new Padding(8, 8, 8, 4)
                };
                Controls.Add(hint);
            }
            Controls.Add(_list);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                _selected = new HashSet<int>();
                for (int i = 0; i < _list.Items.Count; i++)
                {
                    if (_list.GetItemChecked(i))
                        _selected.Add(i + 1);
                }
            }
            base.OnFormClosing(e);
        }
    }
}
