import { createContext } from "react";
import { useLocalStorage } from "../hooks/useLocalStorage";

export const IdentityContext = createContext();

export const IdentityProvider = ({ children }) => {
  const [user, setUser] = useLocalStorage("auth", null);
  const [location, setLocation] = useLocalStorage("location", null);

  const userLogin = (data) => {
    setUser(data);
    setLocation({
      latitude: data.latitude,
      longitude: data.longitude,
    });
  };

  const userLogout = () => {
    setUser(null);
    setLocation(null);
  };

  return (
    <IdentityContext.Provider
      value={{
        user,
        userLogin,
        userLogout,
        isLogged: user && !!user.token,
      }}
    >
      {children}
    </IdentityContext.Provider>
  );
};
