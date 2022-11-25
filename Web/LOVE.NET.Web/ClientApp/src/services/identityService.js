import { instance } from "../services/api";
import { globalConstants, identityConstants } from "../utils/constants";
import * as date from "../utils/date";

const baseUrl = globalConstants.API_URL + "identity";

export async function login(user) {
  try {
    if (!user.email || !user.password) {
      throw new Error(identityConstants.FILL_REQUIRED_FIELDS);
    }
    const response = await instance.post(`${baseUrl}/login`, user);

    return response.data;
  } catch (error) {
    const errorMessage = getErrorMessage(error);
    throw new Error(errorMessage);
  }
}

export async function register(user) {
  var formData = new FormData();
  for (var key in user) {
    formData.append(key, user[key]);
  }

  for (var i = 0; i < user?.newImages?.length; i++) {
    formData.append("NewImages", user.newImages[i]);
  }

  try {
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

    if (user.image) {
      const fileExtension = user.image.name.split(".").at(-1);
      const allowedFileTypes = ["jpg", "jpeg", "png"];

      if (!allowedFileTypes.includes(fileExtension)) {
        throw new Error(identityConstants.UNSUPPORTED_FILE_TYPE);
      }
    }

    const EMAIL_REGEX = identityConstants.EMAIL_REGEX;
    if (!EMAIL_REGEX.test(user.email)) {
      throw new Error(identityConstants.INVALID_EMAIL);
    }
    if (user.bio.length > identityConstants.BIO_MAX_LENGTH) {
      throw new Error(identityConstants.TOO_LONG_BIO);
    }

    if (user.userName.length > identityConstants.USERNAME_MAX_LENGTH) {
      throw new Error(identityConstants.TOO_LONG_USERNAME);
    }

    if (user.birthdate < date.getLatestLegal()) {
      throw new Error(identityConstants.UNDERAGED_USER);
    }

    if (
      user.genderId < 1 ||
      user.genderId > identityConstants.CITIES_MAX_COUNT
    ) {
      throw new Error(identityConstants.INVALID_GENDER);
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

    await instance.post(`${baseUrl}/register`, formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    });
  } catch (error) {
    const errorMessage = getErrorMessage(error);
    throw new Error(errorMessage);
  }
}

export async function logout() {
  try {
    await instance.post(`${baseUrl}/logout`);
  } catch (error) {
    console.log(error);
  }
}

export async function getAccount(id) {
  try {
    const response = await instance.get(`${baseUrl}/account/${id}`);

    return response.data;
  } catch (error) {
    const errorMessage = getErrorMessage(error);
    throw new Error(errorMessage);
  }
}

export async function editAccount(user) {
  var formData = new FormData();
  for (let key in user) {
    if (key === "images") {
      for (let i = 0; i < user?.images?.length; i++) {
        formData.append(`Images`, JSON.stringify(user.images[i]));
      }
    } else {
      formData.append(key, user[key]);
    }
  }

  for (let i = 0; i < user?.newImages?.length; i++) {
    formData.append("NewImages", user.newImages[i]);
  }

  try {
    if (
      !user.userName ||
      !user.password ||
      !user.confirmPassword ||
      !user.bio ||
      !user.countryId ||
      !user.cityId
    ) {
      throw new Error(identityConstants.FILL_REQUIRED_FIELDS);
    }

    if (user.confirmPassword !== user.password) {
      throw new Error(identityConstants.PASSWORDS_DONT_MATCH);
    }

    if (user.bio.length > identityConstants.BIO_MAX_LENGTH) {
      throw new Error(identityConstants.TOO_LONG_BIO);
    }

    if (user.userName.length > identityConstants.USERNAME_MAX_LENGTH) {
      throw new Error(identityConstants.TOO_LONG_USERNAME);
    }

    if (user.birthdate < date.getLatestLegal()) {
      throw new Error(identityConstants.UNDERAGED_USER);
    }

    if (
      user.genderId < 1 ||
      user.genderId > identityConstants.CITIES_MAX_COUNT
    ) {
      throw new Error(identityConstants.INVALID_GENDER);
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

    const response = await instance.put(
      `${baseUrl}/account/${user.id}`,
      formData,
      {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      }
    );

    return response.data;
  } catch (error) {
    const errorMessage = getErrorMessage(error);
    throw new Error(errorMessage);
  }
}

export async function refreshToken() {
  await instance.post(`${baseUrl}/refreshToken`);
}

function getErrorMessage(error) {
  // Client side validation error
  const validationError = error?.response?.data?.Error;
  // Serverside validation error
  const validationErrors =
    (error?.response?.data?.errors &&
      Object.values(error?.response?.data?.errors)) ||
    [];

  let errors = [...validationErrors, validationError].filter((e) => !!e);
  if (!errors.length) {
    errors = [...errors, error.message];
  }

  const errorMessage = errors.join("\n");

  return errorMessage;
}
