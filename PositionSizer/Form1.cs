#nullable disable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        // ── Tema ──────────────────────────────────────────────────
        readonly Color BG = Color.FromArgb(9, 8, 12);
        readonly Color SURF = Color.FromArgb(13, 12, 18);
        readonly Color CARD = Color.FromArgb(18, 17, 26);
        readonly Color ELEV = Color.FromArgb(24, 23, 35);
        readonly Color HOVER = Color.FromArgb(30, 29, 44);

        readonly Color GOLD = Color.FromArgb(255, 195, 50);
        readonly Color LIME = Color.FromArgb(50, 215, 120);
        readonly Color RED = Color.FromArgb(255, 65, 90);
        readonly Color BLUE = Color.FromArgb(75, 160, 255);
        readonly Color PURP = Color.FromArgb(160, 105, 255);
        readonly Color TEAL = Color.FromArgb(0, 200, 175);

        readonly Color TXT = Color.FromArgb(215, 218, 238);
        readonly Color TXT2 = Color.FromArgb(88, 90, 118);
        readonly Color TXT3 = Color.FromArgb(38, 39, 58);
        readonly Color BDR = Color.FromArgb(18, 255, 255, 255);

        // ── State ─────────────────────────────────────────────────
        string selNav = "Position Sizer";
        string selPair = "EURUSD";
        string tradeDir = "BUY";
        string platform = "MT5";
        bool connected = false;

        double balance = 10000.0;
        double riskPct = 1.0;
        double entryPrice = 1.08350;
        double slPrice = 1.08100;
        double tpPrice = 1.08850;
        double lotSize = 0.0;
        double riskAmt = 0.0;
        double rewardAmt = 0.0;
        double rrRatio = 0.0;
        double pipVal = 10.0;
        double livePrice = 1.08350;

        readonly Random RNG = new Random();
        System.Windows.Forms.Timer ticker;

        // ── Trade geçmişi ─────────────────────────────────────────
        class TradeRec
        {
            public string Time = "", Pair = "", Dir = "", Entry = "", SL = "", TP = "", Lot = "", RR = "", PnL = "";
            public bool Win;
        }
        readonly List<TradeRec> history = new List<TradeRec>
        {
            new TradeRec{Time="11:42",Pair="EURUSD",Dir="BUY", Entry="1.08210",SL="1.07910",TP="1.08810",Lot="0.20",RR="1:2.0",PnL="+$120.00",Win=true },
            new TradeRec{Time="10:28",Pair="GBPUSD",Dir="SELL",Entry="1.27350",SL="1.27620",TP="1.26810",Lot="0.15",RR="1:2.0",PnL="-$60.75", Win=false},
            new TradeRec{Time="09:55",Pair="XAUUSD",Dir="BUY", Entry="2341.50",SL="2335.00",TP="2354.00",Lot="0.05",RR="1:2.0",PnL="+$62.50", Win=true },
        };

        // ── Controls ──────────────────────────────────────────────
        Panel pnlSidebar, pnlTopBar, pnlStatus;
        Panel pnlInputCard, pnlResultCard, pnlConnectCard, pnlRiskCard, pnlHistoryCard;
        Label lblPrice, lblPriceChg, lblClock, lblStatusTxt;
        Label lblConnState, lblPlatLbl, lblBotState;
        TextBox txtBalance, txtRisk, txtEntry, txtSL, txtTP;
        Button btnBuy, btnSell, btnCalc, btnTrade, btnConnect;
        ComboBox cmbPair, cmbPlatform, cmbServer;
        TextBox txtLogin, txtPassword;
        TrackBar trkRisk;
        Label lblRiskTrk;
        ListView lvHistory;

        // ═════════════════════════════════════════════════════════
        public Form1()
        {
            this.Text = "Position Sizer EA · AutoScripts";
            this.Size = new Size(1220, 760);
            this.MinimumSize = new Size(1000, 640);
            this.BackColor = BG;
            this.ForeColor = TXT;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9f);
            this.DoubleBuffered = true;
            this.FormBorderStyle = FormBorderStyle.None;

            // Sıra: Top → Bottom → Left → Fill
            MakeTopBar();
            MakeStatusBar();
            MakeSidebar();
            MakeMainArea();    // Fill — en son
            Calculate();
            StartTicker();
        }

        // ── TopBar ────────────────────────────────────────────────
        void MakeTopBar()
        {
            pnlTopBar = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = SURF };
            pnlTopBar.Paint += (s, e) =>
            {
                var g = e.Graphics;
                using (var br = new LinearGradientBrush(new Point(0, 0), new Point(pnlTopBar.Width, 0),
                    Color.FromArgb(70, GOLD.R, GOLD.G, GOLD.B), Color.Transparent))
                    g.FillRectangle(br, 0, 0, pnlTopBar.Width, 3);
                using (var p = new Pen(BDR)) g.DrawLine(p, 0, 47, pnlTopBar.Width, 47);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                DrawShield(g, 14, 13, GOLD);
                using (var f = new Font("Segoe UI", 11f, FontStyle.Bold)) using (var br = new SolidBrush(TXT))
                    g.DrawString("POSITION SIZER  EA", f, br, 46, 12);
                using (var f = new Font("Consolas", 7.5f)) using (var br = new SolidBrush(TXT3))
                    g.DrawString("Lot Calculator  ·  Risk Manager  ·  Auto Trade  ·  MT4 / MT5", f, br, 48, 29);
                int pw = pnlTopBar.Width;
                DrawPill(g, pw - 310, 13, $"BAL  ${balance:N0}", LIME);
                DrawPill(g, pw - 182, 13, $"RISK  {riskPct:F1}%", GOLD);
            };
            pnlTopBar.MouseDown += (s, e) =>
            { if (e.Button == MouseButtons.Left) { NativeMethods.ReleaseCapture(); NativeMethods.SendMessage(Handle, 0xA1, (IntPtr)2, IntPtr.Zero); } };

            var bCl = WinBtn(Color.FromArgb(255, 85, 75)); bCl.Click += (s, e) => Close();
            var bMn = WinBtn(Color.FromArgb(255, 185, 40)); bMn.Click += (s, e) => WindowState = FormWindowState.Minimized;
            var bMx = WinBtn(Color.FromArgb(35, 195, 55)); bMx.Click += (s, e) => WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
            pnlTopBar.Controls.AddRange(new Control[] { bMn, bMx, bCl });
            void PW() { bCl.Location = new Point(pnlTopBar.Width - 26, 18); bMx.Location = new Point(pnlTopBar.Width - 46, 18); bMn.Location = new Point(pnlTopBar.Width - 66, 18); }
            PW(); pnlTopBar.Resize += (s, e) => { PW(); pnlTopBar.Invalidate(); };

            lblPrice = new Label { Text = "1.08350", Location = new Point(610, 10), Size = new Size(110, 26), ForeColor = GOLD, Font = new Font("Consolas", 15f, FontStyle.Bold), BackColor = Color.Transparent };
            lblPriceChg = new Label { Text = "▲ +0.00015", Location = new Point(723, 17), Size = new Size(110, 16), ForeColor = LIME, Font = new Font("Consolas", 9f), BackColor = Color.Transparent };
            pnlTopBar.Controls.AddRange(new Control[] { lblPrice, lblPriceChg });
            this.Controls.Add(pnlTopBar);
        }

        void DrawShield(Graphics g, int x, int y, Color col)
        {
            var pts = new PointF[] { new PointF(x + 11, y), new PointF(x + 22, y + 5), new PointF(x + 22, y + 14), new PointF(x + 11, y + 22), new PointF(x, y + 14), new PointF(x, y + 5) };
            using (var br = new SolidBrush(Color.FromArgb(30, col.R, col.G, col.B))) g.FillPolygon(br, pts);
            using (var pen = new Pen(col, 1.5f)) g.DrawPolygon(pen, pts);
            using (var f = new Font("Segoe UI", 9f, FontStyle.Bold)) using (var br = new SolidBrush(col))
            { var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center }; g.DrawString("R", f, br, new RectangleF(x, y + 2, 22, 18), sf); }
        }

        void DrawPill(Graphics g, int x, int y, string txt, Color col)
        {
            var r = new Rectangle(x, y, 120, 22);
            using (var br = new SolidBrush(Color.FromArgb(22, col.R, col.G, col.B))) g.FillRectangle(br, r);
            using (var pen = new Pen(Color.FromArgb(60, col.R, col.G, col.B))) g.DrawRectangle(pen, r);
            using (var f = new Font("Consolas", 8f, FontStyle.Bold)) using (var br = new SolidBrush(col))
            { var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center }; g.DrawString(txt, f, br, r, sf); }
        }

        // ── StatusBar ─────────────────────────────────────────────
        void MakeStatusBar()
        {
            pnlStatus = new Panel { Dock = DockStyle.Bottom, Height = 26, BackColor = SURF };
            pnlStatus.Paint += (s, e) =>
            {
                var g = e.Graphics;
                using (var p = new Pen(BDR)) g.DrawLine(p, 0, 0, pnlStatus.Width, 0);
                using (var f = new Font("Consolas", 8f)) using (var br = new SolidBrush(TXT3))
                {
                    g.DrawString($"Lot: {lotSize:F2}   Risk: ${riskAmt:N2}   Reward: ${rewardAmt:N2}   R:R  1:{rrRatio:F2}   {platform}", f, br, 210, 7);
                    g.DrawString("EA v2.0", f, br, pnlStatus.Width - 70, 7);
                }
            };
            lblStatusTxt = new Label { Text = "○ Disconnected", Location = new Point(12, 6), Size = new Size(190, 14), ForeColor = TXT2, Font = new Font("Consolas", 8f, FontStyle.Bold), BackColor = Color.Transparent };
            lblClock = new Label { Text = "", Location = new Point(800, 6), Size = new Size(230, 14), ForeColor = TXT3, Font = new Font("Consolas", 8f), BackColor = Color.Transparent };
            pnlStatus.Controls.AddRange(new Control[] { lblStatusTxt, lblClock });
            pnlStatus.Resize += (s, e) => { lblClock.Location = new Point(pnlStatus.Width - 240, 6); pnlStatus.Invalidate(); };
            this.Controls.Add(pnlStatus);
        }

        // ── Sidebar ───────────────────────────────────────────────
        void MakeSidebar()
        {
            pnlSidebar = new Panel { Dock = DockStyle.Left, Width = 212, BackColor = SURF };
            pnlSidebar.Paint += (s, e) =>
            {
                var g = e.Graphics;
                using (var p = new Pen(BDR)) g.DrawLine(p, pnlSidebar.Width - 1, 0, pnlSidebar.Width - 1, pnlSidebar.Height);
                using (var br = new LinearGradientBrush(new Point(0, 0), new Point(0, pnlSidebar.Height),
                    Color.FromArgb(60, GOLD.R, GOLD.G, GOLD.B), Color.Transparent))
                    g.FillRectangle(br, 0, 0, 3, pnlSidebar.Height);
            };

            var logo = new Panel { Height = 56, Dock = DockStyle.Top, BackColor = Color.Transparent };
            logo.Paint += (s, e) =>
            {
                var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                DrawShield(g, 12, 17, GOLD);
                using (var f = new Font("Segoe UI", 10f, FontStyle.Bold)) using (var br = new SolidBrush(TXT)) g.DrawString("POSITION SIZER", f, br, 42, 16);
                using (var f = new Font("Consolas", 7.5f)) using (var br = new SolidBrush(TXT3)) g.DrawString("Risk Manager EA", f, br, 44, 32);
                using (var p = new Pen(BDR)) g.DrawLine(p, 8, 54, 204, 54);
            };
            pnlSidebar.Controls.Add(logo);

            var navDefs = new[]{
                ("","── MENU ──"),("▤","Library"),("⌂","Home"),("◈","Dashboard"),
                ("","── EA TOOLS ──"),("⊞","Position Sizer"),("⚡","Scalping Bot"),("⊛","Account Protector"),("▦","Volume Profile"),
                ("","── SYSTEM ──"),("ℹ","About"),("✉","Contact"),
            };
            int ny = 60;
            foreach (var (icon, label) in navDefs)
            {
                if (icon == "")
                {
                    pnlSidebar.Controls.Add(new Label { Text = label, Location = new Point(10, ny), Size = new Size(192, 18), ForeColor = TXT3, Font = new Font("Consolas", 7.5f, FontStyle.Bold), BackColor = Color.Transparent });
                    ny += 22;
                }
                else
                {
                    string nav = label; bool act = nav == selNav;
                    var row = new Panel { Location = new Point(0, ny), Size = new Size(211, 34), BackColor = act ? Color.FromArgb(22, 255, 195, 50) : Color.Transparent, Cursor = Cursors.Hand, Tag = nav };
                    row.Paint += (s, e) => { if ((string)row.Tag == selNav) using (var p = new Pen(GOLD, 2)) e.Graphics.DrawLine(p, row.Width - 1, 0, row.Width - 1, row.Height); };
                    row.MouseEnter += (s, e) => { if ((string)row.Tag != selNav) row.BackColor = HOVER; };
                    row.MouseLeave += (s, e) => row.BackColor = (string)row.Tag == selNav ? Color.FromArgb(22, 255, 195, 50) : Color.Transparent;
                    row.Click += (s, e) => NavClick((string)row.Tag);
                    var icL = new Label { Text = icon, Location = new Point(14, 9), Size = new Size(18, 16), ForeColor = act ? GOLD : TXT2, Font = new Font("Segoe UI", 9f), BackColor = Color.Transparent, Tag = nav + "_i" };
                    var txL = new Label { Text = nav, Location = new Point(36, 9), Size = new Size(124, 16), ForeColor = act ? GOLD : TXT2, Font = new Font("Segoe UI", 9f), BackColor = Color.Transparent, Tag = nav + "_t" };
                    icL.Click += (s, e) => NavClick(nav); txL.Click += (s, e) => NavClick(nav);
                    row.Controls.AddRange(new Control[] { icL, txL });
                    if (nav == "Position Sizer")
                    {
                        var bdg = new Label { Text = "EA", Location = new Point(168, 10), Size = new Size(24, 14), BackColor = act ? GOLD : TXT3, ForeColor = BG, Font = new Font("Consolas", 7f, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
                        bdg.Click += (s, e) => NavClick(nav); row.Controls.Add(bdg);
                    }
                    pnlSidebar.Controls.Add(row); ny += 36;
                }
            }

            // footer
            var foot = new Panel { Dock = DockStyle.Bottom, Height = 96, BackColor = Color.Transparent };
            foot.Paint += (s, e) =>
            {
                var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var p = new Pen(BDR)) g.DrawLine(p, 8, 0, 204, 0);
                SbSt(g, 14, 6, "LOT SIZE", lotSize > 0 ? $"{lotSize:F2} lots" : "—", GOLD);
                SbSt(g, 14, 30, "RISK $", riskAmt > 0 ? $"${riskAmt:N2}" : "—", RED);
                SbSt(g, 14, 54, "REWARD $", rewardAmt > 0 ? $"${rewardAmt:N2}" : "—", LIME);
                SbSt(g, 14, 74, "R:R", rrRatio > 0 ? $"1 : {rrRatio:F2}" : "—", rrRatio >= 2 ? LIME : rrRatio >= 1 ? GOLD : RED);
            };
            pnlSidebar.Controls.Add(foot);
            this.Controls.Add(pnlSidebar);
        }

        void SbSt(Graphics g, int x, int y, string l, string v, Color c)
        {
            using (var f = new Font("Consolas", 7f)) using (var br = new SolidBrush(TXT3)) g.DrawString(l, f, br, x, y);
            using (var f = new Font("Consolas", 8.5f, FontStyle.Bold)) using (var br = new SolidBrush(c)) g.DrawString(v, f, br, x, y + 12);
        }

        void NavClick(string nav)
        {
            selNav = nav;
            foreach (Control c in pnlSidebar.Controls)
            {
                if (!(c is Panel p) || !(p.Tag is string t)) continue;
                bool act = t == nav;
                p.BackColor = act ? Color.FromArgb(22, 255, 195, 50) : Color.Transparent; p.Invalidate();
                foreach (Control ch in p.Controls)
                {
                    if (!(ch is Label l) || l.Text == "EA") continue;
                    bool mine = l.Tag is string lt && (lt == nav + "_i" || lt == nav + "_t");
                    l.ForeColor = mine ? GOLD : TXT2;
                }
            }
        }

        // ═════════════════════════════════════════════════════════
        //  MAIN AREA  —  TableLayoutPanel (garantili layout)
        // ═════════════════════════════════════════════════════════
        void MakeMainArea()
        {
            // Ana kök: 2 satır × 3 sütun
            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 3,
                BackColor = BG,
                Padding = new Padding(8),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
            };
            // Sütun genişlikleri: Input=262 | Result=268 | Connect=Fill
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 262));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 268));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            // Satır yükseklikleri: Üst=%56 | Alt=%44
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 56));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 44));

            // ── Satır 0 ───────────────────────────────────────────
            // [0,0] Input
            pnlInputCard = MakeCard();
            pnlInputCard.Paint += (s, e) => CardHdr(e.Graphics, pnlInputCard, "Trade Parameters", GOLD);
            BuildInputPanel(pnlInputCard);
            tbl.Controls.Add(pnlInputCard, 0, 0);

            // [1,0] Result
            pnlResultCard = MakeCard();
            pnlResultCard.Paint += Result_Paint;
            tbl.Controls.Add(pnlResultCard, 1, 0);

            // [2,0] Connect
            pnlConnectCard = MakeCard();
            pnlConnectCard.Paint += (s, e) => CardHdr(e.Graphics, pnlConnectCard, "MT4 / MT5 Connection", BLUE);
            BuildConnectPanel(pnlConnectCard);
            tbl.Controls.Add(pnlConnectCard, 2, 0);

            // ── Satır 1 ───────────────────────────────────────────
            // [0,1] Risk Gauge
            pnlRiskCard = MakeCard();
            pnlRiskCard.Paint += Risk_Paint;
            tbl.Controls.Add(pnlRiskCard, 0, 1);

            // [1-2,1] History — 2 sütun span
            pnlHistoryCard = MakeCard();
            pnlHistoryCard.Paint += (s, e) => CardHdr(e.Graphics, pnlHistoryCard, "Trade History", PURP);
            BuildHistoryLV(pnlHistoryCard);
            tbl.Controls.Add(pnlHistoryCard, 1, 1);
            tbl.SetColumnSpan(pnlHistoryCard, 2);

            this.Controls.Add(tbl);
        }

        Panel MakeCard()
        {
            return new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CARD,
                Margin = new Padding(4),
            };
        }

        // ── Input Panel ───────────────────────────────────────────
        void BuildInputPanel(Panel p)
        {
            int y = 34;

            p.Controls.Add(Lbl("Symbol", 10, y)); y += 15;
            cmbPair = new ComboBox { Location = new Point(10, y), Size = new Size(240, 22), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ELEV, ForeColor = TXT, FlatStyle = FlatStyle.Flat, Font = new Font("Consolas", 9.5f, FontStyle.Bold) };
            cmbPair.Items.AddRange(new object[] { "EURUSD", "GBPUSD", "USDJPY", "XAUUSD", "AUDUSD", "USDCHF", "USDCAD", "NZDUSD" });
            cmbPair.SelectedIndex = 0;
            cmbPair.SelectedIndexChanged += (s, e) => { selPair = cmbPair.SelectedItem.ToString(); UpdateDefaults(); Calculate(); };
            p.Controls.Add(cmbPair); y += 28;

            p.Controls.Add(Lbl("Direction", 10, y)); y += 15;
            btnBuy = DirBtn("▲  BUY", 118, LIME, true);
            btnSell = DirBtn("▼  SELL", 118, RED, false);
            btnBuy.Location = new Point(10, y); btnSell.Location = new Point(132, y);
            btnBuy.Click += (s, e) => SetDir("BUY");
            btnSell.Click += (s, e) => SetDir("SELL");
            p.Controls.AddRange(new Control[] { btnBuy, btnSell }); y += 32;

            p.Controls.Add(Lbl("Account Balance ($)", 10, y)); y += 15;
            txtBalance = Inp("10000", 10, y, 240); txtBalance.TextChanged += (s, e) => { if (double.TryParse(txtBalance.Text, out double v)) { balance = v; pnlTopBar?.Invalidate(); Calculate(); } };
            p.Controls.Add(txtBalance); y += 28;

            p.Controls.Add(Lbl("Risk %", 10, y)); y += 15;
            txtRisk = Inp("1.0", 10, y, 100); txtRisk.TextChanged += (s, e) => { if (double.TryParse(txtRisk.Text, out double v)) { riskPct = Math.Min(v, 10); trkRisk.Value = (int)(riskPct * 10); pnlTopBar?.Invalidate(); Calculate(); } };
            p.Controls.Add(txtRisk);
            trkRisk = new TrackBar { Location = new Point(118, y), Size = new Size(118, 26), Minimum = 1, Maximum = 100, Value = 10, TickFrequency = 10, BackColor = CARD };
            trkRisk.Scroll += (s, e) => { riskPct = trkRisk.Value / 10.0; txtRisk.Text = riskPct.ToString("F1"); Calculate(); };
            lblRiskTrk = new Label { Location = new Point(240, y + 5), Size = new Size(34, 16), ForeColor = GOLD, Font = new Font("Consolas", 8.5f, FontStyle.Bold), BackColor = Color.Transparent, Text = "1.0%" };
            p.Controls.AddRange(new Control[] { trkRisk, lblRiskTrk }); y += 30;

            p.Controls.Add(Lbl("Entry Price", 10, y)); y += 15;
            txtEntry = Inp("1.08350", 10, y, 240); txtEntry.TextChanged += (s, e) => { if (double.TryParse(txtEntry.Text, out double v)) { entryPrice = v; Calculate(); } };
            p.Controls.Add(txtEntry); y += 28;

            p.Controls.Add(Lbl("Stop Loss", 10, y)); y += 15;
            txtSL = Inp("1.08100", 10, y, 240); txtSL.TextChanged += (s, e) => { if (double.TryParse(txtSL.Text, out double v)) { slPrice = v; Calculate(); } };
            p.Controls.Add(txtSL); y += 28;

            p.Controls.Add(Lbl("Take Profit", 10, y)); y += 15;
            txtTP = Inp("1.08850", 10, y, 240); txtTP.TextChanged += (s, e) => { if (double.TryParse(txtTP.Text, out double v)) { tpPrice = v; Calculate(); } };
            p.Controls.Add(txtTP); y += 28;

            var btnLive = Btn("↻  Use Live Price", 10, y, 240, GOLD);
            btnLive.Click += (s, e) => { entryPrice = livePrice; txtEntry.Text = livePrice.ToString("F5"); Calculate(); };
            p.Controls.Add(btnLive); y += 30;

            btnCalc = Btn("⟳  CALCULATE", 10, y, 240, GOLD);
            btnCalc.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnCalc.Click += (s, e) => { Calculate(); FlashCard(pnlResultCard); };
            p.Controls.Add(btnCalc);
        }

        Label Lbl(string t, int x, int y) => new Label { Text = t, Location = new Point(x, y), Size = new Size(240, 14), ForeColor = TXT2, Font = new Font("Segoe UI", 8f), BackColor = Color.Transparent };
        TextBox Inp(string v, int x, int y, int w) => new TextBox { Text = v, Location = new Point(x, y), Size = new Size(w, 22), BackColor = ELEV, ForeColor = TXT, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Consolas", 10f, FontStyle.Bold) };

        Button DirBtn(string txt, int w, Color col, bool act)
        {
            var b = new Button { Text = txt, Size = new Size(w, 28), FlatStyle = FlatStyle.Flat, BackColor = act ? Color.FromArgb(35, col.R, col.G, col.B) : Color.Transparent, ForeColor = act ? col : TXT2, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderColor = act ? Color.FromArgb(90, col.R, col.G, col.B) : Color.FromArgb(18, 255, 255, 255);
            b.FlatAppearance.BorderSize = 1;
            return b;
        }

        Button Btn(string txt, int x, int y, int w, Color col)
        {
            var b = new Button { Text = txt, Location = new Point(x, y), Size = new Size(w, 28), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(22, col.R, col.G, col.B), ForeColor = col, Font = new Font("Segoe UI", 9f, FontStyle.Bold), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderColor = Color.FromArgb(70, col.R, col.G, col.B);
            b.FlatAppearance.BorderSize = 1;
            b.MouseEnter += (s, e) => b.BackColor = Color.FromArgb(40, col.R, col.G, col.B);
            b.MouseLeave += (s, e) => b.BackColor = Color.FromArgb(22, col.R, col.G, col.B);
            return b;
        }

        void SetDir(string dir)
        {
            tradeDir = dir;
            Color bc = dir == "BUY" ? LIME : RED; Color oc = dir == "BUY" ? RED : LIME;
            btnBuy.BackColor = dir == "BUY" ? Color.FromArgb(35, LIME.R, LIME.G, LIME.B) : Color.Transparent;
            btnBuy.ForeColor = dir == "BUY" ? LIME : TXT2;
            btnBuy.FlatAppearance.BorderColor = dir == "BUY" ? Color.FromArgb(90, LIME.R, LIME.G, LIME.B) : Color.FromArgb(18, 255, 255, 255);
            btnSell.BackColor = dir == "SELL" ? Color.FromArgb(35, RED.R, RED.G, RED.B) : Color.Transparent;
            btnSell.ForeColor = dir == "SELL" ? RED : TXT2;
            btnSell.FlatAppearance.BorderColor = dir == "SELL" ? Color.FromArgb(90, RED.R, RED.G, RED.B) : Color.FromArgb(18, 255, 255, 255);
            Calculate();
        }

        // ── Result Panel ──────────────────────────────────────────
        void Result_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            int W = pnlResultCard.Width, H = pnlResultCard.Height;
            CardHdr(g, pnlResultCard, "Calculation Result", TEAL);

            Color dc = tradeDir == "BUY" ? LIME : RED;
            var lr = new Rectangle(10, 32, W - 20, 58);
            using (var br = new SolidBrush(Color.FromArgb(16, dc.R, dc.G, dc.B))) g.FillRectangle(br, lr);
            using (var pen = new Pen(Color.FromArgb(40, dc.R, dc.G, dc.B))) g.DrawRectangle(pen, lr);
            using (var f = new Font("Consolas", 7f)) using (var br = new SolidBrush(TXT3)) g.DrawString("LOT SIZE", f, br, 16, 38);
            using (var f = new Font("Consolas", 26f, FontStyle.Bold)) using (var br = new SolidBrush(dc)) g.DrawString(lotSize.ToString("F2"), f, br, 12, 48);
            using (var f = new Font("Segoe UI", 9f)) using (var br = new SolidBrush(TXT2)) g.DrawString("lots", f, br, W - 46, 72);

            int ry = 100;
            Row(g, W, ry, "Risk Amount", "$" + riskAmt.ToString("N2"), RED);
            Row(g, W, ry + 22, "Reward Amount", "$" + rewardAmt.ToString("N2"), LIME);
            Row(g, W, ry + 44, "Risk / Reward", "1 : " + rrRatio.ToString("F2"), rrRatio >= 2 ? LIME : rrRatio >= 1 ? GOLD : RED);
            Row(g, W, ry + 66, "Stop Pips", PipStr() + " pips", RED);
            Row(g, W, ry + 88, "Pip Value", "$" + pipVal.ToString("F2"), TXT);
            Row(g, W, ry + 110, "Est. Margin", "$" + (lotSize * 1000).ToString("N2"), PURP);
            Row(g, W, ry + 132, "Direction", tradeDir, dc);

            // R:R bar
            int bY = ry + 158;
            using (var pen = new Pen(BDR)) g.DrawLine(pen, 10, bY, W - 10, bY);
            using (var f = new Font("Consolas", 7f)) using (var br = new SolidBrush(TXT3)) g.DrawString("RISK  vs  REWARD", f, br, 10, bY + 4);
            int bw = W - 20, bh = bY + 18;
            using (var br = new SolidBrush(ELEV)) g.FillRectangle(br, 10, bh, bw, 11);
            double tot = riskAmt + rewardAmt;
            if (tot > 0)
            {
                int rw = (int)(bw * riskAmt / tot); int rew = bw - rw;
                using (var br = new SolidBrush(Color.FromArgb(150, RED.R, RED.G, RED.B))) g.FillRectangle(br, 10, bh, rw, 11);
                using (var br = new SolidBrush(Color.FromArgb(150, LIME.R, LIME.G, LIME.B))) g.FillRectangle(br, 10 + rw, bh, rew, 11);
                using (var f = new Font("Consolas", 7f))
                {
                    using (var br = new SolidBrush(Color.White)) g.DrawString("RISK", f, br, 14, bh + 1);
                    if (rew > 36) using (var br = new SolidBrush(Color.White)) g.DrawString("REWARD", f, br, 14 + rw, bh + 1);
                }
            }
        }

        void Row(Graphics g, int W, int y, string lbl, string val, Color col)
        {
            using (var pen = new Pen(Color.FromArgb(8, 255, 255, 255))) g.DrawLine(pen, 10, y + 18, W - 10, y + 18);
            using (var f = new Font("Segoe UI", 8.5f)) using (var br = new SolidBrush(TXT2)) g.DrawString(lbl, f, br, 10, y);
            using (var f = new Font("Consolas", 9.5f, FontStyle.Bold)) using (var br = new SolidBrush(col))
            { var sf = new StringFormat { Alignment = StringAlignment.Far }; g.DrawString(val, f, br, new RectangleF(0, y, W - 10, 18), sf); }
        }

        string PipStr()
        {
            double d = Math.Abs(entryPrice - slPrice);
            if (selPair == "USDJPY" || selPair == "XAUUSD") return (d * 10).ToString("F0");
            return (d * 10000).ToString("F0");
        }

        // ── Connect Panel ─────────────────────────────────────────
        void BuildConnectPanel(Panel p)
        {
            int y = 34;
            p.Controls.Add(Lbl("Platform", 10, y));
            p.Controls.Add(Lbl("Server", 150, y)); y += 15;

            cmbPlatform = new ComboBox { Location = new Point(10, y), Size = new Size(130, 22), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ELEV, ForeColor = TXT, FlatStyle = FlatStyle.Flat, Font = new Font("Consolas", 9.5f, FontStyle.Bold) };
            cmbPlatform.Items.AddRange(new object[] { "MT5", "MT4" });
            cmbPlatform.SelectedIndex = 0;
            cmbPlatform.SelectedIndexChanged += (s, e) => { platform = cmbPlatform.SelectedItem.ToString(); pnlTopBar?.Invalidate(); pnlStatus?.Invalidate(); };

            cmbServer = new ComboBox { Location = new Point(150, y), Size = new Size(210, 22), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ELEV, ForeColor = TXT, FlatStyle = FlatStyle.Flat, Font = new Font("Consolas", 8.5f) };
            cmbServer.Items.AddRange(new object[] { "MetaQuotes-Demo", "ICMarkets-Demo", "Pepperstone-Demo", "XM-Demo", "Exness-Demo", "FXCM-Demo", "IG-Demo" });
            cmbServer.SelectedIndex = 0;
            p.Controls.AddRange(new Control[] { cmbPlatform, cmbServer }); y += 28;

            p.Controls.Add(Lbl("Login", 10, y));
            p.Controls.Add(Lbl("Password", 150, y)); y += 15;

            txtLogin = Inp("12345678", "", 10, y, 130);
            txtPassword = Inp("", "●", 150, y, 210, true);
            p.Controls.AddRange(new Control[] { txtLogin, txtPassword }); y += 32;

            btnConnect = Btn("▶  CONNECT TO " + platform, 10, y, 370, BLUE);
            btnConnect.Height = 30; btnConnect.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnConnect.Click += ToggleConnect;
            p.Controls.Add(btnConnect); y += 38;

            lblConnState = new Label { Text = "○  Not connected — enter credentials above", Location = new Point(10, y), Size = new Size(370, 16), ForeColor = TXT2, Font = new Font("Consolas", 8f), BackColor = Color.Transparent };
            p.Controls.Add(lblConnState); y += 28;

            btnTrade = new Button { Text = "▶  OPEN TRADE", Location = new Point(10, y), Size = new Size(370, 36), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(18, 50, 215, 120), ForeColor = TXT2, Font = new Font("Segoe UI", 11f, FontStyle.Bold), Cursor = Cursors.Hand, Enabled = false };
            btnTrade.FlatAppearance.BorderColor = Color.FromArgb(40, LIME.R, LIME.G, LIME.B);
            btnTrade.FlatAppearance.BorderSize = 1;
            btnTrade.Click += OpenTrade;
            p.Controls.Add(btnTrade); y += 44;

            lblPlatLbl = new Label { Text = "", Location = new Point(10, y), Size = new Size(370, 14), ForeColor = TXT3, Font = new Font("Consolas", 7.5f), BackColor = Color.Transparent };
            p.Controls.Add(lblPlatLbl); y += 22;

            // Quick info box
            p.Controls.Add(new Label { Text = "Quick Summary", Location = new Point(10, y), Size = new Size(370, 14), ForeColor = TXT3, Font = new Font("Consolas", 7.5f, FontStyle.Bold), BackColor = Color.Transparent });
            y += 16;
            var info = new Panel { Location = new Point(10, y), Size = new Size(370, 88), BackColor = ELEV };
            info.Paint += InfoBox_Paint;
            p.Controls.Add(info);
        }

        TextBox Inp(string v, string pw, int x, int y, int w, bool isPass = false)
        {
            var t = new TextBox { Text = v, Location = new Point(x, y), Size = new Size(w, 22), BackColor = ELEV, ForeColor = isPass ? TXT2 : TXT, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Consolas", 10f, FontStyle.Bold) };
            if (pw != "") t.PasswordChar = pw[0];
            return t;
        }

        void InfoBox_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            var p = (Panel)sender; int W = p.Width;
            using (var pen = new Pen(BDR)) g.DrawRectangle(pen, 0, 0, W - 1, p.Height - 1);
            var rows = new[]{
                ($"{selPair}", livePrice.ToString("F5"),  GOLD),
                ("Lot Size",   lotSize.ToString("F2")+" lots", tradeDir=="BUY"?LIME:RED),
                ("Risk $",     "$"+riskAmt.ToString("N2"),  RED),
                ("R:R",        "1 : "+rrRatio.ToString("F2"), rrRatio>=2?LIME:GOLD),
                ("Platform",   platform,                    BLUE),
            };
            int ry = 6;
            foreach (var (l, v, c) in rows)
            {
                using (var f = new Font("Segoe UI", 8.5f)) using (var br = new SolidBrush(TXT2)) g.DrawString(l, f, br, 8, ry);
                using (var f = new Font("Consolas", 9f, FontStyle.Bold)) using (var br = new SolidBrush(c))
                { var sf = new StringFormat { Alignment = StringAlignment.Far }; g.DrawString(v, f, br, new RectangleF(0, ry, W - 8, 16), sf); }
                ry += 16;
            }
        }

        // ── Risk Gauge ────────────────────────────────────────────
        void Risk_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            int W = pnlRiskCard.Width, H = pnlRiskCard.Height;
            CardHdr(g, pnlRiskCard, "Risk Meter", PURP);

            double pct = Math.Min(riskPct / 5.0, 1.0);
            Color mc = riskPct <= 1.5 ? LIME : riskPct <= 3 ? GOLD : RED;
            int gX = W / 2, gY = 90, gR = Math.Min(W / 2 - 18, 62);
            var rc = new Rectangle(gX - gR, gY - gR, gR * 2, gR * 2);

            using (var pen = new Pen(ELEV, 14)) g.DrawArc(pen, rc, 180, 180);
            using (var pen = new Pen(Color.FromArgb(40, RED.R, RED.G, RED.B), 14)) g.DrawArc(pen, rc, 144, 36);
            using (var pen = new Pen(mc, 12)) g.DrawArc(pen, rc, 180, (float)(180 * pct));

            double ang = Math.PI + Math.PI * pct;
            float nx = gX + (float)(Math.Cos(ang) * (gR - 10)), ny = gY + (float)(Math.Sin(ang) * (gR - 10));
            using (var pen = new Pen(TXT, 2)) g.DrawLine(pen, gX, gY, nx, ny);
            using (var br = new SolidBrush(TXT)) g.FillEllipse(br, gX - 4, gY - 4, 8, 8);

            var sfC = new StringFormat { Alignment = StringAlignment.Center };
            using (var f = new Font("Consolas", 20f, FontStyle.Bold)) using (var br = new SolidBrush(mc))
                g.DrawString($"{riskPct:F1}%", f, br, new RectangleF(0, gY + 4, W, 28), sfC);
            using (var f = new Font("Segoe UI", 8f)) using (var br = new SolidBrush(TXT3))
                g.DrawString(riskPct <= 1.5 ? "LOW RISK" : riskPct <= 3 ? "MODERATE" : "HIGH RISK", f, br, new RectangleF(0, gY + 30, W, 16), sfC);

            using (var f = new Font("Consolas", 7.5f))
            {
                using (var br = new SolidBrush(LIME)) g.DrawString("0%", f, br, gX - gR - 4, gY + 8);
                using (var br = new SolidBrush(RED)) g.DrawString("5%", f, br, gX + gR - 14, gY + 8);
                using (var br = new SolidBrush(GOLD)) g.DrawString("2.5%", f, br, gX - 12, gY - gR - 14);
            }

            int iy = gY + gR + 16;
            if (iy + 92 < H)
            {
                MBox(g, 8, iy, W / 2 - 12, "Risk $", "$" + riskAmt.ToString("N2"), RED);
                MBox(g, W / 2 + 4, iy, W / 2 - 12, "Reward $", "$" + rewardAmt.ToString("N2"), LIME);
                MBox(g, 8, iy + 46, W / 2 - 12, "Lot", lotSize.ToString("F2") + " lot", GOLD);
                MBox(g, W / 2 + 4, iy + 46, W / 2 - 12, "Margin", "$" + (lotSize * 1000).ToString("N0"), PURP);
            }
        }

        void MBox(Graphics g, int x, int y, int w, string l, string v, Color c)
        {
            using (var br = new SolidBrush(Color.FromArgb(14, 255, 255, 255))) g.FillRectangle(br, x, y, w, 40);
            using (var pen = new Pen(BDR)) g.DrawRectangle(pen, x, y, w - 1, 39);
            using (var f = new Font("Consolas", 7f)) using (var br = new SolidBrush(TXT3)) g.DrawString(l, f, br, x + 5, y + 4);
            using (var f = new Font("Consolas", 8.5f, FontStyle.Bold)) using (var br = new SolidBrush(c)) g.DrawString(v, f, br, x + 5, y + 18);
        }

        // ── History ListView ──────────────────────────────────────
        void BuildHistoryLV(Panel p)
        {
            lvHistory = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,
                BackColor = CARD,
                ForeColor = TXT,
                BorderStyle = BorderStyle.None,
                Font = new Font("Consolas", 8.5f),
                OwnerDraw = true
            };
            lvHistory.Columns.Add("Time", 54);
            lvHistory.Columns.Add("Pair", 70);
            lvHistory.Columns.Add("Dir", 46);
            lvHistory.Columns.Add("Entry", 82);
            lvHistory.Columns.Add("SL", 82);
            lvHistory.Columns.Add("TP", 82);
            lvHistory.Columns.Add("Lot", 50);
            lvHistory.Columns.Add("R:R", 52);
            lvHistory.Columns.Add("P&L", 82);

            foreach (var t in history) AppendHist(t);

            lvHistory.DrawColumnHeader += (s, e) => {
                e.Graphics.FillRectangle(new SolidBrush(SURF), e.Bounds);
                using (var pen = new Pen(BDR)) e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
                using (var f = new Font("Consolas", 7.5f, FontStyle.Bold)) using (var br = new SolidBrush(TXT3))
                    e.Graphics.DrawString(e.Header.Text.ToUpper(), f, br, e.Bounds.Left + 5, e.Bounds.Top + 5);
            };
            lvHistory.DrawItem += (s, e) => e.DrawBackground();
            lvHistory.DrawSubItem += (s, e) => {
                if (!(e.Item.Tag is TradeRec t)) return;
                var g = e.Graphics; var rc = e.Bounds;
                if (e.Item.Index % 2 == 0) using (var br = new SolidBrush(Color.FromArgb(8, 255, 255, 255))) g.FillRectangle(br, rc);
                Color fg = TXT;
                if (e.ColumnIndex == 0) fg = TXT2;
                if (e.ColumnIndex == 1) fg = GOLD;
                if (e.ColumnIndex == 2) fg = t.Dir == "BUY" ? LIME : RED;
                if (e.ColumnIndex == 8) fg = t.Win ? LIME : RED;
                using (var f = new Font("Consolas", 8.5f)) using (var br = new SolidBrush(fg)) g.DrawString(e.SubItem.Text, f, br, rc.X + 5, rc.Y + 4);
                using (var pen = new Pen(Color.FromArgb(7, 255, 255, 255))) g.DrawLine(pen, rc.Left, rc.Bottom - 1, rc.Right, rc.Bottom - 1);
            };

            var wrap = new Panel { Dock = DockStyle.Fill, BackColor = CARD, Padding = new Padding(0, 28, 0, 0) };
            wrap.Controls.Add(lvHistory);
            p.Controls.Add(wrap);
        }

        void AppendHist(TradeRec t)
        {
            var it = new ListViewItem(t.Time) { Tag = t };
            it.SubItems.Add(t.Pair); it.SubItems.Add(t.Dir); it.SubItems.Add(t.Entry);
            it.SubItems.Add(t.SL); it.SubItems.Add(t.TP); it.SubItems.Add(t.Lot); it.SubItems.Add(t.RR); it.SubItems.Add(t.PnL);
            lvHistory?.Items.Insert(0, it);
            if (lvHistory?.Items.Count > 100) lvHistory.Items.RemoveAt(lvHistory.Items.Count - 1);
        }

        // ── Card Hdr ──────────────────────────────────────────────
        void CardHdr(Graphics g, Panel p, string title, Color acc)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var pen = new Pen(Color.FromArgb(22, 255, 255, 255))) g.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
            if (p.Width > 2) using (var br = new LinearGradientBrush(new Point(0, 0), new Point(Math.Max(p.Width / 2, 1), 0), Color.FromArgb(55, acc.R, acc.G, acc.B), Color.Transparent)) g.FillRectangle(br, 0, 0, p.Width / 2, 3);
            using (var br = new SolidBrush(Color.FromArgb(14, 255, 255, 255))) g.FillRectangle(br, 0, 3, p.Width, 23);
            using (var pen = new Pen(BDR)) g.DrawLine(pen, 0, 26, p.Width, 26);
            using (var br = new SolidBrush(acc)) g.FillEllipse(br, 8, 11, 4, 4);
            using (var f = new Font("Consolas", 7.5f, FontStyle.Bold)) using (var br = new SolidBrush(TXT3)) g.DrawString(title.ToUpper(), f, br, 16, 8);
        }

        Panel WinBtn(Color col)
        {
            var p = new Panel { Size = new Size(12, 12), BackColor = col, Cursor = Cursors.Hand };
            p.Paint += (s, e) => { e.Graphics.SmoothingMode = SmoothingMode.AntiAlias; using (var br = new SolidBrush(p.BackColor)) e.Graphics.FillEllipse(br, 0, 0, 11, 11); };
            p.MouseEnter += (s, e) => p.BackColor = ControlPaint.Light(col, .3f);
            p.MouseLeave += (s, e) => p.BackColor = col;
            return p;
        }

        async void FlashCard(Panel p)
        {
            if (p == null) return;
            Color old = p.BackColor;
            p.BackColor = Color.FromArgb(28, 255, 195, 50);
            await System.Threading.Tasks.Task.Delay(200);
            p.BackColor = old;
        }

        // ── Connect / Trade ───────────────────────────────────────
        async void ToggleConnect(object s, EventArgs e)
        {
            if (!connected)
            {
                btnConnect.Text = "◌  Connecting..."; btnConnect.Enabled = false;
                await System.Threading.Tasks.Task.Delay(1800);
                connected = true;
                string srv = cmbServer?.SelectedItem?.ToString() ?? "MetaQuotes-Demo";
                btnConnect.Text = "⏹  DISCONNECT";
                btnConnect.ForeColor = RED; btnConnect.BackColor = Color.FromArgb(20, 255, 65, 90); btnConnect.FlatAppearance.BorderColor = Color.FromArgb(70, RED.R, RED.G, RED.B);
                btnConnect.Enabled = true;
                lblConnState.Text = $"● Connected  ·  {platform}  ·  {srv}"; lblConnState.ForeColor = LIME;
                lblStatusTxt.Text = $"● Connected · {platform}"; lblStatusTxt.ForeColor = LIME;
                btnTrade.Enabled = true; btnTrade.BackColor = Color.FromArgb(35, 50, 215, 120); btnTrade.ForeColor = LIME;
                btnTrade.Text = $"▶  OPEN {tradeDir}  —  {lotSize:F2} Lot";
                lblPlatLbl.Text = $"Ready · {platform} · {selPair} · Lot: {lotSize:F2}"; lblPlatLbl.ForeColor = TEAL;
            }
            else
            {
                connected = false;
                btnConnect.Text = "▶  CONNECT TO " + platform;
                btnConnect.ForeColor = BLUE; btnConnect.BackColor = Color.FromArgb(22, 75, 160, 255); btnConnect.FlatAppearance.BorderColor = Color.FromArgb(70, BLUE.R, BLUE.G, BLUE.B);
                lblConnState.Text = "○  Not connected"; lblConnState.ForeColor = TXT2;
                lblStatusTxt.Text = "○ Disconnected"; lblStatusTxt.ForeColor = TXT2;
                btnTrade.Enabled = false; btnTrade.BackColor = Color.FromArgb(18, 50, 215, 120); btnTrade.ForeColor = TXT2;
                btnTrade.Text = "▶  OPEN TRADE"; lblPlatLbl.Text = "";
            }
            pnlTopBar?.Invalidate(); pnlSidebar?.Invalidate(true);
            pnlResultCard?.Invalidate(); pnlStatus?.Invalidate();
        }

        async void OpenTrade(object s, EventArgs e)
        {
            if (!connected) { MessageBox.Show("Not connected!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            btnTrade.Text = "◌  Sending..."; btnTrade.Enabled = false;
            await System.Threading.Tasks.Task.Delay(1400);
            bool win = RNG.NextDouble() > .38;
            double pnl = win ? rewardAmt : -riskAmt;
            balance += pnl;
            var rec = new TradeRec { Time = DateTime.Now.ToString("HH:mm:ss"), Pair = selPair, Dir = tradeDir, Entry = entryPrice.ToString("F5"), SL = slPrice.ToString("F5"), TP = tpPrice.ToString("F5"), Lot = lotSize.ToString("F2"), RR = "1:" + rrRatio.ToString("F2"), PnL = (pnl >= 0 ? "+" : "") + $"${pnl:N2}", Win = win };
            history.Insert(0, rec); AppendHist(rec);
            lblStatusTxt.Text = $"● {tradeDir} {selPair} {lotSize:F2}L — {(win ? "WIN" : "LOSS")} {rec.PnL}"; lblStatusTxt.ForeColor = win ? LIME : RED;
            btnTrade.Text = "✓  Order Sent!";
            await System.Threading.Tasks.Task.Delay(2000);
            btnTrade.Text = $"▶  OPEN {tradeDir}  —  {lotSize:F2} Lot"; btnTrade.Enabled = true;
            pnlRiskCard?.Invalidate(); pnlSidebar?.Invalidate(true); pnlStatus?.Invalidate(); pnlTopBar?.Invalidate();
            if (lblPlatLbl != null) { lblPlatLbl.Text = $"Last: {tradeDir} {lotSize:F2}L · {rec.PnL}"; lblPlatLbl.ForeColor = win ? LIME : RED; }
        }

        // ── Calculate ─────────────────────────────────────────────
        void Calculate()
        {
            double slPips = Math.Abs(entryPrice - slPrice);
            double tpPips = Math.Abs(tpPrice - entryPrice);
            if (selPair == "XAUUSD") { pipVal = 1.0; slPips *= 10; tpPips *= 10; }
            else if (selPair == "USDJPY") { pipVal = 9.26; slPips *= 100; tpPips *= 100; }
            else { pipVal = 10.0; slPips *= 10000; tpPips *= 10000; }

            riskAmt = balance * riskPct / 100.0;
            lotSize = slPips > 0 ? Math.Round(riskAmt / (slPips * pipVal), 2) : 0;
            lotSize = Math.Max(lotSize, 0);
            rewardAmt = lotSize * tpPips * pipVal;
            rrRatio = riskAmt > 0 ? Math.Round(rewardAmt / riskAmt, 2) : 0;

            if (lblRiskTrk != null) lblRiskTrk.Text = riskPct.ToString("F1") + "%";
            if (connected && btnTrade != null) btnTrade.Text = $"▶  OPEN {tradeDir}  —  {lotSize:F2} Lot";
            if (connected && lblPlatLbl != null) lblPlatLbl.Text = $"Ready · {platform} · {selPair} · Lot: {lotSize:F2}";

            pnlResultCard?.Invalidate(); pnlRiskCard?.Invalidate();
            pnlSidebar?.Invalidate(true); pnlStatus?.Invalidate(); pnlTopBar?.Invalidate();
            pnlConnectCard?.Invalidate();
        }

        void UpdateDefaults()
        {
            switch (selPair)
            {
                case "GBPUSD": entryPrice = 1.27350; slPrice = 1.27050; tpPrice = 1.27950; livePrice = 1.27350; break;
                case "USDJPY": entryPrice = 149.50; slPrice = 149.00; tpPrice = 150.50; livePrice = 149.50; break;
                case "XAUUSD": entryPrice = 2341.50; slPrice = 2335.00; tpPrice = 2354.00; livePrice = 2341.50; break;
                case "AUDUSD": entryPrice = 0.65420; slPrice = 0.65150; tpPrice = 0.65960; livePrice = 0.65420; break;
                default: entryPrice = 1.08350; slPrice = 1.08100; tpPrice = 1.08850; livePrice = 1.08350; break;
            }
            if (txtEntry != null) txtEntry.Text = entryPrice.ToString("F5");
            if (txtSL != null) txtSL.Text = slPrice.ToString("F5");
            if (txtTP != null) txtTP.Text = tpPrice.ToString("F5");
        }

        // ── Ticker ────────────────────────────────────────────────
        void StartTicker()
        {
            ticker = new System.Windows.Forms.Timer { Interval = 1700 };
            ticker.Tick += (s, e) => {
                livePrice += (RNG.NextDouble() - .49) * .00008;
                double chg = livePrice - entryPrice;
                if (lblPrice != null) lblPrice.Text = livePrice.ToString(selPair == "USDJPY" ? "F3" : selPair == "XAUUSD" ? "F2" : "F5");
                if (lblPriceChg != null) { lblPriceChg.Text = (chg >= 0 ? "▲ +" : "▼ ") + chg.ToString("F5"); lblPriceChg.ForeColor = chg >= 0 ? LIME : RED; }
                if (lblClock != null) lblClock.Text = DateTime.Now.ToString("HH:mm:ss  ·  dd.MM.yyyy");
                pnlSidebar?.Invalidate(true); pnlStatus?.Invalidate(); pnlConnectCard?.Invalidate();
            };
            ticker.Start();
        }

        protected override void OnFormClosed(FormClosedEventArgs e) { ticker?.Stop(); base.OnFormClosed(e); }
    }

    static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool ReleaseCapture();
        [System.Runtime.InteropServices.DllImport("user32.dll")] public static extern IntPtr SendMessage(IntPtr h, int m, IntPtr w, IntPtr l);
    }
}
