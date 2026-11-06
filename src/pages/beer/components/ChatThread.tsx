import { useEffect } from "react";
import { useHressAuth } from "../hress";
import { isUnauthorized, useMessages } from "../hooks/useBeerApi";
import { ChatMessage } from "../types";

interface ChatThreadProps {
  seasonID: string;
  beerID: string;
}

const styles = {
  section: { margin: "1.5em 0" },
  list: { listStyle: "none", padding: 0, margin: 0 },
  message: {
    padding: "0.6em 0",
    borderBottom: "solid 1px rgba(160, 160, 160, 0.3)",
  },
  meta: { display: "flex", flexWrap: "wrap" as const, gap: "0 0.75em" },
  // Keep the author's line breaks and wrap long words and links.
  text: {
    margin: "0.25em 0 0",
    whiteSpace: "pre-wrap" as const,
    overflowWrap: "anywhere" as const,
  },
};

const formatTime = (iso: string) =>
  new Date(iso).toLocaleString("is-IS", {
    day: "numeric",
    month: "short",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });

const Message = ({ message }: { message: ChatMessage }) => (
  <li data-testid="chat-message" style={styles.message}>
    <div style={styles.meta}>
      <strong>{message.username}</strong>
      <small>
        <time dateTime={message.created}>{formatTime(message.created)}</time>
      </small>
    </div>
    {/* Plain text from members: rendered as text, never as HTML. */}
    <p style={styles.text}>{message.text}</p>
  </li>
);

// The beer's chat, oldest first. Everyone sees it, rated or not, also after
// the season has ended.
const ChatThread = ({ seasonID, beerID }: ChatThreadProps) => {
  const { signOut } = useHressAuth();
  const {
    data: messages,
    isLoading,
    isError,
    error,
  } = useMessages(seasonID, beerID);
  const unauthorized = isError && isUnauthorized(error);

  // The token expired after the beer loaded. Clearing it makes RequireLogin
  // send the user to the login page, as the page does for its other requests.
  useEffect(() => {
    if (unauthorized) signOut();
  }, [unauthorized]);

  return (
    <section data-testid="chat" style={styles.section}>
      <h3>Spjall</h3>
      {isLoading ? (
        <p data-testid="chat-loading">Sæki spjall…</p>
      ) : isError ? (
        <p data-testid="chat-error">Ekki tókst að sækja spjallið.</p>
      ) : messages && messages.length > 0 ? (
        <ol style={styles.list} aria-label="Skilaboð">
          {messages.map((message) => (
            <Message key={message.id} message={message} />
          ))}
        </ol>
      ) : (
        <p data-testid="chat-empty">Engin skilaboð enn.</p>
      )}
    </section>
  );
};

export default ChatThread;
