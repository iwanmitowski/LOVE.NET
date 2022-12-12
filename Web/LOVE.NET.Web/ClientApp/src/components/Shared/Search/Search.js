import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { Button, Form, InputGroup } from "react-bootstrap";
import { faSearch } from "@fortawesome/free-solid-svg-icons";

export default function Search(props) {
  return (
    <div style={{ width: "30rem", margin: "0px auto" }}>
      <InputGroup>
        <Form.Control
          placeholder="Search for some user info..."
          aria-label="Search for some user info..."
          aria-describedby="Search for some user info..."
        />
        <Button variant="outline-danger" id="button-addon2">
          <FontAwesomeIcon icon={faSearch} /> Search
        </Button>
      </InputGroup>
    </div>
  );
}
