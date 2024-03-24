import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useIdentityContext } from "../../../hooks/useIdentityContext";
import * as identityService from "../../../services/identityService";

export default function Logout() {
    const navigate = useNavigate();
    const { userLogout } = useIdentityContext();

    useEffect(() => {
        identityService.logout()
            .then(() => {
                userLogout();
            })
            .finally(() => {
                navigate('/', { replace: true });
            });
    });

    return null;
}