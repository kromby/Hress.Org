import axios from "axios";
import { useEffect, useState } from "react";
import config from 'react-global-configuration';
import { Post } from "../../../../components";
import { useAuth } from "../../../../context/auth";
import { ElectionModuleProps } from ".";
import { useLocation, useNavigate } from "react-router-dom";

interface TShirtSize {
  id: number;
  name: string;
}

const TShirtSize = ({ ID, Name, onSubmit }: ElectionModuleProps) => {
  const { authTokens } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [selectedSize, setSelectedSize] = useState<number | undefined>(undefined);
  const [savingAllowed, setSavingAllowed] = useState(false);
  const [existingLookupId, setExistingLookupId] = useState<number | undefined>(undefined);
  const [loading, setLoading] = useState(true);

  const userID = Number(localStorage.getItem("userID"));
  const typeId = 226; // T-shirt size TypeId

  // T-shirt sizes: 227 = S, 228 = M, 229 = L, 230 = XL (based on test data)
  const tShirtSizes: TShirtSize[] = [
    { id: 227, name: "S" },
    { id: 228, name: "M" },
    { id: 229, name: "L" },
    { id: 230, name: "XL" },
  ];

  useEffect(() => {
    if (authTokens === undefined) {
      navigate("/login", { state: { from: location.pathname } });
      return;
    }

    const getExistingTShirtSize = async () => {
      try {
        // Check if user already has a t-shirt size lookup
        const url = `${config.get("apiPath")}/api/users/${userID}/lookups?typeId=${typeId}`;
        const response = await axios.get(url, {
          headers: { "X-Custom-Authorization": `token ${authTokens.token}` },
        }).catch(() => null);
        
        if (response?.data) {
          setSelectedSize(response.data.valueId);
          if (response.data.id) {
            setExistingLookupId(response.data.id);
          }
          setSavingAllowed(true);
        }
        setLoading(false);
      } catch (e) {
        // 404 is expected if no lookup exists yet
        if (axios.isAxiosError(e) && e.response?.status !== 404) {
          console.error(e);
        }
        setLoading(false);
      }
    };

    getExistingTShirtSize();
  }, [authTokens, navigate, location]);

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

  return (
    <div>
      <Post
        id={ID}
        title={Name}
        body={
          <section>
            <p>Vinsamlegast veldu stærð á bol sem þú vilt fá:</p>
            <form onSubmit={handleSubmit}>
              <div className="row gtr-uniform">
                {tShirtSizes.map((size) => (
                  <div className="col-6" key={size.id}>
                    <input
                      type="radio"
                      id={`size_${size.id}`}
                      name="tshirtSize"
                      checked={selectedSize === size.id}
                      onChange={() => handleChange(size.id)}
                    />
                    <label htmlFor={`size_${size.id}`} style={{ marginLeft: "10px", cursor: "pointer" }}>
                      <b>{size.name}</b>
                    </label>
                  </div>
                ))}
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

