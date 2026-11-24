import { useLayoutEffect, useState } from 'react';
import { createRoot } from 'react-dom/client';
import { LiquidNavbar, type LiquidNavItem } from '../components/ui/liquid-glass-navbar';

const readTone = () => document.documentElement.dataset.theme === 'light' ? 'light' : 'dark';

function Navigation({ fallback, items, active }: {
  fallback: HTMLElement;
  items: LiquidNavItem[];
  active: number;
}) {
  const [tone, setTone] = useState(readTone);
  useLayoutEffect(() => {
    const update = () => setTone(readTone());
    fallback.hidden = true;
    document.addEventListener('milo:theme', update);
    return () => {
      fallback.hidden = false;
      document.removeEventListener('milo:theme', update);
    };
  }, [fallback]);

  return <LiquidNavbar
    items={items}
    active={active}
    tone={tone}
    label={fallback.getAttribute('aria-label') || 'ناوبری'}
    compact={false}
  />;
}

const desktop = matchMedia('(min-width: 1050px)');
for (const fallback of document.querySelectorAll<HTMLElement>('[data-liquid-nav="admin"]')) {
  const links = [...fallback.querySelectorAll<HTMLAnchorElement>('a[href]')];
  const items: LiquidNavItem[] = links.map(link => ({
    label: link.textContent!.trim(),
    href: link.href,
    fx: 'none',
  }));
  if (items.length === 0) continue;

  const host = document.createElement('div');
  host.className = 'liquid-nav-host liquid-nav-host--admin';
  fallback.after(host);
  let root: ReturnType<typeof createRoot> | undefined;

  const sync = () => {
    if (desktop.matches && !root) {
      root = createRoot(host);
      const active = links.findIndex(link => link.getAttribute('aria-current') === 'page');
      root.render(<Navigation fallback={fallback} items={items} active={active} />);
    } else if (!desktop.matches && root) {
      root.unmount();
      root = undefined;
    }
  };
  desktop.addEventListener('change', sync);
  sync();
}
