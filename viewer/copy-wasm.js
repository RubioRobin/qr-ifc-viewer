import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const publicDir = path.join(__dirname, 'public');
// web-ifc is a dependency of web-ifc-viewer
const wasmDir = path.join(__dirname, 'node_modules', 'web-ifc');

if (!fs.existsSync(publicDir)) {
    fs.mkdirSync(publicDir);
}

const files = ['web-ifc.wasm', 'web-ifc-mt.wasm'];

console.log('Copying WASM files...');

files.forEach(file => {
    const src = path.join(wasmDir, file);
    const dest = path.join(publicDir, file);

    // Check if source exists
    if (fs.existsSync(src)) {
        fs.copyFileSync(src, dest);
        console.log(`✅ Copied ${file} to public/`);
    } else {
        console.warn(`⚠️ Warning: Could not find ${src}. WASM files might be missing.`);
    }
});
