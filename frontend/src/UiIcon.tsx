type Props = { name: keyof typeof paths; size?: number }

const paths = {
  close: 'M6 6l12 12M18 6 6 18',
  chevron: 'm6 9 6 6 6-6',
  overview: 'M3 3h7v7H3z M14 3h7v7h-7z M3 14h7v7H3z M14 14h7v7h-7z',
  category: 'M3 4h7l11 11-6 6L3 9z M7 8h.01',
  transactions: 'M4 7h16m-4-4 4 4-4 4M20 17H4m4-4-4 4 4 4',
  plus: 'M12 5v14M5 12h14',
  logout: 'M9 4H4v16h5M9 12h12m-4-4 4 4-4 4',
  up: 'M7 17 17 7M7 7h10v10',
  down: 'M7 7 17 17M7 17h10V7',
  wallet: 'M20 8V5a1 1 0 0 0-1-1H5a2 2 0 0 0 0 4h15v12H5a2 2 0 0 1-2-2V6m17 6h-5v4h5',
  edit: 'm15 5 4 4M4 20l4-1L21 6l-4-4L4 15z',
  trash: 'M3 6h18M9 6V3h6v3M5 6l1 15h12l1-15M10 10v7m4-7v7',
}

export default function UiIcon({ name, size = 20 }: Props) {
  return <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true"><path d={paths[name]} /></svg>
}
