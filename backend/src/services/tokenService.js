import { nanoid } from 'nanoid';

class TokenService {
    constructor(db) {
        this.db = db;
    }

    generateToken() {
        // Generate URL-safe token (21 characters, ~149 bits of entropy)
        return nanoid();
    }

    async createViewerToken(projectSlug, ifcGlobalId, modelVersion = 'latest', expiryDays = 90) {
        // Get or create project
        let project = this.db.getProjectBySlug(projectSlug);
        if (!project) {
            const result = this.db.createProject(projectSlug, projectSlug);
            project = this.db.getProjectBySlug(projectSlug);
        }

        // Get model version
        let modelVersionRecord;
        if (modelVersion === 'latest') {
            modelVersionRecord = this.db.getLatestModelVersion(project.id);
        } else {
            modelVersionRecord = this.db.getModelVersion(project.id, modelVersion);
        }

        if (!modelVersionRecord) {
            throw new Error(`Model version '${modelVersion}' not found for project '${projectSlug}'`);
        }

        // Generate token
        const token = this.generateToken();

        // Calculate expiry date
        const expiresAt = new Date();
        expiresAt.setDate(expiresAt.getDate() + expiryDays);

        // Store token
        this.db.createToken(
            token,
            project.id,
            modelVersionRecord.id,
            ifcGlobalId,
            expiresAt.toISOString()
        );

        return token;
    }

    getTokenData(token) {
        const data = this.db.getTokenData(token);

        if (!data) {
            return null;
        }

        // Check if expired
        const expiresAt = new Date(data.expires_at);
        if (expiresAt < new Date()) {
            return null;
        }

        return {
            projectSlug: data.project_slug,
            projectName: data.project_name,
            modelVersion: data.model_version,
            ifcFileUrl: data.ifc_file_url,
            ifcGlobalId: data.ifc_global_id,
            expiresAt: data.expires_at
        };
    }

    cleanupExpiredTokens() {
        return this.db.deleteExpiredTokens();
    }
}

export default TokenService;
