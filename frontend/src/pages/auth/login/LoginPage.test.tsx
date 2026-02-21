import { describe, expect, test, vi } from "vitest";
import { fireEvent, render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { LoginPage } from "./LoginPage";
import { useLoginForm } from "./useLoginForm";

vi.mock("./useLoginForm", () => ({
  useLoginForm: vi.fn(),
}));

const mockedUseLoginForm = vi.mocked(useLoginForm);

describe("LoginPage", () => {
  test("renders values from hook and forwards user interactions", () => {
    const onSubmit = vi.fn((e: any) => e.preventDefault());
    const setEmail = vi.fn();
    const setPassword = vi.fn();

    mockedUseLoginForm.mockReturnValue({
      email: "user@example.com",
      password: "Password123!",
      busy: false,
      error: null,
      setEmail,
      setPassword,
      onSubmit,
    });

    render(
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>
    );

    fireEvent.change(screen.getByLabelText(/email/i), { target: { value: "next@example.com" } });
    fireEvent.change(screen.getByLabelText(/password/i), { target: { value: "NextPassword123!" } });
    fireEvent.submit(screen.getByRole("button", { name: /sign in/i }));

    expect(setEmail).toHaveBeenCalledWith("next@example.com");
    expect(setPassword).toHaveBeenCalledWith("NextPassword123!");
    expect(onSubmit).toHaveBeenCalledTimes(1);
  });

  test("shows busy state and error from hook", () => {
    mockedUseLoginForm.mockReturnValue({
      email: "",
      password: "",
      busy: true,
      error: "Login failed",
      setEmail: vi.fn(),
      setPassword: vi.fn(),
      onSubmit: vi.fn(),
    });

    render(
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>
    );

    expect(screen.getByRole("button", { name: /signing in/i })).toBeDisabled();
    expect(screen.getByText("Login failed")).toBeInTheDocument();
  });
});
