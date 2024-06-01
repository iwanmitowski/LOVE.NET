import Moment from "react-moment";

import styles from "../ChatModal.module.css";

export default function HomeMessage(props) {
  const message = props.message;

  return (
    <div className={`${styles["media-home"]} home w-50 ms-auto mb-3`}>
      <div className="media-body">
        <div className="bg-primary rounded py-2 px-3 mb-2">
          <p className="mb-0 text-white d-flex">{message.text}</p>
        </div>
        <p className="small text-muted">
          <Moment local format="D MMMM YYYY HH:mm">
            {message.createdOn}
          </Moment>
        </p>
      </div>
    </div>
  );
}
