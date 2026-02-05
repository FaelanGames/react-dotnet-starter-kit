import { test, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import { BrowserRouter } from "react-router-dom";
import { AuthProvider } from "../auth/AuthProvider";
import { LoginPage } from "../pages/LoginPage";

test("renders login page", () => {
  render(
    <AuthProvider>
      <BrowserRouter>
        <LoginPage />
      </BrowserRouter>
    </AuthProvider>
  );

  expect(screen.getByRole("heading", { name: /login/i })).toBeInTheDocument();
});
