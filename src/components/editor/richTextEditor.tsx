import "./richTextEditor.css";
import { useEffect, useRef, useState } from "react";
import { EditorContent, useEditor, useEditorState } from "@tiptap/react";
import StarterKit from "@tiptap/starter-kit";

type RichTextEditorProps = {
  value: string;
  onChange: (html: string) => void;
  id?: string;
  errorId?: string;
  invalid?: boolean;
};

const contentAttributes = (
  id?: string,
  errorId?: string,
  invalid?: boolean
): Record<string, string> => ({
  class: "rich-text-editor__content",
  role: "textbox",
  "aria-multiline": "true",
  ...(id ? { "aria-labelledby": id } : {}),
  ...(errorId ? { "aria-describedby": errorId } : {}),
  ...(invalid ? { "aria-invalid": "true" } : {}),
});

const RichTextEditor = ({
  value,
  onChange,
  id,
  errorId,
  invalid,
}: RichTextEditorProps) => {
  const [linkHref, setLinkHref] = useState<string | null>(null);
  const linkInput = useRef<HTMLInputElement>(null);
  const linkFieldOpen = linkHref !== null;

  useEffect(() => {
    if (linkFieldOpen) linkInput.current?.focus();
  }, [linkFieldOpen]);

  const editor = useEditor({
    extensions: [
      StarterKit.configure({
        heading: { levels: [2, 3] },
        link: { openOnClick: false, autolink: true },
      }),
    ],
    content: value,
    onUpdate: ({ editor: current }) => {
      onChange(current.isEmpty ? "" : current.getHTML());
    },
    editorProps: {
      attributes: contentAttributes(id, errorId, invalid),
    },
  });

  useEffect(() => {
    editor?.setOptions({
      editorProps: { attributes: contentAttributes(id, errorId, invalid) },
    });
  }, [editor, id, errorId, invalid]);

  // Keep the editor in sync when the value is replaced from outside (e.g. loaded for editing).
  useEffect(() => {
    if (!editor) return;
    const current = editor.isEmpty ? "" : editor.getHTML();
    if (value !== current) {
      editor.commands.setContent(value, { emitUpdate: false });
    }
  }, [editor, value]);

  const state = useEditorState({
    editor,
    selector: ({ editor: e }) => ({
      bold: e?.isActive("bold") ?? false,
      italic: e?.isActive("italic") ?? false,
      bulletList: e?.isActive("bulletList") ?? false,
      orderedList: e?.isActive("orderedList") ?? false,
      link: e?.isActive("link") ?? false,
    }),
  });

  if (!editor) {
    return null;
  }

  const openLinkField = () => {
    const previous = editor.getAttributes("link").href as string | undefined;
    setLinkHref(previous ?? "https://");
  };

  const applyLink = (event?: { preventDefault: () => void }) => {
    event?.preventDefault();
    const href = (linkHref ?? "").trim();
    setLinkHref(null);

    if (href === "" || href === "https://") {
      editor.chain().focus().extendMarkRange("link").unsetLink().run();
      return;
    }

    if (editor.state.selection.empty && !editor.isActive("link")) {
      // Nothing selected: insert the URL itself as the link text.
      editor
        .chain()
        .focus()
        .insertContent({
          type: "text",
          text: href,
          marks: [{ type: "link", attrs: { href } }],
        })
        .run();
      return;
    }

    editor.chain().focus().extendMarkRange("link").setLink({ href }).run();
  };

  const buttons = [
    {
      label: "Feitletra",
      text: <b>B</b>,
      active: state?.bold,
      run: () => editor.chain().focus().toggleBold().run(),
    },
    {
      label: "Skáletra",
      text: <i>I</i>,
      active: state?.italic,
      run: () => editor.chain().focus().toggleItalic().run(),
    },
    {
      label: "Punktalisti",
      text: "•",
      active: state?.bulletList,
      run: () => editor.chain().focus().toggleBulletList().run(),
    },
    {
      label: "Númeraður listi",
      text: "1.",
      active: state?.orderedList,
      run: () => editor.chain().focus().toggleOrderedList().run(),
    },
    {
      label: "Tengill",
      text: "🔗",
      active: state?.link,
      run: openLinkField,
    },
  ];

  return (
    <div className="rich-text-editor">
      <div className="rich-text-editor__toolbar" role="toolbar">
        {buttons.map((button) => (
          <button
            key={button.label}
            type="button"
            className={`button small${button.active ? " primary" : ""}`}
            aria-label={button.label}
            aria-pressed={button.active}
            title={button.label}
            onClick={button.run}
          >
            {button.text}
          </button>
        ))}
      </div>
      {linkHref !== null && (
        <div className="rich-text-editor__link">
          <input
            type="url"
            aria-label="Slóð tengils"
            value={linkHref}
            ref={linkInput}
            onChange={(e) => setLinkHref(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === "Enter") applyLink(e);
              if (e.key === "Escape") setLinkHref(null);
            }}
          />
          <button
            type="button"
            className="button small"
            onClick={() => applyLink()}
          >
            Setja tengil
          </button>
          <button
            type="button"
            className="button small"
            onClick={() => setLinkHref(null)}
          >
            Hætta við
          </button>
        </div>
      )}
      <EditorContent editor={editor} />
    </div>
  );
};

export default RichTextEditor;
