import { create } from "zustand";
import type { LoginResponse } from "@/api/types";

// localStorage keys are load-bearing legacy contracts: the axios interceptor
// and the SignalR accessTokenFactory read "auth" directly.
const AUTH_KEY = "auth";
const LOCATION_KEY = "location";

export interface GeoLocation {
  latitude: number;
  longitude: number;
}

interface AuthState {
  user: LoginResponse | null;
  location: GeoLocation | null;
  login: (data: LoginResponse) => void;
  logout: () => void;
  patchToken: (token: string) => void;
  setLocation: (location: GeoLocation) => void;
}

function read<T>(key: string): T | null {
  try {
    const raw = localStorage.getItem(key);
    return raw ? (JSON.parse(raw) as T) : null;
  } catch {
    return null;
  }
}

export const useAuthStore = create<AuthState>()((set) => ({
  user: read<LoginResponse>(AUTH_KEY),
  location: read<GeoLocation>(LOCATION_KEY),

  login: (data) => {
    localStorage.setItem(AUTH_KEY, JSON.stringify(data));
    const location = { latitude: data.latitude, longitude: data.longitude };
    localStorage.setItem(LOCATION_KEY, JSON.stringify(location));
    set({ user: data, location });
  },

  logout: () => {
    localStorage.removeItem(AUTH_KEY);
    localStorage.removeItem(LOCATION_KEY);
    set({ user: null, location: null });
  },

  patchToken: (token) =>
    set((state) => {
      if (!state.user) return state;
      const user = { ...state.user, token };
      localStorage.setItem(AUTH_KEY, JSON.stringify(user));
      return { user };
    }),

  setLocation: (location) => {
    localStorage.setItem(LOCATION_KEY, JSON.stringify(location));
    set({ location });
  },
}));

export const isLogged = (state: { user: LoginResponse | null }) => !!state.user?.token;

/** Non-hook accessors for the axios/SignalR layers. */
export function currentToken(): string | undefined {
  return useAuthStore.getState().user?.token;
}

export function currentUserId(): string | undefined {
  return useAuthStore.getState().user?.id;
}
