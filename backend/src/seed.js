import DatabaseManager from './db/database.js';
import dotenv from 'dotenv';

dotenv.config();

const DATABASE_PATH = process.env.DATABASE_PATH || './data/qr-ifc-viewer.db';
const db = new DatabaseManager(DATABASE_PATH);

console.log('🌱 Seeding database with sample data...\n');

// Create sample project
const projectSlug = 'sample-office-building';
const projectName = 'Sample Office Building';

try {
    db.createProject(projectSlug, projectName);
    console.log(`✅ Created project: ${projectName} (${projectSlug})`);
} catch (err) {
    console.log(`ℹ️  Project already exists: ${projectSlug}`);
}

const project = db.getProjectBySlug(projectSlug);

// Create sample model version with a public IFC file URL
const modelVersion = 'v1.0';
const ifcFileUrl = 'https://github.com/IFCjs/test-ifc-files/raw/main/Duplex_A_20110505.ifc';

try {
    db.createModelVersion(project.id, modelVersion, ifcFileUrl);
    console.log(`✅ Created model version: ${modelVersion}`);
    console.log(`   IFC File: ${ifcFileUrl}`);
} catch (err) {
    console.log(`ℹ️  Model version already exists: ${modelVersion}`);
}

console.log('\n✨ Database seeded successfully!');
console.log('\n📝 You can now:');
console.log('   1. Start the server: npm start');
console.log('   2. Test token creation with project slug:', projectSlug);
console.log('   3. Use any IFC GlobalId from the sample file\n');

db.close();
