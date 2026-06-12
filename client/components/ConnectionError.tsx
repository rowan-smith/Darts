import { Ionicons } from '@expo/vector-icons';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { API_BASE_URL } from '../constants/config';
import { useTheme } from '../context/ThemeContext';

interface Props {
  message?: string;
  onRetry: () => void;
}

export function ConnectionError({ message, onRetry }: Props) {
  const { colors } = useTheme();

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <Ionicons name="cloud-offline" size={48} color={colors.error} />
      <Text style={[styles.title, { color: colors.text }]}>Cannot reach API</Text>
      <Text style={[styles.message, { color: colors.textSecondary }]}>
        {message ?? 'Check that the backend is running and EXPO_PUBLIC_API_BASE_URL is set correctly in .env'}
      </Text>
      <Text style={[styles.url, { color: colors.textSecondary }]}>{API_BASE_URL}</Text>
      <Pressable onPress={onRetry} style={[styles.button, { backgroundColor: colors.primary }]}>
        <Text style={styles.buttonText}>Retry</Text>
      </Pressable>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, justifyContent: 'center', alignItems: 'center', padding: 32, gap: 12 },
  title: { fontSize: 20, fontWeight: '800', marginTop: 8 },
  message: { fontSize: 14, textAlign: 'center', lineHeight: 20 },
  url: { fontSize: 12, fontFamily: 'monospace', marginTop: 4 },
  button: { marginTop: 16, paddingHorizontal: 24, paddingVertical: 12, borderRadius: 10 },
  buttonText: { color: '#FFF', fontWeight: '700', fontSize: 16 },
});
