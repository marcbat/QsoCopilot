#!/bin/sh

# Remplacer les variables d'environnement dans les fichiers JS
if [ ! -z "$VITE_API_URL" ]; then
    echo "Configuring API URL to: $VITE_API_URL"
    # Remplacer dans tous les fichiers JS de dist
    find /usr/share/nginx/html -name "*.js" -exec sed -i "s|http://localhost:5041/api|$VITE_API_URL|g" {} \;
fi

# Démarrer nginx
nginx -g 'daemon off;'
