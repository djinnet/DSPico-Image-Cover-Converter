
using System.ComponentModel;
using System.Drawing.Design;

namespace PicoLauncher.Core.Controls;
/// <summary>
/// MarqueeLabel is credited to https://stackoverflow.com/a/59034753 
/// </summary>
[ToolboxItem(typeof(ToolboxItem))]
public class MarqueeLabel : Label
{
    private System.Windows.Forms.Timer timer;

    [Browsable(true)]
    public int offset = 3;

    [Browsable(false)]
    private int? left;

    [Browsable(true)]
    public int textWidth = 0;

    [Browsable(true)]
    public int TimerInterval = 100;

    [Browsable(true)]
    public  bool AutoStart = true;

    public MarqueeLabel()
    {
        DoubleBuffered = true;
        timer = new System.Windows.Forms.Timer();
        timer.Enabled = true;
        timer.Interval = TimerInterval;
        timer.Tick += Timer_Tick;
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        if(RightToLeft == RightToLeft.Yes)
        {
            left += offset;
            if (left > Width)
            {
                left = -textWidth;
            }
        }
        else
        {
            left -= offset;
            if (left < -textWidth)
            {
                left = Width;
            }
        }
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(BackColor);
        var s = TextRenderer.MeasureText(Text, Font, new Size(0, 0),
            TextFormatFlags.TextBoxControl | TextFormatFlags.SingleLine);
        textWidth = s.Width;
        if (!left.HasValue) left = Width;
        var format = TextFormatFlags.TextBoxControl | TextFormatFlags.SingleLine |
            TextFormatFlags.VerticalCenter;
        if (RightToLeft == RightToLeft.Yes)
        {
            format |= TextFormatFlags.RightToLeft;
            if (!left.HasValue) left = -textWidth;
        }
        TextRenderer.DrawText(e.Graphics, Text, Font,
            new Rectangle(left.Value, 0, textWidth, Height),
            ForeColor, BackColor, format);
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            timer.Dispose();
        base.Dispose(disposing);
    }
}
