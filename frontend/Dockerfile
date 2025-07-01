# Build stage
FROM node:20-alpine AS build

WORKDIR /app

# Copier les fichiers de configuration
COPY package*.json ./
COPY tsconfig*.json ./
COPY vite.config.ts ./

# Installer les dépendances
RUN npm ci

# Copier le code source
COPY src/ ./src/
COPY public/ ./public/
COPY index.html ./
COPY .env ./

# Build l'application
RUN npm run build

# Production stage
FROM nginx:alpine

# Copier la configuration nginx
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copier les fichiers buildés
COPY --from=build /app/dist /usr/share/nginx/html

# Exposer le port
EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
