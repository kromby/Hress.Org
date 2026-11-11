const MONTHS = [
  "jan.",
  "feb.",
  "mar.",
  "apr.",
  "maí",
  "jún.",
  "júl.",
  "ágú.",
  "sep.",
  "okt.",
  "nóv.",
  "des.",
];

export const relativeTime = (iso: string, now: Date = new Date()): string => {
  const date = new Date(iso);
  const minutes = Math.floor((now.getTime() - date.getTime()) / 60000);
  if (minutes < 1) return "rétt í þessu";
  if (minutes < 60) return `fyrir ${minutes} mín.`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `fyrir ${hours} klst.`;
  const days = Math.floor(hours / 24);
  if (days === 1) return "í gær";
  if (days < 7) return `fyrir ${days} dögum`;
  return `${date.getDate()}. ${MONTHS[date.getMonth()]}`;
};
