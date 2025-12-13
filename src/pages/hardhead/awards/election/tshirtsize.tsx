import axios from "axios";
import { useEffect, useState, useMemo } from "react";
import config from "react-global-configuration";
import { Post } from "../../../../components";
import { useAuth } from "../../../../context/auth";
import { ElectionModuleProps } from ".";
import { useLocation, useNavigate } from "react-router-dom";
import { useLookup } from "../../../../hooks/useLookup";
import { useTypes } from "../../../../hooks/useTypes";

const TShirtSize = ({ ID, Name, Href, onSubmit }: ElectionModuleProps) => {
  const { authTokens } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [selectedSize, setSelectedSize] = useState<number | undefined>(
    undefined
  );
  const [savingAllowed, setSavingAllowed] = useState(false);

  const userID = Number(localStorage.getItem("userID"));

  const {
    types: tShirtSizes,
    loading: typesLoading,
    error: typesError,
  } = useTypes(Href);
  const {
    existingLookup,
    loading: lookupLoading,
    error: lookupError,
  } = useLookup(Href, userID);

  const loading = typesLoading || lookupLoading;
  const error = typesError || lookupError;

  // Parse parentId from Href for lookup query (e.g., "/api/types?parentId=226")
  const getParentIdFromHref = (href: string | undefined): number | null => {
    if (!href) return null;
    const urlParams = new URLSearchParams(href.split("?")[1]);
    const parentId = urlParams.get("parentId");
    return parentId ? parseInt(parentId, 10) : null;
  };

  const existingLookupId = useMemo(() => existingLookup?.id, [existingLookup]);

  useEffect(() => {
    if (authTokens === undefined) {
      navigate("/login", { state: { from: location.pathname } });
      return;
    }

    if (!Href) {
      console.error("No Href provided");
      return;
    }

    const typeId = getParentIdFromHref(Href);
    if (!typeId) {
      console.error("No parentId found in Href");
      return;
    }
  }, [authTokens, navigate, location, Href]);

  // Set selected size when existing lookup is loaded
  useEffect(() => {
    if (existingLookup) {
      setSelectedSize(existingLookup.valueId);
      setSavingAllowed(true);
    }
  }, [existingLookup]);

  const handleChange = (sizeId: number) => {
    setSelectedSize(sizeId);
    setSavingAllowed(true);
  };

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setSavingAllowed(false);

    if (authTokens === undefined) {
      navigate("/login", { state: { from: location.pathname } });
      return;
    }

    if (!selectedSize) {
      alert("Vinsamlegast veldu stærð");
      setSavingAllowed(true);
      return;
    }

    const typeId = getParentIdFromHref(Href);
    if (!typeId) {
      alert("Villa: Ekki tókst að finna parentId");
      setSavingAllowed(true);
      return;
    }

    try {
      const url = `${config.get("apiPath")}/api/users/${userID}/lookups`;

      if (existingLookupId) {
        // Update existing lookup
        const updateUrl = `${url}/${existingLookupId}`;
        await axios.put(
          updateUrl,
          {
            typeId: typeId,
            valueId: selectedSize,
          },
          {
            headers: { "X-Custom-Authorization": `token ${authTokens.token}` },
          }
        );
      } else {
        // Create new lookup
        await axios.post(
          url,
          {
            typeId: typeId,
            valueId: selectedSize,
          },
          {
            headers: { "X-Custom-Authorization": `token ${authTokens.token}` },
          }
        );
      }

      // Save the step completion
      const voteUrl = `${config.get("apiPath")}/api/elections/${ID}/vote`;
      await axios.post(
        voteUrl,
        [
          {
            value: selectedSize,
          },
        ],
        {
          headers: { "X-Custom-Authorization": `token ${authTokens.token}` },
        }
      );
    } catch (e) {
      console.error(e);
      alert("Villa við að vista stærð: " + e);
      setSavingAllowed(true);
      return;
    }

    onSubmit();
  };

  if (loading) {
    return <Post id={ID} title={Name} body={<div>Sæki upplýsingar...</div>} />;
  }

  if (error) {
    return (
      <Post
        id={ID}
        title={Name}
        body={<div>Villa við að sækja upplýsingar: {error.message}</div>}
      />
    );
  }

  return (
    <div>
      <Post
        id={ID}
        title={Name}
        description="Hvaða stærð af Harðhausabol notar þú?"
        body={
          <section>
            <form onSubmit={handleSubmit}>
              <div className="row gtr-uniform">
                {tShirtSizes && tShirtSizes.length > 0 ? (
                  tShirtSizes.map((size) => (
                    <div className="col-12" key={size.id}>
                      <input
                        type="radio"
                        id={`size_${size.id}`}
                        name="tshirtSize"
                        checked={selectedSize === size.id}
                        onChange={() => handleChange(size.id)}
                      />
                      <label
                        htmlFor={`size_${size.id}`}
                        style={{ marginLeft: "10px", cursor: "pointer" }}
                      >
                        {size.name}
                      </label>
                    </div>
                  ))
                ) : (
                  <div>Sæki stærðir...</div>
                )}
              </div>
              <ul className="actions pagination" style={{ marginTop: "20px" }}>
                <li>
                  <button
                    type="submit"
                    disabled={!savingAllowed}
                    className="button large next"
                  >
                    Vista stærð
                  </button>
                </li>
              </ul>
            </form>
          </section>
        }
      />
    </div>
  );
};

export default TShirtSize;
