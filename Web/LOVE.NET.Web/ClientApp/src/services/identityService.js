import { axiosInternal, instance } from "../services/api";
import { globalConstants, identityConstants } from "../utils/constants";
import * as date from "../utils/date";

const baseUrl = globalConstants.API_URL + "identity";

export async function login(user) {
  if (!user.email || !user.password) {
    throw new Error(identityConstants.FILL_REQUIRED_FIELDS);
  }

  try {
    const response = await axiosInternal.post(`${baseUrl}/login`, user);

    return response.data;
  } catch (error) {
    console.log(error.response.data.error);
    throw new Error(error.response.data.error);
  }
}

export async function register(user, validCountries) {
  if (
    !user.email ||
    !user.userName ||
    !user.password ||
    !user.confirmPassword ||
    !user.bio ||
    !user.countryId ||
    !user.cityId
  ) {
    throw new Error(identityConstants.FILL_REQUIRED_FIELDS);
  }

  if (user.password.length < identityConstants.PASSWORD_MIN_LENGTH) {
    throw new Error(identityConstants.TOO_SHORT_PASSWORD);
  }

  if (user.confirmPassword !== user.password) {
    throw new Error(identityConstants.PASSWORDS_DONT_MATCH);
  }

  const EMAIL_REGEX = identityConstants.EMAIL_REGEX;
  if (!EMAIL_REGEX.test(user.email)) {
    throw new Error(identityConstants.INVALID_EMAIL);
  }

  if (user.bio > identityConstants.BIO_MAX_LENGTH) {
    throw new Error(identityConstants.TOO_LONG_BIO);
  }

  if (user.userName > identityConstants.USERNAME_MAX_LENGTH) {
    throw new Error(identityConstants.TOO_LONG_USERNAME);
  }

  if (user.birthdate < date.getLatestLegal()) {
    throw new Error(identityConstants.UNDERAGED_USER);
  }

  if (user.cityId < 1 || user.cityId > identityConstants.CITIES_MAX_COUNT) {
    throw new Error(identityConstants.INVALID_CITY);
  }

  if (
    user.countryId < 1 ||
    user.countryId > identityConstants.COUNTRIES_MAX_COUNT
  ) {
    throw new Error(identityConstants.INVALID_COUNTRY);
  }

  var formData = new FormData();
  for (var key in user) {
    formData.append(key, user[key]);
  }

  try {
    if (user.image) {
      const fileExtension = user.image.name.split(".").at(-1);
      const allowedFileTypes = ["jpg", "jpeg", "png"];
  
      if (!allowedFileTypes.includes(fileExtension)) {
        throw new Error(identityConstants.UNSUPPORTED_FILE_TYPE);
      }
    }
  
    await instance.post(`${baseUrl}/register`, formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    });
  } catch (error) {
    throw new Error(error.response.data.Error[0]);
  }
}

export async function logout() {
  try {
    await axiosInternal.post(`${baseUrl}/logout`);
  } catch (error) {
    console.log(error);
  }
}
