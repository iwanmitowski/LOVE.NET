import React from "react";
import { Modal, Button } from "react-bootstrap";

const ConfirmationModal = ({
  show,
  handleClose,
  handleConfirm,
  contentText,
  cancelButtonText,
  confirmButtonText,
}) => {
  return (
    <Modal show={show} onHide={handleClose} backdrop="static" centered>
      <Modal.Header closeButton>
        <Modal.Title>
          <h4 className="m-0">Confirmation</h4>
        </Modal.Title>
      </Modal.Header>
      <Modal.Body>{contentText}</Modal.Body>
      <Modal.Footer className="d-flex align-items-center justify-content-between">
        <Button variant="danger" onClick={handleClose}>
          {cancelButtonText}
        </Button>
        <Button variant="dark" onClick={handleConfirm}>
          {confirmButtonText}
        </Button>
      </Modal.Footer>
    </Modal>
  );
};

export default ConfirmationModal;
