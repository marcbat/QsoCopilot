/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_URL: string
  // ajouter d'autres variables d'environnement ici
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
