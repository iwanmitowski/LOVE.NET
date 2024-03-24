export default function DatingAdvices() {
  const styles = {
    width: "30rem",
    borderRadius: "21px 21px 0 0",
  };

  const advices = [
    "Be yourself: Don't try to be someone you're not in order to impress your date. It's important to be genuine and authentic in order to build a strong connection with your partner",
    "Be open and honest: Communication is key in any relationship, so make sure to be open and honest with your partner. This will help to build trust and create a sense of intimacy",
    "Be respectful: Treat your partner with respect and kindness, and expect the same in return. Remember to listen to their needs and boundaries, and always communicate openly and honestly",
    "Be adventurous: Try new things and go on exciting dates to keep things interesting. This can include trying new activities, visiting new places, or trying new foods",
    "Be positive: A positive attitude can go a long way in any relationship. Make sure to maintain a positivity and focus on the good aspects of your relationship, rather than negatives",
    "Be patient: Relationships take time to develop, be patient and don't try to force things. Focus on building a strong foundation and enjoying the process of getting to know each other",
    "Be safe. Make sure to take precautions to protect yourself and your personal information when dating. Meet in a public place, letting a friend know where you're going.",
    "Be open to feedback. If your date gives you constructive criticism or feedback, try to take it in stride. Everyone has different experiences and perspectives, and you can learn that.",
  ];

  return (
    <div
      className="d-md-flex flex-wrap w-100 my-md-3 pl-md-3"
      style={{
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
      }}
    >
      <div className="bg-light mr-md-3 pt-3 px-3 pt-md-5 px-md-5 text-center text-dark overflow-hidden">
        <div className="my-3 py-3">
          <h2 className="display-5">#1</h2>
        </div>
        <div
          className="bg-dark box-shadow mx-auto text-light p-3"
          style={styles}
        >
          {advices[0]}
        </div>
      </div>
      <div className="bg-dark mr-md-3 pt-3 px-3 pt-md-5 px-md-5 text-center text-white overflow-hidden">
        <div className="my-3 py-3">
          <h2 className="display-5">#2</h2>
        </div>
        <div
          className="bg-light box-shadow mx-auto text-dark p-3"
          style={styles}
        >
          {advices[1]}
        </div>
      </div>
      <div className="bg-dark mr-md-3 pt-3 px-3 pt-md-5 px-md-5 text-center text-white overflow-hidden">
        <div className="my-3 py-3">
          <h2 className="display-5">#3</h2>
        </div>
        <div
          className="bg-light box-shadow mx-auto text-dark p-3"
          style={styles}
        >
          {advices[2]}
        </div>
      </div>
      <div className="bg-light mr-md-3 pt-3 px-3 pt-md-5 px-md-5 text-center text-dark overflow-hidden">
        <div className="my-3 py-3">
          <h2 className="display-5">#4</h2>
        </div>
        <div
          className="bg-dark box-shadow mx-auto text-light p-3"
          style={styles}
        >
          {advices[3]}
        </div>
      </div>
      <div className="bg-light mr-md-3 pt-3 px-3 pt-md-5 px-md-5 text-center text-dark overflow-hidden">
        <div className="my-3 py-3">
          <h2 className="display-5">#5</h2>
        </div>
        <div
          className="bg-dark box-shadow mx-auto text-light p-3"
          style={styles}
        >
          {advices[4]}
        </div>
      </div>
      <div className="bg-dark mr-md-3 pt-3 px-3 pt-md-5 px-md-5 text-center text-white overflow-hidden">
        <div className="my-3 py-3">
          <h2 className="display-5">#6</h2>
        </div>
        <div
          className="bg-light box-shadow mx-auto text-dark p-3"
          style={styles}
        >
          {advices[5]}
        </div>
      </div>
      <div className="bg-dark mr-md-3 pt-3 px-3 pt-md-5 px-md-5 text-center text-white overflow-hidden">
        <div className="my-3 py-3">
          <h2 className="display-5">#7</h2>
        </div>
        <div
          className="bg-light box-shadow mx-auto text-dark p-3"
          style={styles}
        >
          {advices[6]}
        </div>
      </div>
      <div className="bg-light mr-md-3 pt-3 px-3 pt-md-5 px-md-5 text-center text-dark overflow-hidden">
        <div className="my-3 py-3">
          <h2 className="display-5">#8</h2>
        </div>
        <div
          className="bg-dark box-shadow mx-auto text-light p-3"
          style={styles}
        >
          {advices[7]}
        </div>
      </div>
      );
    </div>
  );
}
