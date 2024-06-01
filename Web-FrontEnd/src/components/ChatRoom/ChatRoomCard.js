import { Button, Card } from "react-bootstrap";
import styles from "./ChatRoom.module.css";

export default function ChatRoomCard(props) {
  const imgSrc = props.imgSrc;
  const title = props.title;
  const join = props.join;

  return (
    <Card
      border="danger"
      style={{ width: "30rem" }}
      className="m-4 w-25 rounded shadow overflow-hidden"
    >
      <Card.Img className={styles["chatroom-img"]} variant="top" src={imgSrc} />
      <Card.Header>
        <Card.Title className="m-0">{title}</Card.Title>
      </Card.Header>
      <Card.Body>
        <Button
          className="m-3"
          variant="dark"
          type="submit"
          onClick={() => {
            join();
          }}
        >
          Join
        </Button>
        {/* <Card.Text>{`People Count: ${count}`}</Card.Text> */}
      </Card.Body>
    </Card>
  );
}
