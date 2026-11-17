import { fireEvent, render, screen } from "@testing-library/react";
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

  it("inserts a link from the inline link field", async () => {
    const onChange = jest.fn();
    render(<RichTextEditor value="" onChange={onChange} />);

    fireEvent.click(await screen.findByRole("button", { name: "Tengill" }));
    fireEvent.change(screen.getByLabelText("Slóð tengils"), {
      target: { value: "https://hress.org" },
    });
    fireEvent.click(screen.getByRole("button", { name: "Setja tengil" }));

    expect(onChange).toHaveBeenLastCalledWith(
      expect.stringContaining('href="https://hress.org"')
    );
    expect(screen.queryByLabelText("Slóð tengils")).not.toBeInTheDocument();
  });

  it("forwards error description and invalid state to the editable area", async () => {
    render(
      <RichTextEditor value="" onChange={jest.fn()} errorId="err" invalid />
    );

    const textbox = await screen.findByRole("textbox");
    expect(textbox).toHaveAttribute("aria-describedby", "err");
    expect(textbox).toHaveAttribute("aria-invalid", "true");
  });
});
