# QR IFC Web Viewer

Web-based IFC model viewer frontend optimized for mobile usage via QR codes.

## Features

- 🏗️ 3D IFC Model rendering (web-ifc-viewer)
- 📱 Mobile-responsive design
- 🔍 Element isolation via GlobalId
- 🔐 Token-based access control

## Local Development

```bash
cd viewer
npm install
npm run dev
```

The viewer will start at `http://localhost:3000`.

## Deployment (Vercel)

This project is configured for Vercel deployment.

1. Push code to GitHub
2. Import project in Vercel
3. Set Environment Variables:
   - `VITE_API_BASE_URL`: URL of your backend API (e.g., `https://qr-ifc-api.onrender.com`)

## URL Structure

The viewer expects URLs in the format:
`/view/<TOKEN>`

Example:
`https://qr-ifc-viewer.vercel.app/view/abc-123-xyz`

## License

MIT
