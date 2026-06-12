import AsyncStorage from '@react-native-async-storage/async-storage';
import React, { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { useColorScheme } from 'react-native';
import { api } from '../services/api';
import type { ThemeMode } from '../types';

const THEME_KEY = '@darts_theme';

export const lightColors = {
  background: '#F4F6F8',
  surface: '#FFFFFF',
  surfaceAlt: '#EEF1F5',
  primary: '#C8102E',
  primaryDark: '#9B0C24',
  accent: '#FFD700',
  text: '#1A1A2E',
  textSecondary: '#6B7280',
  border: '#E5E7EB',
  success: '#16A34A',
  warning: '#F59E0B',
  error: '#DC2626',
  card: '#FFFFFF',
  tabBar: '#FFFFFF',
};

export const darkColors = {
  background: '#0D1117',
  surface: '#161B22',
  surfaceAlt: '#21262D',
  primary: '#E63946',
  primaryDark: '#C8102E',
  accent: '#FFD700',
  text: '#F0F6FC',
  textSecondary: '#8B949E',
  border: '#30363D',
  success: '#3FB950',
  warning: '#D29922',
  error: '#F85149',
  card: '#161B22',
  tabBar: '#0D1117',
};

export type Colors = typeof lightColors;

interface ThemeContextValue {
  colors: Colors;
  isDark: boolean;
  themeMode: ThemeMode;
  setThemeMode: (mode: ThemeMode) => Promise<void>;
}

const ThemeContext = createContext<ThemeContextValue | null>(null);

export function ThemeProvider({ children }: { children: React.ReactNode }) {
  const systemScheme = useColorScheme();
  const [themeMode, setThemeModeState] = useState<ThemeMode>('Dark');

  useEffect(() => {
    (async () => {
      try {
        const stored = await AsyncStorage.getItem(THEME_KEY);
        if (stored) {
          setThemeModeState(stored as ThemeMode);
          return;
        }
        const profile = await api.getProfile();
        setThemeModeState(profile.theme);
      } catch {
        // API may be unavailable on first launch
      }
    })();
  }, []);

  const isDark = themeMode === 'Dark' || (themeMode === 'System' && systemScheme === 'dark');
  const colors = isDark ? darkColors : lightColors;

  const setThemeMode = useCallback(async (mode: ThemeMode) => {
    setThemeModeState(mode);
    await AsyncStorage.setItem(THEME_KEY, mode);
    try {
      await api.updateProfile({ theme: mode });
    } catch {
      // Persist locally even if API fails
    }
  }, []);

  const value = useMemo(
    () => ({ colors, isDark, themeMode, setThemeMode }),
    [colors, isDark, themeMode, setThemeMode],
  );

  return <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>;
}

export function useTheme() {
  const ctx = useContext(ThemeContext);
  if (!ctx) throw new Error('useTheme must be used within ThemeProvider');
  return ctx;
}
