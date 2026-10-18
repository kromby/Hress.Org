import "./richTextEditor.css";
import { useEffect } from "react";
import { EditorContent, useEditor, useEditorState } from "@tiptap/react";
import StarterKit from "@tiptap/starter-kit";

type RichTextEditorProps = {
  value: string;
  onChange: (html: string) => void;
  id?: string;
};

const RichTextEditor = ({ value, onChange, id }: RichTextEditorProps) => {
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
      attributes: {
        class: "rich-text-editor__content",
        role: "textbox",
        "aria-multiline": "true",
        ...(id ? { "aria-labelledby": id } : {}),
      },
    },
  });

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

  const setLink = () => {
    const previous = editor.getAttributes("link").href as string | undefined;
    const href = window.prompt("Slóð tengils", previous ?? "https://");
    if (href === null) return;
    if (href.trim() === "") {
      editor.chain().focus().extendMarkRange("link").unsetLink().run();
      return;
    }
    editor
      .chain()
      .focus()
      .extendMarkRange("link")
      .setLink({ href: href.trim() })
      .run();
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
      run: setLink,
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
      <EditorContent editor={editor} />
    </div>
  );
};

export default RichTextEditor;
