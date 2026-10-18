import DOMPurify from "dompurify";

// News bodies are user-authored HTML. Legacy news links use target="_blank",
// which DOMPurify strips by default.
export const sanitizeNewsHtml = (html: string | null | undefined): string =>
  html ? DOMPurify.sanitize(html, { ADD_ATTR: ["target"] }) : "";
