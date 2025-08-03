# Environment Configuration

This frontend uses environment variables to configure URLs and settings for different deployment environments.

## Environment Files

### `.env` (Default)
Contains safe default values used when no environment-specific file is present.

### `.env.development` (Development)
Used automatically when running `npm run dev`. Contains localhost URLs for local development.

### `.env.production` (Production)
Used automatically when running `npm run build`. Contains production URLs that should be updated for your deployment.

### `.env.staging.template` (Staging Template)
Copy this to `.env.staging` and update with your staging environment URLs.

## Available Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `VITE_API_BASE_URL` | Base URL for the API server | `http://localhost:3001` |
| `VITE_GAME_SERVER_BASE_URL` | Base URL for the game server | `http://localhost:5033` |
| `VITE_APP_NAME` | Application name | `Villagers Game` |
| `VITE_APP_VERSION` | Application version | `0.1.0` |
| `VITE_DEBUG_MODE` | Enable debug logging | `false` |
| `VITE_LOG_LEVEL` | Log level (debug/info/warn/error) | `info` |

## Usage

### Development
```bash
npm run dev
# Uses .env.development automatically
```

### Production Build
```bash
npm run build
# Uses .env.production automatically
```

### Custom Environment
```bash
# Create custom environment file
cp .env.staging.template .env.staging

# Edit with your values
VITE_API_BASE_URL=https://api-staging.example.com
VITE_GAME_SERVER_BASE_URL=wss://game-staging.example.com

# Use it
NODE_ENV=staging npm run build
```

### Runtime Override
You can override any variable at build time:
```bash
VITE_API_BASE_URL=https://custom-api.com npm run build
```

## Security Notes

- All `VITE_` prefixed variables are **public** and will be included in the client bundle
- Never put sensitive data (API keys, secrets) in environment variables
- The `.env.local` files are gitignored for local overrides
- Staging and production URLs should be reviewed before deployment

## Configuration Access

The configuration is centralized in `src/config/env.ts`:

```typescript
import { config, urls } from './config/env';

// Access configuration
console.log(config.apiBaseUrl);        // http://localhost:3001
console.log(urls.auth.login);          // http://localhost:3001/api/auth/login

// Environment detection
if (config.isDevelopment) {
  console.log('Running in development mode');
}
```

## Adding New Variables

1. Add to `.env` files:
```bash
VITE_NEW_FEATURE_URL=https://feature.example.com
```

2. Add TypeScript type in `src/vite-env.d.ts`:
```typescript
interface ImportMetaEnv {
  readonly VITE_NEW_FEATURE_URL: string;
  // ... other vars
}
```

3. Add to configuration in `src/config/env.ts`:
```typescript
export const config = {
  newFeatureUrl: getEnvVar('VITE_NEW_FEATURE_URL'),
  // ... other config
};
```