namespace NewEditor.Forms
{
    partial class GeneShuffleForm
    {
        System.ComponentModel.IContainer components = null;

        // Top strip
        System.Windows.Forms.Panel topPanel;
        System.Windows.Forms.CheckBox includeFairyCheck;
        System.Windows.Forms.Label labelSeed;
        System.Windows.Forms.NumericUpDown seedNumeric;
        System.Windows.Forms.Button applyButton;

        // Tabs
        System.Windows.Forms.TabControl mainTabs;
        System.Windows.Forms.TabPage tabGeneShuffle;
        System.Windows.Forms.TabPage tabPokemonTraits;

        GeneShuffleControl geneShuffleControl;
        PokemonTraitsControl traitsControl;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        void InitializeComponent()
        {
            topPanel = new System.Windows.Forms.Panel();
            includeFairyCheck = new System.Windows.Forms.CheckBox();
            labelSeed = new System.Windows.Forms.Label();
            seedNumeric = new System.Windows.Forms.NumericUpDown();
            applyButton = new System.Windows.Forms.Button();

            mainTabs = new System.Windows.Forms.TabControl();
            tabGeneShuffle = new System.Windows.Forms.TabPage();
            tabPokemonTraits = new System.Windows.Forms.TabPage();

            geneShuffleControl = new GeneShuffleControl();
            traitsControl = new PokemonTraitsControl();

            topPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(seedNumeric)).BeginInit();
            mainTabs.SuspendLayout();
            tabGeneShuffle.SuspendLayout();
            tabPokemonTraits.SuspendLayout();
            SuspendLayout();

            // ----- Top panel -----
            topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            topPanel.Height = 44;
            topPanel.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);

            includeFairyCheck.AutoSize = true;
            includeFairyCheck.Location = new System.Drawing.Point(8, 12);
            includeFairyCheck.Text = "Include Fairy-Types (applies Fairy Vpatch when needed)";

            labelSeed.AutoSize = true;
            labelSeed.Text = "RNG seed (0 = random):";
            labelSeed.Location = new System.Drawing.Point(420, 14);

            seedNumeric.Location = new System.Drawing.Point(580, 10);
            seedNumeric.Size = new System.Drawing.Size(100, 22);
            seedNumeric.Minimum = int.MinValue;
            seedNumeric.Maximum = int.MaxValue;

            applyButton.Text = "Apply Gene Shuffle";
            applyButton.Location = new System.Drawing.Point(700, 6);
            applyButton.Size = new System.Drawing.Size(160, 28);
            applyButton.Click += ApplyClick;

            topPanel.Controls.Add(includeFairyCheck);
            topPanel.Controls.Add(labelSeed);
            topPanel.Controls.Add(seedNumeric);
            topPanel.Controls.Add(applyButton);

            // ----- Tabs -----
            mainTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            mainTabs.Alignment = System.Windows.Forms.TabAlignment.Top;

            tabGeneShuffle.Text = "Gene Shuffle";
            tabGeneShuffle.Padding = new System.Windows.Forms.Padding(8);
            geneShuffleControl.Dock = System.Windows.Forms.DockStyle.Fill;
            tabGeneShuffle.Controls.Add(geneShuffleControl);

            tabPokemonTraits.Text = "Pokemon Traits";
            tabPokemonTraits.Padding = new System.Windows.Forms.Padding(8);
            traitsControl.Dock = System.Windows.Forms.DockStyle.Fill;
            tabPokemonTraits.Controls.Add(traitsControl);

            mainTabs.TabPages.Add(tabGeneShuffle);
            mainTabs.TabPages.Add(tabPokemonTraits);

            AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(900, 600);
            MinimumSize = new System.Drawing.Size(820, 540);
            Controls.Add(mainTabs);
            Controls.Add(topPanel);
            Font = new System.Drawing.Font("Arial", 9.75F);
            Text = "Gene Shuffle (types + traits + FVX learnsets)";

            tabGeneShuffle.ResumeLayout(false);
            tabPokemonTraits.ResumeLayout(false);
            mainTabs.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(seedNumeric)).EndInit();
            topPanel.ResumeLayout(false);
            topPanel.PerformLayout();
            ResumeLayout(false);
        }
    }
}
