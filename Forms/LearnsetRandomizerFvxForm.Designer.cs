namespace NewEditor.Forms
{
    partial class LearnsetRandomizerFvxForm
    {
        System.ComponentModel.IContainer components = null;
        System.Windows.Forms.GroupBox movesetGroup;
        System.Windows.Forms.ComboBox movesetModCombo;
        System.Windows.Forms.CheckBox blockBrokenMovesCheck;
        System.Windows.Forms.CheckBox guaranteedStartCheck;
        System.Windows.Forms.NumericUpDown guaranteedCountNumeric;
        System.Windows.Forms.CheckBox forceGoodDamagingCheck;
        System.Windows.Forms.NumericUpDown goodDamagingPercentNumeric;
        System.Windows.Forms.CheckBox evoMoveAllCheck;
        System.Windows.Forms.CheckBox randomizeEggCheck;
        System.Windows.Forms.GroupBox tmHmGroup;
        System.Windows.Forms.ComboBox tmHmModCombo;
        System.Windows.Forms.CheckBox tmFollowEvoCheck;
        System.Windows.Forms.GroupBox tutorGroup;
        System.Windows.Forms.ComboBox tutorModCombo;
        System.Windows.Forms.CheckBox tutorFollowEvoCheck;
        System.Windows.Forms.NumericUpDown seedNumeric;
        System.Windows.Forms.Button applyButton;
        System.Windows.Forms.Label labelSeed;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        void InitializeComponent()
        {
            movesetGroup = new System.Windows.Forms.GroupBox();
            movesetModCombo = new System.Windows.Forms.ComboBox();
            blockBrokenMovesCheck = new System.Windows.Forms.CheckBox();
            guaranteedStartCheck = new System.Windows.Forms.CheckBox();
            guaranteedCountNumeric = new System.Windows.Forms.NumericUpDown();
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
            seedNumeric = new System.Windows.Forms.NumericUpDown();
            applyButton = new System.Windows.Forms.Button();
            labelSeed = new System.Windows.Forms.Label();
            movesetGroup.SuspendLayout();
            tmHmGroup.SuspendLayout();
            tutorGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(guaranteedCountNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(goodDamagingPercentNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(seedNumeric)).BeginInit();
            SuspendLayout();

            movesetGroup.Text = "Moves && learnsets (FVX-style)";
            movesetGroup.Location = new System.Drawing.Point(12, 12);
            movesetGroup.Size = new System.Drawing.Size(420, 210);
            movesetModCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            movesetModCombo.Location = new System.Drawing.Point(10, 22);
            movesetModCombo.Size = new System.Drawing.Size(390, 24);
            movesetModCombo.Items.AddRange(new object[] { "Unchanged", "Completely random", "Random (prefer same type)", "Metronome only" });
            movesetModCombo.SelectedIndex = 1;
            blockBrokenMovesCheck.Text = "Block broken moves (SonicBoom / Dragon Rage)";
            blockBrokenMovesCheck.Location = new System.Drawing.Point(10, 52);
            blockBrokenMovesCheck.AutoSize = true;
            blockBrokenMovesCheck.Checked = true;
            guaranteedStartCheck.Text = "Start with guaranteed damaging moves (Gen5)";
            guaranteedStartCheck.Location = new System.Drawing.Point(10, 78);
            guaranteedStartCheck.AutoSize = true;
            guaranteedStartCheck.Checked = true;
            guaranteedCountNumeric.Location = new System.Drawing.Point(320, 74);
            guaranteedCountNumeric.Size = new System.Drawing.Size(50, 22);
            guaranteedCountNumeric.Minimum = 2;
            guaranteedCountNumeric.Maximum = 4;
            guaranteedCountNumeric.Value = 4;
            forceGoodDamagingCheck.Text = "Force good damaging moves %";
            forceGoodDamagingCheck.Location = new System.Drawing.Point(10, 104);
            forceGoodDamagingCheck.AutoSize = true;
            goodDamagingPercentNumeric.Location = new System.Drawing.Point(220, 100);
            goodDamagingPercentNumeric.Size = new System.Drawing.Size(50, 22);
            goodDamagingPercentNumeric.Minimum = 0;
            goodDamagingPercentNumeric.Maximum = 100;
            goodDamagingPercentNumeric.Value = 50;
            evoMoveAllCheck.Text = "Evolution moves for all (slot at level 0)";
            evoMoveAllCheck.Location = new System.Drawing.Point(10, 130);
            evoMoveAllCheck.AutoSize = true;
            randomizeEggCheck.Text = "Randomize egg moves (same rules as learnsets)";
            randomizeEggCheck.Location = new System.Drawing.Point(10, 156);
            randomizeEggCheck.AutoSize = true;
            randomizeEggCheck.Checked = true;
            movesetGroup.Controls.Add(movesetModCombo);
            movesetGroup.Controls.Add(blockBrokenMovesCheck);
            movesetGroup.Controls.Add(guaranteedStartCheck);
            movesetGroup.Controls.Add(guaranteedCountNumeric);
            movesetGroup.Controls.Add(forceGoodDamagingCheck);
            movesetGroup.Controls.Add(goodDamagingPercentNumeric);
            movesetGroup.Controls.Add(evoMoveAllCheck);
            movesetGroup.Controls.Add(randomizeEggCheck);

            tmHmGroup.Text = "TM/HM compatibility";
            tmHmGroup.Location = new System.Drawing.Point(12, 228);
            tmHmGroup.Size = new System.Drawing.Size(420, 90);
            tmHmModCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            tmHmModCombo.Location = new System.Drawing.Point(10, 22);
            tmHmModCombo.Size = new System.Drawing.Size(390, 24);
            tmHmModCombo.Items.AddRange(new object[] { "Unchanged", "Completely random", "Random (prefer matching type)" });
            tmHmModCombo.SelectedIndex = 1;
            tmFollowEvoCheck.Text = "TM/HM compatibility follows evolutions";
            tmFollowEvoCheck.Location = new System.Drawing.Point(10, 54);
            tmFollowEvoCheck.AutoSize = true;
            tmHmGroup.Controls.Add(tmHmModCombo);
            tmHmGroup.Controls.Add(tmFollowEvoCheck);

            tutorGroup.Text = "Move tutor compatibility (BW2)";
            tutorGroup.Location = new System.Drawing.Point(12, 324);
            tutorGroup.Size = new System.Drawing.Size(420, 90);
            tutorModCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            tutorModCombo.Location = new System.Drawing.Point(10, 22);
            tutorModCombo.Size = new System.Drawing.Size(390, 24);
            tutorModCombo.Items.AddRange(new object[] { "Unchanged", "Completely random", "Random (prefer matching type)" });
            tutorModCombo.SelectedIndex = 1;
            tutorFollowEvoCheck.Text = "Tutor compatibility follows evolutions";
            tutorFollowEvoCheck.Location = new System.Drawing.Point(10, 54);
            tutorFollowEvoCheck.AutoSize = true;
            tutorGroup.Controls.Add(tutorModCombo);
            tutorGroup.Controls.Add(tutorFollowEvoCheck);

            labelSeed.Text = "RNG seed (0 = random):";
            labelSeed.AutoSize = true;
            labelSeed.Location = new System.Drawing.Point(12, 428);
            seedNumeric.Location = new System.Drawing.Point(180, 424);
            seedNumeric.Size = new System.Drawing.Size(120, 22);
            seedNumeric.Minimum = int.MinValue;
            seedNumeric.Maximum = int.MaxValue;
            applyButton.Text = "Apply";
            applyButton.Location = new System.Drawing.Point(320, 420);
            applyButton.Size = new System.Drawing.Size(110, 28);
            applyButton.Click += ApplyClick;

            AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(450, 470);
            Controls.Add(movesetGroup);
            Controls.Add(tmHmGroup);
            Controls.Add(tutorGroup);
            Controls.Add(labelSeed);
            Controls.Add(seedNumeric);
            Controls.Add(applyButton);
            Font = new System.Drawing.Font("Arial", 9.75F);
            Text = "FVX-style Learnset / TM / Tutor (Gen 5)";
            movesetGroup.ResumeLayout(false);
            movesetGroup.PerformLayout();
            tmHmGroup.ResumeLayout(false);
            tmHmGroup.PerformLayout();
            tutorGroup.ResumeLayout(false);
            tutorGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(guaranteedCountNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(goodDamagingPercentNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(seedNumeric)).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
