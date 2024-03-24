import Moment from "react-moment";

import styles from "../ChatModal.module.css";

export default function AwayMessage(props) {
  const message = props.message;
  const profilePicture = props.profilePicture;

  return (
    <div className={`${styles.media} w-75 mb-3`}>
      <img
        src={profilePicture}
        alt="user"
        width="50"
        height="50"
        className="rounded-circle"
      />
      <div className="media-body ms-3">
        <div className="bg-light rounded py-2 px-3 mb-2">
          <p className="mb-0 text-muted">{message.text}</p>
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
