let provider: () => Promise<string | null> = async () => null;

/** O mesmo provedor de token do client HTTP, para o SignalR (registrado pelo AuthProvider). */
export function setHubTokenProvider(fn: () => Promise<string | null>) {
  provider = fn;
}

export function getAccessTokenForHub() {
  return provider();
}
