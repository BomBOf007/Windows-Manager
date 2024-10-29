using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Windows_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Importieren der benötigten Funktionen aus user32.dll
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        // Delegate für die Fenster-Aufzählungsfunktion
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        // Konstanten für die Positionierung
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;

        public MainWindow()
        {
            InitializeComponent();

            // Button zum Ändern der Fenstergröße hinzufügen
            Button resizeButton = new Button
            {
                Text = "Alle Fenster anpassen",
                Dock = DockStyle.Fill
            };
            resizeButton.Click += ResizeButton_Click;
            this.Controls.Add(resizeButton);
        }

        private void ResizeButton_Click(object sender, EventArgs e)
        {
            // Wunschgröße für alle Fenster
            int targetWidth = 800;
            int targetHeight = 600;

            EnumWindows((hWnd, lParam) =>
            {
                if (IsWindowVisible(hWnd)) // Nur sichtbare Fenster
                {
                    StringBuilder windowText = new StringBuilder(256);
                    GetWindowText(hWnd, windowText, windowText.Capacity);

                    if (windowText.Length > 0) // Fenster mit Titel
                    {
                        SetWindowPos(hWnd, IntPtr.Zero, 100, 100, targetWidth, targetHeight, SWP_NOZORDER | SWP_NOACTIVATE);
                        Console.WriteLine($"Fenster angepasst: {windowText}");
                    }
                }
                return true; // Fortsetzen der Aufzählung
            }, IntPtr.Zero);
        }
    }
}