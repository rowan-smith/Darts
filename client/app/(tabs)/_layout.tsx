import Ionicons from '@expo/vector-icons/Ionicons';
import { DynamicColorIOS, Platform } from 'react-native';
import { Icon, Label, NativeTabs, VectorIcon } from 'expo-router/unstable-native-tabs';
import { useTheme } from '../../context/ThemeContext';

const tabTintColor =
  Platform.OS === 'ios'
    ? DynamicColorIOS({ light: '#C8102E', dark: '#E63946' })
    : '#C8102E';

const tabIconColor =
  Platform.OS === 'ios'
    ? DynamicColorIOS({ light: '#6B7280', dark: '#8B949E' })
    : undefined;

export default function TabLayout() {
  const { isDark } = useTheme();

  return (
    <NativeTabs
      tintColor={tabTintColor}
      iconColor={tabIconColor}
      minimizeBehavior="onScrollDown"
      disableTransparentOnScrollEdge
      labelStyle={
        Platform.OS === 'ios'
          ? {
              color: DynamicColorIOS({ light: '#1A1A2E', dark: '#F0F6FC' }),
              fontSize: 11,
              fontWeight: '600',
            }
          : { fontSize: 12, fontWeight: '600' }
      }
      backgroundColor={Platform.OS === 'android' ? (isDark ? '#0D1117' : '#FFFFFF') : undefined}
    >
      <NativeTabs.Trigger name="index">
        <Label>Home</Label>
        <Icon
          sf={{ default: 'house', selected: 'house.fill' }}
          androidSrc={<VectorIcon family={Ionicons} name="home" />}
          selectedColor={tabTintColor}
        />
      </NativeTabs.Trigger>

      <NativeTabs.Trigger name="matches">
        <Label>Scores</Label>
        <Icon
          sf={{ default: 'sportscourt', selected: 'sportscourt.fill' }}
          androidSrc={<VectorIcon family={Ionicons} name="basketball" />}
          selectedColor={tabTintColor}
        />
      </NativeTabs.Trigger>

      <NativeTabs.Trigger name="profile">
        <Label>Profile</Label>
        <Icon
          sf={{ default: 'person', selected: 'person.fill' }}
          androidSrc={<VectorIcon family={Ionicons} name="person" />}
          selectedColor={tabTintColor}
        />
      </NativeTabs.Trigger>
    </NativeTabs>
  );
}
