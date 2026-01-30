import { Color } from 'three';
import { IfcViewerAPI } from 'web-ifc-viewer';

// Configuration
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

// State
let viewer = null;
let currentToken = null;
let currentModel = null;

// DOM Elements
const loadingScreen = document.getElementById('loading-screen');
const loadingStatus = document.getElementById('loading-status');
const errorScreen = document.getElementById('error-screen');
const errorMessage = document.getElementById('error-message');
const viewerContainer = document.getElementById('viewer-container');
const infoPanel = document.getElementById('info-panel');

// Helper for Render cold starts
async function fetchWithRetry(url, options = {}, retries = 3, backoff = 1000) {
    try {
        return await fetch(url, options);
    } catch (err) {
        if (retries <= 1) throw err;
        loadingStatus.textContent = `Server wordt wakker... (poging ${4 - retries}/3)`;
        await new Promise(r => setTimeout(r, backoff));
        return fetchWithRetry(url, options, retries - 1, backoff * 2);
    }
}

async function init() {
    try {
        // 1. Get token from URL path (e.g., /view/:token)
        // For development/testing, we might check query param as fallback
        const pathParts = window.location.pathname.split('/');
        const tokenIndex = pathParts.indexOf('view');
        currentToken = tokenIndex > -1 ? pathParts[tokenIndex + 1] : new URLSearchParams(window.location.search).get('token');

        if (!currentToken) {
            throw new Error('Geen token gevonden in de URL. Scan de QR code opnieuw.');
        }

        loadingStatus.textContent = 'Verbinding maken met server...';

        // 2. Fetch token data from backend with retry
        const response = await fetchWithRetry(`${API_BASE_URL}/api/tokens/${currentToken}`);

        if (!response.ok) {
            if (response.status === 404) throw new Error('Deze link is verlopen of ongeldig.');
            throw new Error('Kon geen verbinding maken met de server.');
        }
        const data = await response.json();

        // 3. Update UI with project info
        document.getElementById('project-name').textContent = data.projectName;
        document.getElementById('model-version').textContent = data.modelVersion;

        // 4. Initialize Viewer
        loadingStatus.textContent = 'Viewer starten...';
        await initViewer();

        // 5. Load IFC Model
        loadingStatus.textContent = '3D Model downloaden...';
        await loadModel(data.ifcFileUrl);

        // 6. Highlight Element
        loadingStatus.textContent = 'Element zoeken...';
        await isolateElement(data.ifcGlobalId);

        // 7. Show Viewer
        loadingScreen.classList.add('hidden');
        viewerContainer.classList.remove('hidden');

    } catch (error) {
        // Collect diagnostics
        let diag = `\nAPI: ${API_BASE_URL}`;
        try {
            const wasmResp = await fetch('/web-ifc.wasm', { method: 'HEAD' });
            diag += `\nWASM: ${wasmResp.status}`;

            // Only check model if we actually got data back (Fixes "data is not defined")
            // using checking for variable existence implicitly via scope availability of 'data' isn't safe if definition skipped
            // but here 'data' variable is block scoped in try, so it might not be available in catch.
            // We'll skip model diag if data wasn't reached.
        } catch (e) {
            diag += `\nDiag failed: ${e.message}`;
        }

        showError(`${error.message}${diag}`);
    }
}

async function initViewer() {
    const container = document.getElementById('viewer-canvas');
    // Use dark background for better contrast with typical IFC models
    viewer = new IfcViewerAPI({ container, backgroundColor: new Color(0x202020) });

    // Explicitly set WASM path to root (where we copied files)
    viewer.IFC.setWasmPath(window.location.origin + '/');

    // Set up axes and grid for better context
    viewer.axes.setAxes();
    viewer.grid.setGrid();

    // Add some basic interaction
    window.onmousemove = viewer.IFC.selector.prePickIfcItem;
    window.onclick = async () => {
        const result = await viewer.IFC.selector.pickIfcItem();
        if (!result) viewer.IFC.selector.unpickIfcItems();
    };
}

async function loadModel(url) {
    // Check if file exists first
    const check = await fetch(url, { method: 'HEAD' });
    if (!check.ok) throw new Error(`Model bestand niet gevonden (Status ${check.status})`);

    currentModel = await viewer.IFC.loadIfcUrl(url);
    if (!currentModel) throw new Error('Model laden mislukt (Geen 3D data ontvangen).');

    // Remove shadows for now to ensure visibility
    // viewer.shadowDropper.renderShadow(currentModel.modelID);

    // Force camera to look at the model immediately
    setTimeout(() => {
        if (viewer && currentModel) {
            viewer.context.getIfcCamera().cameraControls.fitToBox(currentModel, true);
        }
    }, 100);
}

document.getElementById('toggle-fullscreen').onclick = () => {
    if (!document.fullscreenElement) {
        document.documentElement.requestFullscreen();
    } else {
        document.exitFullscreen();
    }
};

document.getElementById('close-panel').onclick = () => {
    infoPanel.classList.remove('visible');
};

document.getElementById('reset-camera').onclick = () => {
    if (viewer && currentModel) {
        viewer.context.getIfcCamera().cameraControls.fitToBox(currentModel, true);
    }
};

function showError(msg) {
    loadingScreen.classList.add('hidden');
    errorScreen.classList.remove('hidden');
    // Ensure we don't duplicate diagnostics if they are already in msg
    errorMessage.textContent = msg;
}

async function isolateElement(globalId) {
    if (!viewer || !currentModel) return;

    // TODO: Implement exact GlobalId -> ExpressID lookup
    // For now, center model
    viewer.context.getIfcCamera().cameraControls.fitToBox(currentModel, true);

    // Set info panel
    document.getElementById('element-guid').textContent = globalId;
    document.getElementById('info-panel').classList.add('visible');
}

// Start
init();
