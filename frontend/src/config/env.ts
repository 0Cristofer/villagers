// Environment configuration
// All environment variables must be prefixed with VITE_ to be accessible in the browser

interface EnvironmentConfig {
  // API Configuration
  apiBaseUrl: string;
  gameServerBaseUrl: string;
  
  // App Configuration
  appName: string;
  appVersion: string;
  
  // Development flags
  debugMode: boolean;
  logLevel: 'debug' | 'info' | 'warn' | 'error';
  
  // Environment info
  isDevelopment: boolean;
  isProduction: boolean;
}

// Helper function to get environment variable with validation
function getEnvVar(name: string, defaultValue?: string): string {
  const value = import.meta.env[name];
  
  if (!value && !defaultValue) {
    throw new Error(`Environment variable ${name} is required but not set`);
  }
  
  return value || defaultValue!;
}

// Helper function to get boolean environment variable
function getBooleanEnvVar(name: string, defaultValue: boolean = false): boolean {
  const value = import.meta.env[name];
  
  if (!value) {
    return defaultValue;
  }
  
  return value.toLowerCase() === 'true';
}

// Create and export the configuration object
export const config: EnvironmentConfig = {
  // API URLs
  apiBaseUrl: getEnvVar('VITE_API_BASE_URL', 'https://localhost:3002'),
  gameServerBaseUrl: getEnvVar('VITE_GAME_SERVER_BASE_URL', 'https://localhost:5034'),
  
  // App info
  appName: getEnvVar('VITE_APP_NAME', 'Villagers Game'),
  appVersion: getEnvVar('VITE_APP_VERSION', '0.1.0'),
  
  // Development settings
  debugMode: getBooleanEnvVar('VITE_DEBUG_MODE', false),
  logLevel: (getEnvVar('VITE_LOG_LEVEL', 'info') as EnvironmentConfig['logLevel']),
  
  // Environment detection
  isDevelopment: import.meta.env.DEV,
  isProduction: import.meta.env.PROD,
};

// Helper functions for common URL patterns
export const urls = {
  // Authentication endpoints
  auth: {
    login: `${config.apiBaseUrl}/api/auth/login`,
    register: `${config.apiBaseUrl}/api/auth/register`,
    validate: `${config.apiBaseUrl}/api/auth/validate`,
    refresh: `${config.apiBaseUrl}/api/auth/refresh`,
  },
  
  // Game server endpoints
  gameServer: {
    hub: `${config.gameServerBaseUrl}/gamehub`,
    health: `${config.gameServerBaseUrl}/health`,
  },
  
  // API endpoints
  api: {
    base: config.apiBaseUrl,
    health: `${config.apiBaseUrl}/health`,
  }
};

// Development helper - log configuration in dev mode
if (config.isDevelopment && config.debugMode) {
  console.log('🔧 App Configuration:', config);
  console.log('🌐 URLs:', urls);
}