# QR IFC Viewer - Revit Plugin

Production-ready Revit add-in voor het genereren van QR codes die direct linken naar specifieke IFC elementen in een web-based 3D viewer.

## Features

- ✅ Genereer QR codes voor geselecteerde Revit elementen
- ✅ Automatische koppeling met backend API voor token generatie
- ✅ Configureerbare instellingen (API URL, project, QR grootte)
- ✅ Support voor IFC GlobalId mapping
- ✅ Fallback naar Revit UniqueId voor MVP
- ✅ Gebruiksvriendelijke WPF interface

## Installatie

### Vereisten

- Autodesk Revit 2025
- .NET 8 Runtime (included with Revit 2025)
- Windows 10/11

### Stappen

1. **Build de plugin:**
   ```powershell
   cd revit-plugin\QrIfcPlugin
   dotnet build -c Release
   ```

2. **Automatische installatie:**
   De plugin wordt automatisch geïnstalleerd naar:
   ```
   %APPDATA%\Autodesk\Revit\Addins\2024\
   %APPDATA%\Autodesk\Revit\Addins\2025\
   ```
   
   > **Note:** De plugin wordt naar beide versies gekopieerd voor maximale compatibiliteit.

3. **Handmatige installatie (optioneel):**
   - Kopieer `QrIfcPlugin.dll` naar de Addins folder
   - Kopieer `QrIfcPlugin.addin` naar de Addins folder

4. **Start Revit:**
   - Open Revit
   - Zoek het "QR Viewer" tabblad in de ribbon

## Gebruik

### 1. Configureer Instellingen

Klik op **Instellingen** in de QR Viewer ribbon:

- **API Base URL**: URL van de backend API (bijv. `https://api.example.com`)
- **Viewer Base URL**: URL van de webapp (bijv. `https://viewer.example.com`)
- **Project Slug**: Unieke project identifier
- **Default Model Versie**: Versie tag (bijv. `v1`)
- **Token Verlooptijd**: Aantal dagen (standaard 90)
- **QR Grootte**: Grootte in mm (minimaal 25-30mm aanbevolen)

Test de verbinding met de **Test Verbinding** knop.

### 2. Genereer QR Codes

1. Selecteer één of meerdere elementen in Revit
2. Klik op **QR voor Selectie** in de QR Viewer ribbon
3. De plugin:
   - Haalt IFC GlobalId op (of gebruikt Revit UniqueId als fallback)
   - Maakt een token via de backend API
   - Genereert een QR code
   - Plaatst de QR code op de huidige view

### 3. Gebruik QR Codes

- Print de view/sheet met QR codes
- Scan de QR code met een telefoon
- De webapp opent automatisch met het juiste element geïsoleerd

## IFC GlobalId Mapping

### MVP Aanpak (Huidige Implementatie)

De plugin gebruikt **Revit UniqueId** als fallback wanneer geen IFC GlobalId beschikbaar is.

**Waarschuwing:** Dit werkt alleen als de backend een mapping heeft tussen Revit UniqueId en IFC GlobalId.

### Productie Aanpak (Aanbevolen)

Voeg een **Shared Parameter** toe aan je Revit template:

1. Maak een shared parameter file
2. Voeg parameter toe:
   - **Naam**: `IFC_GlobalId`
   - **Type**: Text
   - **Categorie**: Alle model categorieën
3. Vul de parameter tijdens IFC export of via script

## Troubleshooting

### "Geen IFC GlobalId parameter"

**Oplossing:** Voeg de shared parameter "IFC_GlobalId" toe aan je elementen, of zorg dat de backend een mapping heeft.

### "API returned 404"

**Oplossing:** 
- Controleer of de backend API draait
- Controleer of het project bestaat in de database
- Controleer de API Base URL in instellingen

### "Network error"

**Oplossing:**
- Controleer internetverbinding
- Controleer firewall instellingen
- Test de verbinding via Instellingen dialog

### QR Code niet scanbaar

**Oplossing:**
- Vergroot de QR grootte (minimaal 25-30mm)
- Zorg voor voldoende contrast bij printen
- Test met verschillende QR scanner apps

## Configuratie Bestanden

Settings worden opgeslagen in:
```
%APPDATA%\QrIfcPlugin\settings.json
```

## Development

### Project Structure

```
QrIfcPlugin/
├── Commands/
│   ├── GenerateQrCommand.cs    # Main QR generation logic
│   └── SettingsCommand.cs      # Settings dialog opener
├── Models/
│   └── PluginSettings.cs       # Settings model
├── Services/
│   ├── ApiService.cs           # Backend API client
│   └── QrCodeService.cs        # QR code generation
├── UI/
│   ├── SettingsWindow.xaml     # Settings dialog UI
│   └── SettingsWindow.xaml.cs  # Settings dialog logic
├── Resources/                   # Icons and assets
└── QrIfcApplication.cs         # Main entry point
```

### Building

```powershell
# Debug build
dotnet build

# Release build
dotnet build -c Release

# Clean
dotnet clean
```

### Debugging

1. Set Revit as startup program in project properties
2. Set breakpoints in code
3. Press F5 to start debugging
4. Revit will launch with plugin loaded

## Toekomstige Verbeteringen

- [ ] Generic Annotation family met embedded QR image (in plaats van text note)
- [ ] Batch processing met progress dialog
- [ ] QR code update functionaliteit
- [ ] Export QR codes naar aparte PDF
- [ ] Automatische IFC GlobalId sync tijdens export
- [ ] Multi-project support met dropdown
- [ ] QR code preview voor plaatsing

## Support

Voor vragen of problemen, neem contact op met VH Engineering.

## License

Copyright © 2026 VH Engineering. All rights reserved.
