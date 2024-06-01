import React, { useEffect, useState } from "react";
import { Button, Col, Form } from "react-bootstrap";

import ImagesContainer from "../../Image/ImagesContainer";

const Step3 = ({ user, onInputChange, setUser, prevStep, nextStep }) => {
  const [imagePreviews, setImagePreviews] = useState([]);
  const [profilePictureId, setProfilePictureId] = useState(null);

  useEffect(() => {
    if (user.newImages) {
      const filesArray = Array.from(user.newImages);
      const previewsArray = filesArray.map((file) => {
        return new Promise((resolve) => {
          const reader = new FileReader();
          reader.onloadend = () => {
            resolve({
              id: file.name,
              url: reader.result,
              isProfilePicture: file.name === profilePictureId,
            });
          };
          reader.readAsDataURL(file);
        });
      });

      Promise.all(previewsArray).then((newImages) => {
        setImagePreviews(newImages);
      });
    }
  }, [user.newImages, profilePictureId]);

  const removeImageFromList = (id) => {
    setUser((prevState) => {
      const filteredImages = Array.from(prevState.newImages).filter(
        (i) => i.name !== id
      );

      return {
        ...prevState,
        newImages: filteredImages,
      };
    });
    setImagePreviews((prevState) => prevState.filter((img) => img.id !== id));
    setProfilePictureId((prevId) => (prevId === id ? null : prevId));
  };

  const setNewProfilePicture = (id) => {
    setProfilePictureId(id);
    setUser((prevState) => {
      const newImages = Array.from(prevState.newImages);
      const profilePicture = newImages.find(file => file.name === id);
      return {
        ...prevState,
        image: profilePicture, // Update the profile picture in the user state
      };
    });
    setImagePreviews((prevState) =>
      prevState.map((img) =>
        img.id === id
          ? { ...img, isProfilePicture: true }
          : { ...img, isProfilePicture: false }
      )
    );
  };

  return (
    <div>
      <h4>Your photos</h4>
      <Form.Group className="form-group mb-3 d-flex fs-5 fw-normal mt-2 text-black-50 align-items-center justify-content-between">
        <Form.Control
          type="file"
          name="newImages"
          multiple
          value={user?.newImages?.length ? null : ""}
          onChange={onInputChange}
          accept=".jpg,.jpeg,.png"
        />
      </Form.Group>
      <Form.Group
        className="form-group mb-3 d-flex fs-5 fw-normal mt-2 text-black-50 align-items-center justify-content-between"
        controlId="information"
      >
        <Col className="d-flex">
          {imagePreviews.length > 0 && (
            <ImagesContainer
              images={imagePreviews}
              removeImageFromList={removeImageFromList}
              setNewProfilePicture={setNewProfilePicture}
            />
          )}
        </Col>
      </Form.Group>
      <Form.Group className="d-flex justify-content-between align-items-center">
        <Button
          className="btn btn-outline-dark btn btn-light"
          onClick={prevStep}
        >
          Back
        </Button>
        <Button variant="dark" onClick={nextStep}>
          Next
        </Button>
      </Form.Group>
    </div>
  );
};

export default Step3;
