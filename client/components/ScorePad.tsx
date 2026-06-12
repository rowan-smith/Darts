import { Pressable, StyleSheet, Text, View } from 'react-native';
import { useTheme } from '../context/ThemeContext';

interface Props {
  onScore: (score: number) => void;
  disabled?: boolean;
}

const QUICK_SCORES = [180, 140, 100, 85, 60, 45, 26, 0];

export function ScorePad({ onScore, disabled }: Props) {
  const { colors } = useTheme();

  return (
    <View style={styles.container}>
      <Text style={[styles.label, { color: colors.textSecondary }]}>Quick Scores</Text>
      <View style={styles.grid}>
        {QUICK_SCORES.map((score) => (
          <Pressable
            key={score}
            disabled={disabled}
            onPress={() => onScore(score)}
            style={({ pressed }) => [
              styles.button,
              {
                backgroundColor: score === 180 ? colors.accent + '33' : colors.surfaceAlt,
                borderColor: score === 180 ? colors.accent : colors.border,
                opacity: disabled ? 0.4 : pressed ? 0.8 : 1,
              },
            ]}
          >
            <Text style={[styles.buttonText, { color: score === 180 ? colors.accent : colors.text }]}>
              {score}
            </Text>
          </Pressable>
        ))}
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { marginTop: 16 },
  label: { fontSize: 12, fontWeight: '600', marginBottom: 8, textTransform: 'uppercase' },
  grid: { flexDirection: 'row', flexWrap: 'wrap', gap: 8 },
  button: {
    width: '23%',
    paddingVertical: 14,
    borderRadius: 10,
    borderWidth: 1,
    alignItems: 'center',
  },
  buttonText: { fontSize: 16, fontWeight: '700' },
});
