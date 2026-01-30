# QR IFC Viewer Backend API

Backend API server for the QR IFC Viewer system. Manages token generation and resolution for viewing IFC elements via QR codes.

## Features

- 🔐 Secure token generation with expiry
- 📊 SQLite database for projects and tokens
- 🌐 RESTful API endpoints
- ⏰ Automatic token cleanup
- 🚀 Ready for Render.com deployment

## API Endpoints

### Create Token
```http
POST /api/tokens
Content-Type: application/json

{
  "projectSlug": "sample-office-building",
  "ifcGlobalId": "2O2Fr$t4X7Zf8NOew3FLOH",
  "modelVersion": "v1.0",
  "expiryDays": 90
}
```

Response:
```json
{
  "viewerUrl": "https://qr-ifc-viewer.vercel.app/view/abc123...",
  "token": "abc123..."
}
```

### Resolve Token
```http
GET /api/tokens/:token
```

Response:
```json
{
  "projectSlug": "sample-office-building",
  "projectName": "Sample Office Building",
  "modelVersion": "v1.0",
  "ifcFileUrl": "https://example.com/model.ifc",
  "ifcGlobalId": "2O2Fr$t4X7Zf8NOew3FLOH",
  "expiresAt": "2024-04-30T12:00:00.000Z"
}
```

### Health Check
```http
GET /api/health
```

## Local Development

1. Install dependencies:
```bash
npm install
```

2. Create `.env` file:
```env
PORT=5000
DATABASE_PATH=./data/qr-ifc-viewer.db
VIEWER_BASE_URL=http://localhost:3000
CORS_ORIGIN=http://localhost:3000
```

3. Seed database:
```bash
node src/seed.js
```

4. Start server:
```bash
npm start
```

## Deployment to Render.com

1. Push code to GitHub
2. Create new Web Service on Render.com
3. Connect GitHub repository
4. Render will auto-detect and deploy
5. Set environment variables in Render dashboard

## Environment Variables

- `PORT` - Server port (default: 5000)
- `DATABASE_PATH` - SQLite database file path
- `VIEWER_BASE_URL` - Base URL of the web viewer
- `CORS_ORIGIN` - Allowed CORS origin

## Database Schema

- **projects** - Project information
- **model_versions** - IFC model versions per project
- **viewer_tokens** - Generated tokens with expiry

## License

MIT
