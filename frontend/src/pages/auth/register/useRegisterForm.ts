import { type SubmitEventHandler, useState } from "react";
import { useNavigate } from "react-router-dom";
import { register } from "../../../api/auth";
import { useAuth } from "../../../auth/useAuth";

type RegisterFormState = {
  email: string;
  password: string;
  busy: boolean;
  error: string | null;
  setEmail: (value: string) => void;
  setPassword: (value: string) => void;
  onSubmit: SubmitEventHandler<HTMLFormElement>;
};

export function useRegisterForm(): RegisterFormState {
  const nav = useNavigate();
  const { api, setAuthTokens } = useAuth();

  const [email, setEmailState] = useState("");
  const [password, setPasswordState] = useState("");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const onSubmit: SubmitEventHandler<HTMLFormElement> = async (e) => {
    e.preventDefault();
    setBusy(true);
    setError(null);

    try {
      const res = await register(api, email, password);
      setAuthTokens({
        accessToken: res.accessToken,
        refreshToken: res.refreshToken,
      });
      nav("/dashboard", { replace: true });
    } catch (err: any) {
      if (err?.status === 409) {
        setError("Email is already registered.");
      } else {
        setError(err?.message ?? "Registration failed");
      }
    } finally {
      setBusy(false);
    }
  };

  return {
    email,
    password,
    busy,
    error,
    setEmail: setEmailState,
    setPassword: setPasswordState,
    onSubmit,
  };
}
