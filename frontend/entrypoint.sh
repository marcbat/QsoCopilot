#!/bin/sh

echo "Starting frontend container..."
echo "VITE_API_URL: $VITE_API_URL"

# Remplacer les variables d'environnement dans les fichiers JS
if [ ! -z "$VITE_API_URL" ]; then
    echo "Configuring API URL to: $VITE_API_URL"
    
    # Remplacer dans tous les fichiers JS et map de dist
    find /usr/share/nginx/html -type f \( -name "*.js" -o -name "*.js.map" \) -exec sed -i "s|http://localhost:5041/api|$VITE_API_URL|g" {} \;
    
    # Vérifier si le remplacement a fonctionné
    echo "Checking replacement in JS files..."
    grep -r "localhost:5041" /usr/share/nginx/html/ || echo "No localhost:5041 found - replacement successful"
    
else
    echo "VITE_API_URL not set, using default localhost:5041"
fi

echo "Starting nginx..."
# Démarrer nginx
nginx -g 'daemon off;'
