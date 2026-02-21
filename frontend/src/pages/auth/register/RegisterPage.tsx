import { Link } from "react-router-dom";
import { useRegisterForm } from "./useRegisterForm";

export function RegisterPage() {
  const { email, password, busy, error, setEmail, setPassword, onSubmit } = useRegisterForm();

  return (
    <div style={{ maxWidth: 420, margin: "48px auto", padding: 16 }}>
      <h1 style={{ marginBottom: 8 }}>Register</h1>
      <p style={{ marginTop: 0 }}>
        Already have an account? <Link to="/login">Login</Link>
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
          Password (min 8 chars)
          <input
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            type="password"
            autoComplete="new-password"
            style={{ width: "100%", padding: 8, marginTop: 4, marginBottom: 12 }}
          />
        </label>

        {error && (
          <div style={{ marginBottom: 12, color: "crimson" }}>{error}</div>
        )}

        <button disabled={busy} style={{ width: "100%", padding: 10 }}>
          {busy ? "Creating..." : "Create account"}
        </button>
      </form>
    </div>
  );
}
