export function getTelegramWebApp() {
  return window.Telegram?.WebApp;
}

export function getInitData(): string {
  return getTelegramWebApp()?.initData ?? '';
}

export function expandApp(): void {
  getTelegramWebApp()?.expand();
}

export function getThemeParams() {
  return getTelegramWebApp()?.themeParams;
}

export function showMainButton(text: string, onClick: () => void): void {
  const btn = getTelegramWebApp()?.MainButton;
  if (btn) {
    btn.setText(text);
    btn.onClick(onClick);
    btn.show();
  }
}

export function hideMainButton(): void {
  const btn = getTelegramWebApp()?.MainButton;
  if (btn) {
      btn.hide();
  }
}

export type HapticType = 
  | 'light' | 'medium' | 'heavy' | 'rigid' | 'soft' 
  | 'error' | 'success' | 'warning' 
  | 'selection';

export function hapticFeedback(type: HapticType = 'medium'): void {
  const haptic = getTelegramWebApp()?.HapticFeedback;
  if (!haptic) return;

  if (type === 'selection') {
    haptic.selectionChanged();
  } else if (type === 'error' || type === 'success' || type === 'warning') {
    haptic.notificationOccurred(type as any); // Type assertion usually safe with WebApp strings
  } else {
    haptic.impactOccurred(type as any);
  }
}
