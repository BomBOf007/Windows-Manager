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

        // Fensterliste zur Verwaltung der Fenster und deren Handles
        private Dictionary<string, IntPtr> availableWindows = new Dictionary<string, IntPtr>();

        public MainWindow()
        {
            InitializeComponent();
            LoadAvailableWindows();
            SizeChanged += MainWindow_SizeChanged;
        }

        // Klasse zur Darstellung von Fenstern in der ListBox
        public class WindowInfo
        {
            public IntPtr Handle { get; set; }
            public string Title { get; set; }
        }

        // Methode zum Laden aller offenen Fenster
        private void LoadAvailableWindows()
        {
            availableWindows.Clear();
            WindowListBox.Items.Clear();

            EnumWindows((hWnd, lParam) =>
            {
                if (IsWindowVisible(hWnd))
                {
                    StringBuilder windowTitle = new StringBuilder(256);
                    GetWindowText(hWnd, windowTitle, 256);

                    if (windowTitle.Length > 0)
                    {
                        // Fenster zur Liste hinzufügen
                        string title = windowTitle.ToString();
                        availableWindows[title] = hWnd;
                        WindowListBox.Items.Add(title);
                    }
                }
                return true; // Weiter alle Fenster durchgehen
            }, IntPtr.Zero);
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
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateWindowSizes();
        }

        // Anpassung der Fenster an die Rasterzonen
        private void UpdateWindowSizes()
        {
            // Position und Größe der Bereiche erhalten
            var area1 = WindowArea1.TransformToAncestor(this).Transform(new Point(0, 0));
            var area2 = WindowArea2.TransformToAncestor(this).Transform(new Point(0, 0));
            var area3 = WindowArea3.TransformToAncestor(this).Transform(new Point(0, 0));
            var area4 = WindowArea4.TransformToAncestor(this).Transform(new Point(0, 0));

            // Fenster in den Rasterzonen neu positionieren und skalieren
            SetWindowPos(windowHandle1, IntPtr.Zero, (int)area1.X, (int)area1.Y, (int)WindowArea1.ActualWidth, (int)WindowArea1.ActualHeight, SWP_NOZORDER | SWP_NOACTIVATE);
            SetWindowPos(windowHandle2, IntPtr.Zero, (int)area2.X, (int)area2.Y, (int)WindowArea2.ActualWidth, (int)WindowArea2.ActualHeight, SWP_NOZORDER | SWP_NOACTIVATE);
            SetWindowPos(windowHandle3, IntPtr.Zero, (int)area3.X, (int)area3.Y, (int)WindowArea3.ActualWidth, (int)WindowArea3.ActualHeight, SWP_NOZORDER | SWP_NOACTIVATE);
            SetWindowPos(windowHandle4, IntPtr.Zero, (int)area4.X, (int)area4.Y, (int)WindowArea4.ActualWidth, (int)WindowArea4.ActualHeight, SWP_NOZORDER | SWP_NOACTIVATE);
        }

        // Methode zur Zuweisung eines Fensters zu einer Zone
        private void AssignWindowToZone(IntPtr hWnd, Border zone)
        {
            var area = zone.TransformToAncestor(this).Transform(new Point(0, 0));
            SetWindowPos(hWnd, IntPtr.Zero, (int)area.X, (int)area.Y, (int)zone.ActualWidth, (int)zone.ActualHeight, SWP_NOZORDER | SWP_NOACTIVATE);
        }
        private void ArrangeSelectedWindow(object sender, RoutedEventArgs e)
        {
            if (WindowListBox.SelectedItem == null)
            {
                MessageBox.Show("Bitte wählen Sie ein Fenster aus der Liste aus.");
                return;
            }

            // Fenster-Handle des ausgewählten Fensters
            string selectedWindow = WindowListBox.SelectedItem.ToString();
            if (!availableWindows.TryGetValue(selectedWindow, out IntPtr hWnd))
            {
                MessageBox.Show("Fenster nicht verfügbar.");
                return;
            }

            // Benutzer wählt Zone aus (in diesem Beispiel als Dialog oder festgelegt)
            Border targetZone = PromptForZone();
            if (targetZone != null)
            {
                AssignWindowToZone(hWnd, targetZone);
            }
        }
        // Methode zur Auswahl einer Zone (z. B. festgelegt oder Benutzerabfrage)
        private Border PromptForZone()
        {
            // Beispielhaft auf Zone 1 festgelegt
            return WindowArea1;
        }
    }
}