import { sanitizeNewsHtml } from "./sanitizeHtml";

describe("sanitizeNewsHtml", () => {
  it("removes script tags", () => {
    const result = sanitizeNewsHtml('<p>Hæ</p><script>alert("x")</script>');
    expect(result).toBe("<p>Hæ</p>");
  });

  it("removes inline event handlers", () => {
    const result = sanitizeNewsHtml('<img src="x" onerror="alert(1)">');
    expect(result).not.toContain("onerror");
  });

  it("removes javascript: links", () => {
    const result = sanitizeNewsHtml('<a href="javascript:alert(1)">smella</a>');
    expect(result).not.toContain("javascript:");
  });

  it("keeps legacy formatting and target=_blank links", () => {
    const html =
      '<b>Feitt</b> <i>skáletrað</i> <a target="_blank" href="http://hress.org">tengill</a>';
    const result = sanitizeNewsHtml(html);
    expect(result).toContain("<b>Feitt</b>");
    expect(result).toContain("<i>skáletrað</i>");
    expect(result).toContain('target="_blank"');
    expect(result).toContain('href="http://hress.org"');
  });

  it("returns an empty string for missing content", () => {
    expect(sanitizeNewsHtml(undefined)).toBe("");
  });
});
