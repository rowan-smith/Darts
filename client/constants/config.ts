import Constants from 'expo-constants';

const getApiUrl = () => {
  const hostUri = Constants.expoConfig?.hostUri;
  if (hostUri) {
    const host = hostUri.split(':')[0];
    return `http://${host}:8080/api`;
  }
  return 'http://localhost:8080/api';
};

export const API_BASE_URL = getApiUrl();
