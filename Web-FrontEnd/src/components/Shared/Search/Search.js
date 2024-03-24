import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { Button, Form, InputGroup } from "react-bootstrap";
import { faSearch } from "@fortawesome/free-solid-svg-icons";

export default function Search(props) {
  const search = props.search;
  const request = props.request;
  const setRequest = props.setRequest;

  const onInputChange = (e) => {
    setRequest((prevState) => {
      let currentValue = e.target.value;

      return {
        ...prevState,
        search: currentValue,
        hasMore: true,
      };
    });
  };

  return (
    <div style={{ width: "30rem", margin: "0px auto" }}>
      <InputGroup>
        <Form.Control
          placeholder="Search for some user info..."
          aria-label="Search for some user info..."
          aria-describedby="Search for some user info..."
          value={request?.search || ""}
          onChange={onInputChange}
        />
        <Button variant="outline-danger" onClick={search}>
          <FontAwesomeIcon icon={faSearch} /> Search
        </Button>
      </InputGroup>
    </div>
  );
}
