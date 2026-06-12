import { Ionicons } from '@expo/vector-icons';
import { useRouter } from 'expo-router';
import { useCallback, useState } from 'react';
import { FlatList, Pressable, RefreshControl, StyleSheet, Text, View } from 'react-native';
import { useFocusEffect } from '@react-navigation/native';
import { LoadingView } from '../../components/LoadingView';
import { ScreenHeader } from '../../components/ScreenHeader';
import { MatchCard } from '../../components/MatchCard';
import { useTheme } from '../../context/ThemeContext';
import { api } from '../../services/api';
import type { MatchStatus, MatchSummary } from '../../types';

const FILTERS: { label: string; value?: MatchStatus }[] = [
  { label: 'All' },
  { label: 'Live', value: 'InProgress' },
  { label: 'Scheduled', value: 'Scheduled' },
  { label: 'Completed', value: 'Completed' },
];

export default function MatchesScreen() {
  const { colors } = useTheme();
  const router = useRouter();
  const [matches, setMatches] = useState<MatchSummary[]>([]);
  const [filter, setFilter] = useState<MatchStatus | undefined>();
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const loadMatches = useCallback(async () => {
    try {
      const data = await api.getMatches(filter);
      setMatches(data);
    } catch (error) {
      console.error('Failed to load matches:', error);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, [filter]);

  useFocusEffect(
    useCallback(() => {
      setLoading(true);
      loadMatches();
    }, [loadMatches]),
  );

  if (loading) return <LoadingView message="Loading matches..." />;

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <ScreenHeader title="Scores" subtitle="Track and manage matches" />
      <View style={styles.filterRow}>
        {FILTERS.map((f) => (
          <Pressable
            key={f.label}
            onPress={() => setFilter(f.value)}
            style={[
              styles.filterChip,
              {
                backgroundColor: filter === f.value ? colors.primary : colors.surfaceAlt,
                borderColor: filter === f.value ? colors.primary : colors.border,
              },
            ]}
          >
            <Text style={{ color: filter === f.value ? '#FFF' : colors.text, fontWeight: '600', fontSize: 13 }}>
              {f.label}
            </Text>
          </Pressable>
        ))}
      </View>

      <FlatList
        data={matches}
        keyExtractor={(item) => item.id}
        renderItem={({ item }) => <MatchCard match={item} />}
        contentContainerStyle={styles.list}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={() => { setRefreshing(true); loadMatches(); }} tintColor={colors.primary} />}
        ListEmptyComponent={
          <View style={styles.empty}>
            <Ionicons name="basketball-outline" size={48} color={colors.textSecondary} />
            <Text style={[styles.emptyText, { color: colors.textSecondary }]}>No matches found</Text>
          </View>
        }
      />

      <Pressable
        onPress={() => router.push('/match/create')}
        style={[styles.fab, { backgroundColor: colors.primary }]}
      >
        <Ionicons name="add" size={28} color="#FFF" />
      </Pressable>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  filterRow: { flexDirection: 'row', padding: 16, gap: 8, flexWrap: 'wrap' },
  filterChip: { paddingHorizontal: 14, paddingVertical: 8, borderRadius: 20, borderWidth: 1 },
  list: { padding: 16, paddingTop: 0, paddingBottom: 100 },
  empty: { alignItems: 'center', paddingTop: 60, gap: 12 },
  emptyText: { fontSize: 16 },
  fab: {
    position: 'absolute',
    right: 20,
    bottom: 20,
    width: 56,
    height: 56,
    borderRadius: 28,
    alignItems: 'center',
    justifyContent: 'center',
    elevation: 4,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.25,
    shadowRadius: 4,
  },
});
