import { useEffect, useState } from "react";
import { Form } from "react-bootstrap";
import RangeSlider from "react-bootstrap-range-slider";
import { useIdentityContext } from "../../hooks/useIdentityContext";

import * as genderService from "../../services/genderService";
import * as distance from "../../utils/distance";

export default function UserPreferences(props) {
  const { location, preferences, setUserPreferences } = useIdentityContext();
  const defaultPreferences = {
    maxAge: 100,
    maxDistance: 600,
    aroundTheWorld: false,
    gender: -1,
  };

  const [genders, setGenders] = useState([
    {
      id: -1,
      name: "All",
    },
  ]);

  const setFilteredUsers = props.setFilteredUsers;
  const users = props.users;
  const areShown = props.areShown;
  
  useEffect(() => {
    if (!preferences) {
      setUserPreferences(defaultPreferences);
    }
  }, []);

  useEffect(() => {
    let currentFilteredUsers = users.filter((u) => u.age <= preferences.maxAge);

    if (preferences.gender !== -1) {
      currentFilteredUsers = currentFilteredUsers.filter(
        (u) => u.genderId === preferences.gender
      );
    }

    if (!preferences.aroundTheWorld) {
      currentFilteredUsers = currentFilteredUsers.filter(
        (u) =>
          distance.inKms(
            location.latitude,
            location.longitude,
            u.latitude,
            u.longitude
          ) <= preferences.maxDistance
      );
    }

    setFilteredUsers(currentFilteredUsers);
  }, [preferences, users]);

  useEffect(() => {
    if (genders.length === 1) {
      genderService
        .getAll()
        .then((res) => setGenders((prevState) => [...prevState, ...res]));
    }
  }, [genders.length]);

  const onInputChange = (e) => {
    let currentName = e.target.name;
    let currentValue = e.target.value;

    let newPreferences = {
      ...preferences,
    };

    if (currentName === "aroundTheWorld") {
      newPreferences = {
        ...newPreferences,
        [currentName]: !preferences.aroundTheWorld,
      };
    } else {
      newPreferences = {
        ...newPreferences,
        [currentName]: parseInt(currentValue),
      };
    }

    setUserPreferences(newPreferences);
  };

  return (
    <div
      className="d-flex flex-column flex-shrink-0 p-3 bg-light fixed-bottom"
      style={{
        width: "280px",
        minHeight: "100vh",
        textAlign: "left",
        left: "auto",
      }}
    >
      <div className="nav-link text-uppercase" to="/LOVE.NET">
        Filters
      </div>
      <hr />
      <Form.Group className="m-3">
        <h6>
          <strong>Age</strong>
        </h6>
        <RangeSlider
          name="maxAge"
          min={18}
          variant="dark"
          value={preferences.maxAge}
          onChange={onInputChange}
        />
      </Form.Group>
      <Form.Group className="m-3">
        <h6>
          <strong>Distance</strong>
        </h6>
        <div className="d-flex align-items-center">
          <h6 className="m-0">All over the world</h6>
          <Form.Check
            className="mx-2"
            type="checkbox"
            id="all-over-the-world"
            name="aroundTheWorld"
            onChange={onInputChange}
          />
        </div>
        {!preferences.aroundTheWorld && (
          <div className="mt-2">
            <h6>Total distance</h6>
            <RangeSlider
              step={10}
              max={600}
              name="maxDistance"
              variant="dark"
              value={preferences.maxDistance}
              onChange={onInputChange}
            />
          </div>
        )}
      </Form.Group>
      <Form.Group className="m-3">
        <>
          <h6>
            <strong>Genders</strong>
          </h6>
          {genders.map((g) => {
            return (
              <Form.Check
                inline
                label={g.name}
                name="gender"
                type="radio"
                key={`${g.id}-${g.name}`}
                id={`${g.id}-${g.name}`}
                defaultChecked={g.id === -1}
                value={g.id}
                onChange={onInputChange}
              />
            );
          })}
        </>
      </Form.Group>
    </div>
  );
}
