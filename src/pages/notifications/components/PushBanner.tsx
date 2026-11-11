import { useCallback, useEffect, useState } from "react";
import { useAuth } from "../../../context/auth";
import { disablePush, enablePush, getPushState } from "../push";
import { PushState } from "../types";

const PushBanner = () => {
  const { authTokens } = useAuth();
  const token: string = authTokens?.token;
  const [state, setState] = useState<PushState | null>(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    getPushState(token)
      .then((s) => {
        if (!cancelled) setState(s);
      })
      .catch(() => {
        if (!cancelled) setState("unsupported");
      });
    return () => {
      cancelled = true;
    };
  }, [token]);

  const change = useCallback(
    async (action: (token: string) => Promise<void>, message: string) => {
      setBusy(true);
      setError(null);
      try {
        await action(token);
      } catch {
        setError(message);
      }
      try {
        setState(await getPushState(token));
      } catch {
        // Keep the previous state.
      } finally {
        setBusy(false);
      }
    },
    [token]
  );

  if (state === null || state === "unsupported") return null;

  return (
    <div className="push-banner">
      {state === "ios-install" && (
        <p>
          Til að fá tilkynningar á iPhone eða iPad þarftu að bæta Hress.Org á
          heimaskjáinn: ýttu á Deila-hnappinn og veldu „Add to Home Screen“.
        </p>
      )}
      {state === "blocked" && (
        <p>
          Tilkynningar eru læstar í stillingum vafrans fyrir hress.org. Leyfðu
          þær þar til að geta kveikt á þeim.
        </p>
      )}
      {state === "off" && (
        <>
          <p>Fáðu tilkynningu í þetta tæki þegar ný frétt birtist.</p>
          <button
            type="button"
            disabled={busy}
            onClick={() =>
              change(enablePush, "Ekki tókst að kveikja á tilkynningum.")
            }
          >
            Kveikja
          </button>
        </>
      )}
      {state === "on" && (
        <>
          <p>Kveikt á tilkynningum í þessu tæki</p>
          <button
            type="button"
            disabled={busy}
            onClick={() =>
              change(disablePush, "Ekki tókst að slökkva á tilkynningum.")
            }
          >
            Slökkva
          </button>
        </>
      )}
      {error && (
        <p className="push-error" role="alert">
          {error}
        </p>
      )}
    </div>
  );
};

export default PushBanner;
