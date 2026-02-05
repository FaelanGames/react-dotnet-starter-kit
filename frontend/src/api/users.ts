import type { ApiClient } from "./client";

export type MeResponse = {
  id: string;
  email: string;
  createdUtc: string;
};

export async function me(api: ApiClient): Promise<MeResponse> {
  return api.request<MeResponse>("/api/users/me", { method: "GET" });
}
