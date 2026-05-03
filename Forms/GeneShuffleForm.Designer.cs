namespace NewEditor.Forms
{
    partial class GeneShuffleForm
    {
        System.ComponentModel.IContainer components = null;
        System.Windows.Forms.GroupBox geneGroup;
        System.Windows.Forms.CheckBox includeFairyCheck;
        System.Windows.Forms.ComboBox typeModeCombo;
        System.Windows.Forms.Label typeModeLabel;
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
            geneGroup = new System.Windows.Forms.GroupBox();
            includeFairyCheck = new System.Windows.Forms.CheckBox();
            typeModeLabel = new System.Windows.Forms.Label();
            typeModeCombo = new System.Windows.Forms.ComboBox();
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
            geneGroup.SuspendLayout();
            movesetGroup.SuspendLayout();
            tmHmGroup.SuspendLayout();
            tutorGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(guaranteedCountNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(goodDamagingPercentNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(seedNumeric)).BeginInit();
            SuspendLayout();

            geneGroup.Text = "Types";
            geneGroup.Location = new System.Drawing.Point(12, 12);
            geneGroup.Size = new System.Drawing.Size(420, 72);
            includeFairyCheck.Text = "Include Fairy-Types (applies Fairy Vpatch when needed)";
            includeFairyCheck.Location = new System.Drawing.Point(10, 22);
            includeFairyCheck.AutoSize = true;
            typeModeLabel.Text = "Type randomization:";
            typeModeLabel.AutoSize = true;
            typeModeLabel.Location = new System.Drawing.Point(10, 46);
            typeModeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            typeModeCombo.Location = new System.Drawing.Point(160, 42);
            typeModeCombo.Size = new System.Drawing.Size(250, 24);
            typeModeCombo.Items.AddRange(new object[] { "Full random (per species)", "Following evolution (one roll per line)", "Vanilla-type logic (shuffle types within line)" });
            typeModeCombo.SelectedIndex = 0;
            geneGroup.Controls.Add(includeFairyCheck);
            geneGroup.Controls.Add(typeModeLabel);
            geneGroup.Controls.Add(typeModeCombo);

            movesetGroup.Text = "Moves && learnsets (FVX-style, after types)";
            movesetGroup.Location = new System.Drawing.Point(12, 90);
            movesetGroup.Size = new System.Drawing.Size(420, 210);
            movesetModCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            movesetModCombo.Location = new System.Drawing.Point(10, 22);
            movesetModCombo.Size = new System.Drawing.Size(390, 24);
            movesetModCombo.Items.AddRange(new object[] { "Unchanged", "Completely random", "Random (prefer same type)" });
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
            tmHmGroup.Location = new System.Drawing.Point(12, 306);
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
            tutorGroup.Location = new System.Drawing.Point(12, 402);
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
            labelSeed.Location = new System.Drawing.Point(12, 506);
            seedNumeric.Location = new System.Drawing.Point(180, 502);
            seedNumeric.Size = new System.Drawing.Size(120, 22);
            seedNumeric.Minimum = int.MinValue;
            seedNumeric.Maximum = int.MaxValue;
            applyButton.Text = "Apply Gene Shuffle";
            applyButton.Location = new System.Drawing.Point(250, 498);
            applyButton.Size = new System.Drawing.Size(180, 28);
            applyButton.Click += ApplyClick;

            AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(450, 540);
            Controls.Add(geneGroup);
            Controls.Add(movesetGroup);
            Controls.Add(tmHmGroup);
            Controls.Add(tutorGroup);
            Controls.Add(labelSeed);
            Controls.Add(seedNumeric);
            Controls.Add(applyButton);
            Font = new System.Drawing.Font("Arial", 9.75F);
            Text = "Gene Shuffle (types + FVX learnsets)";
            geneGroup.ResumeLayout(false);
            geneGroup.PerformLayout();
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
