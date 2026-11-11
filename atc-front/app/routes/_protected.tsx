import { RequireAuth } from "@/auth/require-auth";

export default function Protected() {
  return <RequireAuth />;
}
