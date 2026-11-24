import { useLayoutEffect, useState } from 'react';
import { createRoot } from 'react-dom/client';
import { LiquidNavbar, NimbusIcons, type LiquidNavItem } from '../components/ui/liquid-glass-navbar';

const icons = {
  ...NimbusIcons,
  cart: <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.6" aria-hidden="true"><path d="M3 4h2l2.5 11h11L21 7H6"/><circle cx="9" cy="19" r="1"/><circle cx="17" cy="19" r="1"/></svg>,
  admin: <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.6" aria-hidden="true"><path d="M4 4h6v6H4zm10 0h6v6h-6zM4 14h6v6H4zm10 0h6v6h-6z"/></svg>,
};

const readTone = () => document.documentElement.dataset.theme === 'light' ? 'light' : 'dark';

function Navigation({ fallback, items, active }: {
  fallback: HTMLElement;
  items: LiquidNavItem[];
  active: number;
}) {
  const [tone, setTone] = useState(readTone);
  const currentItem = () => {
    if (fallback.dataset.liquidNav === 'primary' && location.hash === '#bookshelf' && location.pathname === '/') {
      return items.findIndex(item => item.href === location.href);
    }
    return active;
  };
  const [current, setCurrent] = useState(currentItem);
  useLayoutEffect(() => {
    const update = () => setCurrent(currentItem());
    window.addEventListener('hashchange', update);
    return () => window.removeEventListener('hashchange', update);
  }, [active, fallback, items]);

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
    active={current}
    tone={tone}
    label={fallback.getAttribute('aria-label') || 'ناوبری'}
    compact={false}
  />;
}

const desktop = matchMedia('(min-width: 1050px)');
for (const fallback of document.querySelectorAll<HTMLElement>('[data-liquid-nav="primary"], [data-liquid-nav="admin"]')) {
  const links = [...fallback.querySelectorAll<HTMLAnchorElement>('a[href]')];
  const items: LiquidNavItem[] = links.map(link => ({
    label: link.textContent!.trim(),
    href: link.href,
    icon: icons[link.dataset.liquidIcon as keyof typeof icons],
    fx: link.dataset.liquidIcon ? 'pop' : 'none',
  }));
  if (items.length === 0) continue;

  const host = document.createElement('div');
  host.className = `liquid-nav-host liquid-nav-host--${fallback.dataset.liquidNav}`;
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
