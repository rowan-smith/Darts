import { Ionicons } from '@expo/vector-icons';
import { useRouter } from 'expo-router';
import { useCallback, useState } from 'react';
import {
  FlatList,
  Image,
  Pressable,
  RefreshControl,
  ScrollView,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { useFocusEffect } from '@react-navigation/native';
import { ArticleCard } from '../../components/ArticleCard';
import { ConnectionError } from '../../components/ConnectionError';
import { LoadingView } from '../../components/LoadingView';
import { MatchCard } from '../../components/MatchCard';
import { SectionHeader } from '../../components/SectionHeader';
import { useTheme } from '../../context/ThemeContext';
import { api } from '../../services/api';
import type { HomeFeed } from '../../types';

export default function HomeScreen() {
  const { colors } = useTheme();
  const router = useRouter();
  const [feed, setFeed] = useState<HomeFeed | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadFeed = useCallback(async () => {
    try {
      setError(null);
      await api.healthCheck();
      const data = await api.getHomeFeed();
      setFeed(data);
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Connection failed';
      setError(msg);
      console.error('Failed to load home feed:', err);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useFocusEffect(
    useCallback(() => {
      loadFeed();
    }, [loadFeed]),
  );

  if (loading) return <LoadingView message="Loading PDC Darts..." />;
  if (error && !feed) return <ConnectionError message={error} onRetry={() => { setLoading(true); loadFeed(); }} />;

  return (
    <ScrollView
      style={[styles.container, { backgroundColor: colors.background }]}
      refreshControl={<RefreshControl refreshing={refreshing} onRefresh={() => { setRefreshing(true); loadFeed(); }} tintColor={colors.primary} />}
    >
      <View style={[styles.hero, { backgroundColor: colors.primary }]}>
        <Text style={styles.heroTitle}>PDC Darts Tracker</Text>
        <Text style={styles.heroSubtitle}>Scores · News · Stats</Text>
      </View>

      {feed?.featured && feed.featured.length > 0 && (
        <View style={styles.section}>
          <SectionHeader title="Featured" />
          <ScrollView horizontal showsHorizontalScrollIndicator={false}>
            {feed.featured.map((item) => (
              <Pressable
                key={item.id}
                onPress={() => item.matchId && router.push(`/match/${item.matchId}/recap`)}
                style={[styles.featuredCard, { backgroundColor: colors.card, borderColor: colors.border }]}
              >
                {item.imageUrl && <Image source={{ uri: item.imageUrl }} style={styles.featuredImage} />}
                <View style={styles.featuredOverlay}>
                  {item.badge && (
                    <View style={[styles.featuredBadge, { backgroundColor: colors.accent }]}>
                      <Text style={styles.featuredBadgeText}>{item.badge}</Text>
                    </View>
                  )}
                  <Text style={styles.featuredTitle}>{item.title}</Text>
                  <Text style={styles.featuredSubtitle}>{item.subtitle}</Text>
                </View>
              </Pressable>
            ))}
          </ScrollView>
        </View>
      )}

      <View style={styles.section}>
        <SectionHeader title="Recent Scores" actionLabel="See all" onAction={() => router.push('/matches')} />
        {feed?.recentScores.slice(0, 5).map((match) => (
          <MatchCard key={match.id} match={match} />
        ))}
      </View>

      <View style={styles.section}>
        <SectionHeader title="Latest News" />
        {(feed?.featuredArticles ?? []).length > 0 ? (
          <FlatList
            data={feed?.featuredArticles ?? []}
            horizontal
            showsHorizontalScrollIndicator={false}
            keyExtractor={(item) => item.id}
            renderItem={({ item }) => <ArticleCard article={item} featured />}
          />
        ) : null}
        {feed?.articles.slice(0, 5).map((article) => (
          <ArticleCard key={article.id} article={article} />
        ))}
      </View>

      <View style={[styles.section, styles.lastSection]}>
        <SectionHeader title="Suggestions" />
        {feed?.suggestions.map((suggestion) => (
          <View key={suggestion.id} style={[styles.suggestionCard, { backgroundColor: colors.card, borderColor: colors.border }]}>
            <View style={[styles.suggestionIcon, { backgroundColor: colors.primary + '22' }]}>
              <Ionicons name="bulb" size={20} color={colors.primary} />
            </View>
            <View style={styles.suggestionContent}>
              <Text style={[styles.suggestionTitle, { color: colors.text }]}>{suggestion.title}</Text>
              <Text style={[styles.suggestionDesc, { color: colors.textSecondary }]}>{suggestion.description}</Text>
            </View>
          </View>
        ))}
      </View>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  hero: { padding: 24, paddingTop: 8 },
  heroTitle: { color: '#FFF', fontSize: 26, fontWeight: '900' },
  heroSubtitle: { color: '#FFFFFFCC', fontSize: 14, marginTop: 4 },
  section: { paddingHorizontal: 16, marginTop: 8 },
  lastSection: { paddingBottom: 32 },
  featuredCard: { width: 260, height: 180, borderRadius: 16, overflow: 'hidden', marginRight: 12, borderWidth: 1 },
  featuredImage: { ...StyleSheet.absoluteFillObject, width: '100%', height: '100%' },
  featuredOverlay: { flex: 1, justifyContent: 'flex-end', padding: 14, backgroundColor: 'rgba(0,0,0,0.45)' },
  featuredBadge: { alignSelf: 'flex-start', paddingHorizontal: 8, paddingVertical: 3, borderRadius: 4, marginBottom: 6 },
  featuredBadgeText: { fontSize: 10, fontWeight: '800', color: '#1A1A2E' },
  featuredTitle: { color: '#FFF', fontSize: 16, fontWeight: '800' },
  featuredSubtitle: { color: '#FFFFFFCC', fontSize: 12, marginTop: 2 },
  suggestionCard: { flexDirection: 'row', padding: 14, borderRadius: 12, borderWidth: 1, marginBottom: 10, gap: 12 },
  suggestionIcon: { width: 40, height: 40, borderRadius: 20, alignItems: 'center', justifyContent: 'center' },
  suggestionContent: { flex: 1 },
  suggestionTitle: { fontSize: 14, fontWeight: '700', marginBottom: 4 },
  suggestionDesc: { fontSize: 12, lineHeight: 17 },
});
