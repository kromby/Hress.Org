import { FormEvent, useEffect, useRef, useState } from "react";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import { AxiosError } from "axios";
import { Post } from "../../components";
import RichTextEditor from "../../components/editor/richTextEditor";
import { useAuth } from "../../context/auth";
import { useCreateNews } from "../../hooks/useCreateNews";
import { useNews } from "../../hooks/useNews";
import { useUpdateNews } from "../../hooks/useUpdateNews";

type ValidationErrors = {
  name?: string;
  content?: string;
};

const NOT_AUTHOR = "Þú getur aðeins breytt þínum eigin fréttum.";
const NOT_FOUND = "Fréttin fannst ekki.";

const getErrorMessage = (error: unknown): string => {
  const axiosError = error as AxiosError<{ error?: string }>;
  if (axiosError?.response?.status === 403) return NOT_AUTHOR;
  if (axiosError?.response?.status === 404) return NOT_FOUND;
  return (
    axiosError?.response?.data?.error ?? axiosError?.message ?? "Óþekkt villa"
  );
};

const NewsEdit = () => {
  const { authTokens } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const params = useParams();
  const newsId = params.id ? Number(params.id) : undefined;
  const isEdit = newsId !== undefined;

  const existing = useNews(newsId);
  const createNews = useCreateNews();
  const updateNews = useUpdateNews(newsId);
  const { mutateAsync, isPending, error } = isEdit ? updateNews : createNews;

  const [name, setName] = useState("");
  const [content, setContent] = useState("");
  const [isPublic, setIsPublic] = useState(true);
  const [errors, setErrors] = useState<ValidationErrors>({});
  // isPending only flips after a re-render; this guards against a fast double submit.
  const submitting = useRef(false);
  const prefilled = useRef(false);

  const loaded = existing.data;
  const isAuthor =
    loaded !== undefined &&
    String(loaded.insertedBy ?? loaded.author?.id) ===
      localStorage.getItem("userID");

  useEffect(() => {
    document.title = `${isEdit ? "Breyta frétt" : "Skrifa frétt"} | Hress.Org`;
    if (authTokens === undefined) {
      navigate("/login", { state: { from: location.pathname } });
    }
  }, [authTokens]);

  useEffect(() => {
    if (!loaded || prefilled.current) return;
    prefilled.current = true;
    setName(loaded.name ?? "");
    setContent(loaded.content ?? "");
    // The API omits false values, so a missing flag means not public.
    setIsPublic(loaded.public === true);
  }, [loaded]);

  const clearError = (field: keyof ValidationErrors) => {
    setErrors((current) =>
      current[field] ? { ...current, [field]: undefined } : current
    );
  };

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
      // The image picker (#707) is not built yet; keep any existing image.
      const image =
        isEdit && loaded?.image?.id
          ? { imageId: loaded.image.id, imageAlign: loaded.imageAlign }
          : {};
      const saved = await mutateAsync({
        name: name.trim(),
        content,
        public: isPublic,
        ...image,
      });
      navigate(`/news/${saved.id ?? newsId}`);
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

  const title = isEdit ? "Breyta frétt" : "Skrifa frétt";

  if (isEdit && !isAuthor) {
    let message = loaded ? NOT_AUTHOR : NOT_FOUND;
    if (existing.isLoading) message = "Sæki frétt...";
    else if (existing.error) message = getErrorMessage(existing.error);
    return (
      <div id="main">
        <Post title={title} body={<p>{message}</p>} />
      </div>
    );
  }

  return (
    <div id="main">
      <Post
        title={title}
        body={
          <form onSubmit={handleSubmit} noValidate>
            <div className="row gtr-uniform">
              <div className="col-12">
                <label htmlFor="news-name">Fyrirsögn</label>
                <input
                  id="news-name"
                  type="text"
                  value={name}
                  onChange={(e) => {
                    setName(e.target.value);
                    clearError("name");
                  }}
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
                  onChange={(html) => {
                    setContent(html);
                    clearError("content");
                  }}
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
                  {isPending
                    ? "Er að vista..."
                    : isEdit
                      ? "Vista breytingar"
                      : "Birta frétt"}
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
