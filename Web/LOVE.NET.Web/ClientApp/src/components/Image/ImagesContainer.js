import ImageCard from "./ImageCard";

/* eslint-disable jsx-a11y/alt-text */
export default function ImagesContainer(props) {
  const images = props.images;
  
  
  return (
    <div className="col d-flex flex-wrap justify-content-center">
      {images.map((img) => (
        <ImageCard
          key={img.id}
          imageUrl={img.url}
          imageId={img.id}
          isPfp={img.isProfilePicture}
        />
      ))}
    </div>
  );
}
