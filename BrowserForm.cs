using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace InternetPro;

internal sealed class BrowserForm : Form
{
    private static readonly string[] RobloxDomains =
    [
        "roblox.com", "rbxcdn.com", "robloxusercontent.com", "robloxapis.com",
        "rblx.com", "rbxlcdn.com", "rbx.com", "roblox.cn"
    ];

    private static readonly string[] MinecraftDomains =
    [
        "minecraft.net", "minecraftservices.com", "minecraft-services.net",
        "mojang.com", "mojang.net", "minecraft.wiki", "minecraftforum.net",
        "minecraftservers.org", "minecraftservers.net", "minecraft-mp.com", "planetminecraft.com"
    ];

    private static readonly string[] TikTokDomains =
    [
        "tiktok.com", "tiktokv.com", "tiktokcdn.com", "tiktokcdn-us.com",
        "tiktokapis.com", "tiktokstatic.com", "ttwstatic.com", "byteoversea.com",
        "ibytedtos.com", "bytefcdn.com", "bytefcdn-oversea.com", "bytefcdn-ttpeu.com",
        "tiktokshop.com", "tiktokglobalshop.com", "musical.ly"
    ];

    private static readonly string[] AdultDomains =
    [
        "pornhub.com", "xvideos.com", "xnxx.com", "xhamster.com", "youporn.com",
        "redtube.com", "tube8.com", "spankbang.com", "chaturbate.com", "onlyfans.com",
        "eporner.com", "rule34.xxx"
    ];

    private static readonly string[] AdDomains =
    [
        "doubleclick.net", "googlesyndication.com", "googleadservices.com", "adnxs.com",
        "adform.net", "taboola.com", "outbrain.com", "scorecardresearch.com",
        "serving-sys.com", "adroll.com", "criteo.com", "amazon-adsystem.com"
    ];

    private static readonly BrowserTheme[] AvailableThemes =
    [
        new("Internet Pro clasic", ThemeStyle.Classic, Color.FromArgb(28, 105, 171), Color.FromArgb(119, 181, 225), Color.FromArgb(218, 232, 243), Color.FromArgb(35, 99, 151)),
        new("Windows 11", ThemeStyle.Windows11, Color.FromArgb(26, 104, 180), Color.FromArgb(111, 181, 231), Color.FromArgb(235, 242, 250), Color.FromArgb(22, 92, 164)),
        new("Windows 10", ThemeStyle.Windows10, Color.FromArgb(0, 94, 166), Color.FromArgb(0, 120, 212), Color.FromArgb(239, 243, 247), Color.FromArgb(0, 103, 184)),
        new("Windows 8", ThemeStyle.Windows8, Color.FromArgb(0, 126, 128), Color.FromArgb(0, 177, 170), Color.FromArgb(231, 245, 244), Color.FromArgb(0, 132, 134)),
        new("Windows 7", ThemeStyle.Windows7, Color.FromArgb(27, 89, 147), Color.FromArgb(138, 202, 230), Color.FromArgb(231, 242, 250), Color.FromArgb(40, 107, 159)),
        new("Windows Vista", ThemeStyle.Vista, Color.FromArgb(28, 89, 111), Color.FromArgb(137, 202, 207), Color.FromArgb(231, 243, 245), Color.FromArgb(45, 119, 135)),
        new("Windows 98", ThemeStyle.Windows98, Color.FromArgb(10, 36, 106), Color.FromArgb(58, 110, 165), Color.FromArgb(192, 192, 192), Color.FromArgb(96, 96, 96)),
        new("Windows 95", ThemeStyle.Windows95, Color.FromArgb(0, 0, 128), Color.FromArgb(16, 132, 208), Color.FromArgb(192, 192, 192), Color.FromArgb(0, 0, 128)),
        new("Frătăuții Vechi", ThemeStyle.Countryside, Color.FromArgb(35, 91, 68), Color.FromArgb(150, 190, 125), Color.FromArgb(229, 239, 222), Color.FromArgb(44, 102, 73)),
        new("Maramureș", ThemeStyle.Maramures, Color.FromArgb(70, 99, 121), Color.FromArgb(164, 194, 204), Color.FromArgb(228, 237, 240), Color.FromArgb(62, 91, 112)),
        new("Bomboane colorate", ThemeStyle.Candy, Color.FromArgb(197, 56, 124), Color.FromArgb(250, 169, 79), Color.FromArgb(255, 239, 244), Color.FromArgb(174, 54, 111)),
        new("Galaxii", ThemeStyle.Galaxy, Color.FromArgb(22, 28, 64), Color.FromArgb(101, 65, 142), Color.FromArgb(232, 231, 246), Color.FromArgb(46, 39, 84)),
        new("Pisică pixelată și curcubeu", ThemeStyle.PixelRainbow, Color.FromArgb(31, 135, 180), Color.FromArgb(87, 207, 172), Color.FromArgb(230, 247, 244), Color.FromArgb(31, 112, 145))
    ];

    private enum ThemeStyle
    {
        Classic,
        Windows11,
        Windows10,
        Windows8,
        Windows7,
        Vista,
        Windows98,
        Windows95,
        Countryside,
        Maramures,
        Candy,
        Galaxy,
        PixelRainbow
    }

    private sealed record BrowserTheme(string Name, ThemeStyle Style, Color Start, Color End, Color Surface, Color Accent);

    private readonly WebView2 _webView = new() { Dock = DockStyle.Fill };
    private readonly Panel _brandPanel = new();
    private readonly FlowLayoutPanel _toolbarPanel = new();
    private readonly FlowLayoutPanel _filterPanel = new();
    private readonly MenuStrip _menuStrip;
    private readonly StatusStrip _statusBar = new();
    private readonly TextBox _address = new();
    private readonly ToolStripStatusLabel _status = new("Se inițializează motorul Edge...");
    private readonly List<(string Title, string Url)> _history = [];
    private readonly CheckBox _adsToggle;
    private readonly CheckBox _adultToggle;
    private readonly CheckBox _reputationToggle;
    private BrowserPolicy _policy = LoadPolicy();
    private bool _showingBlockedPage;

    public BrowserForm()
    {
        Text = "Internet Pro";
        Icon = CreateBrowserIcon();
        MinimumSize = new Size(900, 600);
        Size = new Size(1360, 900);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(245, 247, 248);

        var shell = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            BackColor = BackColor
        };
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(shell);

        var brand = _brandPanel;
        brand.Dock = DockStyle.Fill;
        brand.Paint += (_, e) =>
        {
            DrawThemeHeader(e.Graphics, brand.ClientRectangle, GetTheme(_policy.Theme));
        };
        var title = new Label
        {
            Text = "INTERNET PRO",
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(58, 17)
        };
        brand.Controls.Add(title);
        shell.Controls.Add(brand, 0, 0);

        _menuStrip = CreateClassicMenu();
        shell.Controls.Add(_menuStrip, 0, 1);

        var toolbar = _toolbarPanel;
        toolbar.Dock = DockStyle.Fill;
        toolbar.WrapContents = false;
        toolbar.Padding = new Padding(8, 7, 8, 5);
        var back = MakeButton("‹", "Înapoi", 38);
        back.Click += (_, _) => { if (_webView.CoreWebView2?.CanGoBack == true) _webView.CoreWebView2.GoBack(); };
        var forward = MakeButton("›", "Înainte", 38);
        forward.Click += (_, _) => { if (_webView.CoreWebView2?.CanGoForward == true) _webView.CoreWebView2.GoForward(); };
        var reload = MakeButton("↻", "Reîncarcă", 38);
        reload.Click += (_, _) => _webView.CoreWebView2?.Reload();
        var home = MakeButton("⌂", "Acasă", 38);
        home.Click += (_, _) => Navigate(_policy.HomePage);
        var bookmark = MakeButton("☆", "Salvează pagina la semne de carte", 38);
        bookmark.Click += (_, _) => AddCurrentBookmark();
        var history = MakeButton("Istoric", "Vezi paginile vizitate", 76);
        history.Click += (_, _) => ShowHistory();
        var settings = MakeButton("Setări", "Setări Internet Pro", 76);
        settings.Click += (_, _) => ShowBrowserSettings();

        _address.BorderStyle = BorderStyle.FixedSingle;
        _address.Font = new Font("Segoe UI", 10);
        _address.Width = 570;
        _address.Height = 32;
        _address.Margin = new Padding(6, 2, 4, 0);
        _address.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                Navigate(_address.Text);
                e.SuppressKeyPress = true;
            }
        };
        var go = MakeButton("Mergi", "Deschide adresa", 66);
        go.Click += (_, _) => Navigate(_address.Text);
        var clear = MakeButton("Șterge date", "Șterge istoricul, cookie-urile și memoria cache", 112);
        clear.Click += async (_, _) => await ClearBrowsingDataAsync();

        toolbar.Controls.AddRange([back, forward, reload, home, bookmark, history, _address, go, settings, clear]);
        shell.Controls.Add(toolbar, 0, 2);

        var filters = _filterPanel;
        filters.Dock = DockStyle.Fill;
        filters.WrapContents = false;
        filters.Padding = new Padding(12, 8, 8, 4);
        _adsToggle = MakeToggle("Reclame", _policy.BlockAds, "Blochează domenii publicitare cunoscute");
        var robloxLock = MakeLockedToggle("Roblox blocat", "Blocare permanentă în Internet Pro");
        var minecraftLock = MakeLockedToggle("Minecraft blocat", "Blocare permanentă în Internet Pro");
        var tiktokLock = MakeLockedToggle("TikTok blocat", "Blocare permanentă în Internet Pro");
        _adultToggle = MakeToggle("Site-uri pentru adulți", _policy.BlockAdultSites, "Blochează o listă locală de domenii cunoscute");
        _reputationToggle = MakeToggle("Verificare reputație Edge", _policy.CheckReputation, "Folosește verificarea de reputație disponibilă în Edge");
        _adsToggle.CheckedChanged += (_, _) => PersistVisibleSettings();
        _adultToggle.CheckedChanged += (_, _) => PersistVisibleSettings();
        _reputationToggle.CheckedChanged += (_, _) =>
        {
            if (_webView.CoreWebView2 is not null)
                _webView.CoreWebView2.Settings.IsReputationCheckingRequired = _reputationToggle.Checked;
            PersistVisibleSettings();
        };
        filters.Controls.AddRange([_adsToggle, robloxLock, minecraftLock, tiktokLock, _adultToggle, _reputationToggle]);
        shell.Controls.Add(filters, 0, 3);

        var content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0), BackColor = Color.White };
        content.Controls.Add(_webView);
        shell.Controls.Add(content, 0, 4);

        var statusBar = _statusBar;
        statusBar.SizingGrip = false;
        _status.ForeColor = Color.White;
        statusBar.Items.Add(_status);
        Controls.Add(statusBar);

        ApplyTheme();
        Shown += async (_, _) => await InitializeBrowserAsync();
        FormClosed += (_, _) => Icon.Dispose();
    }

    private MenuStrip CreateClassicMenu()
    {
        var menu = new MenuStrip
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(245, 248, 251),
            ForeColor = Color.FromArgb(31, 51, 67),
            Font = new Font("Segoe UI", 9),
            Padding = new Padding(8, 2, 0, 2)
        };

        var file = new ToolStripMenuItem("Fișier");
        file.DropDownItems.Add("Deschide adresă", null, (_, _) =>
        {
            _address.Focus();
            _address.SelectAll();
        });
        file.DropDownItems.Add(new ToolStripSeparator());
        file.DropDownItems.Add("Închide", null, (_, _) => Close());

        var view = new ToolStripMenuItem("Vizualizare");
        view.DropDownItems.Add("Acasă", null, (_, _) => Navigate(_policy.HomePage));
        view.DropDownItems.Add("Reîncarcă", null, (_, _) => _webView.CoreWebView2?.Reload());
        view.DropDownItems.Add("Mărește", null, (_, _) => _webView.ZoomFactor = Math.Min(2, _webView.ZoomFactor + 0.1));
        view.DropDownItems.Add("Micșorează", null, (_, _) => _webView.ZoomFactor = Math.Max(0.5, _webView.ZoomFactor - 0.1));

        var favorites = new ToolStripMenuItem("Favorite");
        favorites.DropDownItems.Add("Istoric", null, (_, _) => ShowHistory());
        favorites.DropDownItems.Add("Semne de carte", null, (_, _) => ShowBookmarks());

        var tools = new ToolStripMenuItem("Instrumente");
        tools.DropDownItems.Add("Setări Internet Pro", null, (_, _) => ShowBrowserSettings());
        tools.DropDownItems.Add("Mod dezvoltator și liste de site-uri", null, (_, _) => ShowDeveloperSettings());
        tools.DropDownItems.Add("Șterge datele de navigare", null, async (_, _) => await ClearBrowsingDataAsync());

        var help = new ToolStripMenuItem("Ajutor");
        help.DropDownItems.Add("Despre Internet Pro", null, (_, _) => MessageBox.Show(
            this,
            "Internet Pro\nBrowser Windows cu motor Microsoft Edge WebView2.",
            "Despre",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information));

        menu.Items.AddRange([file, view, favorites, tools, help]);
        return menu;
    }

    private static Icon CreateBrowserIcon()
    {
        using var bitmap = new Bitmap(64, 64);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(Color.Transparent);
            using var background = new LinearGradientBrush(
                new Rectangle(3, 3, 58, 58),
                Color.FromArgb(17, 93, 172),
                Color.FromArgb(54, 170, 221),
                45f);
            graphics.FillEllipse(background, 3, 3, 58, 58);
            using var letterFont = new Font("Segoe UI", 45, FontStyle.Bold, GraphicsUnit.Pixel);
            graphics.DrawString("e", letterFont, Brushes.White, 8, 1);
            using var orbit = new Pen(Color.FromArgb(112, 224, 169), 4);
            graphics.DrawArc(orbit, 3, 24, 58, 22, 185, 170);
        }

        var iconHandle = bitmap.GetHicon();
        try
        {
            using var icon = Icon.FromHandle(iconHandle);
            return (Icon)icon.Clone();
        }
        finally
        {
            DestroyIcon(iconHandle);
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(IntPtr iconHandle);

    private static BrowserTheme GetTheme(string? themeName)
    {
        return AvailableThemes.FirstOrDefault(theme =>
            string.Equals(theme.Name, themeName, StringComparison.OrdinalIgnoreCase)) ?? AvailableThemes[0];
    }

    private void ApplyTheme()
    {
        var theme = GetTheme(_policy.Theme);
        BackColor = theme.Surface;
        _brandPanel.BackColor = theme.Start;
        _brandPanel.Invalidate();
        _toolbarPanel.BackColor = ControlPaint.Light(theme.Surface);
        _filterPanel.BackColor = theme.Surface;
        _menuStrip.BackColor = ControlPaint.Light(theme.Surface);
        _statusBar.BackColor = theme.Accent;
        foreach (var button in _toolbarPanel.Controls.OfType<Button>())
        {
            button.BackColor = ControlPaint.Light(theme.Surface);
            button.ForeColor = Color.FromArgb(35, 52, 67);
            button.FlatAppearance.BorderColor = ControlPaint.Dark(theme.Surface);
        }
    }

    private void DrawThemePreview(Graphics graphics, Rectangle bounds, string? themeName)
    {
        DrawThemeHeader(graphics, bounds, GetTheme(themeName));
    }

    private static void DrawThemeHeader(Graphics graphics, Rectangle bounds, BrowserTheme theme)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return;

        using var gradient = new LinearGradientBrush(bounds, theme.Start, theme.End, LinearGradientMode.Horizontal);
        graphics.FillRectangle(gradient, bounds);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var artBounds = new Rectangle(bounds.Width * 53 / 100, 0, bounds.Width * 47 / 100, bounds.Height);
        DrawThemeArtwork(graphics, artBounds, theme.Style);

        using var orbit = new Pen(Color.FromArgb(175, 213, 241, 255), Math.Max(2, bounds.Height / 25f));
        graphics.DrawArc(orbit, 6, bounds.Height / 3, 45, Math.Max(18, bounds.Height / 2), 185, 165);
        using var logoFont = new Font("Segoe UI", Math.Max(27, bounds.Height * 0.72f), FontStyle.Bold);
        graphics.DrawString("e", logoFont, Brushes.White, 12, bounds.Height * -0.12f);
        using var labelFont = new Font("Segoe UI", Math.Max(8, bounds.Height * 0.18f), FontStyle.Bold);
        graphics.DrawString(theme.Name, labelFont, Brushes.White, artBounds.X + 12, Math.Max(3, bounds.Height * 0.65f));
    }

    private static void DrawThemeArtwork(Graphics graphics, Rectangle bounds, ThemeStyle style)
    {
        var width = bounds.Width;
        var height = bounds.Height;
        switch (style)
        {
            case ThemeStyle.Countryside:
                using (var farHill = new SolidBrush(Color.FromArgb(100, 202, 222, 162)))
                    graphics.FillPolygon(farHill, [new Point(bounds.X, height), new Point(bounds.X + width / 4, height / 4), new Point(bounds.X + width / 2, height), new Point(bounds.X + width * 3 / 4, height / 3), new Point(bounds.Right, height)]);
                using (var nearHill = new SolidBrush(Color.FromArgb(150, 80, 143, 93)))
                    graphics.FillPolygon(nearHill, [new Point(bounds.X, height), new Point(bounds.X + width / 3, height / 2), new Point(bounds.X + width * 2 / 3, height), new Point(bounds.Right, height / 2), new Point(bounds.Right, height)]);
                using (var river = new Pen(Color.FromArgb(180, 156, 222, 221), Math.Max(2, height / 16f)))
                    graphics.DrawBezier(river, bounds.X + width / 3, height, bounds.X + width / 2, height / 2, bounds.X + width * 2 / 3, height * 0.8f, bounds.Right, height / 2);
                break;
            case ThemeStyle.Maramures:
                using (var mountain = new SolidBrush(Color.FromArgb(125, 48, 77, 95)))
                    graphics.FillPolygon(mountain, [new Point(bounds.X, height), new Point(bounds.X + width / 5, height / 5), new Point(bounds.X + width / 3, height * 62 / 100), new Point(bounds.X + width / 2, height / 8), new Point(bounds.Right, height)]);
                using (var snow = new SolidBrush(Color.FromArgb(180, 239, 247, 244)))
                    graphics.FillPolygon(snow, [new Point(bounds.X + width / 5, height / 5), new Point(bounds.X + width / 4, height / 2), new Point(bounds.X + width / 3, height * 62 / 100)]);
                using (var wood = new Pen(Color.FromArgb(205, 95, 57, 40), Math.Max(3, height / 12f)))
                {
                    graphics.DrawLine(wood, bounds.X + width * 3 / 4, height, bounds.X + width * 3 / 4, height / 3);
                    graphics.DrawLine(wood, bounds.X + width * 4 / 5, height, bounds.X + width * 4 / 5, height / 3);
                    graphics.DrawLine(wood, bounds.X + width * 3 / 4, height / 2, bounds.X + width * 4 / 5, height / 2);
                }
                break;
            case ThemeStyle.Candy:
                Color[] candyColors = [Color.FromArgb(245, 90, 145), Color.FromArgb(255, 202, 74), Color.FromArgb(93, 194, 206), Color.FromArgb(153, 116, 218)];
                for (var index = 0; index < 9; index++)
                {
                    var candySize = Math.Max(10, height / 3);
                    var x = bounds.X + 16 + (index * 43) % Math.Max(1, width - 25);
                    var y = height / 3 + (index % 2) * height / 4;
                    using var candy = new SolidBrush(Color.FromArgb(215, candyColors[index % candyColors.Length]));
                    graphics.FillEllipse(candy, x, y, candySize, candySize);
                    using var shine = new SolidBrush(Color.FromArgb(150, Color.White));
                    graphics.FillEllipse(shine, x + candySize / 5, y + candySize / 6, candySize / 4, candySize / 4);
                }
                break;
            case ThemeStyle.Windows11:
            case ThemeStyle.Windows10:
            case ThemeStyle.Windows8:
                using (var windowBlue = new SolidBrush(Color.FromArgb(205, 225, 244, 255)))
                using (var windowLight = new SolidBrush(Color.FromArgb(205, 103, 192, 225)))
                using (var windowGold = new SolidBrush(Color.FromArgb(205, 246, 190, 79)))
                using (var windowGreen = new SolidBrush(Color.FromArgb(205, 104, 182, 126)))
                {
                    var pane = Math.Max(9, height / 3);
                    var paneX = bounds.X + width * 3 / 4;
                    var paneY = height / 2 - pane;
                    graphics.FillRectangle(windowBlue, paneX, paneY, pane, pane);
                    graphics.FillRectangle(windowLight, paneX + pane + 3, paneY, pane, pane);
                    graphics.FillRectangle(windowGold, paneX, paneY + pane + 3, pane, pane);
                    graphics.FillRectangle(windowGreen, paneX + pane + 3, paneY + pane + 3, pane, pane);
                }
                break;
            case ThemeStyle.Windows7:
            case ThemeStyle.Vista:
                using (var glass = new Pen(Color.FromArgb(145, 226, 248, 255), Math.Max(2, height / 15f)))
                {
                    graphics.DrawArc(glass, new Rectangle(bounds.X + width / 2, height / 5, width / 2, height), 195, 145);
                    graphics.DrawArc(glass, new Rectangle(bounds.X + width * 2 / 3, height / 3, width / 3, height * 2 / 3), 195, 145);
                }
                break;
            case ThemeStyle.Windows98:
            case ThemeStyle.Windows95:
                using (var classicBlue = new SolidBrush(Color.FromArgb(210, 0, 0, 128)))
                using (var classicGray = new SolidBrush(Color.FromArgb(220, 192, 192, 192)))
                using (var classicWhite = new SolidBrush(Color.FromArgb(220, 255, 255, 255)))
                {
                    var block = Math.Max(10, height / 3);
                    var left = bounds.X + width * 3 / 4;
                    var top = height / 2 - block;
                    graphics.FillRectangle(classicGray, left, top, block * 2, block * 2);
                    graphics.FillRectangle(classicWhite, left + 2, top + 2, block * 2 - 5, 3);
                    graphics.FillRectangle(classicBlue, left + 3, top + 5, block * 2 - 7, block / 3);
                }
                break;
            case ThemeStyle.Galaxy:
                var starPositions = new[] { (0.08f, 0.22f), (0.22f, 0.72f), (0.37f, 0.3f), (0.53f, 0.8f), (0.7f, 0.18f), (0.88f, 0.62f) };
                foreach (var (xFactor, yFactor) in starPositions)
                {
                    using var star = new SolidBrush(Color.FromArgb(205, 255, 255, 225));
                    var starSize = Math.Max(2, height / 18);
                    graphics.FillEllipse(star, bounds.X + width * xFactor, height * yFactor, starSize, starSize);
                }
                using (var planet = new SolidBrush(Color.FromArgb(105, 223, 145, 202)))
                    graphics.FillEllipse(planet, bounds.X + width * 0.62f, height * 0.18f, height * 0.72f, height * 0.72f);
                using (var ring = new Pen(Color.FromArgb(170, 220, 202, 255), Math.Max(2, height / 24f)))
                    graphics.DrawEllipse(ring, bounds.X + width * 0.58f, height * 0.36f, height * 0.82f, height * 0.28f);
                break;
            case ThemeStyle.PixelRainbow:
                var rainbow = new[] { Color.FromArgb(230, 76, 83), Color.FromArgb(250, 163, 63), Color.FromArgb(250, 220, 83), Color.FromArgb(68, 181, 112), Color.FromArgb(63, 141, 212), Color.FromArgb(132, 100, 199) };
                for (var index = 0; index < rainbow.Length; index++)
                {
                    using var stripe = new SolidBrush(Color.FromArgb(205, rainbow[index]));
                    graphics.FillRectangle(stripe, bounds.X + 8, height / 4 + index * Math.Max(3, height / 12), width / 2, Math.Max(3, height / 12));
                }
                DrawPixelCat(graphics, bounds);
                break;
            case ThemeStyle.Classic:
                using (var orbit = new Pen(Color.FromArgb(140, 217, 245, 255), Math.Max(2, height / 18f)))
                    graphics.DrawArc(orbit, bounds.X + width / 3, height / 4, width / 2, height / 2, 190, 150);
                break;
        }
    }

    private static void DrawPixelCat(Graphics graphics, Rectangle bounds)
    {
        var pixel = Math.Max(4, bounds.Height / 10);
        var left = bounds.X + bounds.Width * 2 / 3;
        var top = bounds.Height / 3;
        using var fur = new SolidBrush(Color.FromArgb(230, 210, 232, 220));
        using var dark = new SolidBrush(Color.FromArgb(230, 49, 73, 90));
        graphics.FillRectangle(fur, left, top + pixel, pixel * 4, pixel * 3);
        graphics.FillRectangle(fur, left + pixel, top, pixel, pixel * 2);
        graphics.FillRectangle(fur, left + pixel * 3, top, pixel, pixel * 2);
        graphics.FillRectangle(dark, left + pixel, top + pixel * 2, pixel, pixel);
        graphics.FillRectangle(dark, left + pixel * 3, top + pixel * 2, pixel, pixel);
    }

    private static Button MakeButton(string text, string tooltip, int width)
    {
        var button = new Button
        {
            Text = text,
            Width = width,
            Height = 31,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(27, 55, 61),
            Font = new Font("Segoe UI", 9),
            Margin = new Padding(2, 0, 2, 0),
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderColor = Color.FromArgb(207, 218, 218);
        new ToolTip().SetToolTip(button, tooltip);
        return button;
    }

    private static CheckBox MakeToggle(string text, bool enabled, string tooltip)
    {
        var toggle = new CheckBox
        {
            Text = text,
            Checked = enabled,
            AutoSize = true,
            ForeColor = Color.FromArgb(28, 55, 61),
            Font = new Font("Segoe UI", 8.5f),
            Margin = new Padding(4, 2, 14, 0),
            Cursor = Cursors.Hand
        };
        new ToolTip().SetToolTip(toggle, tooltip);
        return toggle;
    }

    private static CheckBox MakeLockedToggle(string text, string tooltip)
    {
        var toggle = MakeToggle(text, true, tooltip);
        toggle.AutoCheck = false;
        return toggle;
    }

    private async Task InitializeBrowserAsync()
    {
        try
        {
            var profilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "InternetPro", "WebView2");
            var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: profilePath);
            await _webView.EnsureCoreWebView2Async(environment);

            var core = _webView.CoreWebView2;
            core.Settings.IsPasswordAutosaveEnabled = false;
            core.Settings.IsGeneralAutofillEnabled = false;
            core.Settings.IsReputationCheckingRequired = _reputationToggle.Checked;
            core.Settings.AreDefaultScriptDialogsEnabled = true;
            core.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
            core.WebResourceRequested += OnWebResourceRequested;
            core.NavigationStarting += OnNavigationStarting;
            core.NavigationCompleted += OnNavigationCompleted;
            core.NewWindowRequested += OnNewWindowRequested;
            core.DownloadStarting += OnDownloadStarting;
            core.PermissionRequested += (_, args) => args.State = CoreWebView2PermissionState.Deny;

            _status.Text = "Edge WebView2 este gata. Filtrele selectate se aplică în acest browser.";
            Navigate(_policy.HomePage);
        }
        catch (Exception error)
        {
            _status.Text = "Motorul Edge WebView2 nu este disponibil.";
            var details = new StringBuilder();
            for (Exception? current = error; current is not null; current = current.InnerException)
                details.AppendLine($"{current.GetType().Name}: {current.Message}");

            MessageBox.Show(
                this,
                "WebView2 nu a putut fi inițializat. Verifică detaliile erorii:\n\n" + details,
                "Motor browser indisponibil",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void OnNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs args)
    {
        if (!Uri.TryCreate(args.Uri, UriKind.Absolute, out var uri))
            return;

        if (IsBlocked(uri))
        {
            args.Cancel = true;
            ShowBlockedPage(uri.Host);
            return;
        }

        if (_policy.PaymentProtection
            && uri.Scheme == Uri.UriSchemeHttp
            && IsPaymentPage(uri))
        {
            args.Cancel = true;
            ShowBlockedPage(uri.Host, "Checkout-ul a fost blocat deoarece folosește HTTP, nu HTTPS.");
            return;
        }

        _showingBlockedPage = false;
        _address.Text = uri.ToString();
    }

    private void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs args)
    {
        if (!args.IsSuccess || _showingBlockedPage || _webView.CoreWebView2 is null)
            return;

        var uri = _webView.Source;
        if (uri is null || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            return;

        var entry = (_webView.CoreWebView2.DocumentTitle, uri.ToString());
        if (_history.Count == 0 || _history[^1].Url != entry.Item2)
            _history.Add(entry);
        if (_history.Count > 500)
            _history.RemoveAt(0);
        _status.Text = $"{uri.Host}  •  {(_history.Count)} pagini în istoric";
    }

    private void OnNewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs args)
    {
        args.Handled = true;
        Navigate(args.Uri);
    }

    private void OnWebResourceRequested(object? sender, CoreWebView2WebResourceRequestedEventArgs args)
    {
        if (!Uri.TryCreate(args.Request.Uri, UriKind.Absolute, out var uri) || !IsBlocked(uri))
            return;

        var body = new MemoryStream(Encoding.UTF8.GetBytes("Blocked by Internet Pro"));
        args.Response = _webView.CoreWebView2.Environment.CreateWebResourceResponse(
            body, 403, "Blocked", "Content-Type: text/plain; charset=utf-8");
    }

    private void OnDownloadStarting(object? sender, CoreWebView2DownloadStartingEventArgs args)
    {
        var fileName = Path.GetFileName(args.ResultFilePath);
        if (!ContainsRobloxReference(args.DownloadOperation.Uri)
            && !ContainsRobloxReference(fileName)
            && Uri.TryCreate(args.DownloadOperation.Uri, UriKind.Absolute, out var downloadUri)
            && !IsBlocked(downloadUri))
            return;

        args.Cancel = true;
        _status.Text = "Descărcarea a fost blocată de regulile Internet Pro.";
    }

    private bool IsBlocked(Uri uri)
    {
        return ContainsRobloxReference(uri.AbsoluteUri) || IsBlocked(uri.Host);
    }

    private static bool ContainsRobloxReference(string value)
    {
        return value.Contains("roblox", StringComparison.OrdinalIgnoreCase)
            || value.Contains("com.roblox.client", StringComparison.OrdinalIgnoreCase)
            || value.Contains("586449749", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPaymentPage(Uri uri)
    {
        var address = uri.Host + uri.AbsolutePath + uri.Query;
        string[] paymentTerms = ["checkout", "payment", "billing", "invoice", "paypal", "stripe", "card-details", "carddetails", "pay-now"];
        return paymentTerms.Any(term => address.Contains(term, StringComparison.OrdinalIgnoreCase))
            || uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Any(segment => string.Equals(segment, "pay", StringComparison.OrdinalIgnoreCase));
    }

    private bool IsBlocked(string host)
    {
        if (IsProtectedDomain(host))
            return true;

        if (MatchesDomain(host, _policy.AllowedDomains))
            return false;

        return (_adultToggle.Checked && MatchesDomain(host, AdultDomains))
            || (_adsToggle.Checked && MatchesDomain(host, AdDomains))
            || MatchesDomain(host, _policy.BlockedDomains);
    }

    private static bool IsProtectedDomain(string host)
    {
        return MatchesDomain(host, RobloxDomains)
            || MatchesDomain(host, MinecraftDomains)
            || MatchesDomain(host, TikTokDomains);
    }

    private static bool MatchesDomain(string host, IEnumerable<string> domains)
    {
        return domains.Any(domain =>
        {
            if (domain.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
                return host.Equals(domain, StringComparison.OrdinalIgnoreCase);

            return host.Equals(domain, StringComparison.OrdinalIgnoreCase)
                || host.EndsWith('.' + domain, StringComparison.OrdinalIgnoreCase);
        });
    }

    private void Navigate(string address)
    {
        if (_webView.CoreWebView2 is null || string.IsNullOrWhiteSpace(address))
            return;

        var value = address.Trim();
        if (!value.Contains(".", StringComparison.Ordinal) && !value.StartsWith("localhost", StringComparison.OrdinalIgnoreCase))
            value = "https://www.google.com/search?q=" + Uri.EscapeDataString(value);
        else if (!value.Contains("://", StringComparison.Ordinal))
            value = "https://" + value;

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            _status.Text = "Adresa nu este validă.";
            return;
        }

        _address.Text = uri.ToString();
        _webView.CoreWebView2.Navigate(uri.ToString());
    }

    private void ShowBlockedPage(string host, string? detail = null)
    {
        _showingBlockedPage = true;
        _address.Text = host;
        _status.Text = $"Acces blocat pentru {host}";
        var safeHost = WebUtility.HtmlEncode(host);
        var safeDetail = WebUtility.HtmlEncode(detail ?? "Accesul la această adresă este blocat de regulile Internet Pro.");
        _webView.NavigateToString($$"""
            <!doctype html><html lang="en"><meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <title>Acces nepermis / Access denied</title>
            <style>
              body { margin: 0; background: #f7f9fa; color: #263238; font: 16px 'Segoe UI', sans-serif; }
              main { max-width: 620px; margin: 15vh auto; padding: 0 28px; }
              .mark { color: #217a70; font-size: 13px; font-weight: 700; letter-spacing: 1px; }
              h1 { font-size: 28px; font-weight: 500; margin: 26px 0 12px; }
              p { color: #526267; line-height: 1.6; }
              code { color: #263238; }
            </style>
            <main><div class="mark">INTERNET PRO</div><h1>Acces nepermis / Access denied</h1>
            <p><code>{{safeHost}}</code></p><p>{{safeDetail}}</p></main>
            </html>
            """);
    }

    private void ShowDeveloperSettings()
    {
        using var dialog = new Form
        {
            Text = "Mod dezvoltator - liste de site-uri",
            Size = new Size(900, 760),
            MinimumSize = new Size(720, 600),
            StartPosition = FormStartPosition.CenterParent,
            BackColor = Color.FromArgb(245, 248, 251)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            Padding = new Padding(14),
            BackColor = dialog.BackColor
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        dialog.Controls.Add(layout);

        var developerMode = new CheckBox
        {
            Text = "Activează modul dezvoltator pentru editarea politicii locale",
            Checked = _policy.DeveloperModeEnabled,
            AutoSize = true,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(31, 67, 98)
        };
        layout.Controls.Add(developerMode, 0, 0);

        var blockLabel = new Label
        {
            Text = "Blocklist - domenii blocate, câte unul pe linie; www.site.ro blochează doar www.site.ro",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        layout.Controls.Add(blockLabel, 0, 1);
        var blockEditor = CreateDomainEditor(_policy.BlockedDomains);
        layout.Controls.Add(blockEditor, 0, 2);

        var allowLabel = new Label
        {
            Text = "Allowed list - excepții pentru blocklist-ul personalizat; www.site.ro se aplică doar acelui host",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        layout.Controls.Add(allowLabel, 0, 3);
        var allowEditor = CreateDomainEditor(_policy.AllowedDomains);
        layout.Controls.Add(allowEditor, 0, 4);

        var footer = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(0, 5, 0, 0)
        };
        footer.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        footer.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        var note = new Label
        {
            Text = "Regulile se aplică doar în Internet Pro. Nu blochează aplicații instalate, apeluri, SMS sau eSIM. Pentru phishing, verificarea de reputație Edge trebuie să rămână activă.",
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(75, 91, 103),
            AutoEllipsis = true
        };
        footer.Controls.Add(note, 0, 0);
        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false
        };
        var close = MakeButton("Închide", "Închide editorul fără alte acțiuni", 82);
        close.Click += (_, _) => dialog.Close();
        var export = MakeButton("Export JSON", "Exportă politica pentru alte instalări Internet Pro", 104);
        var import = MakeButton("Import JSON", "Încarcă o politică Internet Pro existentă", 104);
        var save = MakeButton("Salvează", "Salvează politica pe acest profil Windows", 86);
        buttons.Controls.AddRange([close, export, import, save]);
        footer.Controls.Add(buttons, 0, 1);
        layout.Controls.Add(footer, 0, 5);

        void UpdateEditorAccess()
        {
            var canEdit = developerMode.Checked;
            blockEditor.Enabled = canEdit;
            allowEditor.Enabled = canEdit;
            import.Enabled = canEdit;
            export.Enabled = canEdit;
        }

        developerMode.CheckedChanged += (_, _) => UpdateEditorAccess();
        UpdateEditorAccess();

        save.Click += (_, _) =>
        {
            try
            {
                var policy = CreatePolicyFromEditors(developerMode.Checked, blockEditor.Text, allowEditor.Text);
                SavePolicy(policy);
                _policy = policy;
                RefreshCurrentAddress();
                _status.Text = $"Politică salvată: {policy.BlockedDomains.Count} domenii blocate, {policy.AllowedDomains.Count} permise.";
                dialog.Close();
            }
            catch (Exception error)
            {
                MessageBox.Show(dialog, error.Message, "Politică invalidă", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        };

        export.Click += (_, _) =>
        {
            try
            {
                var policy = CreatePolicyFromEditors(developerMode.Checked, blockEditor.Text, allowEditor.Text);
                using var picker = new SaveFileDialog
                {
                    Title = "Exportă politica Internet Pro",
                    Filter = "Politică Internet Pro (*.json)|*.json",
                    FileName = "internetpro-policy.json"
                };
                if (picker.ShowDialog(dialog) == DialogResult.OK)
                    File.WriteAllText(picker.FileName, JsonSerializer.Serialize(policy, PolicyJsonOptions));
            }
            catch (Exception error)
            {
                MessageBox.Show(dialog, error.Message, "Nu s-a putut exporta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        };

        import.Click += (_, _) =>
        {
            using var picker = new OpenFileDialog
            {
                Title = "Importă politica Internet Pro",
                Filter = "Politică Internet Pro (*.json)|*.json|Toate fișierele (*.*)|*.*"
            };
            if (picker.ShowDialog(dialog) != DialogResult.OK)
                return;

            try
            {
                var policy = ParsePolicy(File.ReadAllText(picker.FileName));
                developerMode.Checked = policy.DeveloperModeEnabled;
                blockEditor.Text = string.Join(Environment.NewLine, policy.BlockedDomains);
                allowEditor.Text = string.Join(Environment.NewLine, policy.AllowedDomains);
            }
            catch (Exception error)
            {
                MessageBox.Show(dialog, error.Message, "Fișier de politică invalid", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        };

        dialog.AcceptButton = save;
        dialog.CancelButton = close;
        dialog.ShowDialog(this);
    }

    private static TextBox CreateDomainEditor(IEnumerable<string> domains)
    {
        return new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            AcceptsReturn = true,
            AcceptsTab = true,
            ScrollBars = ScrollBars.Vertical,
            WordWrap = false,
            Font = new Font("Consolas", 10),
            Text = string.Join(Environment.NewLine, domains)
        };
    }

    private void ShowBrowserSettings()
    {
        using var dialog = new Form
        {
            Text = "Setări Internet Pro",
            Size = new Size(700, 610),
            MinimumSize = new Size(620, 560),
            StartPosition = FormStartPosition.CenterParent,
            BackColor = Color.FromArgb(245, 248, 251)
        };
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 8,
            Padding = new Padding(16),
            BackColor = dialog.BackColor
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        dialog.Controls.Add(layout);

        layout.Controls.Add(new Label { Text = "Pagina Acasă", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        var homePage = new TextBox { Dock = DockStyle.Fill, Text = _policy.HomePage, Font = new Font("Segoe UI", 10) };
        layout.Controls.Add(homePage, 0, 1);
        layout.Controls.Add(new Label { Text = "Temă pentru interfața Internet Pro", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 2);

        var themePicker = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        foreach (var theme in AvailableThemes)
            themePicker.Items.Add(theme.Name);
        themePicker.SelectedItem = GetTheme(_policy.Theme).Name;
        layout.Controls.Add(themePicker, 0, 3);

        var preview = new Panel { Dock = DockStyle.Fill, MinimumSize = new Size(100, 110), BackColor = Color.White };
        preview.Paint += (_, e) => DrawThemePreview(e.Graphics, preview.ClientRectangle, themePicker.SelectedItem?.ToString());
        themePicker.SelectedIndexChanged += (_, _) => preview.Invalidate();
        layout.Controls.Add(preview, 0, 4);

        var firstOptions = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        var blockAds = MakeToggle("Blocare reclame", _adsToggle.Checked, "Domenii publicitare cunoscute");
        var blockAdult = MakeToggle("Blocare site-uri pentru adulți", _adultToggle.Checked, "Domenii cunoscute din lista locală");
        firstOptions.Controls.AddRange([blockAds, blockAdult]);
        layout.Controls.Add(firstOptions, 0, 5);

        var securityOptions = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        var reputation = MakeToggle("Reputație Edge", _reputationToggle.Checked, "Verificare de reputație disponibilă în Edge");
        var paymentProtection = MakeToggle("Protecție plăți HTTP", _policy.PaymentProtection, "Blochează adrese de checkout recunoscute pe conexiuni HTTP");
        securityOptions.Controls.AddRange([reputation, paymentProtection]);
        layout.Controls.Add(securityOptions, 0, 6);

        var footer = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 8, 0, 0) };
        var cancel = MakeButton("Anulează", "Închide fără să salvezi", 86);
        cancel.Click += (_, _) => dialog.Close();
        var save = MakeButton("Salvează", "Salvează setările", 86);
        save.Click += (_, _) =>
        {
            try
            {
                _policy.HomePage = NormalizeHomePage(homePage.Text);
                _policy.Theme = themePicker.SelectedItem?.ToString() ?? AvailableThemes[0].Name;
                _policy.PaymentProtection = paymentProtection.Checked;
                _adsToggle.Checked = blockAds.Checked;
                _adultToggle.Checked = blockAdult.Checked;
                _reputationToggle.Checked = reputation.Checked;
                ApplyTheme();
                PersistVisibleSettings();
                dialog.Close();
            }
            catch (Exception error)
            {
                MessageBox.Show(dialog, error.Message, "Setări invalide", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        };
        footer.Controls.AddRange([cancel, save]);
        layout.Controls.Add(footer, 0, 7);
        dialog.AcceptButton = save;
        dialog.CancelButton = cancel;
        dialog.ShowDialog(this);
    }

    private void PersistVisibleSettings()
    {
        _policy.BlockAds = _adsToggle.Checked;
        _policy.BlockAdultSites = _adultToggle.Checked;
        _policy.CheckReputation = _reputationToggle.Checked;
        try
        {
            SavePolicy(_policy);
        }
        catch (Exception error)
        {
            _status.Text = "Nu s-au putut salva setările: " + error.Message;
        }
    }

    private void AddCurrentBookmark()
    {
        var currentAddress = _webView.Source;
        if (currentAddress is null
            || (currentAddress.Scheme != Uri.UriSchemeHttp && currentAddress.Scheme != Uri.UriSchemeHttps))
        {
            _status.Text = "Deschide o pagină web înainte de a salva un semn de carte.";
            return;
        }

        var existing = _policy.Bookmarks.FirstOrDefault(item =>
            string.Equals(item.Url, currentAddress.ToString(), StringComparison.OrdinalIgnoreCase));
        if (existing is null)
        {
            _policy.Bookmarks.Add(new BookmarkEntry
            {
                Title = string.IsNullOrWhiteSpace(_webView.CoreWebView2?.DocumentTitle)
                    ? currentAddress.Host
                    : _webView.CoreWebView2.DocumentTitle,
                Url = currentAddress.ToString()
            });
        }
        else
        {
            existing.Title = _webView.CoreWebView2?.DocumentTitle ?? currentAddress.Host;
        }

        try
        {
            SavePolicy(_policy);
            _status.Text = "Semnul de carte a fost salvat.";
        }
        catch (Exception error)
        {
            _status.Text = "Nu s-a putut salva semnul de carte: " + error.Message;
        }
    }

    private void ShowBookmarks()
    {
        using var dialog = new Form
        {
            Text = "Semne de carte",
            Size = new Size(760, 520),
            StartPosition = FormStartPosition.CenterParent,
            BackColor = Color.White
        };
        var list = new ListBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), IntegralHeight = false };
        var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 46, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(4) };
        var close = MakeButton("Închide", "Închide lista", 82);
        close.Click += (_, _) => dialog.Close();
        var remove = MakeButton("Șterge", "Șterge semnul de carte selectat", 82);
        var open = MakeButton("Deschide", "Deschide semnul de carte selectat", 82);
        footer.Controls.AddRange([close, remove, open]);
        dialog.Controls.Add(list);
        dialog.Controls.Add(footer);

        void RefreshList()
        {
            list.Items.Clear();
            foreach (var item in _policy.Bookmarks)
                list.Items.Add(item);
        }

        void OpenSelected()
        {
            if (list.SelectedItem is not BookmarkEntry item)
                return;
            Navigate(item.Url);
            dialog.Close();
        }

        list.DoubleClick += (_, _) => OpenSelected();
        open.Click += (_, _) => OpenSelected();
        remove.Click += (_, _) =>
        {
            if (list.SelectedItem is not BookmarkEntry item)
                return;
            _policy.Bookmarks.Remove(item);
            SavePolicy(_policy);
            RefreshList();
        };
        RefreshList();
        dialog.ShowDialog(this);
    }

    private static string NormalizeHomePage(string? value)
    {
        var candidate = value?.Trim() ?? "";
        if (candidate.Length == 0)
            throw new FormatException("Introdu adresa paginii Acasă.");
        if (!candidate.Contains("://", StringComparison.Ordinal))
            candidate = "https://" + candidate;
        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new FormatException("Pagina Acasă trebuie să folosească HTTP sau HTTPS.");
        return uri.ToString();
    }

    private void RefreshCurrentAddress()
    {
        if (_webView.CoreWebView2 is not null
            && _webView.Source is { Scheme: "http" or "https" } currentAddress)
        {
            Navigate(currentAddress.ToString());
        }
    }

    private BrowserPolicy CreatePolicyFromEditors(bool developerMode, string blockedText, string allowedText)
    {
        return new BrowserPolicy
        {
            DeveloperModeEnabled = developerMode,
            BlockedDomains = ParseDomains(blockedText),
            AllowedDomains = ParseDomains(allowedText),
            HomePage = _policy.HomePage,
            Theme = _policy.Theme,
            BlockAds = _policy.BlockAds,
            BlockAdultSites = _policy.BlockAdultSites,
            CheckReputation = _policy.CheckReputation,
            PaymentProtection = _policy.PaymentProtection,
            Bookmarks = [.. _policy.Bookmarks]
        };
    }

    private static List<string> ParseDomains(string text)
    {
        var domains = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        var idn = new IdnMapping();
        foreach (var rawLine in text.Split(["\r\n", "\n"], StringSplitOptions.None))
        {
            var entry = rawLine.Trim();
            if (entry.Length == 0 || entry.StartsWith('#'))
                continue;
            if (entry.Contains("://", StringComparison.Ordinal) || entry.Contains('/') || entry.Contains(':'))
                throw new FormatException($"Introdu un domeniu simplu, fără protocol sau cale: {entry}");

            string domain;
            try
            {
                domain = idn.GetAscii(entry.TrimEnd('.')).ToLowerInvariant();
            }
            catch (ArgumentException)
            {
                throw new FormatException($"Domeniu invalid: {entry}");
            }

            if (Uri.CheckHostName(domain) != UriHostNameType.Dns)
                throw new FormatException($"Domeniu invalid: {entry}");
            domains.Add(domain);
        }

        return [.. domains];
    }

    private static BrowserPolicy ParsePolicy(string json)
    {
        var policy = JsonSerializer.Deserialize<BrowserPolicy>(json)
            ?? throw new FormatException("Fișierul nu conține o politică validă.");
        if (policy.Version != 1)
            throw new FormatException($"Versiunea politicii nu este acceptată: {policy.Version}.");

        policy.BlockedDomains = ParseDomains(string.Join(Environment.NewLine, policy.BlockedDomains ?? []));
        policy.AllowedDomains = ParseDomains(string.Join(Environment.NewLine, policy.AllowedDomains ?? []));
        policy.HomePage = NormalizeHomePage(policy.HomePage);
        policy.Theme = GetTheme(policy.Theme).Name;
        policy.Bookmarks ??= [];
        return policy;
    }

    private static BrowserPolicy LoadPolicy()
    {
        try
        {
            var policyPath = GetPolicyPath();
            return File.Exists(policyPath) ? ParsePolicy(File.ReadAllText(policyPath)) : new BrowserPolicy();
        }
        catch
        {
            return new BrowserPolicy();
        }
    }

    private static void SavePolicy(BrowserPolicy policy)
    {
        var policyPath = GetPolicyPath();
        Directory.CreateDirectory(Path.GetDirectoryName(policyPath)!);
        var temporaryPath = policyPath + ".tmp";
        File.WriteAllText(temporaryPath, JsonSerializer.Serialize(policy, PolicyJsonOptions));
        File.Move(temporaryPath, policyPath, true);
    }

    private static string GetPolicyPath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "InternetPro", "policy.json");
    }

    private static readonly JsonSerializerOptions PolicyJsonOptions = new() { WriteIndented = true };

    private sealed class BrowserPolicy
    {
        public BrowserPolicy()
        {
        }

        public int Version { get; set; } = 1;
        public bool DeveloperModeEnabled { get; set; }
        public List<string> BlockedDomains { get; set; } = [];
        public List<string> AllowedDomains { get; set; } = [];
        public string HomePage { get; set; } = "https://www.youtube.com";
        public string Theme { get; set; } = "Internet Pro clasic";
        public bool BlockAds { get; set; } = true;
        public bool BlockAdultSites { get; set; } = true;
        public bool CheckReputation { get; set; } = true;
        public bool PaymentProtection { get; set; } = true;
        public List<BookmarkEntry> Bookmarks { get; set; } = [];
    }

    private sealed class BookmarkEntry
    {
        public BookmarkEntry()
        {
        }

        public string Title { get; set; } = "";
        public string Url { get; set; } = "";
        public override string ToString() => $"{Title}    {Url}";
    }

    private void ShowHistory()
    {
        using var dialog = new Form
        {
            Text = "Istoric",
            Size = new Size(760, 520),
            StartPosition = FormStartPosition.CenterParent,
            BackColor = Color.White
        };
        var list = new ListBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10),
            IntegralHeight = false
        };
        foreach (var item in _history.AsEnumerable().Reverse())
            list.Items.Add($"{item.Title}    {item.Url}");
        list.DoubleClick += (_, _) =>
        {
            if (list.SelectedIndex >= 0)
                Navigate(_history.AsEnumerable().Reverse().ElementAt(list.SelectedIndex).Url);
            dialog.Close();
        };
        dialog.Controls.Add(list);
        dialog.ShowDialog(this);
    }

    private async Task ClearBrowsingDataAsync()
    {
        if (_webView.CoreWebView2 is null)
            return;

        var answer = MessageBox.Show(
            this,
            "Se vor șterge istoricul WebView2, cookie-urile, memoria cache și datele de site. Internet Pro nu salvează parole; completarea și salvarea parolelor sunt dezactivate. Continui?",
            "Șterge datele de navigare",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (answer != DialogResult.Yes)
            return;

        try
        {
            await _webView.CoreWebView2.Profile.ClearBrowsingDataAsync(CoreWebView2BrowsingDataKinds.AllProfile);
            _history.Clear();
            _status.Text = "Istoricul, cookie-urile și datele de navigare au fost șterse.";
        }
        catch (Exception error)
        {
            MessageBox.Show(this, error.Message, "Nu s-au putut șterge datele", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}