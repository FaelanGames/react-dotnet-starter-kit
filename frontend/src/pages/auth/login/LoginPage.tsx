import { Link } from "react-router-dom";
import { useLoginForm } from "./useLoginForm";

export function LoginPage() {
  const { email, password, busy, error, setEmail, setPassword, onSubmit } = useLoginForm();

  return (
    <div style={{ maxWidth: 420, margin: "48px auto", padding: 16 }}>
      <h1 style={{ marginBottom: 8 }}>Login</h1>
      <p style={{ marginTop: 0 }}>
        Don&apos;t have an account? <Link to="/register">Register</Link>
      </p>

      <form onSubmit={onSubmit}>
        <label>
          Email
          <input
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            type="email"
            autoComplete="email"
            style={{ width: "100%", padding: 8, marginTop: 4, marginBottom: 12 }}
          />
        </label>

        <label>
          Password
          <input
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            type="password"
            autoComplete="current-password"
            style={{ width: "100%", padding: 8, marginTop: 4, marginBottom: 12 }}
          />
        </label>

        {error && (
          <div style={{ marginBottom: 12, color: "crimson" }}>{error}</div>
        )}

        <button disabled={busy} style={{ width: "100%", padding: 10 }}>
          {busy ? "Signing in..." : "Sign in"}
        </button>
      </form>
    </div>
  );
}
