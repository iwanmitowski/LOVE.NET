import { createContext } from "react";
import { useLocalStorage } from "../hooks/useLocalStorage";

export const IdentityContext = createContext();

export const IdentityProvider = ({
    children,
}) => {
    const [user, setUser] =  useLocalStorage('auth', null);

    const userLogin = (data) => {
        setUser(data);
    }

    const userLogout = (data) => {
        setUser(null);
    }

    return (
        <IdentityContext.Provider value={{
            user,
            userLogin,
            userLogout,
            isLogged: user && !!user.token
        }}>
            {children}
        </IdentityContext.Provider>
    )
}