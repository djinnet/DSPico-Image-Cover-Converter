using System.ComponentModel;
using System.Drawing.Drawing2D;
using Timer = System.Windows.Forms.Timer;

namespace PicoLauncher.Core.Controls;

public class AnimatedProgressBar : ProgressBar
{
    private int minimum = 0;
    private int maximum = 100;
    private int value = 0;

    private Color progressColor = Color.DodgerBlue;
    private Color backgroundBarColor = Color.LightGray;
    private Color borderColor = Color.Gray;

    private bool showText = true;
    private ProgressTextMode textMode = ProgressTextMode.Percentage;
    private string customText = "";

    private AnimationStyle animationStyle = AnimationStyle.Smooth;
    private int animationSpeed = 5;

    private readonly Timer animationTimer;
    private int animatedValue;

    public AnimatedProgressBar()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw, true);

        Height = 30;

        animationTimer = new Timer();
        animationTimer.Interval = 15;
        animationTimer.Tick += AnimationTimer_Tick;
    }

    #region Properties

    [Category("Progress")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public int Minimum
    {
        get => minimum;
        set
        {
            minimum = value;
            if (this.value < minimum) this.value = minimum;
            Invalidate();
        }
    }

    [Category("Progress")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public int Maximum
    {
        get => maximum;
        set
        {
            maximum = value;
            if (this.value > maximum) this.value = maximum;
            Invalidate();
        }
    }

    [Category("Progress")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public int Value
    {
        get => value;
        set
        {
            this.value = Math.Max(minimum, Math.Min(maximum, value));
            StartAnimation();
        }
    }

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color ProgressColor
    {
        get => progressColor;
        set { progressColor = value; Invalidate(); }
    }

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color BackgroundBarColor
    {
        get => backgroundBarColor;
        set { backgroundBarColor = value; Invalidate(); }
    }

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color BorderColor
    {
        get => borderColor;
        set { borderColor = value; Invalidate(); }
    }

    [Category("Text")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public bool ShowText
    {
        get => showText;
        set { showText = value; Invalidate(); }
    }

    [Category("Text")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public ProgressTextMode TextMode
    {
        get => textMode;
        set { textMode = value; Invalidate(); }
    }

    [Category("Text")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public string CustomText
    {
        get => customText;
        set { customText = value; Invalidate(); }
    }

    [Category("Animation")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public AnimationStyle AnimationStyle
    {
        get => animationStyle;
        set { animationStyle = value; Invalidate(); }
    }

    [Category("Animation")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public int AnimationSpeed
    {
        get => animationSpeed;
        set => animationSpeed = Math.Max(1, value);
    }

    #endregion

    #region Animation

    private void StartAnimation()
    {
        if (animationStyle == AnimationStyle.Instant)
        {
            animatedValue = value;
            Invalidate();
            return;
        }

        animationTimer.Start();
    }

    private void AnimationTimer_Tick(object sender, EventArgs e)
    {
        if (animatedValue == value)
        {
            animationTimer.Stop();
            return;
        }

        animatedValue = animatedValue < value ? Math.Min(animatedValue + animationSpeed, value) : Math.Max(animatedValue - animationSpeed, value);

        Invalidate();
    }

    #endregion

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        Rectangle rect = ClientRectangle;

        using (var bgBrush = new SolidBrush(backgroundBarColor))
        {
            g.FillRectangle(bgBrush, rect);
        }

        float percent = (float)(animatedValue - minimum) / (maximum - minimum);
        int width = (int)(rect.Width * percent);

        Rectangle progressRect = new(0, 0, width, rect.Height);

        switch (animationStyle)
        {
            case AnimationStyle.Gradient:
                using (var brush = new LinearGradientBrush(progressRect, progressColor, ControlPaint.Light(progressColor), 0f))
                    g.FillRectangle(brush, progressRect);
                break;

            case AnimationStyle.Blocks:
                DrawBlocks(g, progressRect);
                break;

            default:
                using (var brush = new SolidBrush(progressColor))
                    g.FillRectangle(brush, progressRect);
                break;
        }

        using (var pen = new Pen(borderColor))
        {
            g.DrawRectangle(pen, 0, 0, rect.Width - 1, rect.Height - 1);
        }

        if (showText)
        {
            DrawText(g, rect, percent);
        }
    }

    private void DrawBlocks(Graphics g, Rectangle progressRect)
    {
        int blockWidth = 10;
        int spacing = 2;

        using var brush = new SolidBrush(progressColor);

        for (int x = 0; x < progressRect.Width; x += blockWidth + spacing)
        {
            g.FillRectangle(brush, x, 0, blockWidth, progressRect.Height);
        }
    }

    private void DrawText(Graphics g, Rectangle rect, float percent)
    {
        string text = textMode switch
        {
            ProgressTextMode.Percentage => $"{(int)(percent * 100)}%",
            ProgressTextMode.Value => $"{value}/{maximum}",
            ProgressTextMode.Custom => customText,
            _ => ""
        };

        using var brush = new SolidBrush(ForeColor);

        var size = g.MeasureString(text, Font);

        var location = new PointF((rect.Width - size.Width) / 2, (rect.Height - size.Height) / 2);

        g.DrawString(text, Font, brush, location);
    }

}

public enum ProgressTextMode
{
    Percentage,
    Value,
    Custom
}

public enum AnimationStyle
{
    Instant,
    Smooth,
    Gradient,
    Blocks
}
