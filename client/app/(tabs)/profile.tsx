import { Ionicons } from '@expo/vector-icons';
import { useCallback, useState } from 'react';
import {
  Alert,
  Pressable,
  ScrollView,
  StyleSheet,
  Switch,
  Text,
  TextInput,
  View,
} from 'react-native';
import { useFocusEffect } from '@react-navigation/native';
import { LoadingView } from '../../components/LoadingView';
import { useTheme } from '../../context/ThemeContext';
import { api } from '../../services/api';
import type { ThemeMode, UserProfile } from '../../types';

export default function ProfileScreen() {
  const { colors, isDark, themeMode, setThemeMode } = useTheme();
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  const loadProfile = useCallback(async () => {
    try {
      const data = await api.getProfile();
      setProfile(data);
      setName(data.name);
      setEmail(data.email);
    } catch (error) {
      console.error('Failed to load profile:', error);
    } finally {
      setLoading(false);
    }
  }, []);

  useFocusEffect(
    useCallback(() => {
      loadProfile();
    }, [loadProfile]),
  );

  const saveProfile = async () => {
    setSaving(true);
    try {
      const updated = await api.updateProfile({ name, email });
      setProfile(updated);
      Alert.alert('Saved', 'Profile updated successfully');
    } catch {
      Alert.alert('Error', 'Failed to save profile');
    } finally {
      setSaving(false);
    }
  };

  const toggleTheme = async () => {
    const next: ThemeMode = isDark ? 'Light' : 'Dark';
    await setThemeMode(next);
  };

  if (loading) return <LoadingView message="Loading profile..." />;

  return (
    <ScrollView style={[styles.container, { backgroundColor: colors.background }]} contentContainerStyle={styles.content}>
      <View style={[styles.avatarSection, { backgroundColor: colors.card, borderColor: colors.border }]}>
        <View style={[styles.avatar, { backgroundColor: colors.primary }]}>
          <Text style={styles.avatarText}>{name.charAt(0).toUpperCase()}</Text>
        </View>
        <Text style={[styles.profileName, { color: colors.text }]}>{profile?.name}</Text>
        <Text style={[styles.profileEmail, { color: colors.textSecondary }]}>{profile?.email}</Text>
      </View>

      <View style={styles.statsRow}>
        {[
          { label: '180s', value: profile?.total180s ?? 0, icon: 'flash' as const },
          { label: 'Played', value: profile?.matchesPlayed ?? 0, icon: 'basketball' as const },
          { label: 'Won', value: profile?.matchesWon ?? 0, icon: 'trophy' as const },
          { label: 'Avg', value: profile?.averageScore?.toFixed(1) ?? '0', icon: 'analytics' as const },
        ].map((stat) => (
          <View key={stat.label} style={[styles.statCard, { backgroundColor: colors.card, borderColor: colors.border }]}>
            <Ionicons name={stat.icon} size={20} color={colors.primary} />
            <Text style={[styles.statValue, { color: colors.text }]}>{stat.value}</Text>
            <Text style={[styles.statLabel, { color: colors.textSecondary }]}>{stat.label}</Text>
          </View>
        ))}
      </View>

      <View style={[styles.section, { backgroundColor: colors.card, borderColor: colors.border }]}>
        <Text style={[styles.sectionTitle, { color: colors.text }]}>Edit Profile</Text>

        <Text style={[styles.label, { color: colors.textSecondary }]}>Name</Text>
        <TextInput
          value={name}
          onChangeText={setName}
          style={[styles.input, { backgroundColor: colors.surfaceAlt, color: colors.text, borderColor: colors.border }]}
          placeholderTextColor={colors.textSecondary}
        />

        <Text style={[styles.label, { color: colors.textSecondary }]}>Email</Text>
        <TextInput
          value={email}
          onChangeText={setEmail}
          keyboardType="email-address"
          autoCapitalize="none"
          style={[styles.input, { backgroundColor: colors.surfaceAlt, color: colors.text, borderColor: colors.border }]}
          placeholderTextColor={colors.textSecondary}
        />

        <Pressable
          onPress={saveProfile}
          disabled={saving}
          style={[styles.saveButton, { backgroundColor: colors.primary, opacity: saving ? 0.6 : 1 }]}
        >
          <Text style={styles.saveButtonText}>{saving ? 'Saving...' : 'Save Changes'}</Text>
        </Pressable>
      </View>

      <View style={[styles.section, { backgroundColor: colors.card, borderColor: colors.border }]}>
        <Text style={[styles.sectionTitle, { color: colors.text }]}>Appearance</Text>
        <View style={styles.settingRow}>
          <View style={styles.settingInfo}>
            <Ionicons name={isDark ? 'moon' : 'sunny'} size={22} color={colors.primary} />
            <View>
              <Text style={[styles.settingLabel, { color: colors.text }]}>Dark Mode</Text>
              <Text style={[styles.settingDesc, { color: colors.textSecondary }]}>
                Currently: {themeMode}
              </Text>
            </View>
          </View>
          <Switch value={isDark} onValueChange={toggleTheme} trackColor={{ true: colors.primary }} />
        </View>
      </View>

      <View style={[styles.section, { backgroundColor: colors.card, borderColor: colors.border }]}>
        <Text style={[styles.sectionTitle, { color: colors.text }]}>About</Text>
        <Text style={[styles.aboutText, { color: colors.textSecondary }]}>
          PDC Darts Tracker v1.0{'\n'}
          Track scores, read news, and improve your game.
        </Text>
      </View>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  content: { padding: 16, paddingBottom: 40 },
  avatarSection: { alignItems: 'center', padding: 24, borderRadius: 16, borderWidth: 1, marginBottom: 16 },
  avatar: { width: 80, height: 80, borderRadius: 40, alignItems: 'center', justifyContent: 'center', marginBottom: 12 },
  avatarText: { color: '#FFF', fontSize: 32, fontWeight: '800' },
  profileName: { fontSize: 22, fontWeight: '800' },
  profileEmail: { fontSize: 14, marginTop: 4 },
  statsRow: { flexDirection: 'row', gap: 8, marginBottom: 16 },
  statCard: { flex: 1, alignItems: 'center', padding: 12, borderRadius: 12, borderWidth: 1, gap: 4 },
  statValue: { fontSize: 18, fontWeight: '800' },
  statLabel: { fontSize: 11 },
  section: { padding: 16, borderRadius: 16, borderWidth: 1, marginBottom: 16 },
  sectionTitle: { fontSize: 16, fontWeight: '800', marginBottom: 16 },
  label: { fontSize: 12, fontWeight: '600', marginBottom: 6, textTransform: 'uppercase' },
  input: { borderWidth: 1, borderRadius: 10, padding: 12, fontSize: 16, marginBottom: 14 },
  saveButton: { padding: 14, borderRadius: 10, alignItems: 'center', marginTop: 4 },
  saveButtonText: { color: '#FFF', fontSize: 16, fontWeight: '700' },
  settingRow: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' },
  settingInfo: { flexDirection: 'row', alignItems: 'center', gap: 12 },
  settingLabel: { fontSize: 15, fontWeight: '600' },
  settingDesc: { fontSize: 12, marginTop: 2 },
  aboutText: { fontSize: 13, lineHeight: 20 },
});
