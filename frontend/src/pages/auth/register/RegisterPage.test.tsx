import { describe, expect, test, vi } from "vitest";
import { fireEvent, render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { RegisterPage } from "./RegisterPage";
import { useRegisterForm } from "./useRegisterForm";

vi.mock("./useRegisterForm", () => ({
  useRegisterForm: vi.fn(),
}));

const mockedUseRegisterForm = vi.mocked(useRegisterForm);

describe("RegisterPage", () => {
  test("renders values from hook and forwards user interactions", () => {
    const onSubmit = vi.fn((e: any) => e.preventDefault());
    const setEmail = vi.fn();
    const setPassword = vi.fn();

    mockedUseRegisterForm.mockReturnValue({
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
        <RegisterPage />
      </MemoryRouter>
    );

    fireEvent.change(screen.getByLabelText(/email/i), { target: { value: "next@example.com" } });
    fireEvent.change(screen.getByLabelText(/password/i), { target: { value: "NextPassword123!" } });
    fireEvent.submit(screen.getByRole("button", { name: /create account/i }));

    expect(setEmail).toHaveBeenCalledWith("next@example.com");
    expect(setPassword).toHaveBeenCalledWith("NextPassword123!");
    expect(onSubmit).toHaveBeenCalledTimes(1);
  });

  test("shows busy state and error from hook", () => {
    mockedUseRegisterForm.mockReturnValue({
      email: "",
      password: "",
      busy: true,
      error: "Email is already registered.",
      setEmail: vi.fn(),
      setPassword: vi.fn(),
      onSubmit: vi.fn(),
    });

    render(
      <MemoryRouter>
        <RegisterPage />
      </MemoryRouter>
    );

    expect(screen.getByRole("button", { name: /creating/i })).toBeDisabled();
    expect(screen.getByText("Email is already registered.")).toBeInTheDocument();
  });
});
