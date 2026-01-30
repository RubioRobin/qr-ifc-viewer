import express from 'express';
import cors from 'cors';
import dotenv from 'dotenv';
import DatabaseManager from './db/database.js';
import TokenService from './services/tokenService.js';
import setupTokenRoutes from './routes/tokens.js';

// Load environment variables
dotenv.config();

const app = express();
const PORT = process.env.PORT || 5000;
const DATABASE_PATH = process.env.DATABASE_PATH || './data/qr-ifc-viewer.db';
const VIEWER_BASE_URL = process.env.VIEWER_BASE_URL || 'http://localhost:3000';
const CORS_ORIGIN = process.env.CORS_ORIGIN || 'http://localhost:3000';

// Middleware
app.use(cors({
    origin: CORS_ORIGIN,
    credentials: true
}));
app.use(express.json());

// Request logging
app.use((req, res, next) => {
    console.log(`${new Date().toISOString()} - ${req.method} ${req.path}`);
    next();
});

// Root endpoint
app.get('/', (req, res) => {
    res.json({
        name: 'QR IFC Viewer API',
        version: '1.0.0',
        endpoints: {
            health: '/api/health',
            createToken: 'POST /api/tokens',
            resolveToken: 'GET /api/tokens/:token'
        }
    });
});

// Error handling
app.use((err, req, res, next) => {
    console.error('Error:', err);
    res.status(500).json({ error: 'Internal server error' });
});

// Start server
async function startServer() {
    // Initialize database and services
    const db = new DatabaseManager(DATABASE_PATH);
    await db.initialize();
    const tokenService = new TokenService(db);

    // Routes
    app.use('/api', setupTokenRoutes(tokenService, VIEWER_BASE_URL));

    // Cleanup expired tokens every hour
    setInterval(() => {
        const deleted = tokenService.cleanupExpiredTokens();
        if (deleted.changes > 0) {
            console.log(`Cleaned up ${deleted.changes} expired tokens`);
        }
    }, 60 * 60 * 1000);

    app.listen(PORT, () => {
        console.log(`🚀 QR IFC Viewer API running on http://localhost:${PORT}`);
        console.log(`📊 Database: ${DATABASE_PATH}`);
        console.log(`🌐 Viewer URL: ${VIEWER_BASE_URL}`);
        console.log(`🔒 CORS Origin: ${CORS_ORIGIN}`);
    });

    // Graceful shutdown
    process.on('SIGINT', () => {
        console.log('\nShutting down gracefully...');
        db.close();
        process.exit(0);
    });
}

startServer().catch(err => {
    console.error('Failed to start server:', err);
    process.exit(1);
});
