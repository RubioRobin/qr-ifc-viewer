using System;
using System.Linq;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using QrIfcPlugin.Models;
using QrIfcPlugin.Services;

namespace QrIfcPlugin.Commands
{
    /// <summary>
    /// Command to generate QR codes for selected elements
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class GenerateQrCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var uiDoc = uiApp.ActiveUIDocument;
            var doc = uiDoc.Document;

            try
            {
                // Load settings
                var settings = PluginSettings.Load();

                // Get current selection
                var selection = uiDoc.Selection;
                var selectedIds = selection.GetElementIds();

                if (selectedIds.Count == 0)
                {
                    TaskDialog.Show("QR IFC Viewer", "Selecteer eerst één of meerdere elementen.");
                    return Result.Cancelled;
                }

                // Filter to only valid elements (not views, sheets, etc.)
                var validElements = selectedIds
                    .Select(id => doc.GetElement(id))
                    .Where(e => e != null && e.Category != null && !(e is View))
                    .ToList();

                if (validElements.Count == 0)
                {
                    TaskDialog.Show("QR IFC Viewer", "Geen geldige elementen geselecteerd. Selecteer model elementen (geen views of sheets).");
                    return Result.Cancelled;
                }

                // Initialize services
                var apiService = new ApiService(settings);
                var qrService = new QrCodeService();

                int successCount = 0;
                int failCount = 0;
                string lastError = string.Empty;

                // Process each element
                foreach (var element in validElements)
                {
                    try
                    {
                        // Get IFC GlobalId from shared parameter
                        var ifcGlobalId = GetIfcGlobalId(element);

                        if (string.IsNullOrEmpty(ifcGlobalId))
                        {
                            // Fallback: use Revit UniqueId (temporary for MVP)
                            ifcGlobalId = element.UniqueId;
                            
                            TaskDialog.Show(
                                "Waarschuwing",
                                $"Element '{GetElementName(element)}' heeft geen IFC GlobalId parameter.\n\n" +
                                "Gebruik Revit UniqueId als fallback.\n\n" +
                                "Voor productie: voeg shared parameter 'IFC_GlobalId' toe aan elementen.",
                                TaskDialogCommonButtons.Ok
                            );
                        }

                        // Create token via API
                        var viewerUrl = System.Threading.Tasks.Task.Run(async () =>
                            await apiService.CreateTokenAsync(ifcGlobalId)
                        ).GetAwaiter().GetResult();

                        // Generate QR code
                        var qrImagePath = qrService.GenerateQrCodeToFile(viewerUrl, 512);

                        // Place QR code on current view
                        PlaceQrCodeOnView(doc, uiDoc.ActiveView, element, qrImagePath, settings);

                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        lastError = ex.Message;
                        System.Diagnostics.Debug.WriteLine($"Failed to process element {element.Id}: {ex}");
                    }
                }

                // Show result
                var resultMessage = $"QR codes gegenereerd:\n\n" +
                                  $"✓ Succesvol: {successCount}\n" +
                                  $"✗ Mislukt: {failCount}";

                if (failCount > 0)
                {
                    resultMessage += $"\n\nLaatste fout: {lastError}";
                }

                TaskDialog.Show("QR IFC Viewer", resultMessage);

                return successCount > 0 ? Result.Succeeded : Result.Failed;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                TaskDialog.Show("Error", $"Er is een fout opgetreden:\n\n{ex.Message}");
                return Result.Failed;
            }
        }

        /// <summary>
        /// Get IFC GlobalId from element's shared parameter
        /// </summary>
        private string? GetIfcGlobalId(Element element)
        {
            try
            {
                // Try to get shared parameter "IFC_GlobalId"
                var param = element.LookupParameter("IFC_GlobalId");
                if (param != null && param.HasValue && param.StorageType == StorageType.String)
                {
                    return param.AsString();
                }
            }
            catch
            {
                // Parameter doesn't exist
            }

            return null;
        }

        /// <summary>
        /// Get a readable name for the element
        /// </summary>
        private string GetElementName(Element element)
        {
            var name = element.Name;
            if (string.IsNullOrEmpty(name))
            {
                name = element.Category?.Name ?? "Unknown";
            }

            // Try to add Mark if available
            var markParam = element.LookupParameter("Mark");
            if (markParam != null && markParam.HasValue)
            {
                var mark = markParam.AsString();
                if (!string.IsNullOrEmpty(mark))
                {
                    name = $"{mark} - {name}";
                }
            }

            return name;
        }

        /// <summary>
        /// Place QR code annotation on the current view
        /// </summary>
        private void PlaceQrCodeOnView(Document doc, View view, Element element, string qrImagePath, PluginSettings settings)
        {
            using (var transaction = new Transaction(doc, "Place QR Code"))
            {
                transaction.Start();

                try
                {
                    // For MVP: Create a text note with instructions
                    // In production: Use Generic Annotation family with embedded image
                    
                    // Get element location
                    var location = GetElementLocation(element);
                    if (location == null)
                    {
                        throw new InvalidOperationException("Cannot determine element location");
                    }

                    // Create text note as placeholder
                    // TODO: Replace with Generic Annotation family that supports images
                    var textTypeId = doc.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType);
                    
                    if (textTypeId == ElementId.InvalidElementId)
                    {
                        throw new InvalidOperationException("No text note type available");
                    }

                    var elementName = GetElementName(element);
                    var noteText = $"QR CODE\n{elementName}\n(Image: {System.IO.Path.GetFileName(qrImagePath)})";

                    // Offset the text note from the element location
                    var offsetLocation = new XYZ(location.X + 5, location.Y + 5, location.Z);

                    var textNote = TextNote.Create(doc, view.Id, offsetLocation, noteText, textTypeId);

                    // Store QR image path in a parameter (for future reference)
                    // In production version, this would be embedded in the family

                    transaction.Commit();
                }
                catch
                {
                    transaction.RollBack();
                    throw;
                }
            }
        }

        /// <summary>
        /// Get the location point of an element
        /// </summary>
        private XYZ? GetElementLocation(Element element)
        {
            // Try LocationPoint
            if (element.Location is LocationPoint locationPoint)
            {
                return locationPoint.Point;
            }

            // Try LocationCurve
            if (element.Location is LocationCurve locationCurve)
            {
                var curve = locationCurve.Curve;
                return curve.Evaluate(0.5, true); // Midpoint
            }

            // Try bounding box center
            var bbox = element.get_BoundingBox(null);
            if (bbox != null)
            {
                return (bbox.Min + bbox.Max) / 2;
            }

            return null;
        }
    }
}
