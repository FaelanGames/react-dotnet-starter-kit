import { type SubmitEventHandler, useMemo, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { login } from "../../../api/auth";
import { useAuth } from "../../../auth/useAuth";

type LoginFormState = {
  email: string;
  password: string;
  busy: boolean;
  error: string | null;
  setEmail: (value: string) => void;
  setPassword: (value: string) => void;
  onSubmit: SubmitEventHandler<HTMLFormElement>;
};

export function useLoginForm(): LoginFormState {
  const nav = useNavigate();
  const location = useLocation();
  const { api, setAuthTokens } = useAuth();

  const from = useMemo(() => {
    const st = location.state as { from?: string } | null;
    return st?.from ?? "/dashboard";
  }, [location.state]);

  const [email, setEmailState] = useState("");
  const [password, setPasswordState] = useState("");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const onSubmit: SubmitEventHandler<HTMLFormElement> = async (e) => {
    e.preventDefault();
    setBusy(true);
    setError(null);

    try {
      const res = await login(api, email, password);
      setAuthTokens({
        accessToken: res.accessToken,
        refreshToken: res.refreshToken,
      });
      nav(from, { replace: true });
    } catch (err: any) {
      setError(err?.message ?? "Login failed");
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
