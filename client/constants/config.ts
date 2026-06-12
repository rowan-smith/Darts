import Constants from 'expo-constants';

/** Ensures the URL ends with /api (e.g. http://192.168.1.100:5000 → http://192.168.1.100:5000/api) */
export function normalizeApiBaseUrl(url: string): string {
  const trimmed = url.trim().replace(/\/$/, '');
  return trimmed.endsWith('/api') ? trimmed : `${trimmed}/api`;
}

const getApiUrl = (): string => {
  const fromEnv =
    process.env.EXPO_PUBLIC_API_BASE_URL ?? process.env.EXPO_PUBLIC_API_URL;

  if (fromEnv) {
    return normalizeApiBaseUrl(fromEnv);
  }

  const hostUri = Constants.expoConfig?.hostUri;
  if (hostUri) {
    const host = hostUri.split(':')[0];
    return `http://${host}:5000/api`;
  }

  return 'http://localhost:5000/api';
};

export const API_BASE_URL = getApiUrl();

if (__DEV__) {
  console.log('[API] Base URL:', API_BASE_URL);
}
