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
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;

        public MainWindow()
        {
            InitializeComponent();
            LoadWindowList();
        }

        // Klasse zur Darstellung von Fenstern in der ListBox
        public class WindowInfo
        {
            public IntPtr Handle { get; set; }
            public string Title { get; set; }
        }

        // Fensterliste laden
        private void LoadWindowList()
        {
            List<WindowInfo> windows = new List<WindowInfo>();

            EnumWindows((hWnd, lParam) =>
            {
                if (IsWindowVisible(hWnd))
                {
                    StringBuilder windowText = new StringBuilder(256);
                    GetWindowText(hWnd, windowText, windowText.Capacity);

                    if (windowText.Length > 0) // Nur Fenster mit Titel hinzufügen
                    {
                        windows.Add(new WindowInfo { Handle = hWnd, Title = windowText.ToString() });
                    }
                }
                return true;
            }, IntPtr.Zero);

            WindowListBox.ItemsSource = windows; // Fenster in der ListBox anzeigen
        }

        // Anordnen der ausgewählten Fenster
        private void TileButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedWindows = WindowListBox.SelectedItems.Cast<WindowInfo>().ToList();

            if (!selectedWindows.Any())
            {
                MessageBox.Show("Bitte wählen Sie mindestens ein Fenster zur Anordnung aus.");
                return;
            }

            // Bildschirmgröße abrufen
            int screenWidth = (int)SystemParameters.WorkArea.Width;
            int screenHeight = (int)SystemParameters.WorkArea.Height;

            // Fenster in ein Raster anordnen
            int numWindows = selectedWindows.Count;
            int cols = (int)Math.Ceiling(Math.Sqrt(numWindows));
            int rows = (int)Math.Ceiling((double)numWindows / cols);

            int windowWidth = screenWidth / cols;
            int windowHeight = screenHeight / rows;

            for (int i = 0; i < numWindows; i++)
            {
                int row = i / cols;
                int col = i % cols;

                int x = col * windowWidth;
                int y = row * windowHeight;

                SetWindowPos(selectedWindows[i].Handle, IntPtr.Zero, x, y, windowWidth, windowHeight, SWP_NOZORDER | SWP_NOACTIVATE);
            }
        }
    }
}