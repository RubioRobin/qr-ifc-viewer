using System;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

namespace QrIfcPlugin
{
    /// <summary>
    /// Main application class for the QR IFC Viewer plugin
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class QrIfcApplication : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                // Create ribbon tab
                const string tabName = "QR Viewer";
                try
                {
                    application.CreateRibbonTab(tabName);
                }
                catch
                {
                    // Tab might already exist
                }

                // Create ribbon panel
                var panel = application.CreateRibbonPanel(tabName, "QR");

                // Add "QR voor Selectie" button
                var generateQrButton = CreatePushButton(
                    panel,
                    "GenerateQr",
                    "QR voor\nSelectie",
                    "Genereer QR codes voor geselecteerde elementen",
                    typeof(Commands.GenerateQrCommand),
                    "qr_icon.png"
                );

                // Add separator
                panel.AddSeparator();

                // Add "Instellingen" button
                var settingsButton = CreatePushButton(
                    panel,
                    "Settings",
                    "Instellingen",
                    "Plugin instellingen configureren",
                    typeof(Commands.SettingsCommand),
                    "settings_icon.png"
                );

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Failed to initialize QR IFC Viewer plugin:\n\n{ex.Message}");
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            // Cleanup if needed
            return Result.Succeeded;
        }

        /// <summary>
        /// Create a push button on the ribbon
        /// </summary>
        private PushButton CreatePushButton(
            RibbonPanel panel,
            string name,
            string text,
            string tooltip,
            Type commandType,
            string iconName)
        {
            var buttonData = new PushButtonData(
                name,
                text,
                Assembly.GetExecutingAssembly().Location,
                commandType.FullName
            );

            var button = panel.AddItem(buttonData) as PushButton;
            
            if (button != null)
            {
                button.ToolTip = tooltip;

                // Try to load icon
                try
                {
                    var iconPath = System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "",
                        "Resources",
                        iconName
                    );

                    if (System.IO.File.Exists(iconPath))
                    {
                        button.LargeImage = new BitmapImage(new Uri(iconPath));
                    }
                }
                catch
                {
                    // Icon loading failed, continue without icon
                }
            }

            return button!;
        }
    }
}
