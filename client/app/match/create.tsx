import { useRouter } from 'expo-router';
import { useCallback, useState } from 'react';
import {
  Alert,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  View,
} from 'react-native';
import { useFocusEffect } from '@react-navigation/native';
import { LoadingView } from '../../components/LoadingView';
import { useTheme } from '../../context/ThemeContext';
import { api } from '../../services/api';
import type { Player } from '../../types';

export default function CreateMatchScreen() {
  const { colors } = useTheme();
  const router = useRouter();
  const [players, setPlayers] = useState<Player[]>([]);
  const [loading, setLoading] = useState(true);
  const [creating, setCreating] = useState(false);
  const [title, setTitle] = useState('');
  const [tournament, setTournament] = useState('');
  const [venue, setVenue] = useState('');
  const [player1Id, setPlayer1Id] = useState<string | null>(null);
  const [player2Id, setPlayer2Id] = useState<string | null>(null);
  const [setsToWin, setSetsToWin] = useState('3');
  const [legsPerSet, setLegsPerSet] = useState('3');

  useFocusEffect(
    useCallback(() => {
      (async () => {
        try {
          const data = await api.getPlayers();
          setPlayers(data);
          const local = data.find((p) => p.isLocal);
          if (local) setPlayer1Id(local.id);
        } catch {
          Alert.alert('Error', 'Failed to load players');
        } finally {
          setLoading(false);
        }
      })();
    }, []),
  );

  const createMatch = async () => {
    if (!player1Id || !player2Id) {
      Alert.alert('Select Players', 'Please select both players');
      return;
    }
    if (player1Id === player2Id) {
      Alert.alert('Invalid', 'Players must be different');
      return;
    }

    setCreating(true);
    try {
      const match = await api.createMatch({
        title: title.trim() || `${players.find(p => p.id === player1Id)?.name} vs ${players.find(p => p.id === player2Id)?.name}`,
        player1Id,
        player2Id,
        setsToWin: parseInt(setsToWin, 10) || 3,
        legsPerSet: parseInt(legsPerSet, 10) || 3,
        tournament: tournament || undefined,
        venue: venue || undefined,
      });
      router.replace(`/match/${match.id}`);
    } catch {
      Alert.alert('Error', 'Failed to create match');
    } finally {
      setCreating(false);
    }
  };

  if (loading) return <LoadingView message="Loading players..." />;

  const PlayerPicker = ({
    label,
    selectedId,
    onSelect,
    excludeId,
  }: {
    label: string;
    selectedId: string | null;
    onSelect: (id: string) => void;
    excludeId?: string | null;
  }) => (
    <View style={styles.pickerSection}>
      <Text style={[styles.label, { color: colors.textSecondary }]}>{label}</Text>
      <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.playerScroll}>
        {players
          .filter((p) => p.id !== excludeId)
          .map((player) => (
            <Pressable
              key={player.id}
              onPress={() => onSelect(player.id)}
              style={[
                styles.playerChip,
                {
                  backgroundColor: selectedId === player.id ? colors.primary : colors.surfaceAlt,
                  borderColor: selectedId === player.id ? colors.primary : colors.border,
                },
              ]}
            >
              <Text style={{ color: selectedId === player.id ? '#FFF' : colors.text, fontWeight: '600', fontSize: 13 }}>
                {player.name}
              </Text>
              {player.countryCode && (
                <Text style={{ color: selectedId === player.id ? '#FFFFFFAA' : colors.textSecondary, fontSize: 10 }}>
                  {player.countryCode}
                </Text>
              )}
            </Pressable>
          ))}
      </ScrollView>
    </View>
  );

  return (
    <ScrollView style={[styles.container, { backgroundColor: colors.background }]} contentContainerStyle={styles.content}>
      <Text style={[styles.label, { color: colors.textSecondary }]}>Match Title (optional)</Text>
      <TextInput
        value={title}
        onChangeText={setTitle}
        placeholder="e.g. Semi-Final"
        placeholderTextColor={colors.textSecondary}
        style={[styles.input, { backgroundColor: colors.card, color: colors.text, borderColor: colors.border }]}
      />

      <PlayerPicker label="Player 1" selectedId={player1Id} onSelect={setPlayer1Id} excludeId={player2Id} />
      <PlayerPicker label="Player 2" selectedId={player2Id} onSelect={setPlayer2Id} excludeId={player1Id} />

      <View style={styles.row}>
        <View style={styles.halfField}>
          <Text style={[styles.label, { color: colors.textSecondary }]}>Sets to Win</Text>
          <TextInput
            value={setsToWin}
            onChangeText={setSetsToWin}
            keyboardType="number-pad"
            style={[styles.input, { backgroundColor: colors.card, color: colors.text, borderColor: colors.border }]}
          />
        </View>
        <View style={styles.halfField}>
          <Text style={[styles.label, { color: colors.textSecondary }]}>Legs per Set</Text>
          <TextInput
            value={legsPerSet}
            onChangeText={setLegsPerSet}
            keyboardType="number-pad"
            style={[styles.input, { backgroundColor: colors.card, color: colors.text, borderColor: colors.border }]}
          />
        </View>
      </View>

      <Text style={[styles.label, { color: colors.textSecondary }]}>Tournament</Text>
      <TextInput
        value={tournament}
        onChangeText={setTournament}
        placeholder="PDC World Championship"
        placeholderTextColor={colors.textSecondary}
        style={[styles.input, { backgroundColor: colors.card, color: colors.text, borderColor: colors.border }]}
      />

      <Text style={[styles.label, { color: colors.textSecondary }]}>Venue</Text>
      <TextInput
        value={venue}
        onChangeText={setVenue}
        placeholder="Alexandra Palace"
        placeholderTextColor={colors.textSecondary}
        style={[styles.input, { backgroundColor: colors.card, color: colors.text, borderColor: colors.border }]}
      />

      <Pressable
        onPress={createMatch}
        disabled={creating}
        style={[styles.createButton, { backgroundColor: colors.primary, opacity: creating ? 0.6 : 1 }]}
      >
        <Text style={styles.createButtonText}>{creating ? 'Creating...' : 'Create Match'}</Text>
      </Pressable>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  content: { padding: 16, paddingBottom: 40 },
  label: { fontSize: 12, fontWeight: '600', marginBottom: 6, textTransform: 'uppercase' },
  input: { borderWidth: 1, borderRadius: 10, padding: 12, fontSize: 16, marginBottom: 16 },
  pickerSection: { marginBottom: 16 },
  playerScroll: { marginBottom: 4 },
  playerChip: { paddingHorizontal: 14, paddingVertical: 10, borderRadius: 10, borderWidth: 1, marginRight: 8, alignItems: 'center' },
  row: { flexDirection: 'row', gap: 12 },
  halfField: { flex: 1 },
  createButton: { padding: 16, borderRadius: 12, alignItems: 'center', marginTop: 8 },
  createButtonText: { color: '#FFF', fontSize: 16, fontWeight: '700' },
});
