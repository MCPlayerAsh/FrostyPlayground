namespace NewEditor.Forms
{
    partial class GeneShuffleControl
    {
        System.ComponentModel.IContainer components = null;

        System.Windows.Forms.TableLayoutPanel rootTable;

        // Types
        System.Windows.Forms.GroupBox geneGroup;
        System.Windows.Forms.Label typeModeLabel;
        System.Windows.Forms.ComboBox typeModeCombo;

        // Moves & learnsets
        System.Windows.Forms.GroupBox movesetGroup;
        System.Windows.Forms.ComboBox movesetModCombo;
        System.Windows.Forms.CheckBox blockBrokenMovesCheck;
        System.Windows.Forms.CheckBox guaranteedStartCheck;
        System.Windows.Forms.NumericUpDown guaranteedCountNumeric;
        System.Windows.Forms.CheckBox reorderDamagingCheck;
        System.Windows.Forms.CheckBox forceGoodDamagingCheck;
        System.Windows.Forms.NumericUpDown goodDamagingPercentNumeric;
        System.Windows.Forms.CheckBox evoMoveAllCheck;
        System.Windows.Forms.CheckBox randomizeEggCheck;

        // TM/HM
        System.Windows.Forms.GroupBox tmHmGroup;
        System.Windows.Forms.ComboBox tmHmModCombo;
        System.Windows.Forms.CheckBox tmFollowEvoCheck;

        // Tutors
        System.Windows.Forms.GroupBox tutorGroup;
        System.Windows.Forms.ComboBox tutorModCombo;
        System.Windows.Forms.CheckBox tutorFollowEvoCheck;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        void InitializeComponent()
        {
            rootTable = new System.Windows.Forms.TableLayoutPanel();

            geneGroup = new System.Windows.Forms.GroupBox();
            typeModeLabel = new System.Windows.Forms.Label();
            typeModeCombo = new System.Windows.Forms.ComboBox();

            movesetGroup = new System.Windows.Forms.GroupBox();
            movesetModCombo = new System.Windows.Forms.ComboBox();
            blockBrokenMovesCheck = new System.Windows.Forms.CheckBox();
            guaranteedStartCheck = new System.Windows.Forms.CheckBox();
            guaranteedCountNumeric = new System.Windows.Forms.NumericUpDown();
            reorderDamagingCheck = new System.Windows.Forms.CheckBox();
            forceGoodDamagingCheck = new System.Windows.Forms.CheckBox();
            goodDamagingPercentNumeric = new System.Windows.Forms.NumericUpDown();
            evoMoveAllCheck = new System.Windows.Forms.CheckBox();
            randomizeEggCheck = new System.Windows.Forms.CheckBox();

            tmHmGroup = new System.Windows.Forms.GroupBox();
            tmHmModCombo = new System.Windows.Forms.ComboBox();
            tmFollowEvoCheck = new System.Windows.Forms.CheckBox();

            tutorGroup = new System.Windows.Forms.GroupBox();
            tutorModCombo = new System.Windows.Forms.ComboBox();
            tutorFollowEvoCheck = new System.Windows.Forms.CheckBox();

            rootTable.SuspendLayout();
            geneGroup.SuspendLayout();
            movesetGroup.SuspendLayout();
            tmHmGroup.SuspendLayout();
            tutorGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(guaranteedCountNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(goodDamagingPercentNumeric)).BeginInit();
            SuspendLayout();

            rootTable.Dock = System.Windows.Forms.DockStyle.Fill;
            rootTable.ColumnCount = 1;
            rootTable.RowCount = 4;
            rootTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            rootTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            rootTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 210F));
            rootTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            rootTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 90F));

            // ----- Types -----
            geneGroup.Text = "Type randomization";
            geneGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            geneGroup.Padding = new System.Windows.Forms.Padding(6);

            typeModeLabel.Text = "Type randomization:";
            typeModeLabel.AutoSize = true;
            typeModeLabel.Location = new System.Drawing.Point(10, 28);

            typeModeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            typeModeCombo.Location = new System.Drawing.Point(160, 24);
            typeModeCombo.Size = new System.Drawing.Size(330, 24);
            typeModeCombo.Items.AddRange(new object[] { "Full random (per species)", "Following evolution (one roll per line)", "Vanilla-type logic (shuffle types within line)" });
            typeModeCombo.SelectedIndex = 0;

            geneGroup.Controls.Add(typeModeLabel);
            geneGroup.Controls.Add(typeModeCombo);

            // ----- Moves & learnsets -----
            movesetGroup.Text = "Moves && learnsets (FVX-style)";
            movesetGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            movesetGroup.Padding = new System.Windows.Forms.Padding(6);

            movesetModCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            movesetModCombo.Location = new System.Drawing.Point(10, 24);
            movesetModCombo.Size = new System.Drawing.Size(390, 24);
            movesetModCombo.Items.AddRange(new object[] { "Unchanged", "Completely random", "Random (prefer same type)", "Metronome only mode" });
            movesetModCombo.SelectedIndex = 1;

            blockBrokenMovesCheck.Text = "Block broken moves (SonicBoom / Dragon Rage)";
            blockBrokenMovesCheck.Location = new System.Drawing.Point(10, 54);
            blockBrokenMovesCheck.AutoSize = true;
            blockBrokenMovesCheck.Checked = true;

            guaranteedStartCheck.Text = "Start with guaranteed damaging moves (Gen5)";
            guaranteedStartCheck.Location = new System.Drawing.Point(10, 80);
            guaranteedStartCheck.AutoSize = true;
            guaranteedStartCheck.Checked = true;

            guaranteedCountNumeric.Location = new System.Drawing.Point(320, 76);
            guaranteedCountNumeric.Size = new System.Drawing.Size(50, 22);
            guaranteedCountNumeric.Minimum = 2;
            guaranteedCountNumeric.Maximum = 4;
            guaranteedCountNumeric.Value = 4;

            forceGoodDamagingCheck.Text = "Force good damaging moves %";
            forceGoodDamagingCheck.Location = new System.Drawing.Point(10, 106);
            forceGoodDamagingCheck.AutoSize = true;

            goodDamagingPercentNumeric.Location = new System.Drawing.Point(220, 102);
            goodDamagingPercentNumeric.Size = new System.Drawing.Size(50, 22);
            goodDamagingPercentNumeric.Minimum = 0;
            goodDamagingPercentNumeric.Maximum = 100;
            goodDamagingPercentNumeric.Value = 50;

            reorderDamagingCheck.Text = "Reorder damaging moves";
            reorderDamagingCheck.Location = new System.Drawing.Point(10, 132);
            reorderDamagingCheck.AutoSize = true;

            evoMoveAllCheck.Text = "Evolution moves for all (slot at level 0)";
            evoMoveAllCheck.Location = new System.Drawing.Point(10, 158);
            evoMoveAllCheck.AutoSize = true;

            randomizeEggCheck.Text = "Randomize egg moves (same rules as learnsets)";
            randomizeEggCheck.Location = new System.Drawing.Point(10, 184);
            randomizeEggCheck.AutoSize = true;
            randomizeEggCheck.Checked = true;

            movesetGroup.Controls.Add(movesetModCombo);
            movesetGroup.Controls.Add(blockBrokenMovesCheck);
            movesetGroup.Controls.Add(guaranteedStartCheck);
            movesetGroup.Controls.Add(guaranteedCountNumeric);
            movesetGroup.Controls.Add(forceGoodDamagingCheck);
            movesetGroup.Controls.Add(goodDamagingPercentNumeric);
            movesetGroup.Controls.Add(reorderDamagingCheck);
            movesetGroup.Controls.Add(evoMoveAllCheck);
            movesetGroup.Controls.Add(randomizeEggCheck);

            // ----- TM/HM -----
            tmHmGroup.Text = "TM/HM compatibility";
            tmHmGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            tmHmGroup.Padding = new System.Windows.Forms.Padding(6);

            tmHmModCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            tmHmModCombo.Location = new System.Drawing.Point(10, 24);
            tmHmModCombo.Size = new System.Drawing.Size(390, 24);
            tmHmModCombo.Items.AddRange(new object[] { "Unchanged", "Completely random", "Random (prefer matching type)" });
            tmHmModCombo.SelectedIndex = 1;

            tmFollowEvoCheck.Text = "TM/HM compatibility follows evolutions";
            tmFollowEvoCheck.Location = new System.Drawing.Point(10, 54);
            tmFollowEvoCheck.AutoSize = true;

            tmHmGroup.Controls.Add(tmHmModCombo);
            tmHmGroup.Controls.Add(tmFollowEvoCheck);

            // ----- Tutors -----
            tutorGroup.Text = "Move tutor compatibility (BW2)";
            tutorGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            tutorGroup.Padding = new System.Windows.Forms.Padding(6);

            tutorModCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            tutorModCombo.Location = new System.Drawing.Point(10, 24);
            tutorModCombo.Size = new System.Drawing.Size(390, 24);
            tutorModCombo.Items.AddRange(new object[] { "Unchanged", "Completely random", "Random (prefer matching type)" });
            tutorModCombo.SelectedIndex = 1;

            tutorFollowEvoCheck.Text = "Tutor compatibility follows evolutions";
            tutorFollowEvoCheck.Location = new System.Drawing.Point(10, 54);
            tutorFollowEvoCheck.AutoSize = true;

            tutorGroup.Controls.Add(tutorModCombo);
            tutorGroup.Controls.Add(tutorFollowEvoCheck);

            rootTable.Controls.Add(geneGroup, 0, 0);
            rootTable.Controls.Add(movesetGroup, 0, 1);
            rootTable.Controls.Add(tmHmGroup, 0, 2);
            rootTable.Controls.Add(tutorGroup, 0, 3);

            AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(rootTable);
            Font = new System.Drawing.Font("Arial", 9.75F);

            ((System.ComponentModel.ISupportInitialize)(guaranteedCountNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(goodDamagingPercentNumeric)).EndInit();
            geneGroup.ResumeLayout(false);
            geneGroup.PerformLayout();
            movesetGroup.ResumeLayout(false);
            movesetGroup.PerformLayout();
            tmHmGroup.ResumeLayout(false);
            tmHmGroup.PerformLayout();
            tutorGroup.ResumeLayout(false);
            tutorGroup.PerformLayout();
            rootTable.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
