-- Projects table
CREATE TABLE IF NOT EXISTS projects (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  slug TEXT UNIQUE NOT NULL,
  name TEXT NOT NULL,
  created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Model versions table
CREATE TABLE IF NOT EXISTS model_versions (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  project_id INTEGER NOT NULL,
  version TEXT NOT NULL,
  ifc_file_url TEXT NOT NULL,
  created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (project_id) REFERENCES projects(id),
  UNIQUE(project_id, version)
);

-- Viewer tokens table
CREATE TABLE IF NOT EXISTS viewer_tokens (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  token TEXT UNIQUE NOT NULL,
  project_id INTEGER NOT NULL,
  model_version_id INTEGER NOT NULL,
  ifc_global_id TEXT NOT NULL,
  expires_at DATETIME NOT NULL,
  created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (project_id) REFERENCES projects(id),
  FOREIGN KEY (model_version_id) REFERENCES model_versions(id)
);

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_tokens_token ON viewer_tokens(token);
CREATE INDEX IF NOT EXISTS idx_tokens_expires ON viewer_tokens(expires_at);
CREATE INDEX IF NOT EXISTS idx_projects_slug ON projects(slug);
