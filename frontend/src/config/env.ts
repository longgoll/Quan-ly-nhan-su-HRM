export const config = {
  API_BASE_URL: import.meta.env.VITE_API_BASE_URL || 'https://localhost:7093/api',
  APP_NAME: 'HRM System',
  APP_VERSION: '1.0.0',
} as const;
