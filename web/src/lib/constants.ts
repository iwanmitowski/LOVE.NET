// The gateway keeps the monolith's old origin, so localhost:8080 is the
// default everywhere; override via Vite env for other deployments.
export const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:8080";
export const API_URL = `${BASE_URL}/api`;

export const validation = {
  PASSWORD_MIN_LENGTH: 5,
  BIO_MAX_LENGTH: 255,
  USERNAME_MAX_LENGTH: 100,
  MINIMAL_AGE: 18,
} as const;

export const PAGE_SIZE = 10;
