import { useLocalSearchParams } from 'expo-router';
import { useCallback, useState } from 'react';
import { Image, ScrollView, StyleSheet, Text, View } from 'react-native';
import { useFocusEffect } from '@react-navigation/native';
import { LoadingView } from '../../components/LoadingView';
import { useTheme } from '../../context/ThemeContext';
import { api } from '../../services/api';
import type { Article } from '../../types';

export default function ArticleScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const { colors } = useTheme();
  const [article, setArticle] = useState<Article | null>(null);
  const [loading, setLoading] = useState(true);

  const loadArticle = useCallback(async () => {
    if (!id) return;
    try {
      const data = await api.getArticle(id);
      setArticle(data);
    } catch (error) {
      console.error('Failed to load article:', error);
    } finally {
      setLoading(false);
    }
  }, [id]);

  useFocusEffect(
    useCallback(() => {
      setLoading(true);
      loadArticle();
    }, [loadArticle]),
  );

  if (loading || !article) return <LoadingView message="Loading article..." />;

  return (
    <ScrollView style={[styles.container, { backgroundColor: colors.background }]}>
      {article.imageUrl && (
        <Image source={{ uri: article.imageUrl }} style={styles.image} />
      )}
      <View style={styles.content}>
        <Text style={[styles.category, { color: colors.primary }]}>{article.category}</Text>
        <Text style={[styles.title, { color: colors.text }]}>{article.title}</Text>
        <Text style={[styles.meta, { color: colors.textSecondary }]}>
          {article.author && `${article.author} · `}
          {article.readTimeMinutes} min read · {new Date(article.publishedAt).toLocaleDateString()}
        </Text>
        <Text style={[styles.body, { color: colors.text }]}>{article.content}</Text>
      </View>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  image: { width: '100%', height: 220 },
  content: { padding: 20 },
  category: { fontSize: 12, fontWeight: '700', textTransform: 'uppercase', marginBottom: 8 },
  title: { fontSize: 24, fontWeight: '900', lineHeight: 30, marginBottom: 12 },
  meta: { fontSize: 13, marginBottom: 20 },
  body: { fontSize: 16, lineHeight: 26 },
});
