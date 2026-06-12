import { useRouter } from 'expo-router';
import { Image, Pressable, StyleSheet, Text, View } from 'react-native';
import { useTheme } from '../context/ThemeContext';
import type { Article } from '../types';

interface Props {
  article: Article;
  featured?: boolean;
}

export function ArticleCard({ article, featured }: Props) {
  const { colors } = useTheme();
  const router = useRouter();

  return (
    <Pressable
      onPress={() => router.push(`/article/${article.id}`)}
      style={({ pressed }) => [
        featured ? styles.featuredCard : styles.card,
        { backgroundColor: colors.card, borderColor: colors.border, opacity: pressed ? 0.9 : 1 },
      ]}
    >
      {article.imageUrl && (
        <Image source={{ uri: article.imageUrl }} style={featured ? styles.featuredImage : styles.image} />
      )}
      <View style={styles.content}>
        {featured && (
          <View style={[styles.featuredBadge, { backgroundColor: colors.primary }]}>
            <Text style={styles.featuredBadgeText}>FEATURED</Text>
          </View>
        )}
        <Text style={[styles.category, { color: colors.primary }]}>{article.category}</Text>
        <Text style={[styles.title, { color: colors.text }]} numberOfLines={featured ? 3 : 2}>
          {article.title}
        </Text>
        <Text style={[styles.summary, { color: colors.textSecondary }]} numberOfLines={featured ? 3 : 2}>
          {article.summary}
        </Text>
        <Text style={[styles.meta, { color: colors.textSecondary }]}>
          {article.readTimeMinutes} min read · {new Date(article.publishedAt).toLocaleDateString()}
        </Text>
      </View>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  card: {
    borderRadius: 12,
    borderWidth: 1,
    overflow: 'hidden',
    marginBottom: 12,
  },
  featuredCard: {
    borderRadius: 16,
    borderWidth: 1,
    overflow: 'hidden',
    marginBottom: 16,
    width: 280,
    marginRight: 12,
  },
  image: { width: '100%', height: 140 },
  featuredImage: { width: '100%', height: 160 },
  content: { padding: 14 },
  featuredBadge: {
    alignSelf: 'flex-start',
    paddingHorizontal: 8,
    paddingVertical: 3,
    borderRadius: 4,
    marginBottom: 6,
  },
  featuredBadgeText: { color: '#FFF', fontSize: 10, fontWeight: '800' },
  category: { fontSize: 11, fontWeight: '700', marginBottom: 4, textTransform: 'uppercase' },
  title: { fontSize: 16, fontWeight: '700', marginBottom: 6 },
  summary: { fontSize: 13, lineHeight: 18, marginBottom: 8 },
  meta: { fontSize: 11 },
});
