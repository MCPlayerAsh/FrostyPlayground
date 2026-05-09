using System;
using System.Drawing;
using System.Windows.Forms;
using NewEditor.Data.Randomization.FvxGen5;

namespace NewEditor.Forms
{
    public class TmHmTutorsControl : UserControl
    {
        readonly ComboBox _tmMovesMod = new ComboBox();
        readonly CheckBox _keepFieldTms = new CheckBox();
        readonly CheckBox _forceGoodTms = new CheckBox();
        readonly NumericUpDown _goodTmPct = new NumericUpDown();

        readonly ComboBox _tmCompatMod = new ComboBox();
        readonly CheckBox _tmLevelupSanity = new CheckBox();
        readonly CheckBox _tmFollowEvos = new CheckBox();
        readonly CheckBox _fullHmCompat = new CheckBox();

        readonly ComboBox _tutorMovesMod = new ComboBox();
        readonly CheckBox _keepFieldTutors = new CheckBox();
        readonly CheckBox _forceGoodTutors = new CheckBox();
        readonly NumericUpDown _goodTutorPct = new NumericUpDown();

        readonly ComboBox _tutorCompatMod = new ComboBox();
        readonly CheckBox _tutorLevelupSanity = new CheckBox();
        readonly CheckBox _tutorFollowEvos = new CheckBox();
        readonly GroupBox _tutorMovesGroup = new GroupBox();
        readonly GroupBox _tutorCompatGroup = new GroupBox();

        public TmHmTutorsControl()
        {
            BuildUi();
        }

        void BuildUi()
        {
            Dock = DockStyle.Fill;
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(6) };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

            var tmMovesGroup = new GroupBox { Text = "TM Moves", Dock = DockStyle.Fill, Padding = new Padding(8) };
            var tmMovesFlow = NewFlow();
            _tmMovesMod.DropDownStyle = ComboBoxStyle.DropDownList;
            _tmMovesMod.Width = 260;
            _tmMovesMod.Items.AddRange(new object[] { "Unchanged", "Random", "No Game-Breaking Moves" });
            _tmMovesMod.SelectedIndex = 0;
            _keepFieldTms.Text = "Keep Field Move TMs";
            _keepFieldTms.AutoSize = true;
            _forceGoodTms.Text = "Force % of Good Damaging Moves";
            _forceGoodTms.AutoSize = true;
            _goodTmPct.Minimum = 0;
            _goodTmPct.Maximum = 100;
            _goodTmPct.Value = 50;
            _goodTmPct.Width = 60;
            tmMovesFlow.Controls.Add(_tmMovesMod);
            tmMovesFlow.Controls.Add(_keepFieldTms);
            tmMovesFlow.Controls.Add(_forceGoodTms);
            tmMovesFlow.Controls.Add(_goodTmPct);
            tmMovesGroup.Controls.Add(tmMovesFlow);

            var tmCompatGroup = new GroupBox { Text = "TM/HM Compatibility", Dock = DockStyle.Fill, Padding = new Padding(8) };
            var tmCompatFlow = NewFlow();
            _tmCompatMod.DropDownStyle = ComboBoxStyle.DropDownList;
            _tmCompatMod.Width = 260;
            _tmCompatMod.Items.AddRange(new object[] { "Unchanged", "Random (prefer same type)", "Random (completely)", "Full Compatibility" });
            _tmCompatMod.SelectedIndex = 0;
            _tmLevelupSanity.Text = "TM/Levelup Move Sanity";
            _tmLevelupSanity.AutoSize = true;
            _tmFollowEvos.Text = "Follow Evolutions";
            _tmFollowEvos.AutoSize = true;
            _fullHmCompat.Text = "Full HM Compatibility";
            _fullHmCompat.AutoSize = true;
            tmCompatFlow.Controls.Add(_tmCompatMod);
            tmCompatFlow.Controls.Add(_tmLevelupSanity);
            tmCompatFlow.Controls.Add(_tmFollowEvos);
            tmCompatFlow.Controls.Add(_fullHmCompat);
            tmCompatGroup.Controls.Add(tmCompatFlow);

            _tutorMovesGroup.Text = "Move Tutor Moves";
            _tutorMovesGroup.Dock = DockStyle.Fill;
            _tutorMovesGroup.Padding = new Padding(8);
            var tutorMovesFlow = NewFlow();
            _tutorMovesMod.DropDownStyle = ComboBoxStyle.DropDownList;
            _tutorMovesMod.Width = 260;
            _tutorMovesMod.Items.AddRange(new object[] { "Unchanged", "Random", "No Game-Breaking Moves" });
            _tutorMovesMod.SelectedIndex = 0;
            _keepFieldTutors.Text = "Keep Field Move Tutors";
            _keepFieldTutors.AutoSize = true;
            _forceGoodTutors.Text = "Force % of Good Damaging Moves";
            _forceGoodTutors.AutoSize = true;
            _goodTutorPct.Minimum = 0;
            _goodTutorPct.Maximum = 100;
            _goodTutorPct.Value = 50;
            _goodTutorPct.Width = 60;
            tutorMovesFlow.Controls.Add(_tutorMovesMod);
            tutorMovesFlow.Controls.Add(_keepFieldTutors);
            tutorMovesFlow.Controls.Add(_forceGoodTutors);
            tutorMovesFlow.Controls.Add(_goodTutorPct);
            _tutorMovesGroup.Controls.Add(tutorMovesFlow);

            _tutorCompatGroup.Text = "Move Tutor Compatibility";
            _tutorCompatGroup.Dock = DockStyle.Fill;
            _tutorCompatGroup.Padding = new Padding(8);
            var tutorCompatFlow = NewFlow();
            _tutorCompatMod.DropDownStyle = ComboBoxStyle.DropDownList;
            _tutorCompatMod.Width = 260;
            _tutorCompatMod.Items.AddRange(new object[] { "Unchanged", "Random (prefer same type)", "Random (completely)", "Full Compatibility" });
            _tutorCompatMod.SelectedIndex = 0;
            _tutorLevelupSanity.Text = "Tutor/Levelup Move Sanity";
            _tutorLevelupSanity.AutoSize = true;
            _tutorFollowEvos.Text = "Follow Evolutions";
            _tutorFollowEvos.AutoSize = true;
            tutorCompatFlow.Controls.Add(_tutorCompatMod);
            tutorCompatFlow.Controls.Add(_tutorLevelupSanity);
            tutorCompatFlow.Controls.Add(_tutorFollowEvos);
            _tutorCompatGroup.Controls.Add(tutorCompatFlow);

            root.Controls.Add(tmMovesGroup, 0, 0);
            root.Controls.Add(tmCompatGroup, 1, 0);
            root.Controls.Add(_tutorMovesGroup, 0, 1);
            root.Controls.Add(_tutorCompatGroup, 1, 1);
            Controls.Add(root);
        }

        static FlowLayoutPanel NewFlow()
        {
            return new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true
            };
        }

        public void SetTutorEnabled(bool enabled, string messageIfDisabled = null)
        {
            _tutorMovesGroup.Enabled = enabled;
            _tutorCompatGroup.Enabled = enabled;
            if (!enabled && !string.IsNullOrEmpty(messageIfDisabled))
            {
                _tutorMovesGroup.Text = messageIfDisabled;
                _tutorCompatGroup.Text = messageIfDisabled;
            }
        }

        public bool AnyOptionsActive =>
            _tmMovesMod.SelectedIndex != 0 || _tmCompatMod.SelectedIndex != 0 ||
            (_tutorMovesGroup.Enabled && _tutorMovesMod.SelectedIndex != 0) ||
            (_tutorCompatGroup.Enabled && _tutorCompatMod.SelectedIndex != 0);

        public void ApplyToOptions(FvxRandomizerOptions opt)
        {
            opt.TmMovesMod = (FvxTmMoveMod)_tmMovesMod.SelectedIndex;
            opt.KeepFieldMoveTms = _keepFieldTms.Checked;
            opt.TmsForceGoodDamaging = _forceGoodTms.Checked;
            opt.TmsGoodDamagingPercent = (int)_goodTmPct.Value;

            switch (_tmCompatMod.SelectedIndex)
            {
                case 1: opt.TmHmCompatMod = FvxTmHmCompatMod.RandomPreferType; break;
                case 2: opt.TmHmCompatMod = FvxTmHmCompatMod.CompletelyRandom; break;
                case 3: opt.TmHmCompatMod = FvxTmHmCompatMod.FullCompatibility; break;
                default: opt.TmHmCompatMod = FvxTmHmCompatMod.Unchanged; break;
            }
            opt.TmLevelupMoveSanity = _tmLevelupSanity.Checked;
            opt.TmsFollowEvolutions = _tmFollowEvos.Checked;
            opt.FullHmCompatibility = _fullHmCompat.Checked;

            opt.TutorMovesMod = (FvxTutorMoveMod)_tutorMovesMod.SelectedIndex;
            opt.KeepFieldMoveTutors = _keepFieldTutors.Checked;
            opt.TutorsForceGoodDamaging = _forceGoodTutors.Checked;
            opt.TutorsGoodDamagingPercent = (int)_goodTutorPct.Value;
            switch (_tutorCompatMod.SelectedIndex)
            {
                case 1: opt.TutorCompatMod = FvxTutorCompatMod.RandomPreferType; break;
                case 2: opt.TutorCompatMod = FvxTutorCompatMod.CompletelyRandom; break;
                case 3: opt.TutorCompatMod = FvxTutorCompatMod.FullCompatibility; break;
                default: opt.TutorCompatMod = FvxTutorCompatMod.Unchanged; break;
            }
            opt.TutorLevelupMoveSanity = _tutorLevelupSanity.Checked;
            opt.TutorFollowEvolutions = _tutorFollowEvos.Checked;
        }

        public void LoadFromOptions(FvxRandomizerOptions opt)
        {
            if (opt == null) return;
            _tmMovesMod.SelectedIndex = (int)opt.TmMovesMod;
            _keepFieldTms.Checked = opt.KeepFieldMoveTms;
            _forceGoodTms.Checked = opt.TmsForceGoodDamaging;
            _goodTmPct.Value = Math.Max(_goodTmPct.Minimum, Math.Min(_goodTmPct.Maximum, opt.TmsGoodDamagingPercent));

            switch (opt.TmHmCompatMod)
            {
                case FvxTmHmCompatMod.RandomPreferType: _tmCompatMod.SelectedIndex = 1; break;
                case FvxTmHmCompatMod.CompletelyRandom: _tmCompatMod.SelectedIndex = 2; break;
                case FvxTmHmCompatMod.FullCompatibility: _tmCompatMod.SelectedIndex = 3; break;
                default: _tmCompatMod.SelectedIndex = 0; break;
            }
            _tmLevelupSanity.Checked = opt.TmLevelupMoveSanity;
            _tmFollowEvos.Checked = opt.TmsFollowEvolutions;
            _fullHmCompat.Checked = opt.FullHmCompatibility;

            _tutorMovesMod.SelectedIndex = (int)opt.TutorMovesMod;
            _keepFieldTutors.Checked = opt.KeepFieldMoveTutors;
            _forceGoodTutors.Checked = opt.TutorsForceGoodDamaging;
            _goodTutorPct.Value = Math.Max(_goodTutorPct.Minimum, Math.Min(_goodTutorPct.Maximum, opt.TutorsGoodDamagingPercent));
            switch (opt.TutorCompatMod)
            {
                case FvxTutorCompatMod.RandomPreferType: _tutorCompatMod.SelectedIndex = 1; break;
                case FvxTutorCompatMod.CompletelyRandom: _tutorCompatMod.SelectedIndex = 2; break;
                case FvxTutorCompatMod.FullCompatibility: _tutorCompatMod.SelectedIndex = 3; break;
                default: _tutorCompatMod.SelectedIndex = 0; break;
            }
            _tutorLevelupSanity.Checked = opt.TutorLevelupMoveSanity;
            _tutorFollowEvos.Checked = opt.TutorFollowEvolutions;
        }
    }
}
