import { render, screen } from "@testing-library/react";
import RichTextEditor from "./richTextEditor";

describe("RichTextEditor", () => {
  it("renders the initial HTML and the formatting toolbar", async () => {
    render(
      <RichTextEditor
        value="<p>Halló <strong>heimur</strong></p>"
        onChange={jest.fn()}
      />
    );

    expect(await screen.findByText("heimur")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Feitletra" })).toHaveAttribute(
      "aria-pressed",
      "false"
    );
    expect(screen.getByRole("button", { name: "Tengill" })).toBeInTheDocument();
  });
});
