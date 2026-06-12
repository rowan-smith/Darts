import { Ionicons } from '@expo/vector-icons';
import { useRouter } from 'expo-router';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { statusColors } from '../constants/theme';
import { useTheme } from '../context/ThemeContext';
import type { MatchSummary } from '../types';

interface Props {
  match: MatchSummary;
}

export function MatchCard({ match }: Props) {
  const { colors } = useTheme();
  const router = useRouter();

  const onPress = () => {
    if (match.status === 'Completed') {
      router.push(`/match/${match.id}/recap`);
    } else {
      router.push(`/match/${match.id}`);
    }
  };

  return (
    <Pressable
      onPress={onPress}
      style={({ pressed }) => [
        styles.card,
        { backgroundColor: colors.card, borderColor: colors.border, opacity: pressed ? 0.9 : 1 },
      ]}
    >
      <View style={styles.header}>
        <Text style={[styles.tournament, { color: colors.textSecondary }]} numberOfLines={1}>
          {match.tournament || 'Friendly Match'}
        </Text>
        <View style={[styles.badge, { backgroundColor: statusColors[match.status] + '22' }]}>
          <Text style={[styles.badgeText, { color: statusColors[match.status] }]}>
            {match.status === 'InProgress' ? 'LIVE' : match.status}
          </Text>
        </View>
      </View>

      <View style={styles.scoreRow}>
        <View style={styles.playerCol}>
          <Text style={[styles.playerName, { color: colors.text }]} numberOfLines={1}>
            {match.player1Name}
          </Text>
          <Text style={[styles.sets, { color: colors.primary }]}>{match.player1Sets}</Text>
        </View>

        <Text style={[styles.vs, { color: colors.textSecondary }]}>vs</Text>

        <View style={[styles.playerCol, styles.playerColRight]}>
          <Text style={[styles.playerName, { color: colors.text, textAlign: 'right' }]} numberOfLines={1}>
            {match.player2Name}
          </Text>
          <Text style={[styles.sets, { color: colors.primary, textAlign: 'right' }]}>{match.player2Sets}</Text>
        </View>
      </View>

      {match.winnerName && (
        <View style={styles.footer}>
          <Ionicons name="trophy" size={14} color={colors.accent} />
          <Text style={[styles.winner, { color: colors.textSecondary }]}>{match.winnerName} wins</Text>
        </View>
      )}
    </Pressable>
  );
}

const styles = StyleSheet.create({
  card: {
    borderRadius: 12,
    borderWidth: 1,
    padding: 16,
    marginBottom: 12,
  },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 12,
  },
  tournament: { fontSize: 12, flex: 1, marginRight: 8 },
  badge: { paddingHorizontal: 8, paddingVertical: 4, borderRadius: 6 },
  badgeText: { fontSize: 10, fontWeight: '700' },
  scoreRow: { flexDirection: 'row', alignItems: 'center' },
  playerCol: { flex: 1 },
  playerColRight: { alignItems: 'flex-end' },
  playerName: { fontSize: 15, fontWeight: '600', marginBottom: 4 },
  sets: { fontSize: 28, fontWeight: '800' },
  vs: { fontSize: 14, fontWeight: '600', marginHorizontal: 12 },
  footer: { flexDirection: 'row', alignItems: 'center', gap: 6, marginTop: 12 },
  winner: { fontSize: 12 },
});
