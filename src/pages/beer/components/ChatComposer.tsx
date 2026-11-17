import { FormEvent, useEffect, useState } from "react";
import { useHressAuth } from "../hress";
import { isUnauthorized, usePostMessage } from "../hooks/useBeerApi";

// The API allows up to 2000 characters after trimming.
export const MAX_MESSAGE_LENGTH = 2000;

interface ChatComposerProps {
  seasonID: string;
  beerID: string;
}

const styles = {
  form: { marginTop: "1em" },
  actions: {
    display: "flex",
    alignItems: "center",
    gap: "0.75em",
    marginTop: "0.5em",
  },
};

const ChatComposer = ({ seasonID, beerID }: ChatComposerProps) => {
  const { signOut } = useHressAuth();
  const postMessage = usePostMessage(seasonID, beerID);
  const [text, setText] = useState("");
  const canSend = text.trim().length > 0 && !postMessage.isPending;
  const unauthorized = postMessage.isError && isUnauthorized(postMessage.error);

  // The token expired. Clearing it makes RequireLogin send the user to the login page.
  useEffect(() => {
    if (unauthorized) signOut();
  }, [unauthorized]);

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault();
    if (!canSend) return;
    postMessage.mutate(text, {
      // A failed post keeps the text so it can be sent again.
      onSuccess: () => setText(""),
    });
  };

  return (
    <form
      data-testid="chat-composer"
      style={styles.form}
      onSubmit={handleSubmit}
    >
      <textarea
        aria-label="Skilaboð um bjórinn"
        placeholder="Hvað finnst þér um bjórinn?"
        rows={3}
        maxLength={MAX_MESSAGE_LENGTH}
        value={text}
        onChange={(e) => setText(e.target.value)}
      />
      <div style={styles.actions}>
        <button type="submit" className="button" disabled={!canSend}>
          {postMessage.isPending ? "Sendi…" : "Senda"}
        </button>
        {postMessage.isError && !unauthorized && (
          <small data-testid="chat-post-error" role="alert">
            Ekki tókst að senda skilaboðin. Reyndu aftur.
          </small>
        )}
      </div>
    </form>
  );
};

export default ChatComposer;
