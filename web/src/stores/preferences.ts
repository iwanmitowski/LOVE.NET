import { create } from "zustand";
import { persist, createJSONStorage } from "zustand/middleware";

export interface Preferences {
  maxAge: number;
  maxDistance: number;
  aroundTheWorld: boolean;
  gender: number; // -1 = all
}

interface PreferencesState extends Preferences {
  set: (patch: Partial<Preferences>) => void;
}

export const defaultPreferences: Preferences = {
  maxAge: 100,
  maxDistance: 600,
  aroundTheWorld: false,
  gender: -1,
};

export const usePreferencesStore = create<PreferencesState>()(
  persist(
    (set) => ({
      ...defaultPreferences,
      set: (patch) => set(patch),
    }),
    {
      name: "preferences",
      storage: createJSONStorage(() => localStorage),
    },
  ),
);
