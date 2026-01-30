import Database from 'better-sqlite3';
import { readFileSync } from 'fs';
import { fileURLToPath } from 'url';
import { dirname, join } from 'path';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

class DatabaseManager {
    constructor(dbPath) {
        this.db = new Database(dbPath);
        this.db.pragma('journal_mode = WAL');
        this.initializeSchema();
    }

    initializeSchema() {
        const schemaPath = join(__dirname, 'schema.sql');
        const schema = readFileSync(schemaPath, 'utf-8');
        this.db.exec(schema);
    }

    // Projects
    createProject(slug, name) {
        const stmt = this.db.prepare('INSERT INTO projects (slug, name) VALUES (?, ?)');
        return stmt.run(slug, name);
    }

    getProjectBySlug(slug) {
        const stmt = this.db.prepare('SELECT * FROM projects WHERE slug = ?');
        return stmt.get(slug);
    }

    // Model Versions
    createModelVersion(projectId, version, ifcFileUrl) {
        const stmt = this.db.prepare(
            'INSERT INTO model_versions (project_id, version, ifc_file_url) VALUES (?, ?, ?)'
        );
        return stmt.run(projectId, version, ifcFileUrl);
    }

    getModelVersion(projectId, version) {
        const stmt = this.db.prepare(
            'SELECT * FROM model_versions WHERE project_id = ? AND version = ?'
        );
        return stmt.get(projectId, version);
    }

    getLatestModelVersion(projectId) {
        const stmt = this.db.prepare(
            'SELECT * FROM model_versions WHERE project_id = ? ORDER BY created_at DESC LIMIT 1'
        );
        return stmt.get(projectId);
    }

    // Viewer Tokens
    createToken(token, projectId, modelVersionId, ifcGlobalId, expiresAt) {
        const stmt = this.db.prepare(
            'INSERT INTO viewer_tokens (token, project_id, model_version_id, ifc_global_id, expires_at) VALUES (?, ?, ?, ?, ?)'
        );
        return stmt.run(token, projectId, modelVersionId, ifcGlobalId, expiresAt);
    }

    getTokenData(token) {
        const stmt = this.db.prepare(`
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
    `);
        return stmt.get(token);
    }

    deleteExpiredTokens() {
        const stmt = this.db.prepare('DELETE FROM viewer_tokens WHERE expires_at < datetime("now")');
        return stmt.run();
    }

    close() {
        this.db.close();
    }
}

export default DatabaseManager;
