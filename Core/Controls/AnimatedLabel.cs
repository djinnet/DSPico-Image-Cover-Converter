using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using Timer = System.Windows.Forms.Timer;
namespace PicoLauncher.Core.Controls;

[ToolboxItem(typeof(ToolboxItem))]
public class AnimatedLabel : Label
{
    private Timer frameTimer;
    private Stopwatch stopwatch = new();

    private readonly TitleCollection titles = [];
    private int currentTitleIndex;
    private float animationDuration = 0.6f; // seconds
    private float titleInterval = 5f;       // seconds

    [Category("Custom Appearance")]
    [Description("Titles displayed and animated by the control.")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    [Editor(typeof(TitleCollectionEditor), typeof(UITypeEditor))]
    public TitleCollection Titles
    {
        get => titles;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public float AnimationDuration
    {
        get => animationDuration;
        set => animationDuration = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public float TitleInterval
    {
        get => titleInterval;
        set => titleInterval = value;
    }

    public AnimatedLabel()
    {
        InitializeControl();

    }

    private void InitializeControl()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw, true);

        titles.Changed += (_, _) =>
        {
            currentTitleIndex = 0;
            RestartAnimation();
            Invalidate();
        };

        frameTimer = new Timer
        {
            Interval = 16 // ~60 FPS
        };
        frameTimer.Tick += FrameTimer_Tick;

        frameTimer.Start();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if(e.Button == MouseButtons.Left)
        {
            var form = FindForm();
            if (form != null)
            {
                CoreLauncher.DragWindow(form.Handle);
            }
        }
    }

    private void RestartAnimation()
    {
        stopwatch.Restart();
        Invalidate();
    }

    private void FrameTimer_Tick(object sender, EventArgs e)
    {
        if (Titles.Count == 0)
        {
            Debug.WriteLine("AnimatedLabel: No titles to display.");
            return;
        }

        float elapsed = (float)stopwatch.Elapsed.TotalSeconds;

        if (elapsed > titleInterval)
        {
            currentTitleIndex = (currentTitleIndex + 1) % Math.Max(1, Titles.Count);
            RestartAnimation();
        }

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(BackColor);

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        string text;

        if (Titles.Count == 0)
            return;

        text = Titles[currentTitleIndex];

        SizeF textSize = e.Graphics.MeasureString(text, Font);

        float x = (Width - textSize.Width) / 2f;
        float y = (Height - textSize.Height) / 2f;

        float elapsed = (float)stopwatch.Elapsed.TotalSeconds;
        float progress = Math.Min(elapsed / animationDuration, 1f);

        // Smooth easing (ease-out)
        progress = 1f - (float)Math.Pow(1f - progress, 3);

        float visibleWidth = textSize.Width * progress;
        float centerX = x + textSize.Width / 2f;

        RectangleF clip = new(
            centerX - visibleWidth / 2f,
            0,
            visibleWidth,
            Height);

        GraphicsState state = e.Graphics.Save();
        e.Graphics.SetClip(clip);

        using var brush = new SolidBrush(ForeColor);
        e.Graphics.DrawString(text, Font, brush, x, y);

        e.Graphics.Restore(state);
    }
}
