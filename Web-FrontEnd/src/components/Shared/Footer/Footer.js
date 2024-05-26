import { useIdentityContext } from "../../../hooks/useIdentityContext";

export default function Footer(props) {
  const year = new Date().getFullYear();
  const { isLogged } = useIdentityContext();
  const showPreferences = props.showPreferences;
  return (
    <div
      className={`p-3 p-3 ${
        isLogged && showPreferences ? "col-md-10" : "col-md-12"
      }`}
    >
      <span>
        © {year} Copyright <strong>Iwan Mitowski</strong>
      </span>
    </div>
  );
}
