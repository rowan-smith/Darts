import { Ionicons } from '@expo/vector-icons';
import { useLocalSearchParams } from 'expo-router';
import { useCallback, useState } from 'react';
import { ScrollView, StyleSheet, Text, View } from 'react-native';
import { useFocusEffect } from '@react-navigation/native';
import { LoadingView } from '../../../components/LoadingView';
import { useTheme } from '../../../context/ThemeContext';
import { api } from '../../../services/api';
import type { MatchRecap } from '../../../types';

export default function MatchRecapScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const { colors } = useTheme();
  const [recap, setRecap] = useState<MatchRecap | null>(null);
  const [loading, setLoading] = useState(true);

  const loadRecap = useCallback(async () => {
    if (!id) return;
    try {
      const data = await api.getMatchRecap(id);
      setRecap(data);
    } catch (error) {
      console.error('Failed to load recap:', error);
    } finally {
      setLoading(false);
    }
  }, [id]);

  useFocusEffect(
    useCallback(() => {
      setLoading(true);
      loadRecap();
    }, [loadRecap]),
  );

  if (loading || !recap) return <LoadingView message="Loading recap..." />;

  return (
    <ScrollView style={[styles.container, { backgroundColor: colors.background }]} contentContainerStyle={styles.content}>
      <View style={[styles.winnerBanner, { backgroundColor: colors.primary }]}>
        <Ionicons name="trophy" size={32} color={colors.accent} />
        <Text style={styles.winnerText}>{recap.winnerName} wins!</Text>
        <Text style={styles.finalScore}>
          {recap.player1Sets} - {recap.player2Sets}
        </Text>
      </View>

      <View style={styles.statsGrid}>
        {[
          { label: '180s', value: recap.total180s },
          { label: 'Visits', value: recap.totalVisits },
          { label: `${recap.player1.name.split(' ').pop()} Avg`, value: recap.player1Average.toFixed(1) },
          { label: `${recap.player2.name.split(' ').pop()} Avg`, value: recap.player2Average.toFixed(1) },
        ].map((stat) => (
          <View key={stat.label} style={[styles.statBox, { backgroundColor: colors.card, borderColor: colors.border }]}>
            <Text style={[styles.statValue, { color: colors.text }]}>{stat.value}</Text>
            <Text style={[styles.statLabel, { color: colors.textSecondary }]}>{stat.label}</Text>
          </View>
        ))}
      </View>

      <View style={[styles.infoCard, { backgroundColor: colors.card, borderColor: colors.border }]}>
        <Text style={[styles.matchTitle, { color: colors.text }]}>{recap.title}</Text>
        {recap.tournament && <Text style={[styles.infoText, { color: colors.textSecondary }]}>🏆 {recap.tournament}</Text>}
        {recap.venue && <Text style={[styles.infoText, { color: colors.textSecondary }]}>📍 {recap.venue}</Text>}
        {recap.completedAt && (
          <Text style={[styles.infoText, { color: colors.textSecondary }]}>
            📅 {new Date(recap.completedAt).toLocaleString()}
          </Text>
        )}
      </View>

      {recap.sets.map((set) => (
        <View key={set.id} style={[styles.setCard, { backgroundColor: colors.card, borderColor: colors.border }]}>
          <View style={styles.setHeader}>
            <Text style={[styles.setTitle, { color: colors.text }]}>Set {set.setNumber}</Text>
            <Text style={[styles.setScore, { color: colors.primary }]}>
              {set.player1Legs} - {set.player2Legs}
            </Text>
          </View>

          {set.legs.map((leg) => (
            <View key={leg.id} style={[styles.legRow, { borderTopColor: colors.border }]}>
              <Text style={[styles.legLabel, { color: colors.textSecondary }]}>
                Leg {leg.legNumber}
              </Text>
              <Text style={[styles.legWinner, { color: colors.text }]}>
                {leg.winnerId === recap.player1.id ? recap.player1.name : recap.player2.name}
              </Text>
              <Text style={[styles.legDarts, { color: colors.textSecondary }]}>
                {leg.player1DartsThrown + leg.player2DartsThrown} darts
              </Text>
            </View>
          ))}
        </View>
      ))}

      <View style={[styles.readonlyBadge, { backgroundColor: colors.surfaceAlt }]}>
        <Ionicons name="lock-closed" size={14} color={colors.textSecondary} />
        <Text style={[styles.readonlyText, { color: colors.textSecondary }]}>Read-only match recap</Text>
      </View>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  content: { padding: 16, paddingBottom: 40 },
  winnerBanner: { alignItems: 'center', padding: 24, borderRadius: 16, marginBottom: 16 },
  winnerText: { color: '#FFF', fontSize: 22, fontWeight: '900', marginTop: 8 },
  finalScore: { color: '#FFFFFFCC', fontSize: 32, fontWeight: '800', marginTop: 4 },
  statsGrid: { flexDirection: 'row', flexWrap: 'wrap', gap: 8, marginBottom: 16 },
  statBox: { width: '48%', padding: 14, borderRadius: 12, borderWidth: 1, alignItems: 'center' },
  statValue: { fontSize: 22, fontWeight: '800' },
  statLabel: { fontSize: 11, marginTop: 4 },
  infoCard: { padding: 16, borderRadius: 12, borderWidth: 1, marginBottom: 16 },
  matchTitle: { fontSize: 16, fontWeight: '700', marginBottom: 8 },
  infoText: { fontSize: 13, marginBottom: 4 },
  setCard: { borderRadius: 12, borderWidth: 1, marginBottom: 12, overflow: 'hidden' },
  setHeader: { flexDirection: 'row', justifyContent: 'space-between', padding: 14 },
  setTitle: { fontSize: 15, fontWeight: '800' },
  setScore: { fontSize: 15, fontWeight: '800' },
  legRow: { flexDirection: 'row', justifyContent: 'space-between', padding: 12, borderTopWidth: 1 },
  legLabel: { fontSize: 13 },
  legWinner: { fontSize: 13, fontWeight: '600', flex: 1, textAlign: 'center' },
  legDarts: { fontSize: 12 },
  readonlyBadge: { flexDirection: 'row', alignItems: 'center', justifyContent: 'center', gap: 6, padding: 12, borderRadius: 8, marginTop: 8 },
  readonlyText: { fontSize: 12 },
});
