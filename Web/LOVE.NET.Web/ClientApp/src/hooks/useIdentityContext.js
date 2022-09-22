import { useContext } from "react";
import { IdentityContext } from "../contexts/IdentityContext";

export const useIdentityContext = () => {
    const context = useContext(IdentityContext);

    return context;
};