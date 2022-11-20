import React, { useState } from "react";
import Carousel from "react-bootstrap/Carousel";

import styles from "./SwipingCard.module.css";

export default function SwipingCardCarousel(props) {
  const images = props.images;
  const [index, setIndex] = useState(0);

  const handleSelect = (selectedIndex, e) => {
    setIndex(selectedIndex);
  };

  return (
    <Carousel interval={null} activeIndex={index} onSelect={handleSelect}>
      {images.map((image, i) => (
        <Carousel.Item key={i + 1} className={styles["disable-indicators"]}>
          <img
            className={`d-block w-100 ${styles["image-size"]}`}
            src={image.url}
            alt={image}
          />
        </Carousel.Item>
      ))}
    </Carousel>
  );
}
