import { Link } from "react-router-dom";

export default function NotFound() {
  return (
    <div className="text-center">
      <h1>Forbidden! You can't do this</h1>
      <div className="mt-3">
        <Link className="nav-link" to="/">
          Take me back
        </Link>
      </div>
    </div>
  );
}
