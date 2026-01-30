using System;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using QrIfcPlugin.UI;

namespace QrIfcPlugin.Commands
{
    /// <summary>
    /// Command to open settings dialog
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SettingsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var dialog = new SettingsWindow();
                var result = dialog.ShowDialog();

                return result == true ? Result.Succeeded : Result.Cancelled;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                TaskDialog.Show("Error", $"Failed to open settings:\n\n{ex.Message}");
                return Result.Failed;
            }
        }
    }
}
