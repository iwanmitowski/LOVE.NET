import { Card } from "react-bootstrap";
export default function StatisticCard(props) {
    const header = props.header;
    const count = props.count;

    return (
        <Card border="danger" style={{ width: "30rem" }} className="m-4 w-25">
          <Card.Header>
            <Card.Title>
              {header}
            </Card.Title>
          </Card.Header>
          <Card.Body>
            <Card.Text>Count: {count}</Card.Text>
          </Card.Body>
        </Card>
    )
}