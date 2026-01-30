# Quick Start Guide - QR IFC Viewer Revit Plugin

Deze guide helpt je om snel te starten met de QR IFC Viewer plugin.

## 📋 Vereisten

Voordat je begint, zorg dat je het volgende hebt:

- ✅ Autodesk Revit 2024 of 2025 geïnstalleerd
- ✅ .NET Framework 4.8 (meestal al geïnstalleerd met Revit)
- ✅ Backend API draaiend (zie backend README)
- ✅ Webapp gedeployed (zie webapp README)

## 🚀 Installatie (5 minuten)

### Stap 1: Build de Plugin

Open PowerShell in de `revit-plugin` folder en run:

```powershell
.\build.ps1
```

Of met Visual Studio:
```powershell
.\build.ps1 -Configuration Release
```

De plugin wordt automatisch geïnstalleerd naar:
```
%APPDATA%\Autodesk\Revit\Addins\2024\
```

### Stap 2: Start Revit

1. Open Autodesk Revit 2024
2. Je ziet een nieuw tabblad **"QR Viewer"** in de ribbon
3. Als je een waarschuwing ziet over een niet-ondertekende add-in, klik op **"Altijd laden"**

## ⚙️ Eerste Configuratie (2 minuten)

### Stap 3: Open Instellingen

Klik op de **Instellingen** knop in de QR Viewer ribbon.

### Stap 4: Vul de Configuratie In

**API Instellingen:**
- **API Base URL**: `http://localhost:5000` (of je productie URL)
- **Viewer Base URL**: `http://localhost:3000` (of je productie URL)

**Project Instellingen:**
- **Project Slug**: `sample-project` (of je eigen project naam)
- **Default Model Versie**: `v1`
- **Token Verlooptijd**: `90` dagen

**QR Code Instellingen:**
- **QR Grootte**: `30` mm (minimaal 25mm voor betrouwbare scans)
- **Toon label**: ✅ (aangevinkt)

### Stap 5: Test de Verbinding

Klik op **"Test Verbinding"** om te controleren of de API bereikbaar is.

✅ **Verbinding succesvol** = Je bent klaar!  
❌ **Verbinding mislukt** = Controleer of de backend API draait.

Klik op **Opslaan**.

## 🎯 Je Eerste QR Code (1 minuut)

### Stap 6: Open een Revit Project

Open een bestaand Revit project met model elementen (muren, deuren, ramen, etc.).

### Stap 7: Selecteer een Element

Klik op een element in de 3D view of floor plan (bijvoorbeeld een muur).

### Stap 8: Genereer QR Code

Klik op **"QR voor Selectie"** in de QR Viewer ribbon.

De plugin zal:
1. ✅ Het element identificeren
2. ✅ Een token aanmaken via de API
3. ✅ Een QR code genereren
4. ✅ De QR code plaatsen op de view

Je ziet een bevestiging: **"QR codes gegenereerd: ✓ Succesvol: 1"**

### Stap 9: Test de QR Code

**Optie A: Direct testen (zonder printen)**

1. Maak een screenshot van de QR code in Revit
2. Open de screenshot op je computer
3. Scan de QR code met je telefoon
4. De webapp opent met het element!

**Optie B: Print en scan**

1. Print de view naar PDF
2. Print de PDF of open op tablet
3. Scan de QR code met je telefoon
4. De webapp opent met het element!

## 🎉 Gefeliciteerd!

Je hebt succesvol je eerste QR code gemaakt en getest!

## 📱 Volgende Stappen

### Meerdere Elementen

1. Selecteer meerdere elementen (Ctrl + klik)
2. Klik op **"QR voor Selectie"**
3. Alle elementen krijgen een QR code

### Sheets Voorbereiden voor Productie

1. Maak een sheet in Revit
2. Plaats views met elementen op de sheet
3. Selecteer elementen en genereer QR codes
4. Positioneer QR codes naast de elementen
5. Print de sheet → QR codes zijn scanbaar!

### Tips voor Beste Resultaten

✅ **QR Grootte**: Minimaal 25-30mm voor betrouwbare scans  
✅ **Contrast**: Zwart op wit werkt het beste  
✅ **Positie**: Plaats QR codes dicht bij het element  
✅ **Print Kwaliteit**: Gebruik minimaal 300 DPI  
✅ **Test**: Scan altijd een test print voordat je naar de site gaat

## ❓ Problemen?

### "Geen IFC GlobalId parameter"

**Wat betekent dit?**  
Het element heeft geen IFC GlobalId. De plugin gebruikt Revit UniqueId als fallback.

**Oplossing voor MVP:**  
Dit werkt, maar alleen als de backend een mapping heeft. Geen actie nodig voor testen.

**Oplossing voor Productie:**  
Voeg een shared parameter "IFC_GlobalId" toe aan je Revit template.

### "API returned 404"

**Wat betekent dit?**  
De backend kan het project niet vinden.

**Oplossing:**  
1. Controleer of de backend API draait
2. Controleer of het project bestaat in de database
3. Controleer de Project Slug in instellingen

### "Network error"

**Wat betekent dit?**  
De plugin kan geen verbinding maken met de API.

**Oplossing:**  
1. Controleer of de backend API draait (open `http://localhost:5000/health` in browser)
2. Controleer firewall instellingen
3. Controleer de API Base URL in instellingen

### QR Code niet scanbaar

**Wat betekent dit?**  
De QR scanner kan de code niet lezen.

**Oplossing:**  
1. Vergroot de QR grootte in instellingen (minimaal 30mm)
2. Zorg voor voldoende contrast (zwart op wit)
3. Probeer een andere QR scanner app
4. Controleer print kwaliteit

## 📚 Meer Informatie

- **Volledige documentatie**: Zie `README.md`
- **Troubleshooting**: Zie `README.md` → Troubleshooting sectie
- **API Documentatie**: Zie `backend/README.md`
- **Webapp Documentatie**: Zie `webapp/README.md`

## 🆘 Support

Voor vragen of problemen, neem contact op met VH Engineering.

---

**Veel succes met de QR IFC Viewer!** 🎉
