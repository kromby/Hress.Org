import { FormEvent, useEffect, useRef, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { AxiosError } from "axios";
import { Post } from "../../components";
import RichTextEditor from "../../components/editor/richTextEditor";
import { useAuth } from "../../context/auth";
import { useCreateNews } from "../../hooks/useCreateNews";

type ValidationErrors = {
  name?: string;
  content?: string;
};

const getErrorMessage = (error: unknown): string => {
  const axiosError = error as AxiosError<{ error?: string }>;
  return (
    axiosError?.response?.data?.error ?? axiosError?.message ?? "Óþekkt villa"
  );
};

const NewsEdit = () => {
  const { authTokens } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const { mutateAsync, isPending, error } = useCreateNews();

  const [name, setName] = useState("");
  const [content, setContent] = useState("");
  const [isPublic, setIsPublic] = useState(true);
  const [errors, setErrors] = useState<ValidationErrors>({});
  // isPending only flips after a re-render; this guards against a fast double submit.
  const submitting = useRef(false);

  useEffect(() => {
    document.title = "Skrifa frétt | Hress.Org";
    if (authTokens === undefined) {
      navigate("/login", { state: { from: location.pathname } });
    }
  }, [authTokens]);

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    if (submitting.current) return;

    const validation: ValidationErrors = {};
    if (!name.trim()) validation.name = "Fyrirsögn vantar.";
    if (!content.trim()) validation.content = "Meginmál vantar.";
    setErrors(validation);
    if (validation.name || validation.content) return;

    submitting.current = true;
    try {
      const created = await mutateAsync({
        name: name.trim(),
        content,
        public: isPublic,
      });
      navigate(`/news/${created.id}`);
    } catch (e) {
      // Shown through the mutation's error state.
      console.error(e);
    } finally {
      submitting.current = false;
    }
  };

  if (authTokens === undefined) {
    return null;
  }

  return (
    <div id="main">
      <Post
        title="Skrifa frétt"
        body={
          <form onSubmit={handleSubmit} noValidate>
            <div className="row gtr-uniform">
              <div className="col-12">
                <label htmlFor="news-name">Fyrirsögn</label>
                <input
                  id="news-name"
                  type="text"
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                  aria-invalid={Boolean(errors.name)}
                  aria-describedby={errors.name ? "news-name-error" : undefined}
                />
                {errors.name && (
                  <div id="news-name-error" className="error">
                    {errors.name}
                  </div>
                )}
              </div>
              <div className="col-12">
                <label id="news-content-label">Meginmál</label>
                <RichTextEditor
                  id="news-content-label"
                  value={content}
                  onChange={setContent}
                  errorId={errors.content ? "news-content-error" : undefined}
                  invalid={Boolean(errors.content)}
                />
                {errors.content && (
                  <div id="news-content-error" className="error">
                    {errors.content}
                  </div>
                )}
              </div>
              <div className="col-12">
                <input
                  id="news-public"
                  type="checkbox"
                  checked={isPublic}
                  onChange={(e) => setIsPublic(e.target.checked)}
                />
                <label htmlFor="news-public">Opinber</label>
              </div>
              <div className="col-12">
                {error && (
                  <div className="error">
                    Ekki tókst að vista frétt: {getErrorMessage(error)}
                  </div>
                )}
                <button
                  className="button large primary"
                  type="submit"
                  disabled={isPending}
                >
                  {isPending ? "Er að vista..." : "Birta frétt"}
                </button>
              </div>
            </div>
          </form>
        }
      />
    </div>
  );
};

export default NewsEdit;
