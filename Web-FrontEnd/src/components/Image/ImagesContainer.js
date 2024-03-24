import ImageCard from "./ImageCard";

/* eslint-disable jsx-a11y/alt-text */
export default function ImagesContainer(props) {
  const images = props.images;
  const removeImageFromList = props.removeImageFromList;
  const setNewProfilePicture = props.setNewProfilePicture;

  return (
    <div className="col d-flex flex-wrap justify-content-center">
      {images.map((img) => (
        <ImageCard
          key={img.id}
          imageUrl={img.url}
          imageId={img.id}
          isPfp={img.isProfilePicture}
          removeImageFromList={removeImageFromList}
          setNewProfilePicture={setNewProfilePicture}
        />
      ))}
    </div>
  );
}
