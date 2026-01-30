import express from 'express';

const router = express.Router();

export function setupTokenRoutes(tokenService, viewerBaseUrl) {
    // Create token
    router.post('/tokens', async (req, res) => {
        try {
            const { projectSlug, ifcGlobalId, modelVersion, expiryDays } = req.body;

            // Validation
            if (!projectSlug || !ifcGlobalId) {
                return res.status(400).json({
                    error: 'Missing required fields: projectSlug and ifcGlobalId are required'
                });
            }

            // Generate token
            const token = await tokenService.createViewerToken(
                projectSlug,
                ifcGlobalId,
                modelVersion || 'latest',
                expiryDays || 90
            );

            // Return viewer URL
            const viewerUrl = `${viewerBaseUrl}/view/${token}`;

            res.json({ viewerUrl, token });
        } catch (error) {
            console.error('Error creating token:', error);
            res.status(500).json({ error: error.message });
        }
    });

    // Resolve token
    router.get('/tokens/:token', (req, res) => {
        try {
            const { token } = req.params;

            const tokenData = tokenService.getTokenData(token);

            if (!tokenData) {
                return res.status(404).json({ error: 'Token not found or expired' });
            }

            res.json(tokenData);
        } catch (error) {
            console.error('Error resolving token:', error);
            res.status(500).json({ error: error.message });
        }
    });

    // Health check
    router.get('/health', (req, res) => {
        res.json({ status: 'ok', timestamp: new Date().toISOString() });
    });

    return router;
}

export default setupTokenRoutes;
