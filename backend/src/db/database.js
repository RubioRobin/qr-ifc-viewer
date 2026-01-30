import initSqlJs from 'sql.js';
import { readFileSync, writeFileSync, existsSync, mkdirSync } from 'fs';
import { dirname } from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

class DatabaseManager {
    constructor(dbPath) {
        this.dbPath = dbPath;
        this.db = null;
        this.initialized = false;
    }

    async initialize() {
        if (this.initialized) return;

        const SQL = await initSqlJs();

        // Load existing database or create new one
        if (existsSync(this.dbPath)) {
            const buffer = readFileSync(this.dbPath);
            this.db = new SQL.Database(buffer);
        } else {
            // Ensure directory exists
            mkdirSync(dirname(this.dbPath), { recursive: true });
            this.db = new SQL.Database();
        }

        this.initializeSchema();
        this.save();
        this.initialized = true;
    }

    initializeSchema() {
        const schemaPath = new URL('./schema.sql', import.meta.url);
        const schema = readFileSync(schemaPath, 'utf-8');
        this.db.exec(schema);
    }

    save() {
        const data = this.db.export();
        writeFileSync(this.dbPath, data);
    }

    // Projects
    createProject(slug, name) {
        this.db.run('INSERT INTO projects (slug, name) VALUES (?, ?)', [slug, name]);
        this.save();
        return { lastID: this.db.exec('SELECT last_insert_rowid()')[0].values[0][0] };
    }

    getProjectBySlug(slug) {
        const result = this.db.exec('SELECT * FROM projects WHERE slug = ?', [slug]);
        if (result.length === 0 || result[0].values.length === 0) return null;

        const row = result[0].values[0];
        return {
            id: row[0],
            slug: row[1],
            name: row[2],
            created_at: row[3]
        };
    }

    // Model Versions
    createModelVersion(projectId, version, ifcFileUrl) {
        this.db.run(
            'INSERT INTO model_versions (project_id, version, ifc_file_url) VALUES (?, ?, ?)',
            [projectId, version, ifcFileUrl]
        );
        this.save();
        return { lastID: this.db.exec('SELECT last_insert_rowid()')[0].values[0][0] };
    }

    getModelVersion(projectId, version) {
        const result = this.db.exec(
            'SELECT * FROM model_versions WHERE project_id = ? AND version = ?',
            [projectId, version]
        );
        if (result.length === 0 || result[0].values.length === 0) return null;

        const row = result[0].values[0];
        return {
            id: row[0],
            project_id: row[1],
            version: row[2],
            ifc_file_url: row[3],
            created_at: row[4]
        };
    }

    getLatestModelVersion(projectId) {
        const result = this.db.exec(
            'SELECT * FROM model_versions WHERE project_id = ? ORDER BY created_at DESC LIMIT 1',
            [projectId]
        );
        if (result.length === 0 || result[0].values.length === 0) return null;

        const row = result[0].values[0];
        return {
            id: row[0],
            project_id: row[1],
            version: row[2],
            ifc_file_url: row[3],
            created_at: row[4]
        };
    }

    // Viewer Tokens
    createToken(token, projectId, modelVersionId, ifcGlobalId, expiresAt) {
        this.db.run(
            'INSERT INTO viewer_tokens (token, project_id, model_version_id, ifc_global_id, expires_at) VALUES (?, ?, ?, ?, ?)',
            [token, projectId, modelVersionId, ifcGlobalId, expiresAt]
        );
        this.save();
        return { lastID: this.db.exec('SELECT last_insert_rowid()')[0].values[0][0] };
    }

    getTokenData(token) {
        const result = this.db.exec(`
      SELECT 
        vt.token,
        vt.ifc_global_id,
        vt.expires_at,
        p.slug as project_slug,
        p.name as project_name,
        mv.version as model_version,
        mv.ifc_file_url
      FROM viewer_tokens vt
      JOIN projects p ON vt.project_id = p.id
      JOIN model_versions mv ON vt.model_version_id = mv.id
      WHERE vt.token = ?
    `, [token]);

        if (result.length === 0 || result[0].values.length === 0) return null;

        const row = result[0].values[0];
        return {
            token: row[0],
            ifc_global_id: row[1],
            expires_at: row[2],
            project_slug: row[3],
            project_name: row[4],
            model_version: row[5],
            ifc_file_url: row[6]
        };
    }

    deleteExpiredTokens() {
        this.db.run('DELETE FROM viewer_tokens WHERE expires_at < datetime("now")');
        this.save();
        return { changes: 1 }; // sql.js doesn't provide affected rows count
    }

    close() {
        if (this.db) {
            this.save();
            this.db.close();
        }
    }
}

export default DatabaseManager;
