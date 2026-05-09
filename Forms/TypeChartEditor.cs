using NewEditor.Data;
using NewEditor.Data.NARCTypes;
using NewEditor.Data.Randomization.FvxGen5;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NewEditor.Forms
{
    public partial class TypeChartEditor : Form
    {
        /// <summary>Byte offset of the 17×17 chart inside the battle overlay (BW1 vs BW2).</summary>
        int tableLocation;

        /// <summary>Avoid pushing combo initialization back into the overlay.</summary>
        bool _suppressChartPush;

        ComboBox[,] table;

        int BattleOverlayIndex
        {
            get
            {
                if (MainEditor.RomType != RomType.BW1 && MainEditor.RomType != RomType.BW2)
                    return FvxGen5RomLayout.BattleOverlayIndex(true);
                return FvxGen5RomLayout.BattleOverlayIndex(MainEditor.RomType == RomType.BW2);
            }
        }

        /// <summary>Fairy Vpatch uses 18-byte rows at the chart base; vanilla uses 17-byte rows. Delegates to <see cref="FvxGen5TypeChart.DetectBattleOverlayChartStride"/> so UI and randomizers stay aligned.</summary>
        int ChartStrideInRom(int overlayIndex)
        {
            if (MainEditor.fileSystem?.overlays == null || overlayIndex < 0 ||
                overlayIndex >= MainEditor.fileSystem.overlays.Count)
                return 17;
            var o = MainEditor.fileSystem.overlays[overlayIndex];
            return FvxGen5TypeChart.DetectBattleOverlayChartStride(o, tableLocation);
        }

        List<string> names = new List<string>()
        {
            "Norm",
            "Gras",
            "Fire",
            "Watr",
            "Elec",
            "Ice",
            "Pois",
            "Bug",
            "Rock",
            "Grnd",
            "Fly",
            "Fght",
            "Psy",
            "Dark",
            "Ghst",
            "Drgn",
            "Steel",
        };

        List<int> reorder = new List<int>()
        {
            0, 11, 9, 10, 12, 14, 3, 6, 5, 4, 2, 1, 13, 16, 7, 15, 8
        };

        public TypeChartEditor()
        {
            InitializeComponent();

            bool bw2 = MainEditor.RomType == RomType.BW2;
            tableLocation = FvxGen5RomLayout.TypeChartOffsetInBattleOvl(bw2);

            table = new ComboBox[17, 17];
            SuspendLayout();

            for (int x = 0; x < 17; x++)
            {
                Controls.Add(new Label()
                {
                    Text = names[x],
                    Location = new Point(160 + 44 * x, 90),
                    Size = new Size(40, 20)
                });
                Controls.Add(new Label()
                {
                    Text = names[x],
                    Location = new Point(120, 124 + 28 * x),
                    Size = new Size(40, 20)
                });
            }

            for (int x = 0; x < 17; x++)
            {
                for (int y = 0; y < 17; y++)
                {
                    table[y, x] = new ComboBox()
                    {
                        Location = new Point(160 + 44 * x, 120 + 28 * y),
                        Size = new Size(40, 24)
                    };
                    table[y, x].Items.Add(0);
                    table[y, x].Items.Add(0.5);
                    table[y, x].Items.Add(1);
                    table[y, x].Items.Add(2);
                    Controls.Add(table[y, x]);

                    int ovl = BattleOverlayIndex;
                    _suppressChartPush = true;
                    try
                    {
                        byte b = ReadChartByte(ovl, y, x);
                        int index = b == 0 ? 0 : (int)Math.Log(b, 2);
                        table[y, x].SelectedIndex = Math.Min(3, index);
                    }
                    finally
                    {
                        _suppressChartPush = false;
                    }

                    int cy = y, cx = x;
                    table[y, x].SelectedIndexChanged += (s, e) => OnChartCellChanged(cy, cx);
                }
            }
            ResumeLayout();
            tableAddressNumberBox.Value = tableLocation;
        }

        byte ReadChartByte(int overlayIndex, int y, int x)
        {
            var o = MainEditor.fileSystem.overlays[overlayIndex];
            int stride = ChartStrideInRom(overlayIndex);
            return o[tableLocation + reorder[y] * stride + reorder[x]];
        }

        void OnChartCellChanged(int y, int x)
        {
            if (_suppressChartPush) return;
            PushCellToOverlay(y, x);
        }

        /// <summary>Writes the current UI cell to the in-memory battle overlay (same data the randomizer reads; no ROM disk save required).</summary>
        void PushCellToOverlay(int y, int x)
        {
            if (MainEditor.fileSystem?.overlays == null) return;
            int oi = BattleOverlayIndex;
            if (oi < 0 || oi >= MainEditor.fileSystem.overlays.Count) return;
            if (table[y, x].SelectedIndex < 0) return;
            var o = MainEditor.fileSystem.overlays[oi];
            int stride = ChartStrideInRom(oi);
            int p = tableLocation + reorder[y] * stride + reorder[x];
            if (p < 0 || p >= o.Count) return;
            o[p] = (byte)(table[y, x].SelectedIndex == 0 ? 0 : Math.Pow(2, table[y, x].SelectedIndex));
        }

        private void recalculateAddressButton_Click(object sender, EventArgs e)
        {
            tableLocation = (int)tableAddressNumberBox.Value;
            int oi = BattleOverlayIndex;
            _suppressChartPush = true;
            try
            {
                for (int x = 0; x < 17; x++)
                {
                    for (int y = 0; y < 17; y++)
                    {
                        byte b = ReadChartByte(oi, y, x);
                        int index = b == 0 ? 0 : (int)Math.Log(b, 2);
                        table[y, x].SelectedIndex = Math.Min(3, index);
                    }
                }
            }
            finally
            {
                _suppressChartPush = false;
            }
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            for (int x = 0; x < 17; x++)
            {
                for (int y = 0; y < 17; y++)
                    PushCellToOverlay(y, x);
            }
        }
    }
}
