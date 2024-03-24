import { createContext } from "react";
import { useLocalStorage } from "../hooks/useLocalStorage";

export const IdentityContext = createContext();

export const IdentityProvider = ({ children }) => {

  const defaultPreferences = {
    maxAge: 100,
    maxDistance: 600,
    aroundTheWorld: false,
    gender: -1,
  };

  const [user, setUser] = useLocalStorage("auth", null);
  const [location, setLocation] = useLocalStorage("location", null);
  const [preferences, setPreferences] = useLocalStorage("preferences", defaultPreferences);

  const userLogin = (data) => {
    setUser(data);
    setLocation({
      latitude: data.latitude,
      longitude: data.longitude,
    });
    if (!preferences) {
      setPreferences(defaultPreferences)
    }
  };

  const userLogout = () => {
    setUser(null);
    setLocation(null);
  };

  const setUserLocation = (data) => {
    setLocation(data);
  }

  const setUserPreferences = (data) => {
    if (!!data) {
      setPreferences(data);
    }
  }

  return (
    <IdentityContext.Provider
      value={{
        user,
        location,
        setUserLocation,
        preferences,
        setUserPreferences,
        userLogin,
        userLogout,
        isLogged: user && !!user.token,
      }}
    >
      {children}
    </IdentityContext.Provider>
  );
};
