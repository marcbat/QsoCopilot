const fs = require('fs');
const path = require('path');

// Fonction pour lire un fichier et extraire les imports
function getImports(filePath) {
    try {
        const content = fs.readFileSync(filePath, 'utf8');
        const imports = [];
        
        // Regex pour capturer les imports de composants
        const importRegex = /import\s+(?:{\s*([^}]+)\s*}|([^{}\s]+))\s+from\s+['"]\.\/([^'"]+)['"]/g;
        let match;
        
        while ((match = importRegex.exec(content)) !== null) {
            if (match[2]) {
                // Import par défaut
                imports.push({ component: match[2], from: match[3] });
            } else if (match[1]) {
                // Import nommé
                const namedImports = match[1].split(',').map(imp => imp.trim());
                namedImports.forEach(imp => {
                    imports.push({ component: imp, from: match[3] });
                });
            }
        }
        
        return imports;
    } catch (error) {
        console.error(`Erreur lors de la lecture de ${filePath}:`, error.message);
        return [];
    }
}

// Fonction pour obtenir tous les fichiers .tsx dans le dossier components
function getComponentFiles() {
    const componentsDir = path.join(__dirname, 'frontend', 'src', 'components');
    try {
        return fs.readdirSync(componentsDir)
            .filter(file => file.endsWith('.tsx'))
            .map(file => ({
                name: file.replace('.tsx', ''),
                path: path.join(componentsDir, file)
            }));
    } catch (error) {
        console.error('Erreur lors de la lecture du dossier components:', error.message);
        return [];
    }
}

// Fonction pour analyser tous les fichiers du projet
function analyzeProject() {
    const srcDir = path.join(__dirname, 'frontend', 'src');
    const allFiles = [];
    
    function scanDirectory(dir) {
        try {
            const items = fs.readdirSync(dir);
            items.forEach(item => {
                const itemPath = path.join(dir, item);
                const stat = fs.statSync(itemPath);
                
                if (stat.isDirectory()) {
                    scanDirectory(itemPath);
                } else if (item.endsWith('.tsx') || item.endsWith('.ts')) {
                    allFiles.push(itemPath);
                }
            });
        } catch (error) {
            console.error(`Erreur lors du scan de ${dir}:`, error.message);
        }
    }
    
    scanDirectory(srcDir);
    return allFiles;
}

// Analyse principale
console.log('=== ANALYSE DES COMPOSANTS NON UTILISÉS ===\n');

const componentFiles = getComponentFiles();
const allProjectFiles = analyzeProject();

// Collecter tous les imports de composants
const usedComponents = new Set();

allProjectFiles.forEach(file => {
    const imports = getImports(file);
    imports.forEach(imp => {
        usedComponents.add(imp.component);
    });
    
    // Vérifier aussi les imports depuis App.tsx et autres fichiers principaux
    try {
        const content = fs.readFileSync(file, 'utf8');
        
        // Imports depuis ./components/
        const componentImportRegex = /import\s+(?:{\s*([^}]+)\s*}|([^{}\s]+))\s+from\s+['"]\.\/components\/([^'"]+)['"]/g;
        let match;
        
        while ((match = componentImportRegex.exec(content)) !== null) {
            if (match[2]) {
                usedComponents.add(match[2]);
            } else if (match[1]) {
                const namedImports = match[1].split(',').map(imp => imp.trim());
                namedImports.forEach(imp => usedComponents.add(imp));
            }
        }
    } catch (error) {
        // Ignorer les erreurs de lecture
    }
});

// Identifier les composants non utilisés
const unusedComponents = componentFiles.filter(comp => !usedComponents.has(comp.name));

console.log('COMPOSANTS UTILISÉS:');
Array.from(usedComponents).sort().forEach(comp => {
    console.log(`  ✓ ${comp}`);
});

console.log('\nCOMPOSANTS NON UTILISÉS:');
if (unusedComponents.length === 0) {
    console.log('  Aucun composant non utilisé détecté.');
} else {
    unusedComponents.forEach(comp => {
        console.log(`  ✗ ${comp.name} (${comp.path})`);
    });
}

console.log(`\nRÉSUMÉ: ${usedComponents.size} composants utilisés, ${unusedComponents.length} composants non utilisés sur ${componentFiles.length} total.`);
