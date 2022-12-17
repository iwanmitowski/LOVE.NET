export const globalConstants = {
  // BASE_URL: "https://localhost:44319",
  // API_URL: "https://localhost:44319/api/",
  BASE_URL: "https://lovenet.azurewebsites.net",
  API_URL: "https://lovenet.azurewebsites.net/api/",
};

export const identityConstants = {
  FILL_REQUIRED_FIELDS: "Please fill required fields",
  EMAIL_REGEX:
    // eslint-disable-next-line no-useless-escape
    /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/,
  INVALID_EMAIL: "Invalid email",
  PASSWORDS_DONT_MATCH: "Password and confirm password must match",
  PASSWORD_MIN_LENGTH: 5,
  TOO_SHORT_PASSWORD: "Password is too short",
  BIO_MAX_LENGTH: 255,
  TOO_LONG_BIO: "Shorten your bio",
  USERNAME_MAX_LENGTH: 100,
  TOO_LONG_USERNAME: "Shorten your username",
  UNDERAGED_USER: "You must to be 18 years old to register",
  CITIES_MAX_COUNT: 42905,
  INVALID_CITY: "Invalid city",
  COUNTRIES_MAX_COUNT: 239,
  INVALID_COUNTRY: "Invalid country",
  UNSUPPORTED_FILE_TYPE: "Unsupported file type",
  GENDERS_MAX_COUNT: 3,
  INVALID_GENDER: "Invalid gender",
};
