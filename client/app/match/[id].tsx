import { Ionicons } from '@expo/vector-icons';
import { useLocalSearchParams, useRouter } from 'expo-router';
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
import { ScorePad } from '../../components/ScorePad';
import { useTheme } from '../../context/ThemeContext';
import { api } from '../../services/api';
import type { MatchDetail } from '../../types';

export default function MatchDetailScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const { colors } = useTheme();
  const router = useRouter();
  const [match, setMatch] = useState<MatchDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [customScore, setCustomScore] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const loadMatch = useCallback(async () => {
    if (!id) return;
    try {
      const data = await api.getMatch(id);
      setMatch(data);
      if (data.status === 'Completed') {
        router.replace(`/match/${id}/recap`);
      }
    } catch (error) {
      console.error('Failed to load match:', error);
      Alert.alert('Error', 'Failed to load match');
    } finally {
      setLoading(false);
    }
  }, [id, router]);

  useFocusEffect(
    useCallback(() => {
      setLoading(true);
      loadMatch();
    }, [loadMatch]),
  );

  const handleStart = async () => {
    if (!id) return;
    try {
      const data = await api.startMatch(id);
      setMatch(data);
    } catch {
      Alert.alert('Error', 'Could not start match');
    }
  };

  const handleScore = async (score: number) => {
    if (!id || !match || match.status !== 'InProgress') return;

    const currentLeg = match.sets.flatMap((s) => s.legs).find((l) => l.id === match.currentLegId);
    const currentPlayerId = currentLeg?.currentPlayerId;
    if (!currentPlayerId) return;

    setSubmitting(true);
    try {
      const data = await api.recordVisit(id, currentPlayerId, score);
      setMatch(data);
      setCustomScore('');
      if (data.status === 'Completed') {
        router.replace(`/match/${id}/recap`);
      }
    } catch {
      Alert.alert('Invalid Score', 'Could not record this score. Check it is valid and your turn.');
    } finally {
      setSubmitting(false);
    }
  };

  const submitCustomScore = () => {
    const score = parseInt(customScore, 10);
    if (isNaN(score) || score < 0 || score > 180) {
      Alert.alert('Invalid', 'Enter a score between 0 and 180');
      return;
    }
    handleScore(score);
  };

  if (loading || !match) return <LoadingView message="Loading match..." />;

  const currentLeg = match.sets.flatMap((s) => s.legs).find((l) => l.id === match.currentLegId);
  const currentPlayer = currentLeg?.currentPlayerId === match.player1.id ? match.player1 : match.player2;
  const isPlayer1Turn = currentLeg?.currentPlayerId === match.player1.id;

  return (
    <ScrollView style={[styles.container, { backgroundColor: colors.background }]} contentContainerStyle={styles.content}>
      <View style={[styles.headerCard, { backgroundColor: colors.card, borderColor: colors.border }]}>
        <Text style={[styles.tournament, { color: colors.textSecondary }]}>{match.tournament || 'Friendly'}</Text>
        <Text style={[styles.venue, { color: colors.textSecondary }]}>{match.venue}</Text>

        <View style={styles.scoreboard}>
          <View style={[styles.playerBox, isPlayer1Turn && match.status === 'InProgress' && { borderColor: colors.accent, borderWidth: 2 }]}>
            <Text style={[styles.playerLabel, { color: colors.text }]} numberOfLines={1}>{match.player1.name}</Text>
            <Text style={[styles.remaining, { color: colors.primary }]}>
              {currentLeg?.player1Remaining ?? match.startingScore}
            </Text>
            <Text style={[styles.sets, { color: colors.textSecondary }]}>Sets: {match.player1Sets}</Text>
          </View>

          <View style={styles.centerCol}>
            <Text style={[styles.setsScore, { color: colors.text }]}>
              {match.player1Sets} - {match.player2Sets}
            </Text>
            <Text style={[styles.format, { color: colors.textSecondary }]}>
              Best of {match.setsToWin * 2 - 1} sets
            </Text>
          </View>

          <View style={[styles.playerBox, !isPlayer1Turn && match.status === 'InProgress' && { borderColor: colors.accent, borderWidth: 2 }]}>
            <Text style={[styles.playerLabel, { color: colors.text, textAlign: 'right' }]} numberOfLines={1}>{match.player2.name}</Text>
            <Text style={[styles.remaining, { color: colors.primary, textAlign: 'right' }]}>
              {currentLeg?.player2Remaining ?? match.startingScore}
            </Text>
            <Text style={[styles.sets, { color: colors.textSecondary, textAlign: 'right' }]}>Sets: {match.player2Sets}</Text>
          </View>
        </View>
      </View>

      {match.status === 'Scheduled' && (
        <Pressable onPress={handleStart} style={[styles.startButton, { backgroundColor: colors.primary }]}>
          <Ionicons name="play" size={20} color="#FFF" />
          <Text style={styles.startButtonText}>Start Match</Text>
        </Pressable>
      )}

      {match.status === 'InProgress' && (
        <View style={[styles.scoringSection, { backgroundColor: colors.card, borderColor: colors.border }]}>
          <Text style={[styles.turnLabel, { color: colors.accent }]}>
            {currentPlayer?.name}'s turn
          </Text>

          <View style={styles.customScoreRow}>
            <TextInput
              value={customScore}
              onChangeText={setCustomScore}
              keyboardType="number-pad"
              placeholder="Custom score"
              placeholderTextColor={colors.textSecondary}
              style={[styles.customInput, { backgroundColor: colors.surfaceAlt, color: colors.text, borderColor: colors.border }]}
            />
            <Pressable
              onPress={submitCustomScore}
              disabled={submitting}
              style={[styles.submitButton, { backgroundColor: colors.primary, opacity: submitting ? 0.6 : 1 }]}
            >
              <Text style={styles.submitText}>Add</Text>
            </Pressable>
          </View>

          <ScorePad onScore={handleScore} disabled={submitting} />
        </View>
      )}

      {currentLeg && currentLeg.visits.length > 0 && (
        <View style={[styles.visitsSection, { backgroundColor: colors.card, borderColor: colors.border }]}>
          <Text style={[styles.sectionTitle, { color: colors.text }]}>Current Leg</Text>
          {[...currentLeg.visits].reverse().map((visit) => (
            <View key={visit.id} style={[styles.visitRow, { borderBottomColor: colors.border }]}>
              <Text style={[styles.visitPlayer, { color: colors.text }]}>{visit.playerName}</Text>
              <View style={styles.visitScores}>
                {visit.is180 && <Text style={{ color: colors.accent, fontWeight: '800' }}>180!</Text>}
                {visit.isBust && <Text style={{ color: colors.error }}>BUST</Text>}
                {visit.isCheckout && <Text style={{ color: colors.success }}>CHECKOUT</Text>}
                <Text style={[styles.visitScore, { color: colors.primary }]}>{visit.score}</Text>
                <Text style={[styles.visitRemaining, { color: colors.textSecondary }]}>→ {visit.remainingAfter}</Text>
              </View>
            </View>
          ))}
        </View>
      )}
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  content: { padding: 16, paddingBottom: 40 },
  headerCard: { borderRadius: 16, borderWidth: 1, padding: 16, marginBottom: 16 },
  tournament: { fontSize: 12, fontWeight: '700', textTransform: 'uppercase' },
  venue: { fontSize: 12, marginBottom: 16 },
  scoreboard: { flexDirection: 'row', alignItems: 'center' },
  playerBox: { flex: 1, padding: 10, borderRadius: 10, borderWidth: 1, borderColor: 'transparent' },
  playerLabel: { fontSize: 13, fontWeight: '600', marginBottom: 4 },
  remaining: { fontSize: 36, fontWeight: '900' },
  sets: { fontSize: 12, marginTop: 4 },
  centerCol: { alignItems: 'center', paddingHorizontal: 8 },
  setsScore: { fontSize: 22, fontWeight: '800' },
  format: { fontSize: 11, marginTop: 2 },
  startButton: { flexDirection: 'row', alignItems: 'center', justifyContent: 'center', gap: 8, padding: 16, borderRadius: 12, marginBottom: 16 },
  startButtonText: { color: '#FFF', fontSize: 16, fontWeight: '700' },
  scoringSection: { borderRadius: 16, borderWidth: 1, padding: 16, marginBottom: 16 },
  turnLabel: { fontSize: 16, fontWeight: '800', marginBottom: 12, textAlign: 'center' },
  customScoreRow: { flexDirection: 'row', gap: 8, marginBottom: 8 },
  customInput: { flex: 1, borderWidth: 1, borderRadius: 10, padding: 12, fontSize: 18, fontWeight: '700' },
  submitButton: { paddingHorizontal: 20, borderRadius: 10, justifyContent: 'center' },
  submitText: { color: '#FFF', fontWeight: '700' },
  visitsSection: { borderRadius: 16, borderWidth: 1, padding: 16 },
  sectionTitle: { fontSize: 14, fontWeight: '800', marginBottom: 12, textTransform: 'uppercase' },
  visitRow: { flexDirection: 'row', justifyContent: 'space-between', paddingVertical: 10, borderBottomWidth: 1 },
  visitPlayer: { fontSize: 14, fontWeight: '600' },
  visitScores: { flexDirection: 'row', alignItems: 'center', gap: 8 },
  visitScore: { fontSize: 16, fontWeight: '800' },
  visitRemaining: { fontSize: 13 },
});
