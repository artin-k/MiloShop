import React from 'react';
import { createRoot } from 'react-dom/client';
import { BookshelfScene } from '@designcodeio/threeui';
import './scene.css';

// An isolated React component inside a Razor page; no client-side routing.
export function mountBookshelf(element: HTMLElement) {
  const root = createRoot(element);
  root.render(
    <div className="shader-frame">
      <BookshelfScene />
    </div>,
  );
  return () => root.unmount();
}
