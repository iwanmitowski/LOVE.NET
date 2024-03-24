import { Link } from "react-router-dom";

export default function NotFound() {
  return (
    <div className="text-center">
      <h1>Page not found</h1>
      <div className="mt-3">
        <Link className="nav-link" to="/LOVE.NET">
          Take me back
        </Link>
      </div>
    </div>
  );
}
