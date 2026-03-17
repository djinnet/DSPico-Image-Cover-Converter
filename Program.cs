using PicoLauncher.Forms;

namespace PicoLauncher;

internal class Program
{
    static bool runOldForm = false;

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        if (runOldForm)
        {
            Application.Run(new PicoLauncherForm());
        }
        else
        {
            Application.Run(new MainPicoLauncher());
        }
    }
}
