import { faGlobe } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { Link } from "react-router-dom";

export default function NotLoggedHome() {
  return (
    <div style={{ textAlign: "left" }}>
      <div className="p-5 mb-4 bg-light rounded-3 border border-danger">
        <div className="container-fluid py-5">
          <h1 className="display-5 fw-bold">Love.net</h1>
          <h3>
            <q>
              Help people have more successful and fulfilling relationships.
            </q>
          </h3>
          <p className="col-md-8 fs-4">
            Allowing connection and communication with other around the world in
            order to explore new worlds.
          </p>
          <Link className="btn btn-danger btn-lg" type="button" to="/register">
            <FontAwesomeIcon icon={faGlobe} /> Start your journey
          </Link>
        </div>
      </div>
      <div className="row align-items-md-stretch">
        <div className="col-md-8">
          <div className="h-100 p-5 text-white bg-dark rounded-3">
            <h2>Dating advices</h2>
            <p className="fs-5">
              Recommendations offered to individuals who are looking to develop
              romantic relationships. Dating advice may cover a wide range of
              topics, including how to communicate effectively, how to navigate
              conflicts, and how to build a strong and healthy relationship.
            </p>
            <Link
              className="btn btn-lg btn-outline-light"
              type="button"
              to="/info/advices"
            >
              Read more
            </Link>
          </div>
        </div>
        <div className="col-md-4">
          <div className="h-100 bg-light border border-danger rounded-3">
            <img
              alt="homeimage"
              className="img-fluid rounded-3"
              src="https://images.unsplash.com/photo-1500771967326-9b2f6200d1c6?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1170&q=80"
            />
          </div>
        </div>
      </div>
    </div>
  );
}
